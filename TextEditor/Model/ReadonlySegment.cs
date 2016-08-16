using System;
using TextEditor.Attributes;

namespace TextEditor.Model
{
    /// <summary>
    ///     Implementation for the segment that wasn't edited.
    /// </summary>
    public class ReadonlySegment : ISegment
    {
        /// <summary>
        /// Position where the segments text begins in RowData.
        /// </summary>
        public int BeginPosition { get; }

        /// <summary>
        /// Flag shows that segment contains only one word without spaces.
        /// To view segment type
        /// </summary>
        public bool IsMonoWord { get; }

        /// <summary>
        /// Flag shows that segment ends with paragraph ends.
        /// To view segment type
        /// </summary>
        public bool EndsWithNewLine { get; }

        /// <summary>
        /// The end position
        /// </summary>
        private readonly int _endPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadonlySegment" /> class.
        /// </summary>
        /// <param name="data">The segments text data.</param>
        /// <param name="beginPosition">The begin position of the segment in text data.</param>
        /// <param name="endPosition">The end position of the segment in text data.</param>
        /// <param name="isMonoWord">Monoword segment flas.</param>
        /// <param name="endsWithNewLine">EndsWithNewLine segment flag.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public ReadonlySegment([NotNull] char[] data, int beginPosition, int endPosition, bool isMonoWord, bool endsWithNewLine)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (beginPosition < 0 || beginPosition >= data.Length) throw new ArgumentOutOfRangeException(nameof(beginPosition));
            if (endPosition < beginPosition || endPosition >= data.Length) throw new ArgumentOutOfRangeException(nameof(endPosition));

            RowData = data;
            BeginPosition = beginPosition;
            _endPosition = endPosition;
            IsMonoWord = isMonoWord;
            EndsWithNewLine = endsWithNewLine;
        }

        /// <summary>
        /// Length of the text in segment.
        /// </summary>
        public int Length => _endPosition - BeginPosition + 1;

        /// <summary>
        /// Reference to the buffer that contains the segments text.
        /// The returned array must be used only for read. Due to performance issue
        /// </summary>
        [NotNull]
        public char[] RowData { get; }
    }
}
