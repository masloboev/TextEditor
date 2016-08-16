using System;
using TextEditor.Attributes;
using TextEditor.Model;

namespace TextEditor.SupportModel
{
    /// <summary>
    ///     Model for Row to print in viewport. Is similar to <see cref="ReadonlySegment"/>
    /// </summary>
    public class Row
    {
        /// <summary>
        /// The position that row begins in segment.
        /// </summary>
        public int BeginPosition { get; }

        /// <summary>
        /// Is row single word.
        /// </summary>

        public bool IsMonoWord { get; }

        /// <summary>
        /// Is row ends with pararaph symbol
        /// </summary>
        public bool EndsWithNewLine { get; }

        /// <summary>
        /// The end row position in segment
        /// </summary>
        private readonly int _endPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="Row"/> class.
        /// </summary>
        /// <param name="data">Reference to the segments data.</param>
        /// <param name="beginPosition">The row begin position in segment.</param>
        /// <param name="endPosition">The row end position in segment.</param>
        /// <param name="isMonoWord">Is row single word.</param>
        /// <param name="endsWithNewLine">Is row ends with pararaph symbol.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Row([NotNull] char[] data, int beginPosition, int endPosition, bool isMonoWord, bool endsWithNewLine)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (beginPosition < 0) throw new ArgumentOutOfRangeException(nameof(beginPosition));
            if (endPosition < beginPosition || endPosition >= data.Length) throw new ArgumentOutOfRangeException(nameof(endPosition));

            RowData = data;
            BeginPosition = beginPosition;
            _endPosition = endPosition;
            IsMonoWord = isMonoWord;
            EndsWithNewLine = endsWithNewLine;
        }

        /// <summary>
        /// Row length.
        /// </summary>
        public int Length => _endPosition - BeginPosition + 1;

        /// <summary>
        /// Gets the row data.
        /// The returned array must be used only for read. Due to performance issue.
        /// </summary>
        [NotNull]
        public char[] RowData { get; }
    }
}
