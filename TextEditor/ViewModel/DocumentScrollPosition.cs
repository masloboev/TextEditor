using System;
using TextEditor.Attributes;
using TextEditor.Model;

namespace TextEditor.ViewModel
{
    /// <summary>
    /// Struct defines the scroll position in viewport
    /// </summary>
    public struct DocumentScrollPosition
    {
        /// <summary>
        /// The first visible segment
        /// </summary>
        [NotNull]
        public ISegment FirstSegment;

        /// <summary>
        /// The offset in characters in first visible segment 
        /// </summary>
        public int SegmentOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentScrollPosition"/> struct.
        /// </summary>
        /// <param name="firstSegment">The first segment in viewport.</param>
        /// <param name="segmentOffset">The segment offset in symbols.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public DocumentScrollPosition([NotNull] ISegment firstSegment, int segmentOffset)
        {
            if (firstSegment == null) throw new ArgumentNullException(nameof(firstSegment));
            if (segmentOffset < 0) throw new ArgumentOutOfRangeException(nameof(segmentOffset));

            FirstSegment = firstSegment;
            SegmentOffset = segmentOffset;
        }
    }
}
