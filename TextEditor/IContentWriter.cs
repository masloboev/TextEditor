using System.Collections.Generic;
using TextEditor.Attributes;
using TextEditor.ViewModel;

namespace TextEditor
{
    /// <summary>
    ///     Interface to print one row to content builder
    /// </summary>
    public interface IContentWriter
    {
        /// <summary>
        /// Makes the content of segemtns list.
        /// </summary>
        /// <param name="firstSegmentOffset">The offset from first segment.</param>
        /// <param name="segmentViewModels">The segment view models.</param>
        /// <param name="rowsCount">The rows count.</param>
        /// <param name="symbolsInRowCount">The symbols in row count to prelocate content.</param>
        /// <returns>Viewport content</returns>
        [return: NotNull]
        string MakeContent(int firstSegmentOffset, [NotNull] IReadOnlyList<ISegmentViewModel> segmentViewModels, int rowsCount, int symbolsInRowCount);

        /// <summary>
        /// Gets the reserved symbols count for service information printing.
        /// </summary>
        int ReservedSymbolsCount { get; }
    }
}
