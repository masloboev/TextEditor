using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TextEditor.Model;
using TextEditor.SupportModel;
using TextEditor.ViewModel;

namespace TextEditor.UnitTests.ViewModel
{
    [TestClass]
    public class SegmentViewModelTests
    {
        [TestMethod]
        public void Constructor_Initialization_ShouldInitializeProperties()
        {
            var segmentMock = new Mock<ISegment>();
            var segmentData = "012\nabc\nxyz\n".ToCharArray();
            var row1 = new Row(segmentData, 0, 3, true, true);
            var row2 = new Row(segmentData, 4, 7, true, true);
            var row3 = new Row(segmentData, 8, 11, true, true);

            var lineBreakerMock = new Mock<ILineBreaker>();
            var rows = new List <Row> { row1, row2, row3 };
            lineBreakerMock.Setup(p => p.GetRows(It.Is<ISegment>(s => s == segmentMock.Object))).Returns(rows);

            var segmentViewModel = new SegmentViewModel(lineBreakerMock.Object, segmentMock.Object);

            Assert.AreEqual(segmentMock.Object, segmentViewModel.Segment);
            Assert.AreEqual(rows.Count, segmentViewModel.RowsCount);
            CollectionAssert.AreEqual(rows, new List<Row>(segmentViewModel.Rows));
        }        
    }
}
