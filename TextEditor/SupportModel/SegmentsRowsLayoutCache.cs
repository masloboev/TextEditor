using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TextEditor.Attributes;
using TextEditor.Model;

namespace TextEditor.SupportModel
{
    /// <summary>
    ///     Cache that contains and calculates SegmentsRowsLayout for each viewport with
    ///     SegmentsRowsLayout calculation is very slow, so I use cache, cancellation and progress
    /// </summary>
    public class SegmentsRowsLayoutCache : ISegmentsRowsLayoutProvider
    {
        /// <summary>
        ///     Structure to contain and calculate SegmentsRowsLayout for fixed viewport with
        /// </summary>
        private class SegmentsRowsLayoutCalculator
        {
            /// <summary>
            /// The document
            /// </summary>
            [NotNull]
            private readonly IDocument _document;
            /// <summary>
            /// The line breaker
            /// </summary>
            [NotNull]
            private readonly ILineBreaker _lineBreaker;
            /// <summary>
            /// The module factory
            /// </summary>
            [NotNull]
            private readonly IModuleFactory _moduleFactory;

            /// <summary>
            ///     Reference to the async task.
            ///     Is null before start and after cancellation
            /// </summary>
            public Task<ISegmentsRowsLayout> Task { get; private set; }

            /// <summary>
            /// The cancellation token source used to cancel calculation
            /// </summary>
            [NotNull]
            private CancellationTokenSource _cancellationTokenSource;

            /// <summary>
            /// The progress callback.
            /// </summary>
            [NotNull]
            public Progress<string> Progress { get; }

            /// <summary>
            ///     Task complete flag.
            /// </summary>
            public bool Done { get; private set; }

            /// <summary>
            /// Calculated segments rows layout.
            /// </summary>
            public ISegmentsRowsLayout SegmentsRowsLayout { get; private set; }

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="document">The document.</param>
            /// <param name="lineBreaker">The line breaker.</param>
            /// <param name="moduleFactory">The module factory.</param>
            /// <exception cref="ArgumentNullException">
            /// </exception>
            public SegmentsRowsLayoutCalculator([NotNull] IDocument document, [NotNull] ILineBreaker lineBreaker, [NotNull] IModuleFactory moduleFactory)
            {
                if (document == null) throw new ArgumentNullException(nameof(document));
                if (lineBreaker == null) throw new ArgumentNullException(nameof(lineBreaker));
                if (moduleFactory == null) throw new ArgumentNullException(nameof(moduleFactory));

                _moduleFactory = moduleFactory;
                _document = document;
                _lineBreaker = lineBreaker;
                Progress = new Progress<string>();
            }

            /// <summary>
            /// Internal start async SegmentsRowsLayout calculation.
            /// </summary>
            [return: NotNull]
            private Task<ISegmentsRowsLayout> InternalStartAsync()
            {
                _cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _cancellationTokenSource.Token;
                return System.Threading.Tasks.Task.Run(() => Calculate(cancellationToken, Progress), cancellationToken);
            }

            /// <summary>
            ///     Starts async SegmentsRowsLayout calculation and handle cancellation
            /// </summary>
            [return: NotNull]
            public async Task<ISegmentsRowsLayout> StartAsync()
            {
                try
                {
                    Task = InternalStartAsync();
                    SegmentsRowsLayout = await Task;
                    Done = true;
                    return SegmentsRowsLayout;
                }
                catch (OperationCanceledException)
                {
                    SegmentsRowsLayout = null;
                    Done = false;
                    Task = null;
                    throw;
                }             
            }

            /// <summary>
            ///     The SegmentsRowsLayout calculation method.
            /// </summary>
            private ISegmentsRowsLayout Calculate(CancellationToken cancellationToken, IProgress<string> progress)
            {
                var makeSegmentsRowsLayout = _moduleFactory.MakeSegmentsRowsLayout(_document.SegmentsCount);

                var appendedCount = 0;
                var segment = _document.FirstSegment;
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // I calculate only height of the segment in rows. Not rows, due to memory cost
                    var rowsCount = _lineBreaker.GetRowsCount(segment);
                    var newPosition = new SegmentRowsPosition(segment, rowsCount, makeSegmentsRowsLayout.TotalRowsCount);

                    makeSegmentsRowsLayout.Append(newPosition);
                    appendedCount++;

                    progress?.Report($"{_lineBreaker.SymbolsInRowCount} {appendedCount}/{_document.SegmentsCount}");
                    segment = _document.Next(segment);
                } while (segment != null);

                return makeSegmentsRowsLayout;
            }

