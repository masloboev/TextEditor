using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TextEditor.Model;

namespace TextEditor.UnitTests
{
    [TestClass]
    public class StreamDocumentBuilderTests
    {
        [TestMethod]
        public void LoadAsync_Initialization_ShouldCreateDocument()
        {
            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes("Sample text")))
            using (var streamReader = new StreamReader(memoryStream))
            {
                var segmentMock = new Mock<ISegment>();
                var segmentsList = new List<ISegment> {segmentMock.Object};
                var segmentizerMock = new Mock<ISegmentizer>();
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;
                var progressMock = new Mock<IProgress<string>>();
                var documentMock = new Mock<IDocument>();

                segmentizerMock.Setup(p => p.SegmentAsync(
                    // ReSharper disable once AccessToDisposedClosure due to used locally
                    It.Is<StreamReader>(s => s == streamReader),
                    It.Is<CancellationToken>(s => s == cancellationToken),
                    It.Is<IProgress<string>>(s => s == progressMock.Object))).Returns(Task.Run(() => segmentsList, cancellationToken));


                var moduleFactoryMock = new Mock<IModuleFactory>();
                moduleFactoryMock.Setup(p => p.MakeSegmentizer(
                                            It.Is<int>(s => s == Constants.SegmentizerLowerThreshold),
                                            It.Is<int>(s => s == Constants.SegmentizerUpperThreshold),
                                            It.Is<int>(s => s == Constants.SegmentizerErrorThreshold))).Returns(segmentizerMock.Object);

                moduleFactoryMock.Setup(p => p.MakeDocument(It.Is<List<ISegment>>(s => s == segmentsList))).Returns(documentMock.Object);
                
                var streamDocumentBuilder = new StreamDocumentBuilder();
                var document = streamDocumentBuilder.LoadAsync(moduleFactoryMock.Object, streamReader, cancellationToken, progressMock.Object).Result;

                Assert.AreSame(documentMock.Object, document);

                moduleFactoryMock.Verify(p => p.MakeSegmentizer(
                                            It.Is<int>(s => s == Constants.SegmentizerLowerThreshold),
                                            It.Is<int>(s => s == Constants.SegmentizerUpperThreshold),
                                            It.Is<int>(s => s == Constants.SegmentizerErrorThreshold)), Times.Once);

                segmentizerMock.Verify(p => p.SegmentAsync(
                    // ReSharper disable once AccessToDisposedClosure due to used locally
                    It.Is<StreamReader>(s => s == streamReader),
                    It.Is<CancellationToken>(s => s == cancellationToken),
                    It.Is<IProgress<string>>(s => s == progressMock.Object)), Times.Once);

                progressMock.Verify(p => p.Report(It.IsAny<string>()), Times.AtLeastOnce);
            }
        }
    }
}
