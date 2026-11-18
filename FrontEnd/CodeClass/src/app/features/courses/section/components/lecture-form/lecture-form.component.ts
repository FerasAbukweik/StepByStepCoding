import { formatDate } from '@angular/common';
import { finalize, map, tap } from 'rxjs';
import { FileDetailDTO } from '../../../../../core/DTO/files/file-detail.dto';
import {
  computed,
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { LecturesService } from '../../../../../core/services/data/lectures-service';
import { FilesService } from '../../../../../core/services/data/files-service';
import { catchErrorToast } from '../../../../../core/Utilities/catch-error-toast';
import { apiError } from '../../../../../core/utils/api-error';
import { ToastService } from '../../../../../core/services/local/toast-service';
import { ProgrammingLanguage } from '../../../../../core/enums/programming-language';
import { LectureListDTO } from '../../../../../core/DTO/lectures/lecture-list.dto';
import { DialogComponent } from '../../../../../shared/components/dialog/dialog.component';
import { CodeEditorComponent } from '../../../../../shared/components/code-editor/code-editor.component';

@Component({
  selector: 'app-lecture-form',
  imports: [ReactiveFormsModule, DialogComponent, CodeEditorComponent],
  templateUrl: './lecture-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LectureFormComponent implements OnInit {
  readonly sectionId = input.required<string>();
  readonly lecture = input<LectureListDTO | null>(null);
  readonly closed = output<void>();
  readonly created = output<string>();
  private readonly lecturesData = inject(LecturesService);
  private readonly filesData = inject(FilesService);
  private readonly fileError = signal('');
  protected readonly saving = this.lecturesData.saving;
  protected readonly actionError = computed(
    () => this.fileError() || this.lecturesData.actionError(),
  );
  protected readonly loading = signal(false);
  protected readonly file = signal<FileDetailDTO | null>(null);
  protected readonly showCode = signal(false);
  protected readonly language = signal('cpp');
  private readonly toast = inject(ToastService);
  private readonly destroy = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder).nonNullable;

  protected readonly form = this.formBuilder.group({
    title: ['', [Validators.required, Validators.maxLength(250)]],
    scheduledStart: ['', [Validators.required]],
    fileName: ['main.cpp', [Validators.required, Validators.maxLength(250)]],
    language: this.formBuilder.control<ProgrammingLanguage>(ProgrammingLanguage.Cpp),
    initialCode: ['', [Validators.maxLength(100000)]],
  });

  ngOnInit(): void {
    const lecture = this.lecture();
    if (lecture)
      this.form.patchValue({
        title: lecture.title,
        scheduledStart: formatDate(lecture.scheduledStart, "yyyy-MM-dd'T'HH:mm", 'en-US'),
      });
    if (lecture?.fileId) {
      this.loading.set(true);
      this.filesData
        .get(lecture.fileId)
        .pipe(
          tap({ error: (error) => this.fileError.set(apiError(error)) }),
          catchErrorToast('Could not load the lecture file.', this.toast),
          finalize(() => this.loading.set(false)),
          takeUntilDestroyed(this.destroy),
        )
        .subscribe((file) => {
          this.file.set(file);
          this.form.patchValue({
            fileName: file.fileName,
            language: file.language,
            initialCode: file.savedContent,
          });
          this.language.set({ 1: 'cpp', 2: 'java', 3: 'python' }[file.language]);
        });
    }
    this.form.controls.language.valueChanges
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe((value) => {
        this.language.set({ 1: 'cpp', 2: 'java', 3: 'python' }[value]);
        if (['main.cpp', 'Main.java', 'main.py'].includes(this.form.controls.fileName.value))
          this.form.controls.fileName.setValue(
            { 1: 'main.cpp', 2: 'Main.java', 3: 'main.py' }[value],
          );
      });
  }

  save(): void {
    this.form.markAllAsTouched();
    if (
      this.form.invalid ||
      this.saving() ||
      this.loading() ||
      (this.lecture()?.fileId && !this.file())
    )
      return;
    const value = this.form.getRawValue();
    const lecture = this.lecture();
    if (!value.title.trim() || !value.fileName.trim()) return;
    let action: Observable<unknown>;
    const request = {
      title: value.title.trim(),
      scheduledStart: new Date(value.scheduledStart).toISOString(),
      fileName: value.fileName.trim(),
      language: value.language,
      initialCode: value.initialCode,
    };
    if (lecture)
      action = this.lecturesData.update(lecture.id, {
        ...request,
        version: lecture.version,
        fileVersion: this.file()?.version,
        fileId: lecture.fileId ?? undefined,
      });
    else
      action = this.lecturesData
        .create({ ...request, sectionId: this.sectionId() })
        .pipe(map((created) => created.id));

    action.pipe(takeUntilDestroyed(this.destroy)).subscribe((result) => {
      this.toast.success(lecture ? 'Lecture updated.' : 'Lecture created.');
      this.created.emit(lecture?.id ?? (result as string));
    });
  }
}
