namespace TextEditor.ViewModel
{
    /// <summary>
    /// Interface to element that can be scrolled
    /// </summary>
    public interface IScrollable
    {
        /// <summary>
        /// Scrolls to specified position
        /// </summary>
        /// <param name="rowsScrollPosition">The scroll position.</param>
        void ScrollTo(RowsScrollPosition rowsScrollPosition);
    }
}
