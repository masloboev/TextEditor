using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TextEditor.Model;
using TextEditor.SupportModel;
using TextEditor.ViewModel;

namespace TextEditor.UnitTests.SupportModel
{
    [TestClass]
    public class ViewportTests
    {
        #region Mocks
        private Mock<ISegmentViewModel> _segment1ViewModel;
        private Mock<ISegmentViewModel> _segment2ViewModel;
        private Mock<ISegmentViewModel> _segment3ViewModel;
        private Mock<IContentWriter> _contentWriterMock;
        private Mock<ISegment> _segment1Mock;
        private Mock<ISegment> _segment2Mock;
        private Mock<ISegment> _segment3Mock;
        private Mock<IDocument> _documentMock;
        private Mock<ILineBreaker> _lineBreakerMock;
        private Mock<IModuleFactory> _moduleFactoryMock;
        #endregion

        private const int Segment1Height = 2;
        private const int Segment2Height = 4;
        private const int Segment3Height = 2;
        private const int Width1 = 10;

        [TestInitialize]
        public void Initialize()
        {
            var seg1Data = "012\n345\n".ToCharArray();
            var row1 = new Row(seg1Data, 0, 3, true, true);
            var row2 = new Row(seg1Data, 4, 7, true, true);
            var seg2Data = "abc\ndef\nghi\nklm".ToCharArray();
            var row3 = new Row(seg2Data, 0, 3, true, true);
            var row4 = new Row(seg2Data, 4, 7, true, true);
            var row5 = new Row(seg2Data, 8, 11, true, true);
            var row6 = new Row(seg2Data, 12, 14, true, true);
            var seg3Data = "nop\nxyz\n".ToCharArray();
            var row7 = new Row(seg3Data, 0, 3, true, true);
            var row8 = new Row(seg3Data, 4, 7, true, true);

            _segment1Mock = new Mock<ISegment> { Name = "segment1" };
            _segment2Mock = new Mock<ISegment> { Name = "segment2" };
            _segment3Mock = new Mock<ISegment> { Name = "segment3" };

            _documentMock = new Mock<IDocument>();
            _documentMock.Setup(p => p.FirstSegment).Returns(_segment1Mock.Object);
            _documentMock.Setup(p => p.LastSegment).Returns(_segment3Mock.Object);
            _documentMock.Setup(p => p.SegmentsCount).Returns(3);
            _documentMock.Setup(p => p.Next(It.Is<ISegment>(s => s == _segment1Mock.Object))).Returns(_segment2Mock.Object);
            _documentMock.Setup(p => p.Next(It.Is<ISegment>(s => s == _segment2Mock.Object))).Returns(_segment3Mock.Object);
            _documentMock.Setup(p => p.Next(It.Is<ISegment>(s => s == _segment3Mock.Object))).Returns((ISegment)null);
            _documentMock.Setup(p => p.Prev(It.Is<ISegment>(s => s == _segment1Mock.Object))).Returns((ISegment)null);
            _documentMock.Setup(p => p.Prev(It.Is<ISegment>(s => s == _segment2Mock.Object))).Returns(_segment1Mock.Object);
            _documentMock.Setup(p => p.Prev(It.Is<ISegment>(s => s == _segment3Mock.Object))).Returns(_segment2Mock.Object);

            _lineBreakerMock = new Mock<ILineBreaker>();
            _lineBreakerMock.Setup(p => p.GetRowsCount(It.Is<ISegment>(s => s == _segment1Mock.Object))).Returns(Segment1Height);
            _lineBreakerMock.Setup(p => p.GetRows(It.Is<ISegment>(s => s == _segment1Mock.Object))).Returns(new List<Row> {row1, row2});
            _lineBreakerMock.Setup(p => p.GetRowsCount(It.Is<ISegment>(s => s == _segment2Mock.Object))).Returns(Segment2Height);
            _lineBreakerMock.Setup(p => p.GetRows(It.Is<ISegment>(s => s == _segment2Mock.Object))).Returns(new List<Row> { row3, row4, row5, row6 });
            _lineBreakerMock.Setup(p => p.GetRowsCount(It.Is<ISegment>(s => s == _segment3Mock.Object))).Returns(Segment3Height);
            _lineBreakerMock.Setup(p => p.GetRows(It.Is<ISegment>(s => s == _segment3Mock.Object))).Returns(new List<Row> { row7, row8 });

            _contentWriterMock = new Mock<IContentWriter>();

            _segment1ViewModel = new Mock<ISegmentViewModel>();
            _segment1ViewModel.Setup(p => p.Segment).Returns(_segment1Mock.Object);
            _segment1ViewModel.Setup(p => p.Rows).Returns(_lineBreakerMock.Object.GetRows(_segment1Mock.Object));
            _segment1ViewModel.Setup(p => p.RowsCount).Returns(_lineBreakerMock.Object.GetRowsCount(_segment1Mock.Object));


            _segment2ViewModel = new Mock<ISegmentViewModel>();
            _segment2ViewModel.Setup(p => p.Segment).Returns(_segment2Mock.Object);
            _segment2ViewModel.Setup(p => p.Rows).Returns(_lineBreakerMock.Object.GetRows(_segment2Mock.Object));
            _segment2ViewModel.Setup(p => p.RowsCount).Returns(_lineBreakerMock.Object.GetRowsCount(_segment2Mock.Object));


            _segment3ViewModel = new Mock<ISegmentViewModel>();
            _segment3ViewModel.Setup(p => p.Segment).Returns(_segment3Mock.Object);
            _segment3ViewModel.Setup(p => p.Rows).Returns(_lineBreakerMock.Object.GetRows(_segment3Mock.Object));
            _segment3ViewModel.Setup(p => p.RowsCount).Returns(_lineBreakerMock.Object.GetRowsCount(_segment3Mock.Object));

            _moduleFactoryMock = new Mock<IModuleFactory>();
            _moduleFactoryMock.Setup(p => p.MakeSegmentViewModel(
                    It.Is<ILineBreaker>(s => s == _lineBreakerMock.Object), 
                    It.Is<ISegment>(s => s == _segment1Mock.Object)))
                .Returns(_segment1ViewModel.Object);
            _moduleFactoryMock.Setup(p => p.MakeSegmentViewModel(
                    It.Is<ILineBreaker>(s => s == _lineBreakerMock.Object), 
                    It.Is<ISegment>(s => s == _segment2Mock.Object)))
                .Returns(_segment2ViewModel.Object);
            _moduleFactoryMock.Setup(p => p.MakeSegmentViewModel(
                It.Is<ILineBreaker>(s => s == _lineBreakerMock.Object), 
                It.Is<ISegment>(s => s == _segment3Mock.Object)))
                .Returns(_segment3ViewModel.Object);
        }

