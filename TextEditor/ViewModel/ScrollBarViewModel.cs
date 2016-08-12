using System.ComponentModel;
using System.Runtime.CompilerServices;
using TextEditor.SupportModel;

namespace TextEditor.ViewModel
{
    /// <summary>
    ///     Viewmodel for main window
    /// </summary>
    public class ScrollBarViewModel : INotifyPropertyChanged
    {
        private TextViewModel _textViewModel;

        /// <summary>
        ///     Reference to the cache of SegmentsRowsLayout
        /// </summary>
        private SegmentsRowsLayoutCache _segmentsRowsLayoutCache;

        /// <summary>
        ///     Reference to the current layout
        /// </summary>
        private SegmentsRowsLayout _segmentsRowsLayout;

        private Viewport _viewport;

        public void Init(TextViewModel textViewModel)
        {
            _textViewModel = textViewModel;
            _segmentsRowsLayoutCache = new SegmentsRowsLayoutCache(_textViewModel.Document);
            _segmentsRowsLayout = null;           
        }

        /// <summary>
        ///     Updates Viewmodel on viewport change (horizontal resize)
        /// </summary>
        public void Update(Viewport viewPort)
        {
            if (_viewport != null)
                _viewport.EvScrollPositionChanged -= Refresh;
            _viewport = viewPort;
            _viewport.EvScrollPositionChanged += Refresh;
            Update();
        }
        private void Update()
        { 
            _segmentsRowsLayout = _segmentsRowsLayoutCache.TryGet(_textViewModel.LineBreakerSymbolsInRowCount);
            if (_segmentsRowsLayout != null)
                InternalUpdate(); // We've got cached value
            else
            {// We need wait for calculation
                IsEnabled = false;
                var width = _textViewModel.LineBreakerSymbolsInRowCount;
                var task = _segmentsRowsLayoutCache.GetAsync(width, OnProgress);
                task.ContinueWith(t =>
                                  {
                                      _segmentsRowsLayoutCache.Unprogress(width, OnProgress);
                                      if (t.IsCanceled)
                                      {
                                          if (width != _textViewModel.LineBreakerSymbolsInRowCount)
                                              return; // task from previous width. Skip.
                                          Update(); // May place mistake with actual task cancellation. Retry.
                                          return;
                                      }
                                      if (width != _textViewModel.LineBreakerSymbolsInRowCount)
                                          return;  // task from previous width. Skip.
                                      _segmentsRowsLayout = t.Result;
                                      InternalUpdate(); // We receive layout. Update scrollbar.
                                  });
            }
        }

        private void InternalUpdate()
        {
            LargeChange = _textViewModel.RowsCount;
            Refresh(_viewport.ScrollPosition);
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
        private void Refresh(ScrollPosition scrollPosition)
        {
            if(_segmentsRowsLayout == null)
                return;

            var segmentInfo = _segmentsRowsLayout.FindBySegment(scrollPosition.FirstSegment);
            Value = segmentInfo.StartDocumentRowsOffset + scrollPosition.RowsBeforeScrollCount;
            Maximum = _segmentsRowsLayout.TotalHeight - _viewport.RowsCount;
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
            _textViewModel.ScrollTo(new ScrollPosition {
                FirstSegment = segmentInfo.Segment,
                RowsBeforeScrollCount = (int)(offset - segmentInfo.StartDocumentRowsOffset)});
        }

        private bool _isEnabled;

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

        private double _value;
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

        private double _maxium;

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

        private double _largeChange;

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
