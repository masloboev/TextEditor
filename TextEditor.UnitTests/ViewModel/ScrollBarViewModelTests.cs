using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TextEditor.Model;
using TextEditor.SupportModel;
using TextEditor.ViewModel;

namespace TextEditor.UnitTests.ViewModel
{
    [TestClass]
    public class ScrollBarViewModelTests
    {
        private ScrollBarViewModel _scrollBarViewModel;

        private Mock<IScrollable> _scrollTargetMock;
        private Mock<ISegmentsRowsLayout> _layout1Mock;
        private Mock<ISegmentsRowsLayout> _layout2Mock;
        private Mock<ISegmentsRowsLayoutProvider> _segmentsRowsLayoutCache;
        private Mock<ISegment> _segment1Mock;
        private Mock<ISegment> _segment2Mock;

        private const int Segment1Height = 2;
        private const int Segment2Height = 4;
        private const int Width1 = 10;
        private const int Width2 = 5;
        private const int RowsCount = 2;

        private HashSet<string> _changedProperties;

        [TestInitialize]
        public void Initialize()
        {
            _scrollTargetMock = new Mock<IScrollable>();
            _segmentsRowsLayoutCache = new Mock<ISegmentsRowsLayoutProvider>();

            _segment1Mock = new Mock<ISegment>();
            _segment2Mock = new Mock<ISegment>();

            var p11 = new SegmentRowsPosition (_segment1Mock.Object, Segment1Height, 0);
            var p12 = new SegmentRowsPosition (_segment2Mock.Object, Segment2Height, Segment1Height );

            _layout1Mock = new Mock<ISegmentsRowsLayout>();
            _layout1Mock.Setup(p => p.TotalRowsCount).Returns(Segment1Height + Segment2Height);
            _layout1Mock.Setup(p => p.FindBySegment(It.Is<ISegment>(s => s == _segment1Mock.Object))).Returns(p11);
            _layout1Mock.Setup(p => p.FindBySegment(It.Is<ISegment>(s => s == _segment2Mock.Object))).Returns(p12);

            //_layout1Mock.Setup(p => p.FindByOffset(It.Is<int>(s => s == 0))).Returns(p11);
            _layout1Mock.Setup(p => p.FindByOffset(It.IsInRange(0L, Segment1Height - 1L, Range.Inclusive))).Returns(p11);
            _layout1Mock.Setup(p => p.FindByOffset(It.IsInRange(Segment1Height, Segment1Height + Segment2Height - 1L, Range.Inclusive))).Returns(p12);

            var p21 = new SegmentRowsPosition(_segment1Mock.Object, Segment1Height * 2, 0);
            var p22 = new SegmentRowsPosition(_segment2Mock.Object, Segment2Height * 2, Segment1Height * 2);

            _layout2Mock = new Mock<ISegmentsRowsLayout>();
            _layout2Mock.Setup(p => p.TotalRowsCount).Returns(Segment1Height * 2 + Segment2Height * 2);
            _layout2Mock.Setup(p => p.FindBySegment(It.Is<ISegment>(s => s == _segment1Mock.Object))).Returns(p21);
            _layout2Mock.Setup(p => p.FindBySegment(It.Is<ISegment>(s => s == _segment2Mock.Object))).Returns(p22);
            _layout2Mock.Setup(p => p.FindByOffset(It.IsInRange(0L, Segment1Height * 2 - 1L, Range.Inclusive))).Returns(p21);
            _layout2Mock.Setup(p => p.FindByOffset(It.IsInRange(Segment1Height * 2L, Segment1Height * 2L + Segment2Height * 2 - 1, Range.Inclusive))).Returns(p22);

            _scrollTargetMock = new Mock<IScrollable>();
            _scrollBarViewModel = new ScrollBarViewModel();

            _changedProperties = new HashSet<string>();
            _scrollBarViewModel.PropertyChanged += (sender, args) => _changedProperties.Add(args.PropertyName);
            _scrollBarViewModel.Init(_segmentsRowsLayoutCache.Object, _scrollTargetMock.Object);
        }

