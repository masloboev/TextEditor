using System;
using System.Collections.Generic;
using System.Linq;
using TextEditor.Attributes;
using TextEditor.Model;
using TextEditor.ViewModel;

namespace TextEditor.SupportModel
{
    /// <summary>
    ///     Model to present document part in control. Base idea is viewport that can move up and down over the document
    /// </summary>
    public class Viewport : IViewport
    {
        /// <summary>
        /// The document is viewported.
        /// </summary>
        [NotNull]
        private readonly IDocument _document;

        /// <summary>
        /// The module factory
        /// </summary>
        [NotNull]
        private readonly IModuleFactory _moduleFactory;

        /// <summary>
        /// The line breaker
        /// </summary>
        [NotNull]
        private readonly ILineBreaker _lineBreaker;

        /// <summary>
        ///     How many rows of first segment is scrolled before viewport
        /// </summary>
        private int RowsBeforeScrollCount { get; set; }

        /// <summary>
        ///     How many rows of last segment is scrolled after viewport
        /// </summary>
        private int RowsAfterScrollCount => _segmentViewModelsRowsCount - RowsBeforeScrollCount - _rowsCount;

        /// <summary>
        ///     rows count that can be viewed with current calculated segments list
        /// </summary>
        private int _segmentViewModelsRowsCount;

        /// <summary>
        ///     first calculated segment in the list
        /// </summary>
        [NotNull]
        private ISegmentViewModel FirstSegmentViewModel => SegmentViewModels[0];

        /// <summary>
        ///     last calculated segment in the list
        /// </summary>
        [NotNull]
        private ISegmentViewModel LastSegmentViewModel => SegmentViewModels[SegmentViewModels.Count - 1];

        /// <summary>
        ///     calculated segments list
        /// </summary>
        [NotNull]
        public List<ISegmentViewModel> SegmentViewModels;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="document">The document is viewported</param>
        /// <param name="moduleFactory">The module factory.</param>
        /// <param name="lineBreaker">The line breaker.</param>
        /// <param name="rowsCount">current rows count in the control</param>
        /// <param name="documentScrollPosition">The document scroll position.</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Viewport([NotNull] IDocument document, [NotNull] IModuleFactory moduleFactory, ILineBreaker lineBreaker, 
            int rowsCount, DocumentScrollPosition? documentScrollPosition)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (moduleFactory == null) throw new ArgumentNullException(nameof(moduleFactory));
            if (lineBreaker == null) throw new ArgumentNullException(nameof(lineBreaker));
            if (rowsCount < 0) throw new ArgumentOutOfRangeException(nameof(rowsCount));

            _document = document;
            _lineBreaker = lineBreaker;
            _moduleFactory = moduleFactory;

            _rowsCount = rowsCount;
            RowsBeforeScrollCount = 0;

            // first visible segment selection
            var firstSegment = documentScrollPosition.HasValue ? documentScrollPosition.Value.FirstSegment : _document.FirstSegment;

            _segmentViewModelsRowsCount = 0;
            var segmentViewModel = _moduleFactory.MakeSegmentViewModel(_lineBreaker, firstSegment);
            SegmentViewModels = new List<ISegmentViewModel> { segmentViewModel };
            _segmentViewModelsRowsCount += segmentViewModel.RowsCount;

            if (documentScrollPosition.HasValue)
            { // I find the position of row in segment to adjust new scroll position to the most same line
              // If i didn't found it I just reset to the begin of first segment
                var textPosition = documentScrollPosition.Value.SegmentOffset;
                for (var i = 0; i < segmentViewModel.RowsCount; i++)
                    if (segmentViewModel.Rows[i].BeginPosition + segmentViewModel.Rows[i].Length > textPosition)
                    {
                        RowsBeforeScrollCount = i;
                        break;
                    }
            }