        /// <summary>
        /// Verifies the content printed should be printed.
        /// </summary>
        /// <param name="firstSegmentOffset">The first segment offset.</param>
        /// <param name="segmentViewModels">The printed segments.</param>
        /// <param name="rowsCount">The printed rows count.</param>
        public void VerifyPrinted(int firstSegmentOffset, ISegmentViewModel[] segmentViewModels, int rowsCount)
        {
            _contentWriterMock.Verify(p => p.MakeContent(
                It.Is<int>(s => s == firstSegmentOffset),
                It.Is<List<ISegmentViewModel>>(s => s.SequenceEqual(segmentViewModels)),
                It.Is<int>(s => s == rowsCount),
                It.Is<int>(s => s == Width1)));
        }

        [TestMethod]
        public void State_AllDocumentInViewport_ShouldViewAllContent()
        {          
            var viewport = new Viewport(_documentMock.Object, _moduleFactoryMock.Object, _lineBreakerMock.Object,
                Segment1Height + Segment2Height + Segment3Height, null);
            var scrolls = new List<RowsScrollPosition>();            
            viewport.EvScrollPositionChanged += position => scrolls.Add(position);

            viewport.MakeContent(_contentWriterMock.Object, Width1);
            VerifyPrinted(0, new[] {_segment1ViewModel.Object, _segment2ViewModel.Object, _segment3ViewModel.Object}, 
                Segment1Height + Segment2Height + Segment3Height);

            Assert.AreEqual(Segment1Height + Segment2Height + Segment3Height, viewport.RowsCount);
            Assert.AreEqual(false, viewport.CanScrollUp());
            Assert.AreEqual(false, viewport.CanScrollDown());
            Assert.AreEqual(_segment1Mock.Object, viewport.RowsScrollPosition.FirstSegment);
            Assert.AreEqual(0, viewport.RowsScrollPosition.RowsBeforeScrollCount);
            Assert.AreEqual(0, scrolls.Count);
        }

