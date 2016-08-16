using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TextEditor.Attributes;
using TextEditor.Exceptions;

namespace TextEditor.Model
{
    /// <summary>
    ///     Class implements algorithm to segmentize input stream into document segments.
    /// </summary>
    public class Segmentizer : ISegmentizer
    {
        /// <summary>
        ///     Segmest should be greather than this threshold
        /// </summary>
        private readonly int _lowerThreshold;

        /// <summary>
        ///     Segmest should be lower than this threshold
        /// </summary>
        private readonly int _upperThreshold;

        /// <summary>
        ///     Segmest is larger then this threshold TooBigWordException will be thrown 
        ///     <see cref="TooBigWordException"/>
        /// </summary>
        private readonly int _errorThreshold;

        /// <summary>
        /// Initializes a new instance of the <see cref="Segmentizer"/> class.
        /// </summary>
        /// <param name="lowerThreshold">The lower threshold.</param>
        /// <param name="upperThreshold">The upper threshold.</param>
        /// <param name="errorThreshold">The error threshold.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public Segmentizer(int lowerThreshold, int upperThreshold, int errorThreshold)
        {
            if (lowerThreshold < 0) throw new ArgumentOutOfRangeException(nameof(lowerThreshold));
            if (upperThreshold <= lowerThreshold) throw new ArgumentOutOfRangeException(nameof(upperThreshold));
            if (errorThreshold <= upperThreshold) throw new ArgumentOutOfRangeException(nameof(errorThreshold));

            _lowerThreshold = lowerThreshold;
            _upperThreshold = upperThreshold;
            _errorThreshold = errorThreshold;
        }

        /// <summary>
        /// Method fills the prepared buffer from input stream
        /// </summary>
        /// <param name="streamReader">input stream</param>
        /// <param name="buffer">buffer to fill in</param>
        /// <param name="endPosition">reference to the end position of written data</param>
        /// <returns>
        /// Availability to try read another stream part
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static bool FillBuffer([NotNull] TextReader streamReader, [NotNull] char[] buffer, ref int endPosition)
        {
            if (streamReader == null) throw new ArgumentNullException(nameof(streamReader));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (endPosition < 0 || endPosition >= buffer.Length) throw new ArgumentOutOfRangeException(nameof(endPosition));

            var charsToReadCount = buffer.Length - endPosition;
            while (charsToReadCount > 0)
            {
                if (charsToReadCount > Constants.FileLoaderBatchSize) // the read portion limitation
                    charsToReadCount = Constants.FileLoaderBatchSize;

                var readCount = streamReader.Read(buffer, endPosition, charsToReadCount);
                if (readCount == 0)
                    return false;
                endPosition += readCount;
                charsToReadCount = buffer.Length - endPosition;
            }
            return true;
        }

        /// <summary>
        /// Method flushes current readonly segment to segments list and flushes buffer.
        /// Method is too complex for "Segment" method text size and execution speed optimization
        /// </summary>
        /// <param name="data">buffer that segment references</param>
        /// <param name="bufferEndPosition">position in buffer that is flushed too</param>
        /// <param name="segments">building segments list</param>
        /// <param name="endPosition">endposition of the segment in buffer</param>
        /// <param name="isMonoWord">segment paramter</param>
        /// <param name="endsWithNewLine">segment paramter</param>
        /// <returns>
        /// length of the segment
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static int AddSegment([NotNull] char[] data, ref int bufferEndPosition,  ICollection<ISegment> segments, int endPosition, bool isMonoWord, bool endsWithNewLine)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            if (endPosition >= data.Length) throw new ArgumentOutOfRangeException(nameof(endPosition));

            var length = endPosition + 1;
            var segmentData = new char[length];
            Array.Copy(data, segmentData, length);
            segments.Add(new ReadonlySegment(segmentData, 0, length - 1, isMonoWord, endsWithNewLine));
            bufferEndPosition -= length;
            Array.Copy(data, length, data, 0, bufferEndPosition);

            return length;
        }

