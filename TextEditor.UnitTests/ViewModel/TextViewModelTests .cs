using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TextEditor.Model;
using TextEditor.SupportModel;
using TextEditor.ViewModel;

namespace TextEditor.UnitTests.ViewModel
{
    [TestClass]
    public class TextViewModelTests
    {
        private Mock<IDocument> _documentMock;
        private Mock<ILineBreaker> _lineBreaker1Mock;
        private Mock<ILineBreaker> _lineBreaker2Mock;
        private Mock<IScrollBarViewModel> _scrollBarViewModelMock;
        private Mock<ISegmentsRowsLayoutProvider> _segmentsRowsLayoutProviderMock;
        private Mock<IViewport> _viewport1Mock;
        private Mock<IViewport> _viewport2Mock;
        private Mock<IModuleFactory> _moduleFactoryMock;
        private Mock<IContentWriter> _contentWriterMock;
        private Mock<ISegment> _segmentMock;

        private TextViewModel _textViewModel;

        private HashSet<string> _changedProperties;

        private const int ScrollSegmentOffset = 10;
        private const int Width1 = 10;
        private const int Width2 = 5;
        private const string Content1 = "1";
        private const string Content2 = "2";
        private const int RowsCount1 = 20;
        private const int RowsCount2 = 25;
        private const int ContentWriterReservedSymbolsCount = 2;

        [TestInitialize]
        public void Initialize()
        {
            _textViewModel = new TextViewModel();

            _documentMock = new Mock<IDocument>();
            _scrollBarViewModelMock = new Mock<IScrollBarViewModel>();
            _segmentsRowsLayoutProviderMock = new Mock<ISegmentsRowsLayoutProvider>();
            _lineBreaker1Mock = new Mock<ILineBreaker>();
            _lineBreaker2Mock = new Mock<ILineBreaker>();

            _contentWriterMock = new Mock<IContentWriter>();
            _contentWriterMock.Setup(p => p.ReservedSymbolsCount).Returns(ContentWriterReservedSymbolsCount);

            _segmentMock = new Mock<ISegment>();
            _viewport1Mock = new Mock<IViewport>();
            _viewport1Mock.Setup(p => p.MakeContent(It.Is<IContentWriter>(s => s == _contentWriterMock.Object), It.Is<int>(s => s == Width1))).Returns(Content1);
            _viewport1Mock.Setup(p => p.DocumentScrollPosition).Returns(new DocumentScrollPosition {FirstSegment = _segmentMock.Object, SegmentOffset = ScrollSegmentOffset });

            _viewport2Mock = new Mock<IViewport>();
            _viewport2Mock.Setup(p => p.MakeContent(It.Is<IContentWriter>(s => s == _contentWriterMock.Object), It.Is<int>(s => s == Width2))).Returns(Content2);            

            _moduleFactoryMock = new Mock<IModuleFactory>();
            _moduleFactoryMock.Setup(p => p.MakeScrollBarViewModel()).Returns(_scrollBarViewModelMock.Object);
            _moduleFactoryMock.Setup(p => p.MakeSegmentsRowsLayoutProvider(It.Is<IDocument>(s => s == _documentMock.Object))).Returns(_segmentsRowsLayoutProviderMock.Object);
            _moduleFactoryMock.Setup(p => p.MakeLineBreaker(It.Is<int>(s => s == Width1 - ContentWriterReservedSymbolsCount))).Returns(_lineBreaker1Mock.Object);
            _moduleFactoryMock.Setup(p => p.MakeLineBreaker(It.Is<int>(s => s == Width2 - ContentWriterReservedSymbolsCount))).Returns(_lineBreaker2Mock.Object);
            _moduleFactoryMock.Setup(p => p.MakeSegmentsRowsLayoutProvider(It.Is<IDocument>(s => s == _documentMock.Object))).Returns(_segmentsRowsLayoutProviderMock.Object);
            _moduleFactoryMock.Setup(p => p.MakeViewport(
                                    It.Is<IDocument>(s => s == _documentMock.Object),
                                    It.Is<ILineBreaker>(s => s == _lineBreaker1Mock.Object),                                    
                                    It.Is<int>(s => s == RowsCount1),
                                    It.Is<DocumentScrollPosition?>(s => s == null))).Returns(_viewport1Mock.Object);           

            _textViewModel.RowsCount = RowsCount1;
            _textViewModel.SymbolsInRowCount = Width1;

            _changedProperties = new HashSet<string>();
            _textViewModel.PropertyChanged += (sender, args) => _changedProperties.Add(args.PropertyName);
            _textViewModel.Init(_documentMock.Object, _contentWriterMock.Object, _moduleFactoryMock.Object, _scrollBarViewModelMock.Object);
        }