        [TestMethod]
        public async Task State_Initialization_CheckAsync()
        {
            _segmentsRowsLayoutCache.Setup(p => p.TryGet(It.IsAny<int>())).Returns((ISegmentsRowsLayout) null);
            _segmentsRowsLayoutCache.Setup(p => p.GetAsync(It.Is<int>(s => s == Width1), It.Is<EventHandler<string>>(s => s != null)))
                .Returns((int width, EventHandler<string> onProgress) => Task.Run(() =>
                                                                                {
                                                                                    onProgress(null, "status");
                                                                                    return _layout1Mock.Object;
                                                                                }));

            var viewport = new Mock<IViewport>();
            viewport.Setup(p => p.RowsCount).Returns(RowsCount);
            viewport.Setup(p => p.RowsScrollPosition).Returns(new RowsScrollPosition { FirstSegment = _segment1Mock.Object, RowsBeforeScrollCount = 1});

            await _scrollBarViewModel.UpdateAsync(viewport.Object, Width1);

            _segmentsRowsLayoutCache.Verify(p => p.TryGet(It.Is<int>(s => s == Width1)), Times.Once);
            _segmentsRowsLayoutCache.Verify(p => p.GetAsync(It.Is<int>(s => s == Width1), It.Is<EventHandler<string>>(s => s != null)), Times.Once);
            Assert.IsTrue(_scrollBarViewModel.IsEnabled);
            Assert.AreEqual(Segment1Height + Segment2Height - RowsCount, _scrollBarViewModel.Maximum);
            Assert.AreEqual(1, _scrollBarViewModel.Value);
            Assert.AreEqual(RowsCount, _scrollBarViewModel.LargeChange);
            Assert.AreEqual("status", _scrollBarViewModel.Status);
            Assert.IsTrue(_changedProperties.Contains(nameof(_scrollBarViewModel.IsEnabled)));
            Assert.IsTrue(_changedProperties.Contains(nameof(_scrollBarViewModel.Value)));
            Assert.IsTrue(_changedProperties.Contains(nameof(_scrollBarViewModel.Maximum)));
            Assert.IsTrue(_changedProperties.Contains(nameof(_scrollBarViewModel.LargeChange)));
            Assert.IsTrue(_changedProperties.Contains(nameof(_scrollBarViewModel.Status)));

            _scrollTargetMock
                .Verify(p => p.ScrollTo(It.Is<RowsScrollPosition>(s => s.FirstSegment == _segment1Mock.Object && s.RowsBeforeScrollCount == 1)),
                Times.Once);
        }

        [TestMethod]
        public async Task State_Initialization_NoAsyncIfTryGet()
        {
            _segmentsRowsLayoutCache.Setup(p => p.TryGet(It.IsAny<int>())).Returns(_layout1Mock.Object);

            var viewport = new Mock<IViewport>();
            viewport.Setup(p => p.RowsCount).Returns(RowsCount);
            viewport.Setup(p => p.RowsScrollPosition).Returns(new RowsScrollPosition { FirstSegment = _segment1Mock.Object, RowsBeforeScrollCount = 1 });

            await _scrollBarViewModel.UpdateAsync(viewport.Object, Width1);

            _segmentsRowsLayoutCache.Verify(p => p.TryGet(It.Is<int>(s => s == Width1)), Times.Once);
            _segmentsRowsLayoutCache.Verify(p => p.GetAsync(It.IsAny<int>(), It.IsAny<EventHandler<string>>()), Times.Never);
        }

        [TestMethod]
        public async Task State_ScrollFromView_Check()
        {
            _segmentsRowsLayoutCache.Setup(p => p.TryGet(It.IsAny<int>())).Returns(_layout1Mock.Object);

            var viewport = new Mock<IViewport>();
            viewport.Setup(p => p.RowsCount).Returns(RowsCount);
            viewport.Setup(p => p.RowsScrollPosition).Returns(new RowsScrollPosition { FirstSegment = _segment1Mock.Object, RowsBeforeScrollCount = 1 });

            await _scrollBarViewModel.UpdateAsync(viewport.Object, Width1);

            _changedProperties.Clear();
            _scrollBarViewModel.Value = 4;

            Assert.IsTrue(_scrollBarViewModel.IsEnabled);
            Assert.AreEqual(Segment1Height + Segment2Height - RowsCount, _scrollBarViewModel.Maximum);
            Assert.AreEqual(4, _scrollBarViewModel.Value);
            Assert.AreEqual(RowsCount, _scrollBarViewModel.LargeChange);
            Assert.IsFalse(_changedProperties.Contains(nameof(_scrollBarViewModel.IsEnabled)));
            Assert.IsTrue(_changedProperties.Contains(nameof(_scrollBarViewModel.Value)));
            Assert.IsFalse(_changedProperties.Contains(nameof(_scrollBarViewModel.Maximum)));
            Assert.IsFalse(_changedProperties.Contains(nameof(_scrollBarViewModel.LargeChange)));

            _scrollTargetMock
                .Verify(p => p.ScrollTo(It.Is<RowsScrollPosition>(s => s.FirstSegment == _segment2Mock.Object && s.RowsBeforeScrollCount == 2)),
                Times.Once);
        }

