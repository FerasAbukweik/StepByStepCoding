import { DialogService } from '../../../core/services/local/dialog-service';
import { CodeExecutionResultDTO } from '../../../core/DTO/execution/code-execution-result.dto';
import { Subscription, finalize } from 'rxjs';
import { ProgrammingLanguage } from '../../../core/enums/programming-language';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnChanges,
  OnDestroy,
  OnInit,
  SimpleChanges,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { FilesService } from '../../../core/services/data/files-service';
import { SignalRService } from '../../../core/services/signalr/signalr-service';
import { AuthService } from '../../../core/services/data/auth-service';
import { ToastService } from '../../../core/services/local/toast-service';
import { FileDetailDTO } from '../../../core/DTO/files/file-detail.dto';
import { CodeChangesDTO } from '../../../core/DTO/realtime/code-changes.dto';
import { CodeChangeSet } from '../../../core/DTO/realtime/code-change-set';
import { SaveChangesDTO } from '../../../core/DTO/files/save-changes.dto';
import { apiError } from '../../../core/utils/api-error';
import { applyCodeChanges } from '../../Utilities/code-changes';
import { CodeEditorComponent } from '../code-editor/code-editor.component';
import { EditorChange } from '../code-editor/editor-change.model';
import { StatePanelComponent } from '../state-panel/state-panel.component';

