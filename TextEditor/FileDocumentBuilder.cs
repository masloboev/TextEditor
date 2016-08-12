using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TextEditor.Model;

namespace TextEditor
{
    /// <summary>
    ///     Simple document builder: Opens file streams, proceeds segmentation and creates document
    /// </summary>
    public class FileDocumentBuilder
    {
        public async Task<Document> LoadAsync(string path, CancellationToken cancellationToken, Progress<string> progress)
        {
            var segmentizer = new Segmentizer(Constants.SegmentizerLowerThreshold, Constants.SegmentizerUpperThreshold);
            List<ISegment> segments;
            using (var fileStream = new StreamReader(path))
            {
                segments = await segmentizer.SegmentAsync(fileStream, cancellationToken, progress);
            }
            cancellationToken.ThrowIfCancellationRequested();
            ((IProgress<string>)progress).Report("Document building");
            return new Document(segments);
        }
    }
}
