using TextEditor.Attributes;

namespace TextEditor.Model
{
    public interface IDocument
    {
        /// <summary>
        ///     Method to find the next closest segment in the document that comes after.
        /// </summary>
        /// <param name="segment">the segment</param>
        /// <returns>Next segment</returns>
        ISegment Next([NotNull] ISegment segment);

        /// <summary>
        ///     Method to find the next closest segment in the document that comes before.
        /// </summary>
        /// <param name="segment">the segment</param>
        /// <returns>Next segment</returns>
        ISegment Prev([NotNull] ISegment segment);

        /// <summary>
        /// Gets the first segment of the document.
        /// </summary>
        [NotNull] ISegment FirstSegment { get; }

        /// <summary>
        /// Gets the last segment of the document.
        /// </summary>
        [NotNull] ISegment LastSegment { get; }

        /// <summary>
        /// Gets the segments count.
        /// </summary>
        int SegmentsCount { get; }
    }
}
