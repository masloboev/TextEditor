using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TextEditor.Attributes;

namespace TextEditor.Model
{
    /// <summary>
    ///     Interface for algorithm to segmentize input stream into document segments.
    /// </summary>
    public interface ISegmentizer
    {
        /// <summary>
        /// Asynchronous method that separates input stream into segments
        /// </summary>
        /// <param name="streamReader">input stream</param>
        /// <param name="cancellationToken">token to cancel the operation</param>
        /// <param name="progress">progress callback structure</param>
        /// <returns>
        /// segments list async task
        /// </returns>
        [return: NotNull]
        Task<List<ISegment>> SegmentAsync([NotNull] StreamReader streamReader, CancellationToken cancellationToken, IProgress<string> progress);
    }
}
