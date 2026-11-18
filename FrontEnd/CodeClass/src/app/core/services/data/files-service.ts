import { CreateLectureFileDTO } from '../../DTO/lecturefiles/create-lecture-file.dto';
import { ToastService } from '../local/toast-service';
import { catchErrorToast } from '../../Utilities/catch-error-toast';
import { Injectable, effect, inject, signal } from '@angular/core';
import { Subject, Observable, defer, takeUntil, tap } from 'rxjs';
import { FilesApiService } from '../api/files-api-service';
import { AuthService } from './auth-service';
import { SignalRService } from '../signalr/signalr-service';
import { FileDetailDTO } from '../../DTO/files/file-detail.dto';
import { CodeChangesDTO } from '../../DTO/realtime/code-changes.dto';
import { SaveChangesDTO } from '../../DTO/files/save-changes.dto';
import { applyCodeChanges } from '../../../shared/Utilities/code-changes';

@Injectable({ providedIn: 'root' })
export class FilesService {
  private readonly api = inject(FilesApiService);
  private readonly toast = inject(ToastService);
  private readonly auth = inject(AuthService);
  private readonly hub = inject(SignalRService);
  private readonly currentFile = signal<FileDetailDTO | null>(null);
  private readonly cancelled = new Subject<void>();
  private owner = this.auth.user()?.id;

  readonly file = this.currentFile.asReadonly();
  readonly changes$ = this.hub.changes$;

  constructor() {
    effect(() => this.syncOwner());
    this.hub.changes$.subscribe((change) => this.accept(change));
    this.hub.fileDeleted$.subscribe((id) => this.invalidate(id));
    this.hub.reconnected$.subscribe(() => {
      this.currentFile.set(null);
    });
  }

  get(id: string): Observable<FileDetailDTO> {
    return defer(() => {
      this.syncOwner();
      return this.api.get(id).pipe(
        takeUntil(this.cancelled),
        tap((file) => this.set(file)),
      );
    });
  }

  list(page = 1, filters: Record<string, string | undefined> = {}) {
    return this.api.list(page, filters);
  }

  create(request: CreateLectureFileDTO) {
    return this.api
      .create(request)
      .pipe(catchErrorToast('Could not create this file.', this.toast));
  }

  run(id: string, code: string, stdin: string) {
    return this.api
      .run(id, { code, stdin })
      .pipe(
        catchErrorToast(
          'Could not run the code. The execution service may be unavailable or busy.',
          this.toast,
        ),
      );
  }

  save(id: string, changes: SaveChangesDTO): Promise<void> {
    return this.hub.save(id, changes);
  }

  set(file: FileDetailDTO): void {
    this.currentFile.set(file);
  }

  invalidate(id: string): void {
    if (this.currentFile()?.id === id) this.currentFile.set(null);
  }

  private syncOwner(): void {
    const owner = this.auth.user()?.id;
    if (owner !== this.owner) {
      this.owner = owner;
      this.cancelled.next();
      this.currentFile.set(null);
    }
  }

  private accept(change: CodeChangesDTO): void {
    const file = this.currentFile();
    if (file?.id !== change.fileId || change.revision <= file.savedRevision) return;
    if (change.baseRevision !== file.savedRevision) {
      this.invalidate(change.fileId);
      return;
    }

    try {
      this.set({
        ...file,
        fileName: change.fileName ?? file.fileName,
        language: change.language ?? file.language,
        savedContent: applyCodeChanges(file.savedContent, change.changeSets),
        savedRevision: change.revision,
      });
    } catch {
      this.invalidate(change.fileId);
    }
  }
}
