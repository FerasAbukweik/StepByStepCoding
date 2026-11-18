import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-problem-statement',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="space-y-4">
    @for (block of blocks(); track $index) {
      @if (block.code) {
        <pre
          class="overflow-auto rounded-lg border border-borderPrimary bg-editor p-4 font-mono text-sm whitespace-pre-wrap break-words"
          >{{ block.text }}</pre>
      } @else {
        <p class="whitespace-pre-wrap break-words leading-relaxed">{{ block.text }}</p>
      }
    }
    @if (image()) {
      <img
        [src]="image()"
        alt="Question illustration"
        class="max-h-80 max-w-full rounded-lg object-contain"
      />
    }
  </div>`,
})
export class ProblemStatementComponent {
  readonly description = input.required<string>();
  readonly image = input<string | null>(null);
  protected readonly blocks = computed(() => {
    const text = this.description();
    const blocks: { text: string; code: boolean }[] = [];
    let offset = 0;

    // Render fenced code as text, never as executable HTML.
    for (const match of text.matchAll(/```[^\n`]*\n([\s\S]*?)```/g)) {
      if (match.index > offset) blocks.push({ text: text.slice(offset, match.index), code: false });
      blocks.push({ text: match[1], code: true });
      offset = match.index + match[0].length;
    }

    if (offset < text.length) blocks.push({ text: text.slice(offset), code: false });
    return blocks;
  });
}