        [TestMethod]
        public void State_2LinesInViewport_ShouldViewOnlySegment1()
        {
            var viewport = new Viewport(_documentMock.Object, _moduleFactoryMock.Object, _lineBreakerMock.Object, 2, null);
            var scrolls = new List<RowsScrollPosition>();
            viewport.EvScrollPositionChanged += position => scrolls.Add(position);

            viewport.MakeContent(_contentWriterMock.Object, Width1);
            VerifyPrinted(0, new[] { _segment1ViewModel.Object }, 2);

            Assert.AreEqual(2, viewport.RowsCount);
            Assert.AreEqual(false, viewport.CanScrollUp());
            Assert.AreEqual(true, viewport.CanScrollDown());
            Assert.AreEqual(_segment1Mock.Object, viewport.RowsScrollPosition.FirstSegment);
            Assert.AreEqual(0, viewport.RowsScrollPosition.RowsBeforeScrollCount);
            Assert.AreEqual(0, scrolls.Count);
        }

        [TestMethod]
        public void State_2LinesInViewportAfterScrollLineDown_ViewSegment1AndSegment2()
        {
            var viewport = new Viewport(_documentMock.Object, _moduleFactoryMock.Object, _lineBreakerMock.Object, 2, null);
            var scrolls = new List<RowsScrollPosition>();
            viewport.EvScrollPositionChanged += position => scrolls.Add(position);

            viewport.ScrollDown(1);
            viewport.MakeContent(_contentWriterMock.Object, Width1);
            VerifyPrinted(1, new[] { _segment1ViewModel.Object, _segment2ViewModel.Object }, 2);

            Assert.AreEqual(2, viewport.RowsCount);
            Assert.AreEqual(true, viewport.CanScrollUp());
            Assert.AreEqual(true, viewport.CanScrollDown());
            Assert.AreEqual(_segment1Mock.Object, viewport.RowsScrollPosition.FirstSegment);
            Assert.AreEqual(1, viewport.RowsScrollPosition.RowsBeforeScrollCount);
            Assert.AreEqual(1, scrolls.Count);
            Assert.AreEqual(_segment1Mock.Object, scrolls[0].FirstSegment);
            Assert.AreEqual(1, scrolls[0].RowsBeforeScrollCount);
        }

        [TestMethod]
        public void State_2LinesInViewportAfter2ScrollLineDown_ViewOnlySegment2()
        {
            var viewport = new Viewport(_documentMock.Object, _moduleFactoryMock.Object, _lineBreakerMock.Object, 2, null);
            var scrolls = new List<RowsScrollPosition>();
            viewport.EvScrollPositionChanged += position => scrolls.Add(position);

            viewport.ScrollDown(1);
            viewport.ScrollDown(1);
            viewport.MakeContent(_contentWriterMock.Object, Width1);
            VerifyPrinted(0, new[] { _segment2ViewModel.Object }, 2);

            Assert.AreEqual(2, viewport.RowsCount);
            Assert.AreEqual(true, viewport.CanScrollUp());
            Assert.AreEqual(true, viewport.CanScrollDown());
            Assert.AreEqual(_segment2Mock.Object, viewport.RowsScrollPosition.FirstSegment);
            Assert.AreEqual(0, viewport.RowsScrollPosition.RowsBeforeScrollCount);
            Assert.AreEqual(2, scrolls.Count);
            Assert.AreEqual(_segment1Mock.Object, scrolls[0].FirstSegment);
            Assert.AreEqual(1, scrolls[0].RowsBeforeScrollCount);
            Assert.AreEqual(_segment2Mock.Object, scrolls[1].FirstSegment);
            Assert.AreEqual(0, scrolls[1].RowsBeforeScrollCount);
        }

