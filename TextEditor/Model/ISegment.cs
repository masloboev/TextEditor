using TextEditor.Attributes;

namespace TextEditor.Model
{
    /// <summary>
    ///     Base interface for text segment presentation.
    ///     Segment is readonly
    /// </summary>
    public interface ISegment
    {
        /// <summary>
        ///     Length of the text in segment.
        /// </summary>
        int Length { get; }

        /// <summary>
        ///     Position where the segments text begins in RowData.
        /// </summary>
        int BeginPosition { get; }

        /// <summary>
        ///     Reference to the buffer that contains the segments text.
        ///     The returned array must be used only for read. Due to performance issue
        /// </summary>
        [NotNull]
        char[] RowData { get; }

        /// <summary>
        ///     Flag shows that segment contains only one word without spaces.
        ///     To view segment type
        /// </summary>
        bool IsMonoWord { get; }

        /// <summary>
        ///     Flag shows that segment ends with paragraph ends.
        ///     To view segment type
        /// </summary>
        bool EndsWithNewLine { get; }
    }
}
