using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TextEditor.Attributes;
using TextEditor.Model;
using TextEditor.SupportModel;

namespace TextEditor.ViewModel
{
    /// <summary>
    ///     Main Viewmodel for textcontrol. Mediator.
    /// </summary>
    public class TextViewModel : INotifyPropertyChanged, IScrollable, ITextViewModel
    {
        /// <summary>
        /// Gets the document.
        /// </summary>
        public IDocument Document { get; private set; }

        /// <summary>
        /// The content writer
        /// </summary>
        private IContentWriter _contentWriter;

        /// <summary>
        /// The module factory
        /// </summary>
        private IModuleFactory _moduleFactory;

        /// <summary>
        /// The viewport
        /// </summary>
        private IViewport _viewport;

        /// <summary>
        /// The scroll bar view model
        /// </summary>
        private IScrollBarViewModel _scrollBarViewModel;

        /// <summary>
        /// Gets the ScrollBarViewModel.
        /// </summary>
        public IScrollBarViewModel ScrollBarViewModel
        {
            get { return _scrollBarViewModel; }
            private set
            {
                if (_scrollBarViewModel == value)
                    return;
                _scrollBarViewModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the viewmodel.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="contentWriter">The content writer.</param>
        /// <param name="moduleFactory">The module factory.</param>
        /// <param name="scrollBarViewModel">The ScrollBarViewModel.</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <remarks>
        /// Separated to method due to designer
        /// </remarks>
        public void Init([NotNull] IDocument document, [NotNull] IContentWriter contentWriter, 
            [NotNull] IModuleFactory moduleFactory, [NotNull] IScrollBarViewModel scrollBarViewModel)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (contentWriter == null) throw new ArgumentNullException(nameof(contentWriter));
            if (moduleFactory == null) throw new ArgumentNullException(nameof(moduleFactory));
            if (scrollBarViewModel == null) throw new ArgumentNullException(nameof(scrollBarViewModel));

            Document = document;
            _contentWriter = contentWriter;
            _moduleFactory = moduleFactory;
            ScrollBarViewModel = scrollBarViewModel;
            ScrollBarViewModel.Init(_moduleFactory.MakeSegmentsRowsLayoutProvider(document), this);

            var lineBreaker = _moduleFactory.MakeLineBreaker(LineBreakerSymbolsInRowCount);
            _viewport = _moduleFactory.MakeViewport(Document, lineBreaker, _rowsCount, _viewport?.DocumentScrollPosition);
            RebuildContent();
            ScrollBarViewModel.Update(_viewport, LineBreakerSymbolsInRowCount);
        }

        #region Content
        /// <summary>
        /// The content
        /// </summary>
        private string _content;

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public string Content
        {
            get { return _content; }
            set {
                _content = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Rebuilds the content. Gets from viewport
        /// </summary>
        private void RebuildContent() => Content = _viewport?.MakeContent(_contentWriter, SymbolsInRowCount) ?? "";
        #endregion

        #region Scroll

        /// <summary>
        /// Can scroll down. Gets from viewport
        /// </summary>
        public bool CanScrollDown() => _viewport?.CanScrollDown() ?? false;

        /// <summary>
        /// Scroll lines down.
        /// </summary>
        public void LineDown()
        {
            _viewport?.ScrollDown(1);
            RebuildContent();
        }

        /// <summary>
        /// Scroll Page down.
        /// </summary>
        public void PageDown()
        {
            _viewport?.ScrollDown(_viewport.RowsCount);
            RebuildContent();
        }

        /// <summary>
        /// Can scroll up. Gets from viewport
        /// </summary>
        public bool CanScrollUp() => _viewport?.CanScrollUp() ?? false;

        /// <summary>
        /// Scroll line up.
        /// </summary>
        public void LineUp()
        {
            _viewport?.ScrollUp(1);
            RebuildContent();
        }

        /// <summary>
        /// Scroll page up.
        /// </summary>
        public void PageUp()
        {
            _viewport?.ScrollUp(_viewport.RowsCount);
            RebuildContent();
        }

        /// <summary>
        /// Scrolls to specified position
        /// </summary>
        /// <param name="rowsScrollPosition">The scroll position.</param>
        public void ScrollTo(RowsScrollPosition rowsScrollPosition)
        {
            _viewport?.ScrollTo(rowsScrollPosition);
            RebuildContent();
        }

        #endregion

        #region View vertical resize
        /// <summary>
        /// Rows count in view
        /// </summary>
        private int _rowsCount;
        /// <summary>
        /// Gets or sets the rows count in view.
        /// </summary>
        public int RowsCount {
            get { return _rowsCount; }
            set
            {                
                if (_rowsCount == value)
                    return;
                _rowsCount = value;
                if (Document == null)
                    return; // Viewmodel not initialized

                _viewport.RowsCount = _rowsCount;                    
                RebuildContent();                
                OnPropertyChanged();
            }
        }

        #endregion

        #region view horizontal resize
        /// <summary>
        /// The symbols in row count
        /// </summary>
        private int _symbolsInRowCount;

        /// <summary>
        /// Gets or sets the symbols in row count.
        /// </summary>
        /// <value>
        /// The symbols in row count.
        /// </value>
        public int SymbolsInRowCount
        {
            get { return _symbolsInRowCount; }
            set
            {
                if (_symbolsInRowCount == value)
                    return;
                _symbolsInRowCount = value;
                if (Document == null)
                    return; // Viewmodel not initialized

                // Major control update. Viewport recreation.
                var lineBreaker = _moduleFactory.MakeLineBreaker(LineBreakerSymbolsInRowCount);
                _viewport = _moduleFactory.MakeViewport(Document, lineBreaker, _rowsCount, _viewport?.DocumentScrollPosition);
                RebuildContent();
                ScrollBarViewModel.Update(_viewport, LineBreakerSymbolsInRowCount);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Symbols count for lineBreaker is different then symbols row in count in cause of special symbols printing.
        /// </summary>
        /// <remarks>may reserve symbols for "invisible" paragraph tag and latest whitespace/paragraph</remarks>
        private int LineBreakerSymbolsInRowCount => SymbolsInRowCount - _contentWriter.ReservedSymbolsCount;         

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
