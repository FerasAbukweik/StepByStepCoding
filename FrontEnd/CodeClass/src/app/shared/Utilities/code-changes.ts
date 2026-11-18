import { CodeChangeSet } from '../../core/DTO/realtime/code-change-set';

export function applyCodeChanges(code: string, sets: readonly CodeChangeSet[]): string {
  for (const set of sets) {
    const edits = [...set.changes].sort((a, b) => b.rangeOffset - a.rangeOffset);
    let boundary = code.length;

    for (const edit of edits) {
      if (
        !Number.isInteger(edit.rangeOffset) ||
        !Number.isInteger(edit.rangeLength) ||
        edit.rangeOffset < 0 ||
        edit.rangeLength < 0 ||
        edit.rangeOffset + edit.rangeLength > boundary
      )
        throw new Error('Invalid code change range.');
      boundary = edit.rangeOffset;
      code =
        code.slice(0, edit.rangeOffset) +
        edit.text +
        code.slice(edit.rangeOffset + edit.rangeLength);
    }
  }
  return code;
}