@Component({
  selector: 'app-code-page',
  imports: [CodeEditorComponent, StatePanelComponent],
  templateUrl: './code-page.component.html',
  host: { class: 'flex min-h-0 flex-1 flex-col' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CodePageComponent implements OnInit, OnChanges, OnDestroy {
  readonly fileId = input.required<string>();
  readonly readOnly = input(false);
  private readonly files = inject(FilesService);
  private readonly dialogs = inject(DialogService);
  protected readonly hub = inject(SignalRService);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly destroy = inject(DestroyRef);

  protected readonly file = signal<FileDetailDTO | null>(null);
  protected readonly code = signal('');
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly saving = signal(false);
  protected readonly running = signal(false);
  protected readonly stdin = signal('');
  protected readonly output = signal<CodeExecutionResultDTO | null>(null);
  private runRequest?: Subscription;
  protected readonly copying = signal(false);
  protected readonly dirty = signal(false);
  protected readonly conflict = signal(false);
  protected readonly canEdit = computed(() => {
    const file = this.file();
    return (
      !!file &&
      !this.readOnly() &&
      !this.conflict() &&
      (file.userId ? file.userId === this.auth.user()?.id : this.auth.canTeach())
    );
  });
  protected readonly language = computed(() =>
    this.file()?.language === 2 ? 'java' : this.file()?.language === 3 ? 'python' : 'cpp',
  );
  private queue: CodeChangeSet[] = [];
  private metadata?: { fileName?: string; language?: ProgrammingLanguage };
  private pending?: SaveChangesDTO;
  private buffered: CodeChangesDTO[] = [];
  private timer?: ReturnType<typeof setTimeout>;
  private generation = 0;
  private closed = false;

  hasUnsavedChanges(): boolean {
    return this.dirty() || this.saving();
  }

  ngOnInit(): void {
    this.files.changes$.pipe(takeUntilDestroyed(this.destroy)).subscribe((change) => {
      if (change.fileId !== this.fileId()) return;
      if (this.loading()) this.buffered.push(change);
      else this.accept(change);
    });
    this.hub.reconnected$.pipe(takeUntilDestroyed(this.destroy)).subscribe(() => void this.load());
    this.hub.fileDeleted$.pipe(takeUntilDestroyed(this.destroy)).subscribe((id) => {
      if (id === this.fileId()) this.error.set('This file was removed.');
    });

    // Every consumer gets the same default file-loading behavior.
    void this.load();
  }

  ngOnChanges(changes: SimpleChanges): void {
    const change = changes['fileId'];
    if (change && !change.firstChange) {
      this.runRequest?.unsubscribe();
      this.running.set(false);
      this.output.set(null);
      this.stdin.set('');
      void this.hub.leave(change.previousValue).catch(() => undefined);
      this.files.invalidate(change.previousValue);
      this.queue = [];
      this.metadata = undefined;
      this.pending = undefined;
      this.dirty.set(false);
      this.file.set(null);
      void this.load();
    }
  }

  ngOnDestroy(): void {
    this.runRequest?.unsubscribe();
    this.closed = true;
    this.generation++;
    clearTimeout(this.timer);
    this.files.invalidate(this.fileId());
    void this.hub.leave(this.fileId()).catch(() => undefined);
  }

  async load(): Promise<void> {
    const generation = ++this.generation;
    const id = this.fileId();
    this.loading.set(true);
    this.error.set('');
    this.buffered = [];

    try {
      // Join first, then replay broadcasts that arrived while fetching the snapshot.
      await this.hub.join(id);
      const file = await firstValueFrom(this.files.get(id));
      if (this.closed || generation !== this.generation) return;
      this.file.set(file);

      const metadata = { ...this.pending, ...this.metadata };
      const matchesMetadata =
        (!metadata.fileName || metadata.fileName === file.fileName) &&
        (!metadata.language || metadata.language === file.language);
      if (!this.dirty() || (this.code() === file.savedContent && matchesMetadata)) {
        this.code.set(file.savedContent);
        this.dirty.set(false);
        this.conflict.set(false);
        this.queue = [];
        this.metadata = undefined;
      } else {
        this.conflict.set(true);
      }

      this.loading.set(false);
      for (const change of this.buffered.splice(0).sort((a, b) => a.revision - b.revision))
        this.accept(change);
    } catch (error) {
      if (!this.closed && generation === this.generation) this.error.set(apiError(error));
    } finally {
      if (!this.closed && generation === this.generation) this.loading.set(false);
    }
  }

  edit(change: EditorChange): void {
    if (!this.canEdit()) return;
    this.code.set(change.code);
    this.dirty.set(true);
    this.queue.push(change.changeSet);
    clearTimeout(this.timer);
    this.timer = setTimeout(() => void this.save(), 300);
  }

  rename(event: Event): void {
    const name = (event.target as HTMLInputElement).value.trim();
    if (!name || name.length > 250) {
      this.toast.error('Enter a file name between 1 and 250 characters.');
      return;
    }
    if (name === (this.metadata?.fileName ?? this.pending?.fileName ?? this.file()?.fileName))
      return;
    this.changeMetadata({ fileName: name });
  }

  changeLanguage(event: Event): void {
    const language = Number((event.target as HTMLSelectElement).value);
    if (![1, 2, 3].includes(language)) return;
    if (language === (this.metadata?.language ?? this.pending?.language ?? this.file()?.language))
      return;
    this.changeMetadata({ language });
  }

  private changeMetadata(metadata: { fileName?: string; language?: ProgrammingLanguage }): void {
    if (!this.canEdit() || !this.file()?.userId) return;
    this.metadata = { ...this.metadata, ...metadata };
    this.dirty.set(true);
    void this.save();
  }

  async save(): Promise<void> {
    const file = this.file();
    if (
      !file ||
      this.saving() ||
      (!this.queue.length && !this.metadata) ||
      !this.canEdit() ||
      this.closed
    )
      return;
    if (this.hub.state() !== 'connected') return;

    const changes = this.takeChanges();
    if (!changes.length && this.queue.length) {
      this.conflict.set(true);
      this.toast.error('This edit exceeds the save limits. Download your draft before reloading.');
      return;
    }

    const batch: SaveChangesDTO = {
      batchId: crypto.randomUUID(),
      baseRevision: file.savedRevision,
      changeSets: changes,
      ...this.metadata,
    };
    this.metadata = undefined;
    this.pending = batch;
    this.saving.set(true);

    try {
      await this.files.save(file.id, batch);
      if (this.pending?.batchId === batch.batchId) await this.load();
    } catch {
      if (this.closed || this.fileId() !== file.id) return;
      this.conflict.set(true);
      this.toast.error(
        'Could not confirm your changes. Your draft is preserved; download it or reload the saved file.',
      );
    } finally {
      this.pending = undefined;
      this.saving.set(false);
      if (!this.closed && !this.conflict() && (this.queue.length || this.metadata))
        void this.save();
    }
  }

  private takeChanges(): CodeChangeSet[] {
    let count = 0;
    let edits = 0;
    let characters = 0;

    // Match the server limits while preserving Monaco event order across batches.
    for (const set of this.queue) {
      const inserted = set.changes.reduce((total, change) => total + change.text.length, 0);
      if (
        count === 64 ||
        set.changes.length > 256 ||
        edits + set.changes.length > 1000 ||
        characters + inserted > 100000
      )
        break;

      count++;
      edits += set.changes.length;
      characters += inserted;
    }

    return this.queue.splice(0, count);
  }

  async reload(): Promise<void> {
    if (
      this.dirty() &&
      !(await this.dialogs.confirm({
        heading: 'Reload saved file?',
        message: 'Discard your unsaved changes and reload the saved file?',
        label: 'Reload file',
      }))
    )
      return;
    if (this.closed || this.loading()) return;
    this.queue = [];
    this.metadata = undefined;
    this.dirty.set(false);
    this.conflict.set(false);
    await this.load();
  }

  async copyToWorkspace(): Promise<void> {
    const source = this.file();
    const targetId = this.auth.user()?.fileId;
    if (!source || !targetId || this.copying()) return;
    if (
      !(await this.dialogs.confirm({
        heading: 'Replace workspace code?',
        message: 'Replace the code in your personal workspace with a copy of this file?',
        label: 'Copy to workspace',
      }))
    )
      return;
    if (this.closed || this.copying()) return;
    this.copying.set(true);

    try {
      await this.hub.join(targetId);
      const target = await firstValueFrom(this.files.get(targetId));
      await this.files.save(targetId, {
        fileName: source.fileName,
        language: source.language,
        batchId: crypto.randomUUID(),
        baseRevision: target.savedRevision,
        changeSets: [
          {
            changes: [
              { rangeOffset: 0, rangeLength: target.savedContent.length, text: this.code() },
            ],
          },
        ],
      });
      this.toast.success('Copied to your workspace.');
      await this.router.navigateByUrl('/workspace');
    } catch {
      this.toast.error('Could not copy this file to your workspace.');
    } finally {
      this.copying.set(false);
    }
  }

  updateStdin(event: Event): void {
    this.stdin.set((event.target as HTMLTextAreaElement).value);
  }

  run(): void {
    const file = this.file();
    if (!file || this.running() || this.saving() || this.loading()) return;
    this.running.set(true);
    this.output.set(null);
    const id = file.id;

    this.runRequest = this.files
      .run(id, this.code(), this.stdin())
      .pipe(
        finalize(() => {
          if (this.fileId() === id) this.running.set(false);
        }),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe((result) => {
        if (this.fileId() === id) this.output.set(result);
      });
  }

  download(): void {
    const file = this.file();
    if (!file) return;
    const url = URL.createObjectURL(new Blob([this.code()], { type: 'text/plain;charset=utf-8' }));
    const link = document.createElement('a');
    link.href = url;
    link.download = file.fileName;
    link.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }

  private accept(change: CodeChangesDTO): void {
    const file = this.file();
    if (!file || change.revision <= file.savedRevision) return;
    if (change.baseRevision !== file.savedRevision) {
      void this.load();
      return;
    }

    try {
      const content = applyCodeChanges(file.savedContent, change.changeSets);
      const updated = {
        ...file,
        fileName: change.fileName ?? file.fileName,
        language: change.language ?? file.language,
        savedContent: content,
        savedRevision: change.revision,
      };
      this.file.set(updated);
      this.files.set(updated);
      const own = this.pending?.batchId === change.batchId;
      if (own) this.pending = undefined;

      if (!this.dirty() || (own && !this.queue.length && !this.metadata)) {
        this.code.set(content);
        this.dirty.set(false);
      } else if (!own) {
        this.conflict.set(true);
      }
    } catch {
      void this.load();
    }
  }
}
