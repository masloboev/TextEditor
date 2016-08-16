using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TextEditor.Exceptions;
using TextEditor.Model;

namespace TextEditor.UnitTests.Model
{
    [TestClass]
    public class SegmentizerTests
    {
        /// <summary>
        /// Runs the segment procedure asynchronous.
        /// </summary>
        /// <param name="text">The textprovided for segmentizer.</param>
        /// <param name="lowerThreshold">The lower segmentizer threshold.</param>
        /// <param name="upperThreshold">The upper segmentizer threshold.</param>
        /// <param name="errorThreshold">The error segmentizer threshold.</param>
        /// <returns>Segments list Task</returns>
        private static async Task<List<ISegment>> MakeSegmentsAsync(string text, int lowerThreshold, int upperThreshold, int? errorThreshold = null)
        {
            if (!errorThreshold.HasValue)
                errorThreshold = Constants.SegmentizerErrorThreshold;

            var segmentizer = new Segmentizer(lowerThreshold, upperThreshold, errorThreshold.Value);
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(text)))
            using (var streamReader = new StreamReader(stream))
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var progress = Mock.Of<IProgress<string>>();
                return await segmentizer.SegmentAsync(streamReader, cancellationTokenSource.Token, progress);
            }
        }

        /// <summary>
        /// Gets the text of the segment.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns>the text of the segment</returns>
        private static string GetSegmentText(ISegment segment) => new string(segment.RowData, segment.BeginPosition, segment.Length);

        /// <summary>
        /// Runs the segment procedure asynchronous.
        /// </summary>
        /// <param name="text">The textprovided for segmentizer.</param>
        /// <param name="lowerThreshold">The lower segmentizer threshold.</param>
        /// <param name="upperThreshold">The upper segmentizer threshold.</param>
        /// <param name="errorThreshold">The error segmentizer threshold.</param>
        /// <returns>
        /// Segments texts Task
        /// </returns>
        private static async Task<string[]> MakeSegmentsTextsAsync(string text, int lowerThreshold, int upperThreshold, int? errorThreshold = null)
        {
            var segments = await MakeSegmentsAsync(text, lowerThreshold, upperThreshold, errorThreshold);
            return segments.Select(GetSegmentText).ToArray();
        }

        [TestMethod]
        public async Task SegmentAsync_EmptyText_NoSegmentsShouldBeReturned()
        {
            var segments = await MakeSegmentsAsync("", 10, 20);
            Assert.AreEqual(0, segments.Count);
        }

        [TestMethod]
        public async Task SegmentAsync_NonParagraphEnd_ShouldFlushSymbols()
        {
            var segmentsTexts = await MakeSegmentsTextsAsync("012", 5, 10);
            CollectionAssert.AreEqual(new[] { "012" }, segmentsTexts);
        }

        #region Paragraph tests

        [TestMethod]
        public async Task SegmentAsync_Paragraph_ShouldBreakAfterParagraph()
        {
            var segmentsTexts = await MakeSegmentsTextsAsync("0123456\n", 5, 10);
            CollectionAssert.AreEqual(new[] { "0123456\n" }, segmentsTexts);
        }
        [TestMethod]
        public async Task SegmentAsync_CaretParagraph_ShouldBreakAfterParagraph()
        {
            var segmentsTexts = await MakeSegmentsTextsAsync("012345\r\n", 5, 10);
            CollectionAssert.AreEqual(new[] { "012345\r\n" }, segmentsTexts);
        }
        [TestMethod]
        public async Task SegmentAsync_CaretParagraphOnUpperTreshold_ShouldBreakAfterParagraph()
        {
            var segmentsTexts = await MakeSegmentsTextsAsync("012345678\r\n", 5, 10);
            CollectionAssert.AreEqual(new[] {"012345678\r\n"}, segmentsTexts);
        }

        [TestMethod]
        public async Task SegmentAsync_Paragraph_ShouldSetEndsWithNewLine()
        {
            var segments = await MakeSegmentsAsync("0123456\n", 5, 10);
            Assert.AreEqual(true, segments[0].EndsWithNewLine);
        }

        [TestMethod]
        public async Task SegmentAsync_NoParagraph_ShouldntSetEndsWithNewLine()
        {
            var segments = await MakeSegmentsAsync("0123456", 5, 10);
            Assert.AreEqual(false, segments[0].EndsWithNewLine);
        }
        #endregion

        #region Words overflow test

        [TestMethod]
        public async Task SegmentAsync_ManyWordsAfterParagraphOverflow_ShouldBreakTextAfterParagraph()
        {
            var segmentsTexts = await MakeSegmentsTextsAsync("0123\n56789 1234\n", 5, 10);
            CollectionAssert.AreEqual(new[] { "0123\n", "56789 1234\n" }, segmentsTexts);
        }

        [TestMethod]
        public async Task SegmentAsync_ManyWordsWithSpaceAfterParagraphOverflow_ShouldBreakTextAfterParagraphNotSpace()
        {
            var segmentsTexts = await MakeSegmentsTextsAsync("0123\n56 89 12\n", 5, 10);
            CollectionAssert.AreEqual(new[] { "0123\n", "56 89 12\n" }, segmentsTexts);
        }

        [TestMethod]
        public async Task SegmentAsync_ManyWordsOverflow_ShouldBreakTextAfterLatestSpace()
        {
            var segmentsTexts = await MakeSegmentsTextsAsync("012 456 8901 345678 0", 5, 10);
            CollectionAssert.AreEqual(new[] { "012 456 ", "8901 ", "345678 0" }, segmentsTexts);
        }

        #endregion

        #region long word tests

        [TestMethod]
        public async Task SegmentAsync_LongWordOnTextBegin_ShouldBreakTextAfterUpperTreshold()
        {
            var segmentsTexts = await MakeSegmentsTextsAsync("01234567890123", 5, 10);
            CollectionAssert.AreEqual(new[] { "01234567890123" }, segmentsTexts);
        }

        [TestMethod]
        public async Task SegmentAsync_LongWordOnTextMiddle_ShouldExtractLongWordToSegment()
        {
            var segmentsTexts = await MakeSegmentsTextsAsync("012 456789012345689abcdefghij 012", 5, 10);
            CollectionAssert.AreEqual(new[] { "012 ", "456789012345689abcdefghij ", "012" }, segmentsTexts);
        }

        [TestMethod]
        public async Task SegmentAsync_LongWordWithSpacesBefore2ParagraphsOnTextMiddle_ShouldIgnoreSpaceBeforeParagraphPosition()
        {
            var segmentsTexts = await MakeSegmentsTextsAsync("01 3\n456789012345689abcdefghij\n01234567890", 5, 10);
            CollectionAssert.AreEqual(new[] { "01 3\n", "456789012345689abcdefghij\n", "01234567890" }, segmentsTexts);
        }

        [TestMethod]
        public async Task SegmentAsync_LongWordOnTextMiddle_ShouldSetIsMonoWordFlag()
        {
            var segments = await MakeSegmentsAsync("012 456789012345689abcdefghij 012", 5, 10);
            Assert.AreEqual(true, segments[1].IsMonoWord);
        }

        [TestMethod]
        public async Task SegmentAsync_NoLongWordOnText_ShouldntSetIsMonoWordFlag()
        {
            var segments = await MakeSegmentsAsync("0123 456", 5, 10);
            Assert.AreEqual(false, segments[0].IsMonoWord);
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(TooBigWordException))]
        public async Task SegmentAsync_VeryLongWord_ShouldThrowTooBigWordException()
        {
            await MakeSegmentsAsync("0123 456 0123456789012345", 5, 10, 15);
        }

        [TestMethod]
        public async Task SegmentAsync_2MbText_ShouldReportProgress2Times()
        {
            var templateBytes = Encoding.ASCII.GetBytes("012345678 ");

            // Report should appers every 1 Mb
            var neededTextSize = 2 * 1024 * 1024 + templateBytes.Length;
            var bytes = new byte[neededTextSize + templateBytes.Length];
            var position = 0;
            while (position < neededTextSize)
            {
                Array.Copy(templateBytes, 0, bytes, position, templateBytes.Length);
                position += templateBytes.Length;
            }
            if (position < bytes.Length - 1)
                Array.Copy(templateBytes, 0, bytes, position, bytes.Length - 1 - position);

            // set upperThreshold to 1024*1024 / 2 because progress checks comes between reads that is upperThreshold 
            var segmentizer = new Segmentizer(1024 * 1024 / 4, 1024*1024 / 2, Constants.SegmentizerErrorThreshold);
            using (var stream = new MemoryStream(bytes))
            using (var streamReader = new StreamReader(stream))
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var progress = new Mock<IProgress<string>>();
                progress.Setup(p => p.Report(It.IsAny<string>()));
                await segmentizer.SegmentAsync(streamReader, cancellationTokenSource.Token, progress.Object);
                progress.Verify(e => e.Report(It.IsAny<string>()), Times.Exactly(2));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Cancel_SimpleText_ShouldThrowOperationCancelledExecption()
        {
            var segmentizer = new Segmentizer(Constants.SegmentizerLowerThreshold, Constants.SegmentizerUpperThreshold, Constants.SegmentizerErrorThreshold);
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("SimpleText")))
            using (var streamReader = new StreamReader(stream))
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var progress = new Mock<IProgress<string>>();
                progress.Setup(p => p.Report(It.IsAny<string>()));
                var task = segmentizer.SegmentAsync(streamReader, cancellationTokenSource.Token, progress.Object);
                cancellationTokenSource.Cancel();
                await task;
            }
        }
    }
}
