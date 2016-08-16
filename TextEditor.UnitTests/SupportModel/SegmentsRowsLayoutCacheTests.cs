using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TextEditor.Model;
using TextEditor.SupportModel;

namespace TextEditor.UnitTests.SupportModel
{
    [TestClass]
    public class SegmentsRowsLayoutCacheTests
    {
        private SegmentsRowsLayoutCache _segmentsRowsLayoutProvider;

        private Mock<ISegment> _segment1Mock;
        private Mock<ISegment> _segment2Mock;

        private const int Segment1Height = 2;
        private const int Segment2Height = 4;
        private const int Width1 = 10;
        private const int Width2 = 5;

        [TestInitialize]
        public void Initialize()
        {
            _segment1Mock = new Mock<ISegment>();
            _segment2Mock = new Mock<ISegment>();

            var documentMock = new Mock<IDocument>();
            documentMock.Setup(p => p.FirstSegment).Returns(_segment1Mock.Object);
            documentMock.Setup(p => p.LastSegment).Returns(_segment2Mock.Object);
            documentMock.Setup(p => p.SegmentsCount).Returns(2);
            documentMock.Setup(p => p.Next(It.Is<ISegment>(s => s == _segment1Mock.Object))).Returns(_segment2Mock.Object);
            documentMock.Setup(p => p.Next(It.Is<ISegment>(s => s == _segment2Mock.Object))).Returns((ISegment)null);
            documentMock.Setup(p => p.Prev(It.Is<ISegment>(s => s == _segment1Mock.Object))).Returns((ISegment)null);
            documentMock.Setup(p => p.Prev(It.Is<ISegment>(s => s == _segment2Mock.Object))).Returns(_segment1Mock.Object);

            var lineBreaker1Mock = new Mock<ILineBreaker>();
            lineBreaker1Mock.Setup(p => p.GetRowsCount(It.Is<ISegment>(s => s == _segment1Mock.Object))).Returns(Segment1Height);
            lineBreaker1Mock.Setup(p => p.GetRowsCount(It.Is<ISegment>(s => s == _segment2Mock.Object))).Returns(Segment2Height);
            lineBreaker1Mock.Setup(p => p.SymbolsInRowCount).Returns(Width1);

            var lineBreaker2Mock = new Mock<ILineBreaker>();
            lineBreaker2Mock.Setup(p => p.GetRowsCount(It.Is<ISegment>(s => s == _segment1Mock.Object))).Returns(2 * Segment1Height);
            lineBreaker2Mock.Setup(p => p.GetRowsCount(It.Is<ISegment>(s => s == _segment2Mock.Object))).Returns(2 * Segment2Height);
            lineBreaker2Mock.Setup(p => p.SymbolsInRowCount).Returns(Width2);

            var moduleFactoryMock = new Mock<IModuleFactory>();
            moduleFactoryMock.Setup(p => p.MakeLineBreaker(It.Is<int>(s => s == Width1))).Returns(lineBreaker1Mock.Object);
            moduleFactoryMock.Setup(p => p.MakeLineBreaker(It.Is<int>(s => s == Width2))).Returns(lineBreaker2Mock.Object);

            moduleFactoryMock.Setup(p => p.MakeSegmentsRowsLayout(It.IsAny<int>()))
                .Returns((int segmentsCount) => new SegmentsRowsLayout(segmentsCount)); 

            _segmentsRowsLayoutProvider = new SegmentsRowsLayoutCache(documentMock.Object, moduleFactoryMock.Object);
        }

        [TestMethod]
        public async Task GetAsync_FirstWidth_CheckLayout()
        {
            var layout = await _segmentsRowsLayoutProvider.GetAsync(Width1, null);
            Assert.AreEqual(Segment1Height + Segment2Height, layout.TotalRowsCount);
            var position1 = layout.FindBySegment(_segment1Mock.Object);
            Assert.IsNotNull(position1);
            Assert.AreEqual(_segment1Mock.Object, position1.Segment);
            Assert.AreEqual(0, position1.StartDocumentRowsOffset);
            Assert.AreEqual(Segment1Height, position1.RowsCount);

            var position2 = layout.FindBySegment(_segment2Mock.Object);
            Assert.IsNotNull(position2);
            Assert.AreEqual(_segment2Mock.Object, position2.Segment);
            Assert.AreEqual(Segment1Height, position2.StartDocumentRowsOffset);
            Assert.AreEqual(Segment2Height, position2.RowsCount);
        }

        [TestMethod]
        public async Task GetAsync_SecondWidth_CheckLayout()
        {
            await _segmentsRowsLayoutProvider.GetAsync(Width1, null);
            var layout = await _segmentsRowsLayoutProvider.GetAsync(Width2, null);
            Assert.AreEqual((Segment1Height + Segment2Height) * 2, layout.TotalRowsCount);
            var position1 = layout.FindBySegment(_segment1Mock.Object);
            Assert.IsNotNull(position1);
            Assert.AreEqual(_segment1Mock.Object, position1.Segment);
            Assert.AreEqual(0, position1.StartDocumentRowsOffset);
            Assert.AreEqual(Segment1Height * 2, position1.RowsCount);

            var position2 = layout.FindBySegment(_segment2Mock.Object);
            Assert.IsNotNull(position2);
            Assert.AreEqual(_segment2Mock.Object, position2.Segment);
            Assert.AreEqual(Segment1Height * 2, position2.StartDocumentRowsOffset);
            Assert.AreEqual(Segment2Height * 2, position2.RowsCount);
        }

        [TestMethod]
        public async Task Progress_FirstWidth_ShouldReceiveTwice()
        {
            var messages = new List<string>();
            var progress = new EventHandler<string>((sender, s) => messages.Add(s));
            await _segmentsRowsLayoutProvider.GetAsync(Width1, progress);
            _segmentsRowsLayoutProvider.Unprogress(Width1, progress);

            Assert.AreEqual(2, messages.Count);
            Assert.AreEqual($"{Width1} 1/2", messages[0]);
            Assert.AreEqual($"{Width1} 2/2", messages[1]);
        }

        [TestMethod]
        public async Task TryGet_FirstWidthBeforeAsync_ShoultBeNull()
        {
            var layout = _segmentsRowsLayoutProvider.TryGet(Width1);
            await _segmentsRowsLayoutProvider.GetAsync(Width1, null);
            Assert.IsNull(layout);            
        }

        [TestMethod]
        public async Task TryGet_FirstWidthAfter_ShouldntBeNull()
        {
            await _segmentsRowsLayoutProvider.GetAsync(Width1, null);
            var layout = _segmentsRowsLayoutProvider.TryGet(Width1);
            Assert.IsNotNull(layout);
        }

        [TestMethod]
        public async Task TasksState_SecondWidthAfterFirstWait_BothShouldBeDone()
        {
            await _segmentsRowsLayoutProvider.GetAsync(Width1, null);
            await _segmentsRowsLayoutProvider.GetAsync(Width2, null);
            Assert.IsNotNull(_segmentsRowsLayoutProvider.TryGet(Width1));
            Assert.IsNotNull(_segmentsRowsLayoutProvider.TryGet(Width2));
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task TasksState_SecondWidthBeforeFirstWait_FirstShouldBeCancelled()
        {
            var firstTask = _segmentsRowsLayoutProvider.GetAsync(Width1, null);
            await _segmentsRowsLayoutProvider.GetAsync(Width2, null);
            Assert.IsNull(_segmentsRowsLayoutProvider.TryGet(Width1));
            Assert.IsNotNull(_segmentsRowsLayoutProvider.TryGet(Width2));
            await firstTask;
        }
    }
}
