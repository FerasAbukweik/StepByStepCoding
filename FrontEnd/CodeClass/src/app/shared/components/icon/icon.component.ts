import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

const paths: Record<string, string> = {
  menu: 'M4 6h16M4 12h16M4 18h16',
  code: 'm8 8-4 4 4 4m8-8 4 4-4 4m-2-12-4 24',
  terminal: 'm5 7 4 5-4 5m7 0h7M3 3h18v18H3z',
  light_mode:
    'M12 3v2m0 14v2M3 12h2m14 0h2M5.6 5.6 7 7m10 10 1.4 1.4M5.6 18.4 7 17M17 7l1.4-1.4M16 12a4 4 0 1 1-8 0 4 4 0 0 1 8 0',
  dark_mode: 'M20.5 14A9 9 0 0 1 10 3.5 9 9 0 1 0 20.5 14Z',
  notifications: 'M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9M10 21h4',
  content_copy: 'M9 3h12v14H9zM5 7H3v14h14v-2',
  description: 'M5 2h9l5 5v15H5zM14 2v6h5M8 12h8M8 16h8',
  lock: 'M6 10h12v11H6zM8 10V6a4 4 0 0 1 8 0v4',
  history_edu: 'M12 8v5l3 2M4 7V3m0 4h4M4 7a9 9 0 1 1-1 8',
  arrow_forward: 'M4 12h16m-6-6 6 6-6 6',
};

@Component({
  selector: 'app-icon',
  template: `<svg
    width="18"
    height="18"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    stroke-width="1.6"
    stroke-linecap="round"
    stroke-linejoin="round"
    aria-hidden="true"
  >
    <path [attr.d]="path()" />
  </svg>`,
  host: { class: 'inline-flex shrink-0 align-middle' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconComponent {
  readonly name = input('code');
  protected readonly path = computed(() => paths[this.name()] ?? paths['code']);
}
