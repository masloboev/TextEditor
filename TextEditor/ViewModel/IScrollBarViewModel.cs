using System.ComponentModel;
using TextEditor.Attributes;
using TextEditor.SupportModel;

namespace TextEditor.ViewModel
{
    public interface IScrollBarViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes the ScrollbarViewModel.
        /// </summary>
        /// <param name="segmentsRowsLayoutProvider">SegmentsRowsLayout provider</param>
        /// <param name="scrollable">Entry to scroll to</param>
        void Init([NotNull] ISegmentsRowsLayoutProvider segmentsRowsLayoutProvider, [NotNull] IScrollable scrollable);

        /// <summary>
        /// Updates on viewport change (horizontal resize)
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="symbolsInRowCount">The symbols in row count.</param>
        void Update([NotNull] IViewport viewport, int symbolsInRowCount);

        /// <summary>
        ///     Property for displaing calculation progress
        /// </summary>
        string Status { get; }

        /// <summary>
        /// Is scroll ready
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Scroll position
        /// </summary>
        double Value { get; set; }

        /// <summary>
        /// Maximum scroll position
        /// </summary>
        double Maximum { get; }
            
        /// <summary>
        /// Scroll page size
        /// </summary>
        double LargeChange { get; }
    }
}
