namespace TextEditor.Model
{
    /// <summary>
    ///     Base interface for text segment presentation.
    /// </summary>
    public interface ISegment
    {
        /// <summary>
        ///     Length of the text in segment.
        /// </summary>
        long Length { get; }

        /// <summary>
        ///     Position where the segments text begins in RowData.
        /// </summary>
        long RowDataOffset { get; }

        /// <summary>
        ///     Reference to the buffer that contains the segments text.
        /// </summary>
        char[] RowData { get; }

        /// <summary>
        ///     Flag shows that segment contains only one word without spaces.
        ///     For future release
        /// </summary>
        bool IsMonoWord { get; }

        /// <summary>
        ///     Flag shows that segment ends with paragraph ends.
        ///     For future release
        /// </summary>
        bool EndsWithNewLine { get; }
    }
}
