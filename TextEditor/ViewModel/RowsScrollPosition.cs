using System;
using TextEditor.Attributes;
using TextEditor.Model;

namespace TextEditor.ViewModel
{
    /// <summary>
    /// Struct defines the scroll position in viewport
    /// </summary>
    public struct RowsScrollPosition
    {
        /// <summary>
        /// The first visible segment
        /// </summary>
        [NotNull]
        public ISegment FirstSegment;

        /// <summary>
        /// The offset in rows first visible segment 
        /// </summary>
        public int RowsBeforeScrollCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="RowsScrollPosition" /> struct.
        /// </summary>
        /// <param name="firstSegment">The first segment in viewport.</param>
        /// <param name="rowsBeforeScrollCount">The rows before scroll count.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public RowsScrollPosition([NotNull] ISegment firstSegment, int rowsBeforeScrollCount)
        {
            if (firstSegment == null) throw new ArgumentNullException(nameof(firstSegment));
            if (rowsBeforeScrollCount < 0) throw new ArgumentOutOfRangeException(nameof(rowsBeforeScrollCount));

            FirstSegment = firstSegment;
            RowsBeforeScrollCount = rowsBeforeScrollCount;
        }
    }
}
