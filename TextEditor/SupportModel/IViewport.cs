using System;
using TextEditor.Attributes;
using TextEditor.ViewModel;

namespace TextEditor.SupportModel
{
    /// <summary>
    ///     Interface to viewport that present document part in control. Base idea is viewport that can move up and down over the document
    /// </summary>
    public interface IViewport
    {
        /// <summary>
        /// Method makes a text to present in the control.
        /// </summary>
        /// <param name="contentWriter">The content writer.</param>
        /// <param name="symbolsInRowCount">The view symbols in row count.</param>
        /// <returns>viewport content</returns>
        [return: NotNull]
        string MakeContent([NotNull] IContentWriter contentWriter, int symbolsInRowCount);

        /// <summary>
        ///     Scrolls to specified position (By scrollbar)
        /// </summary>
        void ScrollTo(RowsScrollPosition rowsScrollPosition);

        /// <remarks>
        /// We can scroll down when have rows after viewport or not calculated segemtns in document
        /// </remarks>
        bool CanScrollDown();

        /// <summary>
        /// Scrolls down for specified distance in rows
        /// </summary>
        void ScrollDown(int distance);

        /// <remarks>
        /// We can scroll up when have rows before viewport or not calculated segemtns in document
        /// </remarks>
        bool CanScrollUp();

        /// <summary>
        /// Scrolls up for specified distance in rows
        /// </summary>
        void ScrollUp(int distance);

        /// <summary>
        /// Document scroll poistion
        /// </summary>
        DocumentScrollPosition DocumentScrollPosition { get; }

        /// <summary>
        /// Event that indicates that scrollinformation was changed (Neeeded for ScrollBarViewModel)
        /// </summary>
        event Action<RowsScrollPosition> EvScrollPositionChanged;

        /// <summary>
        ///     Gets the current scroll position ov the viewport
        /// </summary>
        RowsScrollPosition RowsScrollPosition { get; }

        /// <summary>
        /// Height of viewport in rows
        /// </summary>
        int RowsCount { get; set; }
    }
}
