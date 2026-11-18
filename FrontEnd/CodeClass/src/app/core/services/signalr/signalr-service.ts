import { MemoryCacheService } from '../local/memory-cache-service';
import { Injectable, effect, inject, signal } from '@angular/core';
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { Subject, firstValueFrom } from 'rxjs';
import { Urls } from '../../constants/urls';
import { AuthService } from '../data/auth-service';
import { CodeChangesDTO } from '../../DTO/realtime/code-changes.dto';
import { NotificationDTO } from '../../DTO/realtime/notification.dto';
import { SaveChangesDTO } from '../../DTO/files/save-changes.dto';

export type ConnectionStatus = 'offline' | 'connecting' | 'connected' | 'reconnecting';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private readonly cache = inject(MemoryCacheService);
  private readonly auth = inject(AuthService);
  private readonly connection = new HubConnectionBuilder()
    .withUrl(Urls.hub, { withCredentials: true })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(LogLevel.Warning)
    .build();
  private readonly currentState = signal<ConnectionStatus>('offline');
  private starting?: Promise<void>;
  private stopping?: Promise<void>;
  private owner = this.auth.user()?.id;
  private generation = 0;
  private wanted = false;

  readonly state = this.currentState.asReadonly();
  readonly changes$ = new Subject<CodeChangesDTO>();
  readonly notifications$ = new Subject<NotificationDTO>();
  readonly fileDeleted$ = new Subject<string>();
  readonly lectureEnded$ = new Subject<string>();
  readonly reconnected$ = new Subject<void>();

  constructor() {
    effect(() => {
      const owner = this.auth.user()?.id;
      if (owner !== this.owner) {
        this.owner = owner;
        void this.stop().catch(() => undefined);
      }
    });

    this.connection.on('UpdateCode', (changes: CodeChangesDTO) => {
      this.cache.clear();
      this.changes$.next(changes);
    });
    this.connection.on('ReceiveNotification', (notice: NotificationDTO) =>
      this.notifications$.next(notice),
    );
    this.connection.on('FileDeleted', (_: string, fileId: string) => {
      this.cache.clear();
      this.fileDeleted$.next(fileId);
    });
    this.connection.on('LectureEnded', (lectureId: string) => {
      this.cache.clear();
      this.lectureEnded$.next(lectureId);
    });
    this.connection.onreconnecting(() => {
      this.currentState.set('reconnecting');
      // Me refreshes an expired cookie through the interceptor without logging out on a network failure.
      void firstValueFrom(this.auth.me()).catch(() => undefined);
    });
    this.connection.onreconnected(() => {
      this.currentState.set('connected');
      this.cache.clear();
      this.reconnected$.next();
    });
    this.connection.onclose(() => this.currentState.set('offline'));
  }

  start(): Promise<void> {
    if (this.stopping) return this.stopping.then(() => this.start());
    if (this.owner !== this.auth.user()?.id) {
      this.owner = this.auth.user()?.id;
      return this.stop().then(() => this.start());
    }
    this.wanted = true;
    if (this.connection.state === HubConnectionState.Reconnecting)
      return Promise.reject(new Error('The connection is reconnecting.'));
    if (this.connection.state === HubConnectionState.Connected) return Promise.resolve();
    if (this.starting) return this.starting;
    this.currentState.set('connecting');

    this.starting = this.connect(this.generation).finally(() => {
      this.starting = undefined;
    });
    return this.starting;
  }

  stop(): Promise<void> {
    if (this.stopping) return this.stopping;
    this.wanted = false;
    this.generation++;
    const pending = this.starting;

    // Finish shutting down the old identity before another session starts a connection.
    this.stopping = (async () => {
      await pending?.catch(() => undefined);
      await this.connection.stop();
      this.currentState.set('offline');
    })().finally(() => {
      this.stopping = undefined;
    });

    return this.stopping;
  }

  async join(fileId: string): Promise<void> {
    await this.start();
    await this.connection.invoke('JoinFile', fileId);
  }

  async leave(fileId: string): Promise<void> {
    if (this.connection.state === HubConnectionState.Connected)
      await this.connection.invoke('LeaveFile', fileId);
  }

  save(fileId: string, request: SaveChangesDTO): Promise<void> {
    return this.connection.invoke('SaveChanges', fileId, request).catch((error) => {
      this.cache.clear();
      throw error;
    });
  }

  private async connect(generation: number): Promise<void> {
    try {
      try {
        await this.connection.start();
      } catch (error) {
        if (!this.wanted || generation !== this.generation) throw error;
        await firstValueFrom(this.auth.refresh());
        await this.connection.start();
      }
      if (!this.wanted || generation !== this.generation) {
        await this.connection.stop();
        throw new Error('The authentication session changed.');
      }
      this.currentState.set('connected');
    } catch (error) {
      this.currentState.set('offline');
      throw error;
    }
  }
}
