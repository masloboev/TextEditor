using System;
using TextEditor.Attributes;
using TextEditor.Model;

namespace TextEditor.SupportModel
{
    /// <summary>
    ///     Segment position
    /// </summary>
    public class SegmentRowsPosition
    {
        /// <summary>
        ///     Segment reference
        /// </summary>
        [NotNull]
        public ISegment Segment;
        /// <summary>
        ///     Segment height in viewport rows
        /// </summary>
        public int RowsCount;
        /// <summary>
        ///     Segment rowPosition in rows from the beginning of the document
        /// </summary>
        public long StartDocumentRowsOffset;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="rowsCount">Segment height in viewport rows.</param>
        /// <param name="startDocumentRowsOffset"> Segment rowPosition in rows from the beginning of the document</param>
        public SegmentRowsPosition([NotNull] ISegment segment, int rowsCount, long startDocumentRowsOffset)
        {
            if (segment == null) throw new ArgumentNullException(nameof(segment));

            Segment = segment;
            RowsCount = rowsCount;
            StartDocumentRowsOffset = startDocumentRowsOffset;
        }
    }
}
