import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LectureListDTO } from '../../../core/DTO/lectures/lecture-list.dto';
import { SectionsService } from '../../../core/services/data/sections-service';
import { AuthService } from '../../../core/services/data/auth-service';
import { LectureStatusComponent } from '../lecture-status/lecture-status.component';

@Component({
  selector: 'app-lecture-table',
  imports: [DatePipe, RouterLink, LectureStatusComponent],
  templateUrl: './lecture-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LectureTableComponent {
  readonly manage = input(false);
  readonly edit = output<LectureListDTO>();
  readonly changeStatus = output<LectureListDTO>();
  readonly remove = output<LectureListDTO>();
  readonly lectures = input.required<LectureListDTO[]>();
  readonly title = input('Lectures');
  protected readonly sectionsData = inject(SectionsService);
  protected readonly auth = inject(AuthService);
}
