import { AuthService } from '../../../core/services/data/auth-service';
import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, output } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { SectionsService } from '../../../core/services/data/sections-service';
import { LecturesService } from '../../../core/services/data/lectures-service';
import { ExamsService } from '../../../core/services/data/exams-service';
import { SignalRService } from '../../../core/services/signalr/signalr-service';
import { StatePanelComponent } from '../state-panel/state-panel.component';
import { LectureStatusComponent } from '../lecture-status/lecture-status.component';

@Component({
  selector: 'app-sidebar',
  imports: [DatePipe, RouterLink, RouterLinkActive, StatePanelComponent, LectureStatusComponent],
  templateUrl: './sidebar.component.html',
  host: { class: 'block h-full' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent {
  readonly chosen = output<void>();
  protected readonly auth = inject(AuthService);
  protected readonly sectionsData = inject(SectionsService);
  protected readonly lecturesData = inject(LecturesService);
  protected readonly examsData = inject(ExamsService);
  protected readonly hub = inject(SignalRService);
  private readonly destroy = inject(DestroyRef);

  loadMore(): void {
    this.lecturesData.more().pipe(takeUntilDestroyed(this.destroy)).subscribe();
  }
}
