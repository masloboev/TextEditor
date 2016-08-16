using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TextEditor.Attributes;
using TextEditor.Model;

namespace TextEditor
{
    /// <summary>
    ///     Interface for document building
    /// </summary>
    public interface IDocumentBuilder
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
        Task<IDocument> LoadAsync([NotNull] IModuleFactory moduleFactory, [NotNull] StreamReader streamReader,
            CancellationToken cancellationToken, IProgress<string> progress);
    }
}
