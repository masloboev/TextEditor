using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TextEditor.Model;

namespace TextEditor.SupportModel
{
    /// <summary>
    ///     Cache that contains and calculates SegmentsRowsLayout for each viewport with
    ///     SegmentsRowsLayout calculation is very slow, so I use cache, cancellation and progress
    /// </summary>
    class SegmentsRowsLayoutCache
    {
        /// <summary>
        ///     Structure to contain and calculate SegmentsRowsLayout for fixed viewport with
        /// </summary>
        private class SegmentsRowsLayoutCalculator
        {
            private readonly Document _document;
            private readonly LineBreaker _lineBreaker;

            /// <summary>
            ///     Reference to the async task.
            ///     Is null before start and after cancellation
            /// </summary>
            public Task<SegmentsRowsLayout> Task { get; private set; }

            private CancellationTokenSource _cancellationTokenSource;

            public Progress<string> Progress { get; }

            /// <summary>
            ///     Task complete flag.
            /// </summary>
            public bool Done { get; private set; }

            public SegmentsRowsLayout SegmentsRowsLayout { get; private set; }

            public SegmentsRowsLayoutCalculator(Document document, LineBreaker lineBreaker)
            {
                _document = document;
                _lineBreaker = lineBreaker;
                Progress = new Progress<string>();
            }

            /// <summary>
            ///     Starts async SegmentsRowsLayout calculation.
            /// </summary>
            public Task<SegmentsRowsLayout> StartAsync()
            {
                _cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _cancellationTokenSource.Token;
                Task = System.Threading.Tasks.Task.Run(() => Calculate(cancellationToken, Progress), cancellationToken);
                // ReSharper disable once MethodSupportsCancellation due to we must continue even task cancelled
                Task = Task.ContinueWith(calculationTask =>
                                  { // Works in UI context
                                      if (calculationTask.IsCanceled)
                                      { // Cancelled. So reset rersult and done flag
                                          SegmentsRowsLayout = null;
                                          Done = false;
                                          Task = null;
                                      }
                                      else
                                      { // Calculation done. Remember.
                                          SegmentsRowsLayout = calculationTask.Result;
                                          Done = true;
                                      }
                                      return SegmentsRowsLayout;
                                  }, TaskScheduler.FromCurrentSynchronizationContext());
                return Task;
            }

            /// <summary>
            ///     The SegmentsRowsLayout calculation method.
            /// </summary>
            private SegmentsRowsLayout Calculate(CancellationToken cancellationToken, Progress<string> progress)
            {
                var dataForWidth = new SegmentsRowsLayout
                                   {
                                       TotalHeight = 0,
                                       PositionsByOffset = new List<SegmentsRowsLayout.SegmentRowsPosition>(_document.SegmentsCount),
                                       PositionsBySegment = new Dictionary<ISegment, SegmentsRowsLayout.SegmentRowsPosition>(_document.SegmentsCount)
                                   };

                var segment = _document.FirstSegment;
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // I calculate only height of the segment in rows. Not rows, due to memory cost
                    var rowsCount = _lineBreaker.GetRowsCount(segment);
                    var newPosition = new SegmentsRowsLayout.SegmentRowsPosition
                                       {
                                           RowsCount = rowsCount,
                                           Segment = segment,
                                           StartDocumentRowsOffset = dataForWidth.TotalHeight
                                       };
                    dataForWidth.PositionsByOffset.Add(newPosition);
                    dataForWidth.PositionsBySegment[segment] = newPosition;

                    dataForWidth.TotalHeight += rowsCount;
                    segment = _document.Next(segment);

                    ((IProgress<string>)progress).Report($"{_lineBreaker.Width} {dataForWidth.PositionsByOffset.Count}/{_document.SegmentsCount}");

                } while (segment != null);

                return dataForWidth;
            }

            /// <summary>
            ///     Method cancels calculations.
            /// </summary>
            public void Cancel() => _cancellationTokenSource.Cancel();
        }

        private readonly Document _document;

        /// <summary>
        ///     Calculator by viewport width map.
        /// </summary>
        readonly Dictionary<int, SegmentsRowsLayoutCalculator> _scrollbarWidthMap;

        public SegmentsRowsLayoutCache(Document document)
        {
            _document = document;
            _scrollbarWidthMap = new Dictionary<int, SegmentsRowsLayoutCalculator>();
        }

        /// <summary>
        ///     Reference to the actual calculator for current displaying viewport.
        /// </summary>
        private SegmentsRowsLayoutCalculator _latestSegmentsRowsLayoutCalculator;

        /// <summary>
        ///     Tries to get the precalulated layout for specified width .
        /// </summary>
        /// <param name="width">viewports width</param>
        /// <returns>if precalculated layout exists returns it</returns>
        public SegmentsRowsLayout TryGet(int width)
        {
            UpdateLatestScrollbarForWidth(width);
            return _latestSegmentsRowsLayoutCalculator.Done ? _latestSegmentsRowsLayoutCalculator.SegmentsRowsLayout : null;
        }

        /// <summary>
        ///     Sets the actual calculator for current displaying viewport.
        /// </summary>
        /// <param name="width">viewports width</param>
        private void UpdateLatestScrollbarForWidth(int width)
        {
            SegmentsRowsLayoutCalculator segmentsRowsLayoutCalculator;
            if (!_scrollbarWidthMap.TryGetValue(width, out segmentsRowsLayoutCalculator))
            {
                segmentsRowsLayoutCalculator = new SegmentsRowsLayoutCalculator(_document, new LineBreaker(width));
                _scrollbarWidthMap.Add(width, segmentsRowsLayoutCalculator);
            }

            // if another calculation is in progress I just cancel it to user not wait for actual calculations
            if (_latestSegmentsRowsLayoutCalculator!= null && _latestSegmentsRowsLayoutCalculator != segmentsRowsLayoutCalculator)
                _latestSegmentsRowsLayoutCalculator.Cancel(); 

            _latestSegmentsRowsLayoutCalculator = segmentsRowsLayoutCalculator;
        }

        /// <summary>
        ///     View model progress unsubscription method.
        /// </summary>
        public void Unprogress(int width, EventHandler<string> onProgress)
        {
            SegmentsRowsLayoutCalculator segmentsRowsLayoutCalculator;
            if (!_scrollbarWidthMap.TryGetValue(width, out segmentsRowsLayoutCalculator))
                return;
            segmentsRowsLayoutCalculator.Progress.ProgressChanged -= onProgress;
        }

        /// <summary>
        ///     Initiation for async calculation with progress
        /// </summary>
        /// <returns>I return calculating task or start new task. Task creates and erases only in UI thread, so this code is safe</returns>
        public async Task<SegmentsRowsLayout> GetAsync(int width, EventHandler<string> onProgress)
        {
            UpdateLatestScrollbarForWidth(width);
            _latestSegmentsRowsLayoutCalculator.Progress.ProgressChanged += onProgress;
            return await (_latestSegmentsRowsLayoutCalculator.Task ?? _latestSegmentsRowsLayoutCalculator.StartAsync());
        }
    }
}
