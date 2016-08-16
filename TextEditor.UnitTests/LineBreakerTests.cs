using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TextEditor.Model;
using TextEditor.UnitTests.Utils;

namespace TextEditor.UnitTests
{
    [TestClass]
    public class LineBreakerTests
    {
        private const int Width = 10;

        private LineBreaker _lineBreaker;

        [TestInitialize]
        public void Initialize()
        {
            _lineBreaker = new LineBreaker(Width);
        }

        /// <summary>
        ///     Mocks segment
        /// </summary>
        /// <param name="segmentText">Text to put into segment</param>
        /// <param name="rowDataOffset">Segment offset in text</param>
        /// <param name="length">Segment length</param>
        /// <param name="isMonoWord">is one word segment</param>
        /// <param name="endsWithNewLine">is segment end with paragraph symbol</param>
        /// <returns>Mocked segment</returns>
        private static ISegment MakeSegment(string segmentText, int rowDataOffset = 0, int? length = null, bool isMonoWord = false, bool endsWithNewLine = false)
        {
            if (!length.HasValue)
                length = segmentText.Length - rowDataOffset;

            return Mock.Of<ISegment>(s =>
                s.RowData == segmentText.ToCharArray() &&
                s.BeginPosition == rowDataOffset &&
                s.Length == length &&
                s.IsMonoWord == isMonoWord &&
                s.EndsWithNewLine == endsWithNewLine);
        }

        /// <summary>
        ///     Creates mock segment and breaks text into strings array with preinitialized LineBreaker
        /// </summary>
        /// <param name="segmentText">Text to put into segment</param>
        /// <param name="rowDataOffset">Segment offset in text</param>
        /// <param name="length">Segment length</param>
        /// <param name="isMonoWord">is one word segment</param>
        /// <param name="endsWithNewLine">is segment end with paragraph symbol</param>
        /// <returns>string presentation of generated rows</returns>
        private string[] GetRowsText(string segmentText, int rowDataOffset = 0, int? length = null, bool isMonoWord = false, bool endsWithNewLine = false)
        {
            var segment = MakeSegment(segmentText, rowDataOffset, length, isMonoWord, endsWithNewLine);
            var rows = _lineBreaker.GetRows(segment);
            return rows.Select(row => new string(row.RowData, row.BeginPosition, row.Length)).ToArray();
        }

        /// <summary>
        ///     Creates mock segment and breaks text into strings array with preinitialized LineBreaker
        /// </summary>
        /// <param name="segmentText">Text to put into segment</param>
        /// <param name="rowDataOffset">Segment offset in text</param>
        /// <param name="length">Segment length</param>
        /// <param name="isMonoWord">is one word segment</param>
        /// <param name="endsWithNewLine">is segment end with paragraph symbol</param>
        /// <returns>Rows count</returns>
        private int GetRowsCount(string segmentText, int rowDataOffset = 0, int? length = null, bool isMonoWord = false, bool endsWithNewLine = false)
        {
            var segment = MakeSegment(segmentText, rowDataOffset, length, isMonoWord, endsWithNewLine);
            return _lineBreaker.GetRowsCount(segment);
        }

        [TestMethod]
        public void GetRows_NonZeroPositions_ShouldSkipSymbols()
        {
            var rows = GetRowsText("012\n45\n", 1, 3);
            CollectionAssert.AreEqual(new[] { "12\n" }, rows);
        }

        [TestMethod]
        public void GetRows_NonParagraphEnd_ShouldFlushSymbols()
        {
            var rows = GetRowsText("012\n45");
            CollectionAssert.AreEqual(new[] { "012\n", "45" }, rows);
        }

        #region Paragraph tests

        [TestMethod]
        public void GetRows_Paragraph_ShouldBreakAfterParagraph()
        {
            var rows = GetRowsText("0\n0");
            CollectionAssert.AreEqual(new[] { "0\n", "0" }, rows);
        }
        [TestMethod]
        public void GetRows_ParagraphOnLineBegin_ShouldBreakAfterParagraph()
        {
            var rows = GetRowsText("\n0");
            CollectionAssert.AreEqual(new[] { "\n", "0" }, rows);
        }
        [TestMethod]
        public void GetRows_CaretParagraph_ShouldBreakAfterParagraph()
        {
            var rows = GetRowsText("0\r\n0");
            CollectionAssert.AreEqual(new[] { "0\r\n", "0" }, rows);
        }
        [TestMethod]
        public void GetRows_CaretParagraphOnLineEnd_ShouldTakeParagraph()
        {
            var rows = GetRowsText("012345678\r\n0");
            CollectionAssert.AreEqual(new[] { "012345678\r\n", "0" }, rows);
        }
        #endregion

        #region Words overflow test

