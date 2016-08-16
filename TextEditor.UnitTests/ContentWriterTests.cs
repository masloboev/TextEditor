using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TextEditor.Model;
using TextEditor.SupportModel;
using TextEditor.ViewModel;

namespace TextEditor.UnitTests
{
    [TestClass]
    public class ContentWriterTests
    {
        private ContentWriter _contentWriter;

        private Mock<ISegment> _segment1Mock;
        private Mock<ISegment> _segment2Mock;
        private Mock<ISegmentViewModel> _segment1ViewModelMock;
        private Mock<ISegmentViewModel> _segment2ViewModelMock;

        private const int Segment1RowsCount = 3;
        private const int Segment2RowsCount = 3;

        [TestInitialize]
        public void Initialize()
        {
            var segment1Data = "012\nabc\nxyz\n".ToCharArray();
            var row1 = new Row(segment1Data, 0, 3, true, true);
            var row2 = new Row(segment1Data, 4, 7, true, true);
            var row3 = new Row(segment1Data, 8, 11, true, true);
            _segment1Mock = new Mock<ISegment>();
            _segment1Mock.Setup(p => p.IsMonoWord).Returns(false);
            _segment1Mock.Setup(p => p.EndsWithNewLine).Returns(true);

            _segment1ViewModelMock = new Mock<ISegmentViewModel>();
            _segment1ViewModelMock.Setup(p => p.Rows).Returns(new List<Row> {row1, row2, row3});
            _segment1ViewModelMock.Setup(p => p.RowsCount).Returns(Segment1RowsCount);
            _segment1ViewModelMock.Setup(p => p.Segment).Returns(_segment1Mock.Object);

            var segment2Data = "def\nijk\nlmn\n".ToCharArray();
            var row4 = new Row(segment2Data, 0, 3, true, true);
            var row5 = new Row(segment2Data, 4, 7, true, true);
            var row6 = new Row(segment2Data, 8, 11, true, true);
            _segment2Mock = new Mock<ISegment>();
            _segment2Mock.Setup(p => p.IsMonoWord).Returns(false);
            _segment2Mock.Setup(p => p.EndsWithNewLine).Returns(true);

            _segment2ViewModelMock = new Mock<ISegmentViewModel>();
            _segment2ViewModelMock.Setup(p => p.Rows).Returns(new List<Row> { row4, row5, row6 });
            _segment2ViewModelMock.Setup(p => p.RowsCount).Returns(Segment2RowsCount);
            _segment2ViewModelMock.Setup(p => p.Segment).Returns(_segment2Mock.Object);

            _contentWriter = new ContentWriter();
        }

        [TestMethod]
        public void ReservedSymbolsCount_Initialization_ShouldReturn2()
        {
            Assert.AreEqual(2, _contentWriter.ReservedSymbolsCount);
        }

        #region write row tests
        /// <summary>
        /// Helper method to test WriteRow
        /// </summary>
        /// <param name="data">Rows data</param>
        /// <param name="beginPosition">The row begin position.</param>
        /// <param name="endPosition">The row end position.</param>
        /// <param name="isMonoWord">Monoword flag</param>
        /// <param name="endsWithNewline">Ends with new line flag</param>
        /// <returns></returns>
        private string WriteRow(string data, int beginPosition = 0, int? endPosition = null, bool isMonoWord = false, bool endsWithNewline = false)
        {
            if (!endPosition.HasValue)
                endPosition = data.Length - 1;

            var row = new Row(data.ToCharArray(), beginPosition, endPosition.Value, isMonoWord, endsWithNewline);
            var sb = new StringBuilder();
            ContentWriter.WriteRow(row, sb);
            return sb.ToString();
        }

        [TestMethod]
        public void WriteRow_NonZeroPositions_ShouldSkipSymbols()
        {
            var data = "01234".ToCharArray();
            var result = WriteRow("01234", 2, data.Length - 1 - 1);
            Assert.AreEqual("23", result);
        }

        [TestMethod]
        public void WriteRow_SpaceReplace_ShouldReplaceSpaces()
        {
            var result = WriteRow("01 45");
            Assert.AreEqual("01·45", result);
        }

        [TestMethod]
        public void WriteRow_ParagraphReplace_ShouldReplaceSpaces()
        {
            var result = WriteRow("0\n2");
            Assert.AreEqual("0¶2", result);
        }

        [TestMethod]
        public void WriteRow_CaretParagraphReplace_ShouldReplaceSpaces()
        {
            var result = WriteRow("0\r\n3");
            Assert.AreEqual("0¶3", result);
        }

        [TestMethod]
        public void WriteRow_TabStart_ShouldFill4Positions()
        {
            var result = WriteRow("\t4");
            Assert.AreEqual("   →4", result);
        }