        [TestMethod]
        public void State_2LinesInViewportAfterLongScrollDown_ViewSegment2AndSegment3()
        {
            var viewport = new Viewport(_documentMock.Object, _moduleFactoryMock.Object, _lineBreakerMock.Object, 2, null);
            var scrolls = new List<RowsScrollPosition>();
            viewport.EvScrollPositionChanged += position => scrolls.Add(position);

            viewport.ScrollDown(Segment1Height + Segment2Height - 1);

            viewport.MakeContent(_contentWriterMock.Object, Width1);
            VerifyPrinted(3, new[] { _segment2ViewModel.Object, _segment3ViewModel.Object }, 2);

            Assert.AreEqual(2, viewport.RowsCount);
            Assert.AreEqual(true, viewport.CanScrollUp());
            Assert.AreEqual(true, viewport.CanScrollDown());
            Assert.AreEqual(_segment2Mock.Object, viewport.RowsScrollPosition.FirstSegment);
            Assert.AreEqual(3, viewport.RowsScrollPosition.RowsBeforeScrollCount);
            Assert.AreEqual(1, scrolls.Count);
            Assert.AreEqual(_segment2Mock.Object, scrolls[0].FirstSegment);
            Assert.AreEqual(3, scrolls[0].RowsBeforeScrollCount);
        }

        [TestMethod]
        public void State_2LinesInViewportScrollEnd_ViewOnlySegment3()
        {
            var viewport = new Viewport(_documentMock.Object, _moduleFactoryMock.Object, _lineBreakerMock.Object, 2, null);
            var scrolls = new List<RowsScrollPosition>();
            viewport.EvScrollPositionChanged += position => scrolls.Add(position);

            viewport.ScrollDown(Segment1Height + Segment2Height);

            viewport.MakeContent(_contentWriterMock.Object, Width1);
            VerifyPrinted(0, new[] { _segment3ViewModel.Object }, 2);

            Assert.AreEqual(2, viewport.RowsCount);
            Assert.AreEqual(true, viewport.CanScrollUp());
            Assert.AreEqual(false, viewport.CanScrollDown());
            Assert.AreEqual(_segment3Mock.Object, viewport.RowsScrollPosition.FirstSegment);
            Assert.AreEqual(0, viewport.RowsScrollPosition.RowsBeforeScrollCount);
            Assert.AreEqual(1, scrolls.Count);
            Assert.AreEqual(_segment3Mock.Object, scrolls[0].FirstSegment);
            Assert.AreEqual(0, scrolls[0].RowsBeforeScrollCount);
        }

