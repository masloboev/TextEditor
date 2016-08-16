using System;
using System.Collections.Generic;
using System.Text;
using TextEditor.Attributes;
using TextEditor.SupportModel;
using TextEditor.ViewModel;

namespace TextEditor
{
    /// <summary>
    ///     Tool to print one row to content builder
    ///     Prints invisible symbols
    /// </summary>
    public class ContentWriter : IContentWriter
    {
        /// <summary>
        /// Writes the specified row to specified string builder
        /// </summary>
        /// <param name="row">The row to print.</param>
        /// <param name="sb">The string builder to print to.</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <remarks>
        /// Mathod is public only for testing
        /// </remarks>
        public static void WriteRow([NotNull] Row row, [NotNull] StringBuilder sb)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            for (var currentPosition = row.BeginPosition; currentPosition < row.BeginPosition + row.Length; currentPosition++)
            {
                var current = row.RowData[currentPosition];
                if (current == '\r' && currentPosition + 1 < row.Length && row.RowData[currentPosition + 1] == '\n')
                {
                    current = '\n';
                    currentPosition++;
                }

                if (current == '\n')
                    sb.Append('¶');
                else if (char.IsWhiteSpace(current))
                    if (current == '\t')
                    {
                        if (currentPosition < row.Length - 1)
                        {
                            var count = Constants.TabSize - (currentPosition - row.BeginPosition)%Constants.TabSize;
                            for (var i = 0; i < count - 1; i++)
                                sb.Append(' ');
                        }
                        sb.Append('→');
                    }
                    else
                        sb.Append('·');
                else
                    sb.Append(current);
            }
        }

        /// <summary>
        /// Method prints whole segment to contents builder
        /// </summary>
        /// <param name="segmentViewModel">The segment view model.</param>
        /// <param name="sb">The sb.</param>
        /// <param name="linePosition">The line position.</param>
        /// <param name="rowsCount">The rows count.</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        /// <remarks>
        /// Methods prints different symbols depends on segment type
        /// ↓ nornal segments with paragraph end
        /// ⇣ bad segments separated by whitespace not paragraph.
        /// ⇃ segments with only one word
        /// </remarks>
        /// <remarks>
        /// Mathod is public only for testing
        /// </remarks>
        public static void WriteSegment([NotNull] ISegmentViewModel segmentViewModel, StringBuilder sb, int linePosition, int rowsCount)
        {
            if (segmentViewModel == null) throw new ArgumentNullException(nameof(segmentViewModel));
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (linePosition < 0) throw new ArgumentOutOfRangeException(nameof(linePosition));
            if (rowsCount < 0 || linePosition + rowsCount > segmentViewModel.RowsCount) throw new ArgumentOutOfRangeException(nameof(rowsCount));

            for (var i = linePosition; i < linePosition + rowsCount; i++)
            {
                WriteRow(segmentViewModel.Rows[i], sb);
                if (i == segmentViewModel.RowsCount - 1)
                    sb.AppendLine(segmentViewModel.Segment.IsMonoWord ? "⇃" : segmentViewModel.Segment.EndsWithNewLine ? "↓" : "⇣");
                else
                    sb.AppendLine();
            }
        }

        /// <summary>
        /// Makes the content of segemtns list.
        /// </summary>
        /// <param name="firstSegmentOffset">The offset from first segment.</param>
        /// <param name="segmentViewModels">The segment view models.</param>
        /// <param name="rowsCount">The rows count.</param>
        /// <param name="symbolsInRowCount">The symbols in row count to prelocate content.</param>
        /// <returns>
        /// Viewport content
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        [return: NotNull]
        public string MakeContent(int firstSegmentOffset, [NotNull] IReadOnlyList<ISegmentViewModel> segmentViewModels, int rowsCount, int symbolsInRowCount)
        {
            if (segmentViewModels == null) throw new ArgumentNullException(nameof(segmentViewModels));
            if (firstSegmentOffset < 0) throw new ArgumentOutOfRangeException(nameof(firstSegmentOffset));
            if (rowsCount < 0) throw new ArgumentOutOfRangeException(nameof(rowsCount));
            if (symbolsInRowCount <= 0) throw new ArgumentOutOfRangeException(nameof(symbolsInRowCount));

            var sb = new StringBuilder(rowsCount * symbolsInRowCount); // Maximum symbols count in content
            var segmentOffset = firstSegmentOffset;
            var printedRowsCount = 0;
            foreach (var segmentViewModel in segmentViewModels)
            {
                var rowsToWriteCount = Math.Min(rowsCount - printedRowsCount, segmentViewModel.RowsCount - segmentOffset);
                WriteSegment(segmentViewModel, sb, segmentOffset, rowsToWriteCount);
                printedRowsCount += rowsToWriteCount;
                segmentOffset = 0;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the reserved symbols count for service information printing.
        /// </summary>
        public int ReservedSymbolsCount { get; } = 2; // One for latest line space/paragraph, One for segment break Symbol
    }
}
