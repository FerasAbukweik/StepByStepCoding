import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnChanges,
  OnInit,
  SimpleChanges,
  inject,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { finalize, tap } from 'rxjs';
import { SectionsService } from '../../core/services/data/sections-service';
import { LecturesService } from '../../core/services/data/lectures-service';
import { AuthService } from '../../core/services/data/auth-service';
import { ToastService } from '../../core/services/local/toast-service';
import { SignalRService } from '../../core/services/signalr/signalr-service';
import { LectureDetailDTO } from '../../core/DTO/lectures/lecture-detail.dto';
import { catchErrorToast } from '../../core/Utilities/catch-error-toast';
import { CodePageComponent } from '../../shared/components/code-page/code-page.component';
import { StatePanelComponent } from '../../shared/components/state-panel/state-panel.component';
import { LectureStatusComponent } from '../../shared/components/lecture-status/lecture-status.component';

@Component({
  selector: 'app-classroom',
  imports: [RouterLink, CodePageComponent, StatePanelComponent, LectureStatusComponent],
  templateUrl: './classroom.component.html',
  host: { class: 'block min-h-full' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClassroomComponent implements OnInit, OnChanges {
  private readonly editor = viewChild(CodePageComponent);

  readonly lectureId = input.required<string>();
  protected readonly lecture = signal<LectureDetailDTO | null>(null);
  protected readonly loading = signal(true);
  protected readonly failed = signal(false);
  protected readonly sectionsData = inject(SectionsService);
  protected readonly lecturesData = inject(LecturesService);
  protected readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly hub = inject(SignalRService);
  private readonly destroy = inject(DestroyRef);

  hasUnsavedChanges(): boolean {
    return this.editor()?.hasUnsavedChanges() ?? false;
  }

  ngOnInit(): void {
    this.load();
    this.hub.lectureEnded$.pipe(takeUntilDestroyed(this.destroy)).subscribe((id) => {
      if (id !== this.lectureId()) return;
      this.load();
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['lectureId'] && !changes['lectureId'].firstChange) this.load();
  }

  load(): void {
    this.loading.set(true);
    this.failed.set(false);
    const id = this.lectureId();
    this.lecturesData
      .get(id)
      .pipe(
        tap({
          next: (lecture) => {
            if (this.lectureId() === id) this.lecture.set(lecture);
          },
          error: () => this.failed.set(true),
        }),
        catchErrorToast('Could not load this lecture.', this.toast),
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe();
  }
}
