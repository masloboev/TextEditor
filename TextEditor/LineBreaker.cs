using System.Collections.Generic;
using TextEditor.Model;
using TextEditor.SupportModel;

namespace TextEditor
{
    /// <summary>
    ///     Algorythm to split segment lines into rows with specified row length
    /// </summary>
    public class LineBreaker
    {
        private readonly int _width;

        // ReSharper disable once ConvertToAutoProperty due to performance issue
        public int Width => _width;

        private enum State
        {
            InWord,
            InSpace
        }

        public LineBreaker(int width)
        {
            _width = width;
        }

        /// <summary>
        ///     Just height of segment in rows calculation (used in layout)
        /// </summary>
        public int GetRowsCount(ISegment segment) => InternalGetRows(segment, null);

        /// <summary>
        ///     Calculates rows list of segment
        /// </summary>
        public List<Row> GetRows(ISegment segment)
        {
            var lines = new List<Row>((int)(segment.Length / Width));
            InternalGetRows(segment, lines);
            return lines;
        }

        /// <summary>
        ///     Calculates rows list of segment. May not store rows, only calculate segment height
        /// </summary>
        public int InternalGetRows(ISegment segment, List<Row> lines)
        {
            var linesCount = 0;
            var acc = 0L;

            var lineBeginPosition = segment.RowDataOffset;
            long? wordBeginPosition = null;
            var state = State.InWord;
            var wasParagraph = false;

            var endPosition = segment.RowDataOffset + segment.Length;
            var data = segment.RowData;
            var currentPosition = segment.RowDataOffset;
            for (; currentPosition < endPosition; currentPosition++)
            {
                var current = data[currentPosition];
                if (current == '\r' && currentPosition + 1 < endPosition && data[currentPosition + 1] == '\n')
                { // skip caret in \r\n
                    current = '\n';
                    currentPosition++;
                }
                if (current == '\n')
                { // always flush row on paragraph
                    linesCount++;
                    lines?.Add(new Row(data, lineBeginPosition, currentPosition, false, true));
                    acc = 0;

                    lineBeginPosition = currentPosition + 1;
                    wordBeginPosition = null;
                    state = State.InWord;
                    wasParagraph = true;
                    continue;
                }

                if (char.IsWhiteSpace(current))
                {
                    state = State.InSpace;
                    if (current == '\t')
                        acc += Constants.TabSize - acc% Constants.TabSize;
                    else
                        acc++;

                    if (acc > Width)
                    {// We can flush row everywhere in space sequence
                        linesCount++;
                        lines?.Add(new Row(data, lineBeginPosition, currentPosition, false, false));
                        acc = 0;

                        lineBeginPosition = currentPosition + 1;
                        wordBeginPosition = null;
                        state = State.InWord;
                        wasParagraph = false;
                    }
                    continue;
                }

                acc++;
                if (acc > Width)
                { // Row oversize 
                    if (state == State.InWord && wordBeginPosition.HasValue)
                    { // Flush all row before latest word begin
                        linesCount++;
                        lines?.Add(new Row(data, lineBeginPosition, wordBeginPosition.Value - 1, false, false));
                        lineBeginPosition = wordBeginPosition.Value;
                        acc = currentPosition - wordBeginPosition.Value + 1;
                        wordBeginPosition = null;
                        wasParagraph = false;
                        continue;
                    }
                    // Very long word
                    linesCount++;
                    lines?.Add(new Row(data, lineBeginPosition, currentPosition - 1, true, false));
                    lineBeginPosition = currentPosition;
                    acc = 1;
                    wordBeginPosition = null;
                    wasParagraph = false;
                    continue;
                }

                if (state == State.InSpace)
                {
                    wordBeginPosition = currentPosition;
                    state = State.InWord;
                }
            }

            if (currentPosition - 1 > lineBeginPosition || !wasParagraph)
            {
                linesCount++;
                lines?.Add(new Row(data, lineBeginPosition, currentPosition - 1, false, false));
            }

            return linesCount;
        }
    }
}