        [TestMethod]
        public void GetRows_ManyWordsOverflow_ShouldBreakLine()
        {
            var rows = GetRowsText("012 456 8901234 6789");
            CollectionAssert.AreEqual(new[] { "012 456 ", "8901234 ", "6789" }, rows);
        }

        [TestMethod]
        public void GetRows_ManyWordsOverflow_ShouldRememberTransferredWordLength()
        {
            var rows = GetRowsText("012 456 8901 345678 0");
            CollectionAssert.AreEqual(new[] { "012 456 ", "8901 ", "345678 0" }, rows);
        }

        [TestMethod]
        public void GetRows_LastWordsEndsOnRowEnd_ShouldTakeSpace()
        {
            var rows = GetRowsText("012 456 89 1234 6789");
            CollectionAssert.AreEqual(new[] { "012 456 89 ", "1234 6789" }, rows);
        }
        #endregion

        #region long word tests

        [TestMethod]
        public void GetRows_LongWordOnLineBegin_ShouldBreakLine()
        {
            var rows = GetRowsText("01234567890123");
            CollectionAssert.AreEqual(new[] { "0123456789", "0123" }, rows);
        }

        [TestMethod]
        public void GetRows_LongWordOnLineMiddle_ShouldTransferAndBreakLine()
        {
            var rows = GetRowsText("012 456789012345");
            CollectionAssert.AreEqual(new[] { "012 ", "4567890123", "45" }, rows);
        }

        [TestMethod]
        public void GetRows_VeryLongWordOnLineMiddle_ShouldTransferAndBreakLineTwice()
        {
            var rows = GetRowsText("012 456789012345689abcdefghij");
            CollectionAssert.AreEqual(new[] { "012 ", "4567890123", "45689abcde", "fghij" }, rows);
        }

        #endregion

        #region tab tests

        [TestMethod]
        public void GetRows_0OffsetTabOnLineBegin_ShouldTake4Symbols()
        {
            var rows = GetRowsText("\t456789 0");
            CollectionAssert.AreEqual(new[] { "\t456789 ", "0" }, rows);
        }

        [TestMethod]
        public void GetRows_1OffsetTabOnLineBegin_ShouldTake3Symbols()
        {
            var rows = GetRowsText("0\t456789 0");
            CollectionAssert.AreEqual(new[] { "0\t456789 ", "0" }, rows);
        }

        [TestMethod]
        public void GetRows_0OffsetTabOnLineMiddle_ShouldTake3Symbols()
        {
            var rows = GetRowsText("01234\t9 1");
            CollectionAssert.AreEqual(new[] { "01234\t9 ", "1" }, rows);
        }

        [TestMethod]
        public void GetRows_1OffsetTabOnLineMiddle_ShouldTake3Symbols()
        {
            var rows = GetRowsText("0123\t9 1");
            CollectionAssert.AreEqual(new[] { "0123\t9 ", "1" }, rows);
        }

        [TestMethod]
        public void GetRows_TabOnLineEnd_ShouldBreakLine()
        {
            var rows = GetRowsText("01234567\t0");
            CollectionAssert.AreEqual(new[] { "01234567\t", "0" }, rows);
        }

        #endregion

        [TestMethod]
        public void GetRows_ManySpacesOnLineEnd_ShouldBreakLineBetweenSpaces()
        {
            var rows = GetRowsText("01234567    2");
            CollectionAssert.AreEqual(new[] { "01234567   ", " 2" }, rows);
        }

        [TestMethod]
        public void GetRows_ComplexText_ShouldBreakLinesAndDontSkipSymbols()
        {
            var text = new TextGenerator().GenerateText(new []{"x", "abc", "01234567890", " ", "\t", "\n"});
            var rows = GetRowsText(text);
            var sb = new StringBuilder();
            for (var i = 0; i < rows.Length; i++)
            { // each row should be less then width
                var rowLength = rows[i].Length;
                Assert.IsTrue(rowLength <= Width ||
                            rowLength == Width + 1 && char.IsWhiteSpace(rows[i][rowLength - 1])
                    , $"Bad row {i}");
                sb.Append(rows[i]);
            }

            // all rows should be combined in initial text
            Assert.AreEqual(text, sb.ToString(), "Text different");
        }

        #region GetRowsCount
        [TestMethod]
        public void GetRowsCount_1RowText_ShouldReturn1()
        {
            var rowsCount = GetRowsCount("0");
            Assert.AreEqual(1, rowsCount);
        }

        [TestMethod]
        public void GetRowsCount_ComplexText_ShouldReturnCountEqualsToRowsCount()
        {
            var text = new TextGenerator().GenerateText(new[] { "x", "abc", "01234567890", " ", "\t", "\n" });
            var rows = GetRowsText(text);
            var rowsCount = GetRowsCount(text);

            Assert.AreEqual(rows.Length, rowsCount);
        }
        #endregion
    }
}
