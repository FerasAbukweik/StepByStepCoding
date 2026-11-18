import {
  ViewEncapsulation,
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  effect,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import type * as Monaco from 'monaco-editor/editor/editor.api';
import { ThemeService } from '../../../core/services/local/theme-service';
import { EditorChange } from './editor-change.model';
import { registerEditorCompletions } from './editor-completions';

@Component({
  selector: 'app-code-editor',
  styleUrl: '../../../../../node_modules/monaco-editor/min/vs/editor/editor.main.css',
  encapsulation: ViewEncapsulation.None,
  template: `
    <div
      #container
      class="absolute inset-0 overflow-hidden"
      role="region"
      [attr.aria-label]="label()"
    ></div>
    @if (!ready() && !failed()) {
      <div
        class="pointer-events-none absolute inset-0 flex items-center justify-center bg-editor text-sm text-textSecondary"
        role="status"
      >
        Loading editor…
      </div>
    }
    @if (failed()) {
      <div
        class="absolute inset-0 flex items-center justify-center bg-editor p-6 text-sm text-statusError"
        role="alert"
      >
        Could not load the code editor. Reload the page to try again.
      </div>
    }
  `,
  host: { class: 'relative block min-h-[28rem] min-w-0 flex-1 overflow-hidden bg-editor' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CodeEditorComponent {
  readonly code = input('');
  readonly language = input('cpp');
  readonly readOnly = input(false);
  readonly label = input('Code editor');
  readonly changed = output<EditorChange>();

  protected readonly ready = signal(false);
  protected readonly failed = signal(false);

  private readonly container = viewChild.required<ElementRef<HTMLElement>>('container');
  private readonly theme = inject(ThemeService);
  private readonly destroy = inject(DestroyRef);
  private editor?: Monaco.editor.IStandaloneCodeEditor;
  private monaco?: typeof Monaco;
  private applying = false;
  private disposed = false;

  constructor() {
    afterNextRender(() => {
      void this.initialize().catch(() => {
        if (!this.disposed) this.failed.set(true);
      });
    });

    effect(() => {
      const code = this.code();
      if (!this.editor || this.editor.getValue() === code) return;
      const view = this.editor.saveViewState();
      this.applying = true;
      this.editor.setValue(code);
      this.applying = false;
      if (view) this.editor.restoreViewState(view);
    });
    effect(() => {
      const readOnly = this.readOnly();
      this.editor?.updateOptions({ readOnly });
    });
    effect(() => {
      const language = this.language();
      const model = this.editor?.getModel();
      if (model) this.monaco?.editor.setModelLanguage(model, language);
    });
    effect(() => {
      const theme = this.theme.theme();
      this.monaco?.editor.setTheme(theme === 'dark' ? 'codeclass-dark' : 'vs');
    });

    this.destroy.onDestroy(() => {
      this.disposed = true;
      const model = this.editor?.getModel();
      this.editor?.dispose();
      model?.dispose();
    });
  }

  private async initialize(): Promise<void> {
    const environment = globalThis as typeof globalThis & {
      MonacoEnvironment?: Monaco.Environment;
    };
    environment.MonacoEnvironment = {
      getWorker: () =>
        new Worker(new URL('../../../../editor.worker', import.meta.url), {
          type: 'module',
        }),
    };
    const monaco = await import('monaco-editor/editor/editor.api');
    await Promise.all([
      import('monaco-editor/basic-languages/monaco.contribution'),
      import('monaco-editor/editor/contrib/suggest/browser/suggestController'),
      import('monaco-editor/features/snippet/register'),
    ]);
    if (this.disposed) return;
    this.monaco = monaco;
    registerEditorCompletions(monaco);
    monaco.editor.defineTheme('codeclass-dark', {
      base: 'vs-dark',
      inherit: true,
      rules: [],
      colors: {
        'editor.background': '#0B0F14',
        'editor.foreground': '#E6EDF3',
        'editorLineNumber.foreground': '#6E7681',
        'editor.selectionBackground': '#1B2740',
        'editorCursor.foreground': '#58A6FF',
      },
    });
    this.editor = monaco.editor.create(this.container().nativeElement, {
      value: this.code(),
      language: this.language(),
      readOnly: this.readOnly(),
      theme: this.theme.theme() === 'dark' ? 'codeclass-dark' : 'vs',
      automaticLayout: true,
      quickSuggestions: { other: 'on', comments: 'off', strings: 'off' },
      quickSuggestionsDelay: 150,
      suggestOnTriggerCharacters: true,
      wordBasedSuggestions: 'currentDocument',
      snippetSuggestions: 'top',
      tabCompletion: 'on',
      autoClosingBrackets: 'languageDefined',
      autoClosingQuotes: 'languageDefined',
      lineNumbers: 'on',
      scrollBeyondLastColumn: 2,
      minimap: { enabled: false },
      fontSize: 14,
      lineHeight: 22,
      fontFamily: '"JetBrains Mono", monospace',
      scrollBeyondLastLine: false,
      tabSize: 2,
      padding: { top: 18, bottom: 18 },
      fixedOverflowWidgets: true,
    });
    this.ready.set(true);

    this.editor.onDidChangeModelContent((event) => {
      if (this.applying || !this.editor) return;
      this.changed.emit({
        code: this.editor.getValue(),
        changeSet: {
          changes: event.changes.map((change) => ({
            rangeOffset: change.rangeOffset,
            rangeLength: change.rangeLength,
            text: change.text,
          })),
        },
      });
    });
  }
}