        [TestMethod]
        public void State_2LinesInViewportScrollUpFromEndPreviousSegment_ViewSegment2AndSegment3()
        {
            var viewport = new Viewport(_documentMock.Object, _moduleFactoryMock.Object, _lineBreakerMock.Object, 2, null);
            var scrolls = new List<RowsScrollPosition>();
            viewport.EvScrollPositionChanged += position => scrolls.Add(position);

            viewport.ScrollDown(Segment1Height + Segment2Height);
            viewport.ScrollUp(1);
            viewport.MakeContent(_contentWriterMock.Object, Width1);

            VerifyPrinted(3, new[] { _segment2ViewModel.Object, _segment3ViewModel.Object }, 2);

            Assert.AreEqual(2, viewport.RowsCount);
            Assert.AreEqual(true, viewport.CanScrollUp());
            Assert.AreEqual(true, viewport.CanScrollDown());
            Assert.AreEqual(_segment2Mock.Object, viewport.RowsScrollPosition.FirstSegment);
            Assert.AreEqual(3, viewport.RowsScrollPosition.RowsBeforeScrollCount);
            Assert.AreEqual(2, scrolls.Count);
            Assert.AreEqual(_segment3Mock.Object, scrolls[0].FirstSegment);
            Assert.AreEqual(0, scrolls[0].RowsBeforeScrollCount);
            Assert.AreEqual(_segment2Mock.Object, scrolls[1].FirstSegment);
            Assert.AreEqual(3, scrolls[1].RowsBeforeScrollCount);
        }

        [TestMethod]
        public void State_4LinesInViewportSetScrollWithShiftonTheSameViewPort_ViewSegment2AndSegment3()
        {
            var viewport = new Viewport(_documentMock.Object, _moduleFactoryMock.Object, _lineBreakerMock.Object, 4, null);
            var scrolls = new List<RowsScrollPosition>();
            viewport.EvScrollPositionChanged += position => scrolls.Add(position);

            viewport.ScrollTo(new RowsScrollPosition() {FirstSegment = _segment2Mock.Object, RowsBeforeScrollCount = 1});
            viewport.MakeContent(_contentWriterMock.Object, Width1);

            VerifyPrinted(1, new[] { _segment2ViewModel.Object, _segment3ViewModel.Object }, 4);

            Assert.AreEqual(4, viewport.RowsCount);
            Assert.AreEqual(true, viewport.CanScrollUp());
            Assert.AreEqual(true, viewport.CanScrollDown());
            Assert.AreEqual(_segment2Mock.Object, viewport.RowsScrollPosition.FirstSegment);
            Assert.AreEqual(1, viewport.RowsScrollPosition.RowsBeforeScrollCount);
            Assert.AreEqual(1, scrolls.Count);
            Assert.AreEqual(_segment2Mock.Object, scrolls[0].FirstSegment);
            Assert.AreEqual(1, scrolls[0].RowsBeforeScrollCount);
        }

        [TestMethod]
        public void State_ViewportResizeAtBegin_ViewSegment1AndSegment2()
        {
            var viewport = new Viewport(_documentMock.Object, _moduleFactoryMock.Object, _lineBreakerMock.Object, 2, null);
            var scrolls = new List<RowsScrollPosition>();
            viewport.EvScrollPositionChanged += position => scrolls.Add(position);

            viewport.RowsCount = 3;
            viewport.MakeContent(_contentWriterMock.Object, Width1);

            VerifyPrinted(0, new[] { _segment1ViewModel.Object, _segment2ViewModel.Object }, 3);

            Assert.AreEqual(3, viewport.RowsCount);
            Assert.AreEqual(false, viewport.CanScrollUp());
            Assert.AreEqual(true, viewport.CanScrollDown());
            Assert.AreEqual(_segment1Mock.Object, viewport.RowsScrollPosition.FirstSegment);
            Assert.AreEqual(0, viewport.RowsScrollPosition.RowsBeforeScrollCount);
            Assert.AreEqual(1, scrolls.Count);
            Assert.AreEqual(_segment1Mock.Object, scrolls[0].FirstSegment);
            Assert.AreEqual(0, scrolls[0].RowsBeforeScrollCount);
        }

