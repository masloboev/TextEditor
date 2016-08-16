using System;
using System.Collections.Generic;
using TextEditor.Attributes;
using TextEditor.Model;
using TextEditor.SupportModel;
using TextEditor.ViewModel;

namespace TextEditor
{
    /// <summary>
    /// Concrete module factory
    /// </summary>
    public class ModuleFactory : IModuleFactory
    {
        /// <summary>
        /// Makes the file document builder.
        /// </summary>
        /// <returns></returns>
        [return: NotNull]
        public IDocumentBuilder MakeDocumentBuilder() => new StreamDocumentBuilder();

        /// <summary>
        /// Makes the document.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns>
        /// The Document
        /// </returns>
        [return: NotNull]
        public IDocument MakeDocument([NotNull] List<ISegment> segments)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));

            return new Document(segments);
        }

        /// <summary>
        /// Makes the segmentizer.
        /// </summary>
        /// <param name="lowerThreshold">The lower threshold.</param>
        /// <param name="upperThreshold">The upper threshold.</param>
        /// <param name="errorThreshold">The error threshold.</param>
        /// <returns>
        /// Segmentizer
        /// </returns>
        [return: NotNull]
        public ISegmentizer MakeSegmentizer(int lowerThreshold, int upperThreshold, int errorThreshold) 
            => new Segmentizer(lowerThreshold, upperThreshold, errorThreshold);

        /// <summary>
        /// Makes the SegmentViewModel.
        /// </summary>
        /// <param name="lineBreaker">The line breaker.</param>
        /// <param name="segment">The segment.</param>
        /// <returns>
        /// SegmentViewModel
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        [return: NotNull]
        public ISegmentViewModel MakeSegmentViewModel([NotNull] ILineBreaker lineBreaker, [NotNull] ISegment segment)
        {
            if (lineBreaker == null) throw new ArgumentNullException(nameof(lineBreaker));
            if (segment == null) throw new ArgumentNullException(nameof(segment));

            return new SegmentViewModel(lineBreaker, segment);
        }

        /// <summary>
        /// Makes the line breaker.
        /// </summary>
        /// <param name="symbolsInRowCount">Width in symbols to break line.</param>
        /// <returns></returns>
        [return: NotNull]
        public ILineBreaker MakeLineBreaker(int symbolsInRowCount) => new LineBreaker(symbolsInRowCount);

        /// <summary>
        /// Makes the SegmentsRowsLayout.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <returns>
        /// SegmentsRowsLayout
        /// </returns>
        [return: NotNull]
        public ISegmentsRowsLayout MakeSegmentsRowsLayout(int capacity) => new SegmentsRowsLayout(capacity);

        /// <summary>
        /// Makes the SegmentsRowsLayoutProvider.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// SegmentsRowsLayoutProvider
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        [return: NotNull]
        public ISegmentsRowsLayoutProvider MakeSegmentsRowsLayoutProvider([NotNull] IDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            return new SegmentsRowsLayoutCache(document, this);
        }

        /// <summary>
        /// Makes the viewport.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="lineBreaker">The line breaker.</param>
        /// <param name="rowsCount">The rows count in view.</param>
        /// <param name="documentScrollPosition">The document scroll position.</param>
        /// <returns>
        /// Viewport
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        [return: NotNull]
        public IViewport MakeViewport([NotNull] IDocument document, [NotNull] ILineBreaker lineBreaker, int rowsCount, DocumentScrollPosition? documentScrollPosition)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (lineBreaker == null) throw new ArgumentNullException(nameof(lineBreaker));

            return new Viewport(document, this, lineBreaker, rowsCount, documentScrollPosition);
        }

        /// <summary>
        /// Makes the scroll bar view model.
        /// </summary>
        /// <returns>ScrollBarViewModel</returns>
        [return: NotNull]
        public IScrollBarViewModel MakeScrollBarViewModel() => new ScrollBarViewModel();

        /// <summary>
        /// Makes the text view model.
        /// </summary>
        /// <returns>TextViewModel</returns>
        [return: NotNull]
        public ITextViewModel MakeTextViewModel() => new TextViewModel();

        /// <summary>
        /// Makes the main window view model.
        /// </summary>
        /// <returns>MainWindowViewModel</returns>
        [return: NotNull]
        public IMainWindowViewModel MakeMainWindowViewModel() => new MainWindowViewModel();

        /// <summary>
        /// Makes the content writer.
        /// </summary>
        /// <returns>ContentWriter</returns>
        [return: NotNull]
        public IContentWriter MakeContentWriter() => new ContentWriter();
    }
}