        [TestMethod]
        public void Constructor_Initialization_CheckInitialization()
        {
            Assert.AreSame(_documentMock.Object, _textViewModel.Document);
            Assert.AreEqual(RowsCount1, _textViewModel.RowsCount);
            Assert.AreEqual(Width1, _textViewModel.SymbolsInRowCount);

            // Should Initialize scrollbar
            Assert.AreSame(_scrollBarViewModelMock.Object, _textViewModel.ScrollBarViewModel);
            _scrollBarViewModelMock.Verify(p => p.Init(
                It.Is<ISegmentsRowsLayoutProvider>(s => s == _segmentsRowsLayoutProviderMock.Object),
                It.Is<IScrollable>(s => s == _textViewModel)), Times.Once);

            Assert.IsTrue(_changedProperties.Contains(nameof(_textViewModel.ScrollBarViewModel)));
            _scrollBarViewModelMock.Verify(p => p.Update(
                It.Is<IViewport>(s => s == _viewport1Mock.Object),
                It.Is<int>(s => s == Width1 - 2)), Times.Once);

            // Should Initialize viewport
            _moduleFactoryMock.Verify(p => p.MakeLineBreaker(It.Is<int>(s => s == Width1 - ContentWriterReservedSymbolsCount)));

            _moduleFactoryMock.Verify(p => p.MakeViewport(
                It.Is<IDocument>(s => s == _documentMock.Object),
                It.Is<ILineBreaker>(s => s == _lineBreaker1Mock.Object),
                It.Is<int>(s => s == RowsCount1),
                It.IsAny<DocumentScrollPosition?>()), Times.Once());

            // Should Make content
            _viewport1Mock.Verify(p => p.MakeContent(It.Is<IContentWriter>(s => s == _contentWriterMock.Object), It.Is<int>(s => s == Width1)), Times.Once);
            Assert.AreEqual(Content1, _textViewModel.Content);
            Assert.IsTrue(_changedProperties.Contains(nameof(_textViewModel.Content)));
        }

        [TestMethod]
        public void Update_VerticalResize_CheckProcedure()
        {
            _viewport1Mock.ResetCalls();
            _changedProperties.Clear();
            _textViewModel.RowsCount = RowsCount2;


            Assert.IsTrue(_changedProperties.Contains(nameof(_textViewModel.RowsCount)));

            //Should update viewport
            _viewport1Mock.VerifySet(p => p.RowsCount = RowsCount2);

            // Should update content
            _viewport1Mock.Verify(p => p.MakeContent(It.Is<IContentWriter>(s => s == _contentWriterMock.Object), It.Is<int>(s => s == Width1)), Times.Once);
            Assert.AreEqual(Content1, _textViewModel.Content);
            Assert.IsTrue(_changedProperties.Contains(nameof(_textViewModel.Content)));
        }

        [TestMethod]
        public void Update_HorizontalResize_CheckProcedure()
        {
            _moduleFactoryMock.Setup(p => p.MakeViewport(
                                    It.Is<IDocument>(s => s == _documentMock.Object),
                                    It.Is<ILineBreaker>(s => s == _lineBreaker2Mock.Object),
                                    It.Is<int>(s => s == RowsCount1),
                                    It.IsAny<DocumentScrollPosition?>())).Returns(_viewport2Mock.Object);

            _changedProperties.Clear();
            _textViewModel.SymbolsInRowCount = Width2;
            
            Assert.IsTrue(_changedProperties.Contains(nameof(_textViewModel.SymbolsInRowCount)));

            // Should update scrollbar
            Assert.AreSame(_scrollBarViewModelMock.Object, _textViewModel.ScrollBarViewModel);
            _scrollBarViewModelMock.Verify(p => p.Init(
                It.Is<ISegmentsRowsLayoutProvider>(s => s == _segmentsRowsLayoutProviderMock.Object),
                It.Is<IScrollable>(s => s == _textViewModel)), Times.Once);

            // Should reinitialize viewport
            _moduleFactoryMock.Verify(p => p.MakeLineBreaker(It.Is<int>(s => s == Width1 - ContentWriterReservedSymbolsCount)));

            _moduleFactoryMock.Verify(p => p.MakeViewport(
                It.Is<IDocument>(s => s == _documentMock.Object),
                It.Is<ILineBreaker>(s => s == _lineBreaker2Mock.Object),
                It.Is<int>(s => s == RowsCount1),
                It.Is<DocumentScrollPosition?>(
                    s => s.HasValue && s.Value.FirstSegment == _segmentMock.Object && s.Value.SegmentOffset == ScrollSegmentOffset)), Times.Once());

            // Should update content
            _viewport2Mock.Verify(p => p.MakeContent(It.Is<IContentWriter>(s => s == _contentWriterMock.Object), It.Is<int>(s => s == Width2)), Times.Once);
            Assert.AreEqual(Content2, _textViewModel.Content);
            Assert.IsTrue(_changedProperties.Contains(nameof(_textViewModel.Content)));
        }

