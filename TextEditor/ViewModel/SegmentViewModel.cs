using System;
using System.Collections.Generic;
using TextEditor.Attributes;
using TextEditor.Model;
using TextEditor.SupportModel;

namespace TextEditor.ViewModel
{
    /// <summary>
    /// ViewModel for presenting segments in viewport
    /// </summary>
    public class SegmentViewModel : ISegmentViewModel
    {
        /// <summary>
        /// The segment under model
        /// </summary>
        [NotNull]
        public ISegment Segment { get; }

        /// <summary>
        ///     Rows prepared for viewport
        /// </summary>
        [NotNull]
        public IReadOnlyList<Row> Rows { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="lineBreaker">The line breaker used to break lines on content generation.</param>
        /// <param name="segment">The segment under model</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public SegmentViewModel([NotNull] ILineBreaker lineBreaker, [NotNull]  ISegment segment)
        {
            if (lineBreaker == null) throw new ArgumentNullException(nameof(lineBreaker));
            if (segment == null) throw new ArgumentNullException(nameof(segment));

            Segment = segment;
            Rows = lineBreaker.GetRows(Segment);
        }

        /// <summary>
        ///     calculated segment rows in segment count
        /// </summary>
        public int RowsCount => Rows.Count;
    }
}