        [TestMethod]
        public void WriteRow_1OffsetTabStart_ShouldFill3Positions()
        {
            var result = WriteRow("0\t4");
            Assert.AreEqual("0  →4", result);
        }

        [TestMethod]
        public void WriteRow_0OffsetTabInTheMiddle_ShouldFill4Positions()
        {
            var result = WriteRow("0123\t8");
            Assert.AreEqual("0123   →8", result);
        }

        [TestMethod]
        public void WriteRow_3OffsetTabInTheMiddle_ShouldFill1Positions()
        {
            var result = WriteRow("0123456\t8");
            Assert.AreEqual("0123456→8", result);
        }

        [TestMethod]
        public void WriteRow_0OffsetTabOnTheEndLine_ShouldFill1Positions()
        {
            var result = WriteRow("0123\t");
            Assert.AreEqual("0123→", result);
        }
        #endregion

        #region write segment tests

        [TestMethod]
        public void WriteSegment_ParagraphSegment_ShouldWriteAllSegmentAndSpecialSymbol()
        {
            var sb = new StringBuilder();
            _segment1Mock.Setup(p => p.IsMonoWord).Returns(false);
            _segment1Mock.Setup(p => p.EndsWithNewLine).Returns(true);
            ContentWriter.WriteSegment(_segment1ViewModelMock.Object, sb, 0, 3);
            Assert.AreEqual("012¶\r\nabc¶\r\nxyz¶↓\r\n", sb.ToString());
        }

        [TestMethod]
        public void WriteSegment_WordsSegment_ShouldWriteAllSegmentAndSpecialSymbol()
        {
            var sb = new StringBuilder();
            _segment1Mock.Setup(p => p.IsMonoWord).Returns(false);
            _segment1Mock.Setup(p => p.EndsWithNewLine).Returns(false);
            ContentWriter.WriteSegment(_segment1ViewModelMock.Object, sb, 0, 3);
            Assert.AreEqual("012¶\r\nabc¶\r\nxyz¶⇣\r\n", sb.ToString());
        }

        [TestMethod]
        public void WriteSegment_MonoWordSegment_ShouldWriteAllSegmentAndSpecialSymbol()
        {
            var sb = new StringBuilder();
            _segment1Mock.Setup(p => p.IsMonoWord).Returns(true);
            _segment1Mock.Setup(p => p.EndsWithNewLine).Returns(false);
            ContentWriter.WriteSegment(_segment1ViewModelMock.Object, sb, 0, 3);
            Assert.AreEqual("012¶\r\nabc¶\r\nxyz¶⇃\r\n", sb.ToString());
        }

        [TestMethod]
        public void WriteSegment_1RowFromSegment_ShouldWriteOneRow()
        {
            var sb = new StringBuilder();
            ContentWriter.WriteSegment(_segment1ViewModelMock.Object, sb, 1, 1);
            Assert.AreEqual("abc¶\r\n", sb.ToString());
        }

        #endregion

        #region make content tests
        [TestMethod]
        public void MakeContent_1Segment_ShouldWriteAllSegment()
        {
            _segment1Mock.Setup(p => p.IsMonoWord).Returns(false);
            _segment1Mock.Setup(p => p.EndsWithNewLine).Returns(true);
            var content = _contentWriter.MakeContent(0, new List<ISegmentViewModel> {_segment1ViewModelMock.Object}, Segment1RowsCount, 10);
            Assert.AreEqual("012¶\r\nabc¶\r\nxyz¶↓\r\n", content);
        }

        [TestMethod]
        public void MakeContent_2Segment_ShouldWriteBothSegments()
        {
            _segment1Mock.Setup(p => p.IsMonoWord).Returns(false);
            _segment1Mock.Setup(p => p.EndsWithNewLine).Returns(true);
            var content = _contentWriter.MakeContent(0, new List<ISegmentViewModel> { _segment1ViewModelMock.Object, _segment2ViewModelMock.Object }, 
                Segment1RowsCount + Segment2RowsCount, 10);
            Assert.AreEqual("012¶\r\nabc¶\r\nxyz¶↓\r\ndef¶\r\nijk¶\r\nlmn¶↓\r\n", content);
        }

        [TestMethod]
        public void MakeContent_2SegmentWithOffsets_ShouldWriteBothSegmentParts()
        {
            _segment1Mock.Setup(p => p.IsMonoWord).Returns(false);
            _segment1Mock.Setup(p => p.EndsWithNewLine).Returns(true);
            var content = _contentWriter.MakeContent(1, new List<ISegmentViewModel> { _segment1ViewModelMock.Object, _segment2ViewModelMock.Object },
                Segment1RowsCount + Segment2RowsCount - 2, 10);
            Assert.AreEqual("abc¶\r\nxyz¶↓\r\ndef¶\r\nijk¶\r\n", content);
        }
        #endregion
    }
}