            Fix();
        }

        /// <summary>
        ///     Method cleanups invisible calculated segments that was scrolled up or down.
        ///     This segments can be cached, but now it just forgotten
        /// </summary>
        private void CleanUp()
        {
            while (RowsBeforeScrollCount >= FirstSegmentViewModel.RowsCount && SegmentViewModels.Count > 1)
            { // cleanup segements before viewport
                RowsBeforeScrollCount -= FirstSegmentViewModel.RowsCount;
                _segmentViewModelsRowsCount -= FirstSegmentViewModel.RowsCount;
                SegmentViewModels.RemoveAt(0);
            }
            while (RowsAfterScrollCount >= LastSegmentViewModel.RowsCount && SegmentViewModels.Count > 1)
            {// cleanup segements after viewport
                _segmentViewModelsRowsCount -= LastSegmentViewModel.RowsCount;
                SegmentViewModels.RemoveAt(SegmentViewModels.Count - 1);
            }
        }

        /// <summary>
        ///     Method fixes the scroll position and calculates neccessary segments to present in viewport.
        /// </summary>
        private void Fix()
        {
            if (RowsBeforeScrollCount < 0)
            { // We need segments in the beginning of viewport
                var currentSegment = _document.Prev(FirstSegmentViewModel.Segment);
                while (currentSegment != null)
                {
                    var segmentViewModel = _moduleFactory.MakeSegmentViewModel(_lineBreaker, currentSegment);
                    SegmentViewModels.Insert(0, segmentViewModel);
                    _segmentViewModelsRowsCount += segmentViewModel.RowsCount;
                    RowsBeforeScrollCount += segmentViewModel.RowsCount;

                    if (RowsBeforeScrollCount >= 0)
                    {
                        CleanUp();
                        return;
                    }

                    currentSegment = _document.Prev(currentSegment);
                }
                // We are on the first symbol of the document
                RowsBeforeScrollCount = 0;
            }

            if (RowsAfterScrollCount < 0)
            {
                // We need segments in the end of viewport
                var currentSegment = _document.Next(LastSegmentViewModel.Segment);
                while (currentSegment != null)
                {
                    var segmentViewModel = _moduleFactory.MakeSegmentViewModel(_lineBreaker, currentSegment);
                    SegmentViewModels.Add(segmentViewModel);
                    _segmentViewModelsRowsCount += segmentViewModel.RowsCount;

                    if (RowsAfterScrollCount >= 0)
                    {
                        CleanUp();
                        return;
                    }
                    currentSegment = _document.Next(currentSegment);
                }
                // Not anought segments to fill the end part of the viewport
                // Try to move and fill begin of the begin part of the viewport
                RowsBeforeScrollCount = _segmentViewModelsRowsCount - _rowsCount;
                if (RowsBeforeScrollCount < 0)
                    if (FirstSegmentViewModel.Segment == _document.FirstSegment)
                        RowsBeforeScrollCount = 0; // No data on begin. Just scroll
                    else
                    {
                        // ReSharper disable once TailRecursiveCall No Recursion. Just one more call
                        Fix(); //Move and fix from beginning
                        return;
                    }
            }
            CleanUp();
        }

        /// <summary>
        /// Method makes a text to present in the control.
        /// </summary>
        /// <param name="contentWriter">The content writer.</param>
        /// <param name="symbolsInRowCount">TThe view symbols in row count.</param>
        /// <returns>
        /// viewport content
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [return: NotNull]
        public string MakeContent([NotNull] IContentWriter contentWriter, int symbolsInRowCount)
        {
            if (contentWriter == null) throw new ArgumentNullException(nameof(contentWriter));
            if (symbolsInRowCount <= 0) throw new ArgumentOutOfRangeException(nameof(symbolsInRowCount));

            return contentWriter.MakeContent(RowsBeforeScrollCount, SegmentViewModels, Math.Min(_rowsCount, _segmentViewModelsRowsCount - RowsBeforeScrollCount), symbolsInRowCount);
        }

        #region Scroll

        /// <summary>
        ///     Event that indicates that scrollinformation was changed (Neeeded for ScrollBarViewModel)
        /// </summary>
        public event Action<RowsScrollPosition> EvScrollPositionChanged;

        /// <summary>
        ///     Gets the current scroll position ov the viewport
        /// </summary>
        [NotNull]
        public RowsScrollPosition RowsScrollPosition => 
            new RowsScrollPosition {FirstSegment = FirstSegmentViewModel.Segment, RowsBeforeScrollCount = RowsBeforeScrollCount };

        /// <summary>
        /// Document scroll poistion
        /// </summary>
        [NotNull]
        public DocumentScrollPosition DocumentScrollPosition => 
            new DocumentScrollPosition {FirstSegment = FirstSegmentViewModel.Segment, SegmentOffset = FirstSegmentViewModel.Rows[RowsBeforeScrollCount].BeginPosition };

        /// <summary>
        ///     Scrolls to specified position (By scrollbar)
        /// </summary>
        public void ScrollTo(RowsScrollPosition rowsScrollPosition)
        {
            var skipSegmentsCount = 0;
            var segmentFound = false;
            // first whe suppose near scroll and we already can contain the proper calculated segments
            foreach (var segmentViewModel in SegmentViewModels)
                if (segmentViewModel.Segment == rowsScrollPosition.FirstSegment)
                {
                    if (skipSegmentsCount > 0)
                    {
                        _segmentViewModelsRowsCount -= SegmentViewModels.Take(skipSegmentsCount).Sum(vm => vm.RowsCount);
                        SegmentViewModels.RemoveRange(0, skipSegmentsCount);
                    }
                    segmentFound = true;
                    break;
                }
                else
                    skipSegmentsCount++;

            if (!segmentFound)
            {
                // It's far scroll. Just reset and rebuild viewport
                SegmentViewModels.Clear();
                SegmentViewModels.Add(_moduleFactory.MakeSegmentViewModel(_lineBreaker, rowsScrollPosition.FirstSegment));
                _segmentViewModelsRowsCount = FirstSegmentViewModel.RowsCount;
            }
            RowsBeforeScrollCount = rowsScrollPosition.RowsBeforeScrollCount;
            Fix();
            EvScrollPositionChanged?.Invoke(RowsScrollPosition);
        }

        /// <remarks>
        /// We can scroll down when have rows after viewport or not calculated segemtns in document
        /// </remarks>
        public bool CanScrollDown() => RowsAfterScrollCount > 0 || LastSegmentViewModel.Segment != _document.LastSegment;

        /// <summary>
        /// Scrolls down for specified distance in rows
        /// </summary>
        public void ScrollDown(int distance)
        {
            RowsBeforeScrollCount += distance;
            Fix();
            EvScrollPositionChanged?.Invoke(RowsScrollPosition);
        }

        /// <remarks>
        /// We can scroll up when have rows before viewport or not calculated segemtns in document
        /// </remarks>
        public bool CanScrollUp() => RowsBeforeScrollCount > 0 || FirstSegmentViewModel.Segment != _document.FirstSegment;

        /// <summary>
        /// Scrolls up for specified distance in rows
        /// </summary>
        public void ScrollUp(int distance)
        {
            RowsBeforeScrollCount -= distance;
            Fix();
            EvScrollPositionChanged?.Invoke(RowsScrollPosition);
        }

        #endregion

        #region View vertical resize
        /// <summary>
        /// Height of viewport in rows
        /// </summary>
        private int _rowsCount;

        /// <summary>
        /// Height of viewport in rows
        /// </summary>
        public int RowsCount
        {
            get { return _rowsCount; }
            set
            {
                if (_rowsCount == value)
                    return;
                SetRowsCount(value);
            }
        }

        /// <summary>
        /// Resize viewport method
        /// </summary>
        /// <param name="rowsCount">new size of viewport in rows</param>
        private void SetRowsCount(int rowsCount)
        {
            if (rowsCount < 0) throw new ArgumentOutOfRangeException(nameof(rowsCount));

            _rowsCount = rowsCount;
            Fix();
            EvScrollPositionChanged?.Invoke(RowsScrollPosition);
        }

        #endregion
    }
}
