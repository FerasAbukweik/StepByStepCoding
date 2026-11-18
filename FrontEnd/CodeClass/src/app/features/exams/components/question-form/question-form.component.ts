import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  input,
  output,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { DialogComponent } from '../../../../shared/components/dialog/dialog.component';
import { ExamQuestionsService } from '../../../../core/services/data/exam-questions-service';
import { ExamsService } from '../../../../core/services/data/exams-service';
import { ToastService } from '../../../../core/services/local/toast-service';
import { ExamQuestionDetailDTO } from '../../../../core/DTO/examquestions/exam-question-detail.dto';

@Component({
  selector: 'app-question-form',
  imports: [ReactiveFormsModule, DialogComponent],
  templateUrl: './question-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuestionFormComponent implements OnInit {
  readonly question = input<ExamQuestionDetailDTO | null>(null);
  readonly closed = output<void>();
  readonly saved = output<void>();
  protected readonly questionsData = inject(ExamQuestionsService);
  private readonly examsData = inject(ExamsService);
  private readonly toast = inject(ToastService);
  private readonly destroy = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder).nonNullable;

  protected readonly form = this.formBuilder.group({
    title: ['', [Validators.required, Validators.maxLength(250)]],
    description: ['', [Validators.required, Validators.maxLength(20000)]],
    imagePath: ['', [Validators.maxLength(1024)]],
    maxScore: [10, [Validators.required, Validators.min(0.01), Validators.max(10000)]],
    displayOrder: [0, [Validators.required, Validators.min(0), Validators.max(1000)]],
  });

  ngOnInit(): void {
    const question = this.question();
    if (question) this.form.patchValue({ ...question, imagePath: question.imagePath ?? '' });
    else
      this.form.controls.displayOrder.setValue(
        Math.max(-1, ...this.questionsData.loadedItems().map((item) => item.displayOrder)) + 1,
      );
  }

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.questionsData.saving()) return;
    const value = this.form.getRawValue();
    if (!value.title.trim() || !value.description.trim()) return;
    const request = {
      ...value,
      title: value.title.trim(),
      description: value.description.trim(),
      imagePath: value.imagePath.trim() || null,
    };
    const question = this.question();
    const action: Observable<unknown> = question
      ? this.questionsData.update(question.id, { ...request, version: question.version })
      : this.questionsData.create({ ...request, examId: this.examsData.current()!.id });
    action.pipe(takeUntilDestroyed(this.destroy)).subscribe(() => {
      this.toast.success('Question saved.');
      this.saved.emit();
    });
  }
}
