using System.Collections.Generic;
using TextEditor.Attributes;
using TextEditor.Model;
using TextEditor.SupportModel;

namespace TextEditor.ViewModel
{
    /// <summary>
    /// Interface for SegmentViewModel
    /// Model presents segments in viewport
    /// </summary>
    public interface ISegmentViewModel
    {
        /// <summary>
        /// The segment under model
        /// </summary>
        [NotNull]
        ISegment Segment { get; }

        /// <summary>
        ///     Rows prepared for viewport
        /// </summary>
        [NotNull]
        IReadOnlyList<Row> Rows { get; }

        /// <summary>
        ///     calculated segment rows in segment count
        /// </summary>
        int RowsCount { get; }
    }
}
