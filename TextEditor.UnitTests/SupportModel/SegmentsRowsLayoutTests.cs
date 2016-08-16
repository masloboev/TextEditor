using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TextEditor.Model;
using TextEditor.SupportModel;

namespace TextEditor.UnitTests.SupportModel
{
    [TestClass]
    public class SegmentsRowsLayoutTests
    {
        private SegmentsRowsLayout _segmentsRowsLayout;
        private SegmentRowsPosition _segmentRowsPosition1;
        private SegmentRowsPosition _segmentRowsPosition2;

        [TestInitialize]
        public void Initialize()
        {
            _segmentsRowsLayout = new SegmentsRowsLayout(2);
            _segmentRowsPosition1 = new SegmentRowsPosition(Mock.Of<ISegment>(), 2, 0);

            _segmentsRowsLayout.Append(_segmentRowsPosition1);
            _segmentRowsPosition2 = new SegmentRowsPosition(Mock.Of<ISegment>(), 4, 10);

            _segmentsRowsLayout.Append(_segmentRowsPosition2);
        }

        [TestMethod]
        public void FindBySegment_2Segment_ShouldCalculateTotalLength()
        {
            Assert.AreEqual(_segmentRowsPosition1.RowsCount + _segmentRowsPosition2.RowsCount, _segmentsRowsLayout.TotalRowsCount);
        }

        [TestMethod]
        public void FindBySegment_2Segment_ShouldReturnSegment()
        {
            var foundSegmentRowsPosition = _segmentsRowsLayout.FindBySegment(_segmentRowsPosition1.Segment);
            Assert.AreSame(_segmentRowsPosition1, foundSegmentRowsPosition);
            foundSegmentRowsPosition = _segmentsRowsLayout.FindBySegment(_segmentRowsPosition2.Segment);
            Assert.AreSame(_segmentRowsPosition2, foundSegmentRowsPosition);
        }

        [TestMethod]
        public void FindByOffset_FindBySegmentBegin_ShouldReturnSegment()
        {
            var foundSegmentRowsPosition = _segmentsRowsLayout.FindByOffset(_segmentRowsPosition1.StartDocumentRowsOffset);
            Assert.AreSame(_segmentRowsPosition1, foundSegmentRowsPosition);
            foundSegmentRowsPosition = _segmentsRowsLayout.FindByOffset(_segmentRowsPosition2.StartDocumentRowsOffset);
            Assert.AreSame(_segmentRowsPosition2, foundSegmentRowsPosition);
        }

        [TestMethod]
        public void FindByOffset_FindBySegmentMiddle_ShouldReturnSegment()
        {
            var foundSegmentRowsPosition = _segmentsRowsLayout.FindByOffset(_segmentRowsPosition1.StartDocumentRowsOffset + 1);
            Assert.AreSame(_segmentRowsPosition1, foundSegmentRowsPosition);
            foundSegmentRowsPosition = _segmentsRowsLayout.FindByOffset(_segmentRowsPosition2.StartDocumentRowsOffset + 2);
            Assert.AreSame(_segmentRowsPosition2, foundSegmentRowsPosition);
        }
    }
}
