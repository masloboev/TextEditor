using System.Collections.Generic;
using TextEditor.Attributes;
using TextEditor.Model;
using TextEditor.SupportModel;
using TextEditor.ViewModel;

namespace TextEditor
{
    /// <summary>
    /// Module components factory
    /// </summary>
    public interface IModuleFactory
    {
        /// <summary>
        /// Makes the file document builder.
        /// see <see cref="IDocumentBuilder" />
        /// </summary>
        /// <returns></returns>
        [return: NotNull]
        IDocumentBuilder MakeDocumentBuilder();

        /// <summary>
        /// Makes the document.
        /// see <see cref="IDocument" />
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns>The Document</returns>
        [return: NotNull]
        IDocument MakeDocument(List<ISegment> segments);

        /// <summary>
        /// Makes the segmentizer.
        /// see <see cref="ISegmentizer" />
        /// </summary>
        /// <param name="lowerThreshold">The lower threshold.</param>
        /// <param name="upperThreshold">The upper threshold.</param>
        /// <param name="errorThreshold">The error threshold.</param>
        /// <returns>Segmentizer</returns>
        [return: NotNull]
        ISegmentizer MakeSegmentizer(int lowerThreshold, int upperThreshold, int errorThreshold);

        /// <summary>
        /// Makes the SegmentViewModel.
        /// see <see cref="ISegmentViewModel" />
        /// </summary>
        /// <param name="lineBreaker">The line breaker.</param>
        /// <param name="segment">The segment.</param>
        /// <returns>SegmentViewModel</returns>
        [return: NotNull]
        ISegmentViewModel MakeSegmentViewModel([NotNull] ILineBreaker lineBreaker, [NotNull] ISegment segment);

        /// <summary>
        /// Makes the line breaker.
        /// see <see cref="ILineBreaker" />
        /// </summary>
        /// <param name="symbolsInRowCount">Width in symbols to break line.</param>
        /// <returns>LineBreaker</returns>
        [return: NotNull]
        ILineBreaker MakeLineBreaker(int symbolsInRowCount);

        /// <summary>
        /// Makes the SegmentsRowsLayout.
        /// see <see cref="ISegmentsRowsLayout" />
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <returns>SegmentsRowsLayout</returns>
        [return: NotNull]
        ISegmentsRowsLayout MakeSegmentsRowsLayout(int capacity);

        /// <summary>
        /// Makes the SegmentsRowsLayoutProvider.
        /// see <see cref="ISegmentsRowsLayoutProvider" />
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>SegmentsRowsLayoutProvider</returns>
        [return: NotNull]
        ISegmentsRowsLayoutProvider MakeSegmentsRowsLayoutProvider([NotNull] IDocument document);

        /// <summary>
        /// Makes the viewport.
        /// see <see cref="IViewport" />
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="lineBreaker">The line breaker.</param>
        /// <param name="rowsCount">The rows count in view.</param>
        /// <param name="documentScrollPosition">The document scroll position.</param>
        /// <returns>Viewport</returns>
        [return: NotNull]
        IViewport MakeViewport([NotNull] IDocument document, [NotNull] ILineBreaker lineBreaker, int rowsCount, DocumentScrollPosition? documentScrollPosition);

        /// <summary>
        /// Makes the scroll bar view model.
        /// see <see cref="IScrollBarViewModel" />
        /// </summary>
        /// <returns>ScrollBarViewModel</returns>
        [return: NotNull]
        IScrollBarViewModel MakeScrollBarViewModel();

        /// <summary>
        /// Makes the text view model.
        /// see <see cref="ITextViewModel" />
        /// </summary>
        /// <returns>TextViewModel</returns>
        [return: NotNull]
        ITextViewModel MakeTextViewModel();

        /// <summary>
        /// Makes the main window view model.
        /// see <see cref="MakeMainWindowViewModel" />
        /// </summary>
        /// <returns>MainWindowViewModel</returns>
        [return: NotNull]
        IMainWindowViewModel MakeMainWindowViewModel();

        /// <summary>
        /// Makes the content writer.
        /// see <see cref="MakeContentWriter" />
        /// </summary>
        /// <returns>ContentWriter</returns>
        [return: NotNull]
        IContentWriter MakeContentWriter();
    }
}
