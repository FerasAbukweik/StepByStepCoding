using System.Buffers;
using System.Text;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Realtime;

namespace StepByStepCoding.Application.Services
{
    public static class CodeEditApplicator
    {
        public static string Apply(string content, IReadOnlyList<CodeChangeSet> sets)
        {
            foreach (var set in sets)
            {
                var previousStart = content.Length + 1;

                // Monaco offsets refer to the document before this event. Apply from right to left.
                foreach (var edit in set.Changes.OrderByDescending(change => change.RangeOffset))
                {
                    if (edit.RangeOffset < 0 || edit.RangeLength < 0
                        || edit.RangeOffset > content.Length || edit.RangeLength > content.Length - edit.RangeOffset
                        || (long)edit.RangeOffset + edit.RangeLength > previousStart || edit.RangeOffset == previousStart
                        || !Boundary(content, edit.RangeOffset) || !Boundary(content, edit.RangeOffset + edit.RangeLength)
                        || !ValidText(edit.Text))
                        throw new CustomValidationException("Code edits must have valid, nonoverlapping UTF-16 ranges.");

                    if ((long)content.Length - edit.RangeLength + edit.Text.Length > 100000)
                        throw new CustomValidationException("File content must not exceed 100000 characters.");

                    content = content.Remove(edit.RangeOffset, edit.RangeLength).Insert(edit.RangeOffset, edit.Text);
                    previousStart = edit.RangeOffset;
                }
            }

            return content;
        }

        private static bool Boundary(string content, int offset)
        {
            return offset == 0 || offset == content.Length
                || !(char.IsHighSurrogate(content[offset - 1]) && char.IsLowSurrogate(content[offset]));
        }

        private static bool ValidText(string text)
        {
            var remaining = text.AsSpan();

            while (!remaining.IsEmpty)
            {
                if (Rune.DecodeFromUtf16(remaining, out _, out var consumed) != OperationStatus.Done)
                    return false;

                remaining = remaining[consumed..];
            }

            return true;
        }
    }
}
