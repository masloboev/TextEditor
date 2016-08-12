using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextEditor.ViewModel;

namespace TextEditor.SupportModel
{
    /// <summary>
    ///     Model to present document part in control. Base idea is viewport that can move up and down over the document
    /// </summary>
    public class Viewport
    {
        private readonly TextViewModel _textViewModel;

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
        private SegmentViewModel FirstSegmentViewModel => SegmentViewModels[0];

        /// <summary>
        ///     last calculated segment in the list
        /// </summary>
        private SegmentViewModel LastSegmentViewModel => SegmentViewModels[SegmentViewModels.Count - 1];

        /// <summary>
        ///     calculated segments list
        /// </summary>
        public List<SegmentViewModel> SegmentViewModels;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="textViewModel">controls view model</param>
        /// <param name="height">current height of the control in letters</param>
        /// <param name="prevViewPort">reference for previous viewport to adjust scroll position</param>
        public Viewport(TextViewModel textViewModel, int height, Viewport prevViewPort)
        {
            _textViewModel = textViewModel;
            _rowsCount = height;
            RowsBeforeScrollCount = 0;

            // first visible segment selection
            var firstSegment = prevViewPort != null ? prevViewPort.FirstSegmentViewModel.Segment : _textViewModel.Document.FirstSegment;

            _segmentViewModelsRowsCount = 0;
            var segmentViewModel = new SegmentViewModel(_textViewModel.LineBreaker, firstSegment);
            SegmentViewModels = new List<SegmentViewModel> { segmentViewModel };
            _segmentViewModelsRowsCount += segmentViewModel.RowsCount;

            if (prevViewPort != null)
            { // I find the position of row in segment to adjust new scroll position to the most same line
              // If i didn't found it I just reset to the begin of first segment
                var textPosition = prevViewPort.FirstSegmentViewModel.Rows[prevViewPort.RowsBeforeScrollCount].BeginPosition;
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
            while (RowsBeforeScrollCount > FirstSegmentViewModel.RowsCount)
            { // cleanup segements before viewport
                RowsBeforeScrollCount -= FirstSegmentViewModel.RowsCount;
                _segmentViewModelsRowsCount -= FirstSegmentViewModel.RowsCount;
                SegmentViewModels.RemoveAt(0);
            }
            while (RowsAfterScrollCount > LastSegmentViewModel.RowsCount)
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
                var currentSegment = _textViewModel.Document.Prev(FirstSegmentViewModel.Segment);
                while (currentSegment != null)
                {
                    var segmentViewModel = new SegmentViewModel(_textViewModel.LineBreaker, currentSegment);
                    SegmentViewModels.Insert(0, segmentViewModel);
                    _segmentViewModelsRowsCount += segmentViewModel.RowsCount;
                    RowsBeforeScrollCount += segmentViewModel.RowsCount;

                    if (RowsBeforeScrollCount >= 0)
                    {
                        CleanUp();
                        return;
                    }

                    currentSegment = _textViewModel.Document.Prev(currentSegment);
                }
                // We are on the first symbol of the document
                RowsBeforeScrollCount = 0;
            }

            if (RowsAfterScrollCount < 0)
            {
                // We need segments in the end of viewport
                var currentSegment = _textViewModel.Document.Next(LastSegmentViewModel.Segment);
                while (currentSegment != null)
                {
                    var segmentViewModel = new SegmentViewModel(_textViewModel.LineBreaker, currentSegment);
                    SegmentViewModels.Add(segmentViewModel);
                    _segmentViewModelsRowsCount += segmentViewModel.RowsCount;

                    if (RowsAfterScrollCount >= 0)
                    {
                        CleanUp();
                        return;
                    }
                    currentSegment = _textViewModel.Document.Next(currentSegment);
                }
                // Not anought segments to fill the end part of the viewport
                // Try to move and fill begin of the begin part of the viewport
                RowsBeforeScrollCount = _segmentViewModelsRowsCount - _rowsCount;
                if (RowsBeforeScrollCount < 0)
                    if (FirstSegmentViewModel.Segment == _textViewModel.Document.FirstSegment)
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
        ///     Method makes a text to present in the control.
        /// </summary>
        public string MakeContent()
        {
            var sb = new StringBuilder(_rowsCount * _textViewModel.SymbolsInRowCount); // Maximum symbols count in content
            var segmentOffset = RowsBeforeScrollCount;
            var rowsCount = 0;
            foreach (var segmentViewModel in SegmentViewModels)
            {
                var rowsToWriteCount = Math.Min(_rowsCount - rowsCount, segmentViewModel.RowsCount - segmentOffset);
                segmentViewModel.WriteContent(_textViewModel.RowWriter, sb, segmentOffset, rowsToWriteCount);
                rowsCount += rowsToWriteCount;
                if (rowsCount > _rowsCount)
                    break;
                segmentOffset = 0;
            }
            return sb.ToString();
        }


        #region Scroll

        /// <summary>
        ///     Event that indicates that scrollinformation was changed (Neeeded for ScrollBarViewModel)
        /// </summary>
        public event Action<ScrollPosition> EvScrollPositionChanged;

        /// <summary>
        ///     Gets the current scroll position ov the viewport
        /// </summary>
        public ScrollPosition ScrollPosition => 
            new ScrollPosition {FirstSegment = FirstSegmentViewModel.Segment, RowsBeforeScrollCount = RowsBeforeScrollCount };

        /// <summary>
        ///     Scrolls to specified position (By scrollbar)
        /// </summary>
        public void ScrollTo(ScrollPosition scrollPosition)
        {
            var skipSegmentsCount = 0;
            var segmentFound = false;
            // first whe suppose near scroll and we already can contain the proper calculated segments
            foreach (var segmentViewModel in SegmentViewModels)
                if (segmentViewModel.Segment == scrollPosition.FirstSegment)
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
                SegmentViewModels.Add(new SegmentViewModel(_textViewModel.LineBreaker, scrollPosition.FirstSegment));
                _segmentViewModelsRowsCount = FirstSegmentViewModel.RowsCount;
            }
            RowsBeforeScrollCount = scrollPosition.RowsBeforeScrollCount;
            Fix();
            EvScrollPositionChanged?.Invoke(ScrollPosition);
        }

        /// <remarks>
        /// We can scroll down when have rows after viewport or not calculated segemtns in document
        /// </remarks>
        public bool CanScrollDown() => RowsAfterScrollCount > 0 || LastSegmentViewModel.Segment != _textViewModel.Document.LastSegment;

        /// <summary>
        /// Scrolls down for specified distance in rows
        /// </summary>
        public void ScrollDown(int distance)
        {
            RowsBeforeScrollCount += distance;
            Fix();
            EvScrollPositionChanged?.Invoke(ScrollPosition);
        }

        /// <remarks>
        /// We can scroll up when have rows before viewport or not calculated segemtns in document
        /// </remarks>
        public bool CanScrollUp() => RowsBeforeScrollCount > 0 || FirstSegmentViewModel.Segment != _textViewModel.Document.FirstSegment;

        /// <summary>
        /// Scrolls up for specified distance in rows
        /// </summary>
        public void ScrollUp(int distance)
        {
            RowsBeforeScrollCount -= distance;
            Fix();
            EvScrollPositionChanged?.Invoke(ScrollPosition);
        }

        #endregion

        #region View vertical resize
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
            _rowsCount = rowsCount;
            if (SegmentViewModels == null)
                return;
            Fix();
            EvScrollPositionChanged?.Invoke(ScrollPosition);
        }

        #endregion
    }
}
