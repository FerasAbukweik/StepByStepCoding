import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterOutlet } from '@angular/router';
import { filter, forkJoin, interval, switchMap } from 'rxjs';
import { SidebarComponent } from '../shared/components/sidebar/sidebar.component';
import { TopbarComponent } from '../shared/components/topbar/topbar.component';
import { CoursesService } from '../core/services/data/courses-service';
import { SectionsService } from '../core/services/data/sections-service';
import { LecturesService } from '../core/services/data/lectures-service';
import { ExamsService } from '../core/services/data/exams-service';
import { catchErrorToast } from '../core/Utilities/catch-error-toast';
import { ToastService } from '../core/services/local/toast-service';
import { SignalRService } from '../core/services/signalr/signalr-service';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, SidebarComponent, TopbarComponent],
  template: `<div class="h-dvh flex overflow-hidden bg-app text-textPrimary">
    @if (menuOpen()) {
      <button
        class="fixed inset-0 z-40 bg-black/50 md:hidden"
        aria-label="Close navigation"
        (click)="menuOpen.set(false)"
      ></button>
    }
    <div class="sidebar-container" [class.mobile-open]="menuOpen()">
      <app-sidebar (chosen)="menuOpen.set(false)" />
    </div>
    <div class="flex min-w-0 flex-1 flex-col">
      <app-topbar (menu)="menuOpen.update(toggle)" />
      <main id="main-content" class="flex-1 min-h-0 overflow-y-auto custom-scrollbar">
        <router-outlet />
      </main>
    </div>
  </div>`,
})
export class MainLayoutComponent implements OnInit {
  private readonly courses = inject(CoursesService);
  private readonly sections = inject(SectionsService);
  private readonly lectures = inject(LecturesService);
  private readonly exams = inject(ExamsService);
  private readonly toast = inject(ToastService);
  private readonly hub = inject(SignalRService);
  private readonly destroy = inject(DestroyRef);
  protected readonly menuOpen = signal(false);
  protected readonly toggle = (value: boolean) => !value;

  ngOnInit(): void {
    this.load().pipe(takeUntilDestroyed(this.destroy)).subscribe();
    void this.hub.start().catch(() => undefined);
    interval(30000)
      .pipe(
        filter(() => document.visibilityState === 'visible'),
        switchMap(() => this.load()),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe();
    this.destroy.onDestroy(() => void this.hub.stop());
  }
  private load() {
    return forkJoin([
      this.courses.load(),
      this.sections.load(),
      this.lectures.load(),
      this.exams.load(),
    ]).pipe(catchErrorToast('Could not load your academic data.', this.toast));
  }
}