        /// <summary>
        /// Method that separates input stream into segments
        /// </summary>
        /// <param name="streamReader">input stream</param>
        /// <param name="cancellationToken">token to cancel the operation</param>
        /// <param name="progress">progress callback structure</param>
        /// <returns>
        /// segments list
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="TooBigWordException"></exception>
        [return: NotNull]
        private List<ISegment> Segment([NotNull] TextReader streamReader, CancellationToken cancellationToken, IProgress<string> progress)
        {
            if (streamReader == null) throw new ArgumentNullException(nameof(streamReader));

            var segments = new List<ISegment>();
            // the position of latest found whitespace
            int? rightmostSpacePosition = null;
            // the position of latest found paragraph end
            int? rightmostParagraphPosition = null;

            // the position of the end in the input buffer
            var bufferEndPosition = 0;
            var buffer = new char[_upperThreshold];
            var moreDataAvailable = FillBuffer(streamReader, buffer, ref bufferEndPosition);

            var latestReported = 0L;
            var totalHandled = 0L;
            var currentPosition = 0;
            while (currentPosition < bufferEndPosition || moreDataAvailable)
            {
                for (; currentPosition < bufferEndPosition; currentPosition++)
                {
                    var current = buffer[currentPosition];

                    totalHandled++;
                    if (currentPosition >= _errorThreshold)
                        throw new TooBigWordException(); // collected segment is too big to work with it

                    if (currentPosition >= _upperThreshold)
                    {
                        // collected segment is bigger then should be
                        if (rightmostParagraphPosition.HasValue)
                        {
                            // if was the paragraph end then split segment there
                            var length = AddSegment(buffer, ref bufferEndPosition, segments, rightmostParagraphPosition.Value, false, true);
                            currentPosition -= length;
                            if (rightmostSpacePosition.HasValue)
                                if (rightmostSpacePosition.Value > rightmostParagraphPosition.Value)
                                    rightmostSpacePosition -= length;
                                else
                                    rightmostSpacePosition = null;


                            rightmostParagraphPosition = null;
                            continue;
                        }

                        if (rightmostSpacePosition.HasValue)
                        {
                            // if was the whitespace then split segment there (it is bad for presentation)
                            currentPosition -= AddSegment(buffer, ref bufferEndPosition, segments, rightmostSpacePosition.Value, false, false);
                            rightmostSpacePosition = null;
                            // ReSharper disable once RedundantAssignment
                            rightmostParagraphPosition = null;
                            continue;
                        }

                        // after i'm waiting for the word end to make the big monoword segment
                        if (current == '\n')
                        {
                            currentPosition -= AddSegment(buffer, ref bufferEndPosition, segments, currentPosition, true, true);
                            // ReSharper disable once RedundantAssignment
                            rightmostSpacePosition = null;
                            // ReSharper disable once RedundantAssignment
                            rightmostParagraphPosition = null;
                            continue;
                        }

                        //if (char.IsWhiteSpace(current)) good choice declined due to optimization issue
                        if (current == ' ')
                        {
                            var length = AddSegment(buffer, ref bufferEndPosition, segments, currentPosition, true, false);
                            currentPosition -= length;
                            // ReSharper disable once RedundantAssignment
                            rightmostSpacePosition = null;
                            // ReSharper disable once RedundantAssignment
                            rightmostParagraphPosition = null;
                            // ReSharper disable once RedundantJumpStatement
                            continue;
                        }
                    }

                    if (current == '\n')
                    {
                        if (currentPosition >= _lowerThreshold)
                        {
                            // if I find paragraph end between threshold I finalize segment. The best case.
                            currentPosition -= AddSegment(buffer, ref bufferEndPosition, segments, currentPosition, false, true);
                            rightmostSpacePosition = null;
                            rightmostParagraphPosition = null;
                            continue;
                        }
                        rightmostParagraphPosition = currentPosition;
                    }

                    //if (char.IsWhiteSpace(current)) good choice declined due to optimization issue
                    if (current == ' ') // Just remember the latest whitespace for future big segments
                        rightmostSpacePosition = currentPosition;
                }

                if (bufferEndPosition == buffer.Length && moreDataAvailable)
                {
                    // enlarge input buffer, because we need more space for big segment
                    var newBuffer = new char[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, bufferEndPosition);
                    buffer = newBuffer;
                }
                // read data from input stream
                moreDataAvailable = FillBuffer(streamReader, buffer, ref bufferEndPosition);

                var toReport = totalHandled/(1024*1024);
                if (latestReported < toReport)
                {
                    // we report not every symbol. Just only 1Mb
                    latestReported = toReport;
                    cancellationToken.ThrowIfCancellationRequested();
                    progress?.Report($"{toReport}M");
                }
            }

            if (currentPosition > 0)
            {// Finalize segment with input tail
                AddSegment(buffer, ref bufferEndPosition, segments, currentPosition - 1,
                    !rightmostSpacePosition.HasValue && !rightmostParagraphPosition.HasValue,
                    rightmostParagraphPosition.HasValue && rightmostParagraphPosition.Value == bufferEndPosition - 1);
            }
            
            return segments;
        }

        /// <summary>
        /// Asynchronous method that separates input stream into segments
        /// </summary>
        /// <param name="streamReader">input stream</param>
        /// <param name="cancellationToken">token to cancel the operation</param>
        /// <param name="progress">progress callback structure</param>
        /// <returns>
        /// segments list async task
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        [return: NotNull] 
        public Task<List<ISegment>> SegmentAsync([NotNull] StreamReader streamReader, CancellationToken cancellationToken, IProgress<string> progress)
        {
            if (streamReader == null) throw new ArgumentNullException(nameof(streamReader));

            return Task.Factory.StartNew(() => Segment(streamReader, cancellationToken, progress), cancellationToken);
        }
    }
}
