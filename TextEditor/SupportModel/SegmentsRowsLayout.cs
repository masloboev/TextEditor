using System.Collections.Generic;
using TextEditor.Model;

namespace TextEditor.SupportModel
{
    /// <summary>
    ///     Layout rows view structure. Used in scrollbar to map view row position to segment and back
    /// </summary>
    public class SegmentsRowsLayout
    {
        /// <summary>
        ///     Segment information
        /// </summary>
        public class SegmentRowsPosition
        {
            /// <summary>
            ///     Segment reference
            /// </summary>
            public ISegment Segment;
            /// <summary>
            ///     Segment height in viewport rows
            /// </summary>
            public int RowsCount;
            /// <summary>
            ///     Segment rowPosition in rows from the beginning of the document
            /// </summary>
            public long StartDocumentRowsOffset;
        }
        /// <summary>
        ///     Total height of the document in viewports rows 
        /// </summary>
        public long TotalHeight;

        /// <summary>
        ///     sorted segments list in document order
        /// </summary>
        public List<SegmentRowsPosition> PositionsByOffset;
        /// <summary>
        ///     segment information by segment index
        /// </summary>
        public Dictionary<ISegment, SegmentRowsPosition> PositionsBySegment;

        /// <summary>
        ///     segment order comaprer for lists by row binary search
        /// </summary>
        private class OffsetComparer : IComparer<SegmentRowsPosition>
        {
            public static OffsetComparer Instance { get; } = new OffsetComparer();
            public int Compare(SegmentRowsPosition x, SegmentRowsPosition y) => x.StartDocumentRowsOffset.CompareTo(y.StartDocumentRowsOffset);
        }

        /// <summary>
        ///     Method to find the segment position by viewports row all document index 
        /// </summary>
        public SegmentRowsPosition FindBySegment(ISegment segment) => PositionsBySegment[segment];

        /// <summary>
        ///     Method to find the segment by viewports row position 
        /// </summary>
        /// <param name="rowPosition">viewports row position</param>
        /// <returns>Segment that contains the row</returns>
        public SegmentRowsPosition FindByOffset(long rowPosition)
        {
            var index = PositionsByOffset.BinarySearch(new SegmentRowsPosition { StartDocumentRowsOffset = rowPosition }, OffsetComparer.Instance);
            return index >= 0 ? PositionsByOffset[index] : PositionsByOffset[~index - 1];
        }
    }
}
