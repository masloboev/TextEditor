using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TextEditor.Attributes;
using TextEditor.Model;

namespace TextEditor
{
    /// <summary>
    ///     Builds document from stream: proceeds segmentation and creates document
    /// </summary>
    public class StreamDocumentBuilder : IDocumentBuilder
    {
        /// <summary>
        /// Loads the document asynchronously.
        /// </summary>
        /// <param name="moduleFactory">The module factory.</param>
        /// <param name="streamReader">The stream reader to load document text.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="progress">The progress.</param>
        /// <returns>
        /// Task with document
        /// </returns>
        [return: NotNull]
        public async Task<IDocument> LoadAsync([NotNull] IModuleFactory moduleFactory, [NotNull] StreamReader streamReader, 
            CancellationToken cancellationToken, IProgress<string> progress)
        {
            if (moduleFactory == null) throw new ArgumentNullException(nameof(moduleFactory));
            if (streamReader == null) throw new ArgumentNullException(nameof(streamReader));

            var segmentizer = moduleFactory.MakeSegmentizer(Constants.SegmentizerLowerThreshold, Constants.SegmentizerUpperThreshold, Constants.SegmentizerErrorThreshold);

            var segments = await segmentizer.SegmentAsync(streamReader, cancellationToken, progress);

            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report("Document building");
            return moduleFactory.MakeDocument(segments);
        }
    }
}
