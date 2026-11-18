import { map, switchMap, tap } from 'rxjs';
import { FilesService } from './files-service';
import { computed } from '@angular/core';
import { Injectable, inject } from '@angular/core';
import { LecturesApiService } from '../api/lectures-api-service';
import { ResourceState } from '../../Utilities/resource-state';
import { LectureListDTO } from '../../DTO/lectures/lecture-list.dto';
import { LectureDetailDTO } from '../../DTO/lectures/lecture-detail.dto';
import { CreateLectureDTO } from '../../DTO/lectures/create-lecture.dto';
import { UpdateLectureDTO } from '../../DTO/lectures/update-lecture.dto';

@Injectable({ providedIn: 'root' })
export class LecturesService extends ResourceState<LectureListDTO, LectureDetailDTO> {
  private readonly files = inject(FilesService);
  private readonly api = inject(LecturesApiService);

  list(page = 1, filters: Record<string, string | undefined> = {}) {
    return this.listResult((filters) => this.api.list(page, filters), page, filters);
  }

  load(page = 1, filters: Record<string, string | undefined> = {}) {
    return this.loadPage(this.list(page, filters), page);
  }

  more(filters: Record<string, string | undefined> = {}) {
    return this.nextPage((page) => this.load(page, filters));
  }

  get(id: string) {
    return this.detail(this.api.get(id));
  }

  create(request: CreateLectureDTO) {
    return this.write(
      this.api
        .create(request)
        .pipe(
          switchMap((created) =>
            this.load(1, { sectionId: request.sectionId }).pipe(map(() => created)),
          ),
        ),
      'Could not create this lecture.',
    );
  }

  update(id: string, request: UpdateLectureDTO) {
    return this.write(
      this.api.update(id, request).pipe(
        tap(() => {
          if (request.fileId) this.files.invalidate(request.fileId);
        }),
        switchMap(() => this.refresh(id)),
        map(() => undefined),
      ),
      'Could not update this lecture.',
    );
  }

  remove(id: string, version: string) {
    return this.write(
      this.api.remove(id, version).pipe(
        tap(() => {
          const fileId = this.find(id)?.fileId;
          if (fileId) this.files.invalidate(fileId);
          this.updateItems((items) => items.filter((item) => item.id !== id));
          this.total.update((total) => Math.max(0, total - 1));
          if (this.current()?.id === id) this.current.set(null);
        }),
      ),
      'Could not delete this lecture.',
    );
  }

  readonly live = computed(() => this.loadedItems().filter((item) => item.status === 2));
  readonly ended = computed(() =>
    this.loadedItems()
      .filter((item) => item.status === 3)
      .reverse(),
  );

  start(id: string, version: string) {
    return this.write(
      this.api.start(id, version).pipe(
        switchMap(() => this.refresh(id)),
        map(() => undefined),
      ),
      'Could not start this lecture.',
    );
  }

  end(id: string, version: string) {
    return this.write(
      this.api.end(id, version).pipe(
        switchMap(() => this.refresh(id)),
        map(() => undefined),
      ),
      'Could not end this lecture.',
    );
  }
  private refresh(id: string) {
    return this.get(id).pipe(
      tap((lecture) => {
        if (this.current()?.id === id) this.current.set(lecture);
        this.updateItems((items) => items.map((item) => (item.id === id ? lecture : item)));
      }),
    );
  }
}
