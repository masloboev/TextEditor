using TextEditor.Attributes;

namespace TextEditor.Model
{
    /// <summary>
    ///     Empty segment for empty document.
    /// </summary>
    public class EmptySegment : ISegment
    {
        /// <summary>
        /// Position where the segments text begins in RowData.
        /// </summary>
        public int BeginPosition { get; } = 0;

        /// <summary>
        /// Flag shows that segment contains only one word without spaces.
        /// To view segment type
        /// </summary>
        public bool IsMonoWord { get; } = false;

        /// <summary>
        /// Flag shows that segment ends with paragraph ends.
        /// To view segment type
        /// </summary>
        public bool EndsWithNewLine { get; } = true;

        public int Length { get; } = 0;

        [NotNull]
        public char[] RowData { get; } = new char[0]; // Paragraph to not handle empty segments
    }
}
