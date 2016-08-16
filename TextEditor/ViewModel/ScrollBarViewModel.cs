using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TextEditor.Attributes;
using TextEditor.SupportModel;

namespace TextEditor.ViewModel
{
    /// <summary>
    ///     Viewmodel for main window
    /// </summary>
    public class ScrollBarViewModel : IScrollBarViewModel
    {
        /// <summary>
        ///     Reference to the SegmentsRowsLayout provider
        /// </summary>
        /// <remarks>public only for tests</remarks>
        [NotNull]
        public ISegmentsRowsLayoutProvider SegmentsRowsLayoutProvider;

        /// <summary>
        ///     Reference to the current layout
        /// </summary>
        [NotNull]
        private ISegmentsRowsLayout _segmentsRowsLayout;

        /// <summary>
        /// The viewport
        /// </summary>
        [NotNull]
        private IViewport _viewport;

        /// <summary>
        /// The scroll target
        /// </summary>
        [NotNull]
        private IScrollable _scrollable;

        /// <summary>
        /// The symbols in row count
        /// </summary>
        private int _symbolsInRowCount;

        /// <summary>
        /// Initializes the ScrollbarViewModel.
        /// </summary>
        /// <param name="segmentsRowsLayoutProvider">SegmentsRowsLayout provider</param>
        /// <param name="scrollable">Entry to scroll to</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public void Init([NotNull] ISegmentsRowsLayoutProvider segmentsRowsLayoutProvider, [NotNull] IScrollable scrollable)
        {
            if (segmentsRowsLayoutProvider == null) throw new ArgumentNullException(nameof(segmentsRowsLayoutProvider));
            if (scrollable == null) throw new ArgumentNullException(nameof(scrollable));

            _scrollable = scrollable;
            SegmentsRowsLayoutProvider = segmentsRowsLayoutProvider;
            _segmentsRowsLayout = null;           
        }

        /// <summary>
        /// Updates Viewmodel on viewport change (horizontal resize)
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="symbolsInRowCount">The symbols in row count.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Update([NotNull] IViewport viewport, int symbolsInRowCount)
        {
            if (viewport == null) throw new ArgumentNullException(nameof(viewport));
            if (symbolsInRowCount <= 0) throw new ArgumentOutOfRangeException(nameof(symbolsInRowCount));

            // ReSharper disable once UnusedVariable due to async Task loose
            var task = UpdateAsync(viewport, symbolsInRowCount);
        }

        /// <summary>
        /// Updates Viewmodel on viewport change (horizontal resize)
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="symbolsInRowCount">The symbols in row count.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [return: NotNull]
        public async Task UpdateAsync([NotNull] IViewport viewport, int symbolsInRowCount)
        {
            if (viewport == null) throw new ArgumentNullException(nameof(viewport));
            if (symbolsInRowCount <= 0) throw new ArgumentOutOfRangeException(nameof(symbolsInRowCount));

            _symbolsInRowCount = symbolsInRowCount;
            if (_viewport != null)
                _viewport.EvScrollPositionChanged -= Refresh;
            _viewport = viewport;
            _viewport.EvScrollPositionChanged += Refresh;

            await UpdateAsync();
        }

        /// <summary>
        /// Updates Viewmodel on viewport change (horizontal resize)
        /// </summary>
        /// <returns></returns>
        [return: NotNull]
        private async Task UpdateAsync()
        { 
            _segmentsRowsLayout = SegmentsRowsLayoutProvider.TryGet(_symbolsInRowCount);
            if (_segmentsRowsLayout != null)
                InternalUpdate(); // We've got cached value
            else
            {// We need wait for calculation
                IsEnabled = false;
                var symbolsInRowCount = _symbolsInRowCount; // for clsoure
                try
                {
                    var segmentsRowsLayout = await SegmentsRowsLayoutProvider.GetAsync(symbolsInRowCount, OnProgress);
                    if (symbolsInRowCount != _symbolsInRowCount)
                        return; // task from previous symbolsInRowCount. Skip.
                    _segmentsRowsLayout = segmentsRowsLayout;
                    InternalUpdate(); // We receive layout. Update scrollbar.
                }
                catch (OperationCanceledException)
                {
                    if (symbolsInRowCount == _symbolsInRowCount)
                        await UpdateAsync(); // May place mistake with actual task cancellation. Retry.
                }
                finally
                {
                    SegmentsRowsLayoutProvider.Unprogress(symbolsInRowCount, OnProgress);
                }              
            }
        }

        /// <summary>
        /// Updates state.
        /// </summary>
        private void InternalUpdate()
        {
            LargeChange = _viewport.RowsCount;
            Refresh(_viewport.RowsScrollPosition);
            IsEnabled = true;
        }

        /// <summary>
        ///     Layout calculation progress callback
        /// </summary>
        private void OnProgress(object sender, string e) => Status = e;

        /// <summary>
        ///     Minor scroll update
        /// </summary>
        /// <remarks>
        ///     On Scroll position change or viewport vertical resize
        /// </remarks>
        private void Refresh(RowsScrollPosition rowsScrollPosition)
        {
            if(_segmentsRowsLayout == null)
                return;

            var segmentInfo = _segmentsRowsLayout.FindBySegment(rowsScrollPosition.FirstSegment);
            Value = segmentInfo.StartDocumentRowsOffset + rowsScrollPosition.RowsBeforeScrollCount;
            Maximum = _segmentsRowsLayout.TotalRowsCount - _viewport.RowsCount;
        }

        /// <summary>
        ///     Update scroll position from scroll control
        /// </summary>
        private void SetValue(double value)
        {
            _value = value;
            var offset = (long)value;
            // find segment by absolute document row position
            var segmentInfo = _segmentsRowsLayout.FindByOffset(offset); 
            _scrollable.ScrollTo(new RowsScrollPosition {
                FirstSegment = segmentInfo.Segment,
                RowsBeforeScrollCount = (int)(offset - segmentInfo.StartDocumentRowsOffset)});
        }

        /// <summary>
        /// Is scroll ready
        /// </summary>
        private bool _isEnabled;

        /// <summary>
        /// Is scroll ready
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            private set
            {
                if (_isEnabled == value)
                    return;
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Scroll position
        /// </summary>
        private double _value;

        /// <summary>
        /// Scroll position
        /// </summary>
        public double Value {
            get { return _value; }
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_value == value)
                    return;
                SetValue(value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Maximum scroll position
        /// </summary>
        private double _maxium;

        /// <summary>
        /// Maximum scroll position
        /// </summary>
        public double Maximum
        {
            get { return _maxium; }
            private set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_maxium == value)
                    return;
                _maxium = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Scroll page size
        /// </summary>
        private double _largeChange;

        /// <summary>
        /// Scroll page size
        /// </summary>
        public double LargeChange
        {
            get { return _largeChange; }
            private set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_largeChange == value)
                    return;
                _largeChange = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Scroll calculation progress
        /// </summary>
        private string _status;
        
        /// <summary>
        ///     Property for displaing calculation progress
        /// </summary>
        public string Status
        {
            get { return _status; }
            private set { _status = value; OnPropertyChanged(); }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