        [TestMethod]
        public void State_ViewportResizeAtEnd_ViewSegment2AndSegment3()
        {
            var viewport = new Viewport(_documentMock.Object, _moduleFactoryMock.Object, _lineBreakerMock.Object, 2, null);
            var scrolls = new List<RowsScrollPosition>();
            viewport.EvScrollPositionChanged += position => scrolls.Add(position);

            viewport.ScrollDown(Segment1Height + Segment2Height);
            viewport.RowsCount = 3;
            viewport.MakeContent(_contentWriterMock.Object, Width1);

            VerifyPrinted(3, new[] { _segment2ViewModel.Object, _segment3ViewModel.Object }, 3);

            Assert.AreEqual(3, viewport.RowsCount);
            Assert.AreEqual(true, viewport.CanScrollUp());
            Assert.AreEqual(false, viewport.CanScrollDown());
            Assert.AreEqual(_segment2Mock.Object, viewport.RowsScrollPosition.FirstSegment);
            Assert.AreEqual(3, viewport.RowsScrollPosition.RowsBeforeScrollCount);
            Assert.AreEqual(2, scrolls.Count);
            Assert.AreEqual(_segment3Mock.Object, scrolls[0].FirstSegment);
            Assert.AreEqual(0, scrolls[0].RowsBeforeScrollCount);
            Assert.AreEqual(_segment2Mock.Object, scrolls[1].FirstSegment);
            Assert.AreEqual(3, scrolls[1].RowsBeforeScrollCount);
        }

        [TestMethod]
        public void State_ViewportLargeResizeAtEnd_ViewAllTheDocument()
        {
            var viewport = new Viewport(_documentMock.Object, _moduleFactoryMock.Object, _lineBreakerMock.Object, 2, null);
            var scrolls = new List<RowsScrollPosition>();
            viewport.EvScrollPositionChanged += position => scrolls.Add(position);

            viewport.ScrollDown(Segment1Height + Segment2Height);
            viewport.RowsCount = 10;
            viewport.MakeContent(_contentWriterMock.Object, Width1);

            VerifyPrinted(0, new[] { _segment1ViewModel.Object, _segment2ViewModel.Object, _segment3ViewModel.Object }, 
                Segment1Height + Segment2Height + Segment3Height);

            Assert.AreEqual(10, viewport.RowsCount);
            Assert.AreEqual(false, viewport.CanScrollUp());
            Assert.AreEqual(false, viewport.CanScrollDown());
            Assert.AreEqual(_segment1Mock.Object, viewport.RowsScrollPosition.FirstSegment);
            Assert.AreEqual(0, viewport.RowsScrollPosition.RowsBeforeScrollCount);
            Assert.AreEqual(2, scrolls.Count);
            Assert.AreEqual(_segment3Mock.Object, scrolls[0].FirstSegment);
            Assert.AreEqual(0, scrolls[0].RowsBeforeScrollCount);
            Assert.AreEqual(_segment1Mock.Object, scrolls[1].FirstSegment);
            Assert.AreEqual(0, scrolls[1].RowsBeforeScrollCount);
        }       

        [TestMethod]
        public void State_NewViewportBasedOnPreviousViewport_ShouldCorrectScroll()
        {
            var prevViewport = new Viewport(_documentMock.Object, _moduleFactoryMock.Object, _lineBreakerMock.Object, 2, null);
            prevViewport.ScrollDown(Segment1Height + 2);

            Assert.AreEqual(_segment2Mock.Object, prevViewport.RowsScrollPosition.FirstSegment);
            Assert.AreEqual(2, prevViewport.RowsScrollPosition.RowsBeforeScrollCount);

            var viewport = new Viewport(_documentMock.Object, _moduleFactoryMock.Object, _lineBreakerMock.Object, 2, prevViewport.DocumentScrollPosition);
            viewport.MakeContent(_contentWriterMock.Object, Width1);

            VerifyPrinted(2, new[] { _segment2ViewModel.Object }, 2);

            Assert.AreEqual(2, viewport.RowsCount);
            Assert.AreEqual(true, viewport.CanScrollUp());
            Assert.AreEqual(true, viewport.CanScrollDown());
            Assert.AreEqual(_segment2Mock.Object, viewport.RowsScrollPosition.FirstSegment);
            Assert.AreEqual(2, viewport.RowsScrollPosition.RowsBeforeScrollCount);
        }
    }
}
