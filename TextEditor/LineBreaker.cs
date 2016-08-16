using System;
using System.Collections.Generic;
using TextEditor.Attributes;
using TextEditor.Model;
using TextEditor.SupportModel;

namespace TextEditor
{
    /// <summary>
    ///     Algorythm to split segment lines into rows with specified row length
    /// </summary>
    public class LineBreaker : ILineBreaker
    {
        /// <summary>
        /// The symbols in row count
        /// </summary>
        private readonly int _symbolsInRowCount;

        // ReSharper disable once ConvertToAutoProperty due to performance issue
        /// <summary>
        /// Gets count of symbols in row.
        /// </summary>
        public int SymbolsInRowCount => _symbolsInRowCount;

        /// <summary>
        /// string proceeding state
        /// </summary>
        private enum State
        {
            InWord,
            InSpace
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="symbolsInRowCount">Width in symbols to break line.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public LineBreaker(int symbolsInRowCount)
        {
            if (symbolsInRowCount <= 0) throw new ArgumentOutOfRangeException(nameof(symbolsInRowCount));

            _symbolsInRowCount = symbolsInRowCount;
        }

        /// <summary>
        /// Just height of segment in rows calculation (used in layout)
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns>row count</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public int GetRowsCount([NotNull] ISegment segment)
        {
            if (segment == null) throw new ArgumentNullException(nameof(segment));

            return InternalGetRows(segment, null);
        }

        /// <summary>
        /// Calculates rows list of segment
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns>rows list</returns>
        /// <exception cref="ArgumentNullException"></exception>
        [return: NotNull]
        public List<Row> GetRows([NotNull] ISegment segment)
        {
            if (segment == null) throw new ArgumentNullException(nameof(segment));

            var lines = new List<Row>(segment.Length / SymbolsInRowCount);
            InternalGetRows(segment, lines);
            return lines;
        }

        /// <summary>
        /// Calculates rows list of segment. May not store rows, only calculate segment height
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="lines">The lines.</param>
        /// <returns>rows count</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public int InternalGetRows([NotNull] ISegment segment, List<Row> lines)
        {
            if (segment == null) throw new ArgumentNullException(nameof(segment));

            var linesCount = 0;
            var acc = 0L;

            var lineBeginPosition = segment.BeginPosition;
            int? wordBeginPosition = null;
            var state = State.InWord;
            var wasParagraph = false;

            var endPosition = segment.BeginPosition + segment.Length;
            var data = segment.RowData;
            var currentPosition = segment.BeginPosition;
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

                    if (acc > _symbolsInRowCount)
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
                if (acc > _symbolsInRowCount)
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

            if (currentPosition - 1 >= lineBeginPosition || (!wasParagraph && currentPosition - 1 >= segment.BeginPosition))
            {
                linesCount++;
                lines?.Add(new Row(data, lineBeginPosition, currentPosition - 1, false, false));
            }

            return linesCount;
        }
    }
}
