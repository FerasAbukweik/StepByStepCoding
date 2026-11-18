import { DestroyRef, Directive, ElementRef, afterNextRender, inject, output } from '@angular/core';

@Directive({ selector: '[appVisible]' })
export class VisibleDirective {
  private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly destroy = inject(DestroyRef);

  readonly appVisible = output<void>();

  constructor() {
    afterNextRender(() => {
      const observer = new IntersectionObserver(
        (entries) => {
          if (entries.some((entry) => entry.isIntersecting)) this.appVisible.emit();
        },
        { rootMargin: '160px' },
      );

      observer.observe(this.element.nativeElement);
      this.destroy.onDestroy(() => observer.disconnect());
    });
  }
}
