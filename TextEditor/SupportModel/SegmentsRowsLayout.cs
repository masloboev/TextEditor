using System;
using System.Collections.Generic;
using TextEditor.Attributes;
using TextEditor.Model;

namespace TextEditor.SupportModel
{
    /// <summary>
    ///     Layout rows view structure. Used in scrollbar to map view row position to segment and back
    /// </summary>
    public class SegmentsRowsLayout : ISegmentsRowsLayout
    {
        /// <summary>
        ///     the document total count viewports rows 
        /// </summary>
        public long TotalRowsCount { get; set; }

        /// <summary>
        ///     sorted segments list in document order
        /// </summary>
        [NotNull]
        private readonly List<SegmentRowsPosition> _positionsByOffset;
        /// <summary>
        ///     segment information by segment index
        /// </summary>
        [NotNull]
        private readonly Dictionary<ISegment, SegmentRowsPosition> _positionsBySegment;

        /// <summary>
        ///     segment order comaprer for lists by row binary search
        /// </summary>
        private class OffsetComparer : IComparer<SegmentRowsPosition>
        {
            public static OffsetComparer Instance { get; } = new OffsetComparer();

            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>
            /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
            /// </returns>
            public int Compare([NotNull] SegmentRowsPosition x, [NotNull] SegmentRowsPosition y)
            {
                if (x == null) throw new ArgumentNullException(nameof(x));
                if (y == null) throw new ArgumentNullException(nameof(y));

                return x.StartDocumentRowsOffset.CompareTo(y.StartDocumentRowsOffset);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentsRowsLayout"/> class.
        /// </summary>
        /// <param name="capacity">The capacity to prealloc internal containers</param>
        public SegmentsRowsLayout(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));

            TotalRowsCount = 0;
            _positionsByOffset = new List<SegmentRowsPosition>(capacity);
            _positionsBySegment = new Dictionary<ISegment, SegmentRowsPosition>(capacity);
        }

        /// <summary>
        /// Appends the specified segment to containers.
        /// </summary>
        /// <param name="segmentRowsPosition">The segment information.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Append([NotNull] SegmentRowsPosition segmentRowsPosition)
        {
            if (segmentRowsPosition == null) throw new ArgumentNullException(nameof(segmentRowsPosition));

            _positionsByOffset.Add(segmentRowsPosition);
            _positionsBySegment[segmentRowsPosition.Segment] = segmentRowsPosition;
            TotalRowsCount += segmentRowsPosition.RowsCount;
        }

        /// <summary>
        /// Method to find the segment position by viewports row all document index
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        [return: NotNull]
        public SegmentRowsPosition FindBySegment([NotNull] ISegment segment)
        {
            if (segment == null) throw new ArgumentNullException(nameof(segment));
            return _positionsBySegment[segment];
        }

        /// <summary>
        ///     Method to find the segment by viewports row position 
        /// </summary>
        /// <param name="rowPosition">viewports row position</param>
        /// <returns>Segment that contains the row</returns>
        public SegmentRowsPosition FindByOffset(long rowPosition)
        {
            if (_positionsByOffset.Count == 0)
                return null;

            // use _positionsByOffset[0].Segment just for stub
            var index = _positionsByOffset.BinarySearch(new SegmentRowsPosition(_positionsByOffset[0].Segment, 0, rowPosition), OffsetComparer.Instance);
            return index >= 0 ? _positionsByOffset[index] : _positionsByOffset[~index - 1];
        }
    }
}
