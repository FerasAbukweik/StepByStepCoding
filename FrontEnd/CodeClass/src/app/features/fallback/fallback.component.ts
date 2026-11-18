import { Location } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-fallback',
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="page-container flex min-h-[65vh] items-center justify-center">
    <section class="panel w-full max-w-xl text-center">
      <p class="font-mono text-sm text-accentBlue">{{ code() }}</p>
      <h1 class="mt-3 text-headline-lg font-semibold">{{ content().title }}</h1>
      <p class="mt-4 leading-relaxed text-textSecondary">{{ content().message }}</p>
      <div class="mt-7 flex flex-wrap justify-center gap-3">
        <a routerLink="/" class="button-primary">Go home</a
        ><button class="button-secondary" (click)="back()">Go back</button>
      </div>
    </section>
  </div>`,
})
export class FallbackComponent {
  readonly code = input('404');
  private readonly location = inject(Location);
  protected readonly content = computed(() => {
    switch (this.code()) {
      case '403':
        return {
          title: 'Access denied',
          message: 'Your account does not have permission to open this page.',
        };
      case '503':
        return {
          title: 'Service temporarily unavailable',
          message:
            'Please try again shortly. You can return to the previous page and retry your request.',
        };
      default:
        return {
          title: 'Page not found',
          message: 'This page may have moved or the link may be incorrect.',
        };
    }
  });

  back(): void {
    this.location.back();
  }
}
