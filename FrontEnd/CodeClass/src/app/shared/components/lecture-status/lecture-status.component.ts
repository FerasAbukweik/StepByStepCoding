import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { LectureStatus } from '../../../core/enums/lecture-status';

@Component({
  selector: 'app-lecture-status',
  template: `<span
    class="inline-flex items-center gap-1.5 whitespace-nowrap text-xs font-mono"
    [class.text-statusLive]="status() === 2"
    [class.text-textMuted]="status() !== 2"
    ><span class="w-1.5 h-1.5 rounded-full bg-current"></span>{{ label() }}</span
  >`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LectureStatusComponent {
  readonly status = input<LectureStatus>(LectureStatus.Scheduled);
  protected readonly label = computed(() =>
    this.status() === LectureStatus.Live
      ? 'Live now'
      : this.status() === LectureStatus.Ended
        ? 'Ended'
        : 'Scheduled',
  );
}