        #region scroll
        [TestMethod]
        public void ViewportProxy_CanScrollDown_ShouldCallViewport()
        {
            _viewport1Mock.ResetCalls();
            _viewport1Mock.Setup(p => p.CanScrollDown()).Returns(true);
            var canScrollDown = _textViewModel.CanScrollDown();

            Assert.IsTrue(canScrollDown);
            _viewport1Mock.Verify(p => p.CanScrollDown(), Times.Once);
        }

        [TestMethod]
        public void ViewportProxy_ScrollLineDown_ShouldCallViewport()
        {
            _changedProperties.Clear();
            _viewport1Mock.ResetCalls();
            _textViewModel.LineDown();
            _viewport1Mock.Setup(p => p.ScrollDown(It.Is<int>(s => s == 1)));
            
            // Should Make content
            _viewport1Mock.Verify(p => p.MakeContent(It.Is<IContentWriter>(s => s == _contentWriterMock.Object), It.Is<int>(s => s == Width1)), Times.Once);
            Assert.AreEqual(Content1, _textViewModel.Content);
            Assert.IsTrue(_changedProperties.Contains(nameof(_textViewModel.Content)));
        }

        [TestMethod]
        public void ViewportProxy_ScrollPageDown_ShouldCallViewport()
        {
            _changedProperties.Clear();
            _viewport1Mock.ResetCalls();
            _textViewModel.PageDown();
            _viewport1Mock.Setup(p => p.ScrollDown(It.Is<int>(s => s == RowsCount1)));

            // Should Make content
            _viewport1Mock.Verify(p => p.MakeContent(It.Is<IContentWriter>(s => s == _contentWriterMock.Object), It.Is<int>(s => s == Width1)), Times.Once);
            Assert.AreEqual(Content1, _textViewModel.Content);
            Assert.IsTrue(_changedProperties.Contains(nameof(_textViewModel.Content)));
        }

        [TestMethod]
        public void ViewportProxy_CanScrollUp_ShouldCallViewport()
        {
            _viewport1Mock.ResetCalls();
            _viewport1Mock.Setup(p => p.CanScrollUp()).Returns(true);
            var canScrollUp = _textViewModel.CanScrollUp();

            Assert.IsTrue(canScrollUp);
            _viewport1Mock.Verify(p => p.CanScrollUp(), Times.Once);
        }

        [TestMethod]
        public void ViewportProxy_ScrollLineUp_ShouldCallViewport()
        {
            _changedProperties.Clear();
            _viewport1Mock.ResetCalls();
            _textViewModel.LineUp();
            _viewport1Mock.Setup(p => p.ScrollUp(It.Is<int>(s => s == 1)));

            // Should Make content
            _viewport1Mock.Verify(p => p.MakeContent(It.Is<IContentWriter>(s => s == _contentWriterMock.Object), It.Is<int>(s => s == Width1)), Times.Once);
            Assert.AreEqual(Content1, _textViewModel.Content);
            Assert.IsTrue(_changedProperties.Contains(nameof(_textViewModel.Content)));

        }

        [TestMethod]
        public void ViewportProxy_ScrollPageUp_ShouldCallViewport()
        {
            _changedProperties.Clear();
            _viewport1Mock.ResetCalls();
            _textViewModel.PageUp();
            _viewport1Mock.Setup(p => p.ScrollUp(It.Is<int>(s => s == RowsCount1)));

            // Should Make content
            _viewport1Mock.Verify(p => p.MakeContent(It.Is<IContentWriter>(s => s == _contentWriterMock.Object), It.Is<int>(s => s == Width1)), Times.Once);
            Assert.AreEqual(Content1, _textViewModel.Content);
            Assert.IsTrue(_changedProperties.Contains(nameof(_textViewModel.Content)));
        }

        [TestMethod]
        public void ViewportProxy_ScrollTo_ShouldCallViewport()
        {
            _changedProperties.Clear();
            _viewport1Mock.ResetCalls();            
            _textViewModel.ScrollTo(new RowsScrollPosition {FirstSegment = _segmentMock.Object, RowsBeforeScrollCount = ScrollSegmentOffset });
            _viewport1Mock.Setup(p => p.ScrollTo(
                It.Is<RowsScrollPosition>(s => s.FirstSegment == _segmentMock.Object && s.RowsBeforeScrollCount == ScrollSegmentOffset)));

            // Should Make content
            _viewport1Mock.Verify(p => p.MakeContent(It.Is<IContentWriter>(s => s == _contentWriterMock.Object), It.Is<int>(s => s == Width1)), Times.Once);
            Assert.AreEqual(Content1, _textViewModel.Content);
            Assert.IsTrue(_changedProperties.Contains(nameof(_textViewModel.Content)));
        }
        #endregion
    }
}
