using TextEditor.Attributes;
using TextEditor.Model;

namespace TextEditor.SupportModel
{
    /// <summary>
    ///     Interface for Layout rows view structure. Used in scrollbar to map view row position to segment and back
    /// </summary>
    public interface ISegmentsRowsLayout
    {
        /// <summary>
        ///     the document total count viewports rows 
        /// </summary>
        long TotalRowsCount { get; }

        /// <summary>
        /// Appends the specified segment to containers.
        /// </summary>
        /// <param name="segmentRowsPosition">The segment information.</param>
        void Append([NotNull] SegmentRowsPosition segmentRowsPosition);

        /// <summary>
        ///     Method to find the segment position by viewports row all document index 
        /// </summary>
        [return: NotNull]
        SegmentRowsPosition FindBySegment([NotNull] ISegment segment);

        /// <summary>
        ///     Method to find the segment by viewports row position 
        /// </summary>
        /// <param name="rowPosition">viewports row position</param>
        /// <returns>Segment that contains the row</returns>
        SegmentRowsPosition FindByOffset([NotNull] long rowPosition);
    }
}