        [TestMethod]
        public async Task State_ScrollFromSupplier_Check()
        {
            _segmentsRowsLayoutCache.Setup(p => p.TryGet(It.IsAny<int>())).Returns(_layout1Mock.Object);

            var viewport = new Mock<IViewport>();
            viewport.Setup(p => p.RowsCount).Returns(RowsCount);
            viewport.Setup(p => p.RowsScrollPosition).Returns(new RowsScrollPosition { FirstSegment = _segment1Mock.Object, RowsBeforeScrollCount = 1 });
            await _scrollBarViewModel.UpdateAsync(viewport.Object, Width1);

            _changedProperties.Clear();
            viewport.Raise(p => p.EvScrollPositionChanged += null, new RowsScrollPosition {FirstSegment = _segment2Mock.Object, RowsBeforeScrollCount = 1});

            Assert.IsTrue(_scrollBarViewModel.IsEnabled);
            Assert.AreEqual(Segment1Height + Segment2Height - RowsCount, _scrollBarViewModel.Maximum);
            Assert.AreEqual(3, _scrollBarViewModel.Value);
            Assert.AreEqual(RowsCount, _scrollBarViewModel.LargeChange);
            Assert.IsFalse(_changedProperties.Contains(nameof(_scrollBarViewModel.IsEnabled)));
            Assert.IsTrue(_changedProperties.Contains(nameof(_scrollBarViewModel.Value)));
            Assert.IsFalse(_changedProperties.Contains(nameof(_scrollBarViewModel.Maximum)));
            Assert.IsFalse(_changedProperties.Contains(nameof(_scrollBarViewModel.LargeChange)));

            _scrollTargetMock
                .Verify(p => p.ScrollTo(It.Is<RowsScrollPosition>(s => s.FirstSegment == _segment2Mock.Object && s.RowsBeforeScrollCount == 1)),
                Times.Once);
        }

        [TestMethod]
        public async Task State_ViewResizeInInitialization_Check()
        {
            _segmentsRowsLayoutCache.Setup(p => p.TryGet(It.IsAny<int>())).Returns((ISegmentsRowsLayout)null);
            _segmentsRowsLayoutCache.Setup(p => p.GetAsync(It.Is<int>(s => s == Width1), It.Is<EventHandler<string>>(s => s != null)))
                 .Returns(Task.Run(() => _layout1Mock.Object));
            _segmentsRowsLayoutCache.Setup(p => p.GetAsync(It.Is<int>(s => s == Width2), It.Is<EventHandler<string>>(s => s != null)))
                 .Returns(Task.Run(() => _layout2Mock.Object));

            var viewport = new Mock<IViewport>();
            viewport.Setup(p => p.RowsCount).Returns(RowsCount);
            viewport.Setup(p => p.RowsScrollPosition).Returns(new RowsScrollPosition { FirstSegment = _segment1Mock.Object, RowsBeforeScrollCount = 0 });

            _scrollBarViewModel.Update(viewport.Object, Width1);
            await _scrollBarViewModel.UpdateAsync(viewport.Object, Width2);

            Assert.IsTrue(_scrollBarViewModel.IsEnabled);
            Assert.AreEqual(Segment1Height * 2 + Segment2Height * 2 - RowsCount, _scrollBarViewModel.Maximum);
            Assert.AreEqual(0, _scrollBarViewModel.Value);
            Assert.AreEqual(RowsCount, _scrollBarViewModel.LargeChange);
            Assert.IsTrue(_changedProperties.Contains(nameof(_scrollBarViewModel.IsEnabled)));
            Assert.IsFalse(_changedProperties.Contains(nameof(_scrollBarViewModel.Value)));
            Assert.IsTrue(_changedProperties.Contains(nameof(_scrollBarViewModel.Maximum)));
            Assert.IsTrue(_changedProperties.Contains(nameof(_scrollBarViewModel.LargeChange)));

            _scrollTargetMock.Verify(p => p.ScrollTo(It.IsAny<RowsScrollPosition>()), Times.Never);
        }

        [TestMethod]
        public async Task State_ViewResizeAfterInitialization_Check()
        {
            _segmentsRowsLayoutCache.Setup(p => p.TryGet(It.Is<int>(s => s == Width2))).Returns(_layout2Mock.Object);
            _segmentsRowsLayoutCache.Setup(p => p.GetAsync(It.Is<int>(s => s == Width1), It.Is<EventHandler<string>>(s => s != null)))
                 .Returns(Task.Run(() => _layout1Mock.Object));


            var viewport = new Mock<IViewport>();
            viewport.Setup(p => p.RowsCount).Returns(RowsCount);
            viewport.Setup(p => p.RowsScrollPosition).Returns(new RowsScrollPosition { FirstSegment = _segment1Mock.Object, RowsBeforeScrollCount = 0 });

            await _scrollBarViewModel.UpdateAsync(viewport.Object, Width1);
            _changedProperties.Clear();
            await _scrollBarViewModel.UpdateAsync(viewport.Object, Width2);

            Assert.IsTrue(_scrollBarViewModel.IsEnabled);
            Assert.AreEqual(Segment1Height * 2 + Segment2Height * 2 - RowsCount, _scrollBarViewModel.Maximum);
            Assert.AreEqual(0, _scrollBarViewModel.Value);
            Assert.AreEqual(RowsCount, _scrollBarViewModel.LargeChange);
            Assert.IsFalse(_changedProperties.Contains(nameof(_scrollBarViewModel.IsEnabled)));
            Assert.IsFalse(_changedProperties.Contains(nameof(_scrollBarViewModel.Value)));
            Assert.IsTrue(_changedProperties.Contains(nameof(_scrollBarViewModel.Maximum)));
            Assert.IsFalse(_changedProperties.Contains(nameof(_scrollBarViewModel.LargeChange)));

            _scrollTargetMock.Verify(p => p.ScrollTo(It.IsAny<RowsScrollPosition>()), Times.Never);
        }       
    }
}
