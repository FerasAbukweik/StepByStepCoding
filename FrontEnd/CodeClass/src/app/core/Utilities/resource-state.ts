import { DestroyRef, Injectable, computed, effect, inject, signal } from '@angular/core';
import { Observable, Subject, defer, finalize, of, takeUntil, tap } from 'rxjs';
import { AuthService } from '../services/data/auth-service';
import { MemoryCacheService } from '../services/local/memory-cache-service';
import { ToastService } from '../services/local/toast-service';
import { PageDTO } from '../DTO/common/page.dto';
import { apiError } from '../utils/api-error';
import { catchErrorToast } from './catch-error-toast';

@Injectable()
export abstract class ResourceState<TList extends { id: string }, TDetail extends { id: string }> {
  readonly items = signal<ReadonlyMap<number, TList[]>>(new Map());
  readonly pageItems = computed(() => this.items().get(this.page()) ?? []);
  readonly loadedItems = computed(() =>
    [...this.items().entries()]
      .filter(([page]) => page <= this.page())
      .sort(([left], [right]) => left - right)
      .flatMap(([, items]) => items),
  );
  readonly current = signal<TDetail | null>(null);
  readonly total = signal(0);
  readonly page = signal(1);
  readonly error = signal('');
  readonly actionError = signal('');
  readonly hasMore = computed(() => this.page() * 20 < this.total());
  private readonly reads = signal(0);
  private readonly writes = signal(0);
  readonly loading = computed(() => this.reads() > 0);
  readonly saving = computed(() => this.writes() > 0);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly cancelled = new Subject<void>();
  private owner = this.auth.user()?.id;
  private readonly cache = inject(MemoryCacheService);
  private cacheVersion = this.cache.version;
  private filters = '';

  constructor() {
    effect(() => this.syncOwner());
    inject(DestroyRef).onDestroy(() => {
      this.cancelled.next();
      this.cancelled.complete();
    });
  }

  reset(): void {
    this.cancelled.next();
    this.resetState();
  }

  private resetState(): void {
    this.items.set(new Map());
    this.current.set(null);
    this.filters = '';
    this.total.set(0);
    this.page.set(1);
    this.error.set('');
    this.actionError.set('');
  }

  protected read<T>(source: Observable<T>): Observable<T> {
    return defer(() => {
      this.syncOwner();
      this.reads.update((count) => count + 1);
      this.error.set('');
      return source.pipe(
        takeUntil(this.cancelled),
        tap({ error: (error) => this.error.set(apiError(error)) }),
        finalize(() => this.reads.update((count) => Math.max(0, count - 1))),
      );
    });
  }

  protected write<T>(source: Observable<T>, message: string): Observable<T> {
    return defer(() => {
      this.syncOwner();
      this.writes.update((count) => count + 1);
      this.actionError.set('');
      return source.pipe(
        takeUntil(this.cancelled),
        tap({ error: (error) => this.actionError.set(apiError(error)) }),
        catchErrorToast(message, this.toast),
        finalize(() => this.writes.update((count) => Math.max(0, count - 1))),
      );
    });
  }

  private syncOwner(): void {
    const owner = this.auth.user()?.id;
    if (owner !== this.owner) {
      this.owner = owner;
      this.reset();
    }
  }

  private checkCacheVersion(): void {
    if (this.cacheVersion === this.cache.version) return;

    // Clear state without cancelling a mutation that is refreshing its data.
    this.resetState();
    this.cacheVersion = this.cache.version;
  }

  protected find(id: string): TList | undefined {
    return [...this.items().values()].flat().find((item) => item.id === id);
  }

  protected updateItems(update: (items: TList[]) => TList[]): void {
    this.items.update((pages) => new Map([...pages].map(([page, items]) => [page, update(items)])));
  }

  protected detail(source: Observable<TDetail>) {
    return this.read(source);
  }

  protected listResult<TFilters extends Record<string, string | undefined>>(
    source: (filters: TFilters) => Observable<PageDTO<TList>>,
    page: number,
    filters: TFilters,
  ) {
    return defer(() => {
      this.syncOwner();
      this.checkCacheVersion();
      const query = JSON.stringify(
        Object.entries(filters)
          .filter(([, value]) => value)
          .sort(),
      );
      if (this.filters !== query) {
        this.items.set(new Map());
        this.filters = query;
      }
      const cached = this.items().get(page);
      if (cached) {
        this.error.set('');
        return of({ items: cached, totalCount: this.total() });
      }

      const version = this.cache.version;
      return this.read(source(filters)).pipe(
        tap((result) => {
          if (this.filters === query && this.cache.version === version) {
            this.items.update((pages) => new Map(pages).set(page, result.items));
            this.total.set(result.totalCount);
          }
        }),
      );
    });
  }

  protected loadPage(source: Observable<PageDTO<TList>>, page: number) {
    return source.pipe(
      tap((result) => {
        this.total.set(result.totalCount);
        this.page.set(page);
      }),
    );
  }

  protected nextPage(
    load: (page: number) => Observable<PageDTO<TList>>,
  ): Observable<PageDTO<TList> | undefined> {
    this.syncOwner();
    this.checkCacheVersion();
    if (this.loading() || (this.items().size > 0 && !this.hasMore())) return of(undefined);

    const page = this.items().size === 0 ? this.page() : this.page() + 1;
    return load(page).pipe(catchErrorToast('Could not load more records.', this.toast));
  }
}
