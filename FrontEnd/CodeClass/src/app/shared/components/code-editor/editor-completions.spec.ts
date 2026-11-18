import type * as Monaco from 'monaco-editor/editor/editor.api';
import { describe, expect, it, vi } from 'vitest';
import { registerEditorCompletions } from './editor-completions';

describe('shared editor completions', () => {
  it('registers once and offers language keywords and insertable snippets at the current word range', async () => {
    const providers = new Map<string, Monaco.languages.CompletionItemProvider>();
    const register = vi.fn(
      (language: string, provider: Monaco.languages.CompletionItemProvider) => {
        providers.set(language, provider);
      },
    );
    class Range {
      constructor(
        readonly startLineNumber: number,
        readonly startColumn: number,
        readonly endLineNumber: number,
        readonly endColumn: number,
      ) {}
    }
    const monaco = {
      Range,
      languages: {
        registerCompletionItemProvider: register,
        CompletionItemKind: { Keyword: 17, Snippet: 27 },
        CompletionItemInsertTextRule: { InsertAsSnippet: 4 },
      },
    } as unknown as typeof Monaco;
    registerEditorCompletions(monaco);
    registerEditorCompletions(monaco);
    expect(register).toHaveBeenCalledTimes(3);

    for (const [language, keyword, snippet] of [
      ['cpp', 'int', 'main'],
      ['java', 'public', 'sout'],
      ['python', 'print', 'def'],
    ]) {
      const result = await providers.get(language)!.provideCompletionItems(
        {
          getWordUntilPosition: () => ({ word: 'pr', startColumn: 3, endColumn: 5 }),
        } as unknown as Monaco.editor.ITextModel,
        { lineNumber: 2, column: 5 } as Monaco.Position,
        {} as Monaco.languages.CompletionContext,
        {} as Monaco.CancellationToken,
      );
      expect(result?.suggestions.some((item) => item.label === keyword)).toBe(true);
      const completion = result?.suggestions.find(
        (item) => item.label === snippet && item.kind === 27,
      );
      expect(completion?.insertTextRules).toBe(4);
      expect(completion?.range).toEqual(new Range(2, 3, 2, 5));
    }
  });
});