            /// <summary>
            ///     Method cancels calculations.
            /// </summary>
            public void Cancel() => _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// The document
        /// </summary>
        [NotNull]
        private readonly IDocument _document;

        /// <summary>
        /// The module factory
        /// </summary>
        [NotNull]
        private readonly IModuleFactory _moduleFactory;

        /// <summary>
        ///     Calculator by viewport symbolsInRowCount map.
        /// </summary>
        [NotNull]
        readonly Dictionary<int, SegmentsRowsLayoutCalculator> _layoutBySymbolsInRowCountMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentsRowsLayoutCache" /> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="moduleFactory">The module factory.</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public SegmentsRowsLayoutCache([NotNull] IDocument document, [NotNull] IModuleFactory moduleFactory)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (moduleFactory == null) throw new ArgumentNullException(nameof(moduleFactory));

            _document = document;
            _moduleFactory = moduleFactory;
            _layoutBySymbolsInRowCountMap = new Dictionary<int, SegmentsRowsLayoutCalculator>();
        }

        /// <summary>
        ///     Reference to the actual calculator for current displaying viewport.
        /// </summary>
        private SegmentsRowsLayoutCalculator _latestSegmentsRowsLayoutCalculator;

        /// <summary>
        ///     Tries to get the precalulated layout for specified symbolsInRowCount .
        /// </summary>
        /// <param name="symbolsInRowCount">symbols in row count to break lines</param>
        /// <returns>if precalculated layout exists returns it</returns>
        public ISegmentsRowsLayout TryGet(int symbolsInRowCount)
        {
            UpdateLatestScrollbar(symbolsInRowCount);
            return _latestSegmentsRowsLayoutCalculator.Done ? _latestSegmentsRowsLayoutCalculator.SegmentsRowsLayout : null;
        }

        /// <summary>
        ///     Sets the actual calculator for current displaying viewport.
        /// </summary>
        /// <param name="symbolsInRowCount">symbols in row count to break lines</param>
        private void UpdateLatestScrollbar(int symbolsInRowCount)
        {
            SegmentsRowsLayoutCalculator segmentsRowsLayoutCalculator;
            if (!_layoutBySymbolsInRowCountMap.TryGetValue(symbolsInRowCount, out segmentsRowsLayoutCalculator))
            {
                segmentsRowsLayoutCalculator = new SegmentsRowsLayoutCalculator(_document, _moduleFactory.MakeLineBreaker(symbolsInRowCount), _moduleFactory);
                _layoutBySymbolsInRowCountMap.Add(symbolsInRowCount, segmentsRowsLayoutCalculator);
            }

            // if another calculation is in progress I just cancel it to user not wait for actual calculations
            if (_latestSegmentsRowsLayoutCalculator!= null && _latestSegmentsRowsLayoutCalculator != segmentsRowsLayoutCalculator)
                _latestSegmentsRowsLayoutCalculator.Cancel(); 

            _latestSegmentsRowsLayoutCalculator = segmentsRowsLayoutCalculator;
        }

        /// <summary>
        /// View model progress unsubscription method.
        /// </summary>
        /// <param name="symbolsInRowCount">viewports symbolsInRowCount</param>
        /// <param name="onProgress">The progress callback</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Unprogress(int symbolsInRowCount, [NotNull] EventHandler<string> onProgress)
        {
            if (onProgress == null) throw new ArgumentNullException(nameof(onProgress));

            SegmentsRowsLayoutCalculator segmentsRowsLayoutCalculator;
            if (!_layoutBySymbolsInRowCountMap.TryGetValue(symbolsInRowCount, out segmentsRowsLayoutCalculator))
                return;
            segmentsRowsLayoutCalculator.Progress.ProgressChanged -= onProgress;
        }

        /// <summary>
        ///     Initiation for async calculation with progress
        /// </summary>
        /// <param name="symbolsInRowCount">symbols in row count to break lines</param>
        /// <param name="onProgress">The progress callback</param>
        /// <returns>I return calculating task or start new task. Task creates and erases only in UI thread, so this code is safe</returns>
        [return: NotNull]
        public async Task<ISegmentsRowsLayout> GetAsync(int symbolsInRowCount, EventHandler<string> onProgress)
        {
            UpdateLatestScrollbar(symbolsInRowCount);
            if (onProgress != null)
                _latestSegmentsRowsLayoutCalculator.Progress.ProgressChanged += onProgress;
            return await (_latestSegmentsRowsLayoutCalculator.Task ?? _latestSegmentsRowsLayoutCalculator.StartAsync());
        }
    }
}
