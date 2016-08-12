using System.ComponentModel;
using System.Runtime.CompilerServices;
using TextEditor.Model;
using TextEditor.SupportModel;

namespace TextEditor.ViewModel
{
    /// <summary>
    ///     Main Viewmodel for textcontrol. Mediator.
    /// </summary>
    public class TextViewModel : INotifyPropertyChanged
    {
        public Document Document { get; private set; }
        public RowWriter RowWriter { get; private set; }

        public LineBreaker LineBreaker { get; private set; }

        private Viewport _viewport;
        public ScrollBarViewModel ScrollBarViewModel { get; } = new ScrollBarViewModel();

        public void Init(Document document, RowWriter rowWriter)
        {
            Document = document;
            RowWriter = rowWriter;
            ScrollBarViewModel.Init(this);

            LineBreaker = new LineBreaker(LineBreakerSymbolsInRowCount);
            _viewport = new Viewport(this, _rowsCount, _viewport);
            RebuildContent();
            ScrollBarViewModel.Update(_viewport);
        }

        #region Content
        private string _content;

        public string Content
        {
            get { return _content; }
            set {
                _content = value;
                OnPropertyChanged();
            }
        }

        private void RebuildContent() => Content = _viewport?.MakeContent() ?? "";
        #endregion

        #region Scroll

        public bool CanScrollDown() => _viewport?.CanScrollDown() ?? false;
        
        public void LineDown()
        {
            _viewport?.ScrollDown(1);
            RebuildContent();
        }

        public void PageDown()
        {
            _viewport?.ScrollDown(_viewport.RowsCount);
            RebuildContent();
        }

        public bool CanScrollUp() => _viewport?.CanScrollUp() ?? false;

        public void LineUp()
        {
            _viewport?.ScrollUp(1);
            RebuildContent();
        }       

        public void PageUp()
        {
            _viewport?.ScrollUp(_viewport.RowsCount);
            RebuildContent();
        }

        public void ScrollTo(ScrollPosition scrollPosition)
        {
            _viewport?.ScrollTo(scrollPosition);
            RebuildContent();
        }

        #endregion

        #region View vertical resize
        private int _rowsCount;
        public int RowsCount {
            get { return _rowsCount; }
            set
            {                
                if (_rowsCount == value)
                    return;
                _rowsCount = value;
                if (Document == null)
                    return;

                if (_viewport != null)
                {
                    _viewport.RowsCount = _rowsCount;                    
                    RebuildContent();
                }
                OnPropertyChanged();
            }
        }

        #endregion

        #region view horizontal resize
        private int _symbolsInRowCount;

        /// <summary>
        ///     Major control update. Viewport recreation.
        /// </summary>
        public int SymbolsInRowCount
        {
            get { return _symbolsInRowCount; }
            set
            {
                if (_symbolsInRowCount == value)
                    return;
                _symbolsInRowCount = value;
                if (Document == null)
                    return;
                LineBreaker = new LineBreaker(LineBreakerSymbolsInRowCount);
                _viewport = new Viewport(this, _rowsCount, _viewport);
                RebuildContent();
                ScrollBarViewModel.Update(_viewport);
                OnPropertyChanged();
            }
        }

        public int LineBreakerSymbolsInRowCount => SymbolsInRowCount - 2; // two symbols for "invisible" paragraph tag and latest whitespace/paragraph

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
