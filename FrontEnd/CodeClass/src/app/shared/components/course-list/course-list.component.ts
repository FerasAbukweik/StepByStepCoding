import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SectionListDTO } from '../../../core/DTO/sections/section-list.dto';
import { SectionsService } from '../../../core/services/data/sections-service';
import { LecturesService } from '../../../core/services/data/lectures-service';

@Component({
  selector: 'app-course-list',
  imports: [RouterLink],
  templateUrl: './course-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CourseListComponent {
  readonly manage = input(false);
  readonly edit = output<SectionListDTO>();
  readonly remove = output<SectionListDTO>();
  readonly sections = input.required<SectionListDTO[]>();
  protected readonly sectionsData = inject(SectionsService);
  protected readonly lecturesData = inject(LecturesService);

  hasLive(id: string): boolean {
    return this.lecturesData.live().some((lecture) => lecture.sectionId === id);
  }
}
