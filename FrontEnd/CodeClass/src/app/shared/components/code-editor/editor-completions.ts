import type * as Monaco from 'monaco-editor/editor/editor.api';

type Language = 'cpp' | 'java' | 'python';

interface LanguageSuggestions {
  keywords: string[];
  snippets: { label: string; code: string; description: string }[];
}

const suggestions: Record<Language, LanguageSuggestions> = {
  cpp: {
    keywords: [
      'auto',
      'bool',
      'break',
      'case',
      'char',
      'class',
      'const',
      'continue',
      'double',
      'else',
      'false',
      'float',
      'for',
      'if',
      'int',
      'namespace',
      'nullptr',
      'private',
      'public',
      'return',
      'std',
      'string',
      'struct',
      'switch',
      'true',
      'using',
      'vector',
      'void',
      'while',
    ],
    snippets: [
      { label: 'main', description: 'Main function', code: 'int main() {\n\t${0}\n\treturn 0;\n}' },
      {
        label: 'for',
        description: 'Indexed for loop',
        code: 'for (int ${1:i} = 0; ${1:i} < ${2:count}; ++${1:i}) {\n\t${0}\n}',
      },
      {
        label: 'cout',
        description: 'Write to standard output',
        code: 'std::cout << ${1:value} << std::endl;',
      },
    ],
  },
  java: {
    keywords: [
      'boolean',
      'break',
      'case',
      'catch',
      'char',
      'class',
      'continue',
      'double',
      'else',
      'extends',
      'false',
      'final',
      'float',
      'for',
      'if',
      'implements',
      'import',
      'int',
      'interface',
      'long',
      'new',
      'null',
      'package',
      'private',
      'protected',
      'public',
      'return',
      'static',
      'String',
      'super',
      'switch',
      'this',
      'throw',
      'true',
      'try',
      'void',
      'while',
    ],
    snippets: [
      {
        label: 'main',
        description: 'Main method',
        code: 'public static void main(String[] args) {\n\t${0}\n}',
      },
      { label: 'sout', description: 'Print a line', code: 'System.out.println(${1:value});' },
      {
        label: 'for',
        description: 'Indexed for loop',
        code: 'for (int ${1:i} = 0; ${1:i} < ${2:count}; ${1:i}++) {\n\t${0}\n}',
      },
    ],
  },
  python: {
    keywords: [
      'and',
      'as',
      'assert',
      'async',
      'await',
      'break',
      'class',
      'continue',
      'def',
      'del',
      'elif',
      'else',
      'except',
      'False',
      'finally',
      'for',
      'from',
      'if',
      'import',
      'in',
      'is',
      'lambda',
      'None',
      'not',
      'or',
      'pass',
      'raise',
      'return',
      'True',
      'try',
      'while',
      'with',
      'yield',
      'print',
      'input',
      'len',
      'range',
      'str',
      'int',
      'list',
      'dict',
    ],
    snippets: [
      {
        label: 'def',
        description: 'Function definition',
        code: 'def ${1:function_name}(${2:args}):\n\t${0:pass}',
      },
      { label: 'for', description: 'For loop', code: 'for ${1:item} in ${2:items}:\n\t${0:pass}' },
      {
        label: 'main',
        description: 'Main entry point',
        code: 'if __name__ == "__main__":\n\t${0:pass}',
      },
    ],
  },
};

let registered = false;

export function registerEditorCompletions(monaco: typeof Monaco): void {
  if (registered) return;
  registered = true;

  // Providers belong to the Monaco runtime, not to individual editor instances.
  for (const language of Object.keys(suggestions) as Language[]) {
    monaco.languages.registerCompletionItemProvider(language, {
      provideCompletionItems(model, position) {
        const word = model.getWordUntilPosition(position);
        const range = new monaco.Range(
          position.lineNumber,
          word.startColumn,
          position.lineNumber,
          word.endColumn,
        );
        const definition = suggestions[language];

        return {
          suggestions: [
            ...definition.keywords.map((label) => ({
              label,
              insertText: label,
              kind: monaco.languages.CompletionItemKind.Keyword,
              range,
            })),
            ...definition.snippets.map((snippet) => ({
              label: snippet.label,
              detail: snippet.description,
              insertText: snippet.code,
              insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
              kind: monaco.languages.CompletionItemKind.Snippet,
              range,
            })),
          ],
        };
      },
    });
  }
}
