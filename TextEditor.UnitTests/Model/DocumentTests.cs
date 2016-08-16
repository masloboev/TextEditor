using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TextEditor.Model;

namespace TextEditor.UnitTests.Model
{
    [TestClass]
    public class DocumentTests
    {
        /// <summary>
        /// Mocks segment that contains only length
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns>Mocked segment</returns>
        private static ISegment MakeSegment(int length) => Mock.Of<ISegment>(s => s.Length == length);

        [TestMethod]
        public void Construction_EmptyDocument_DocumentCorrectness()
        {
            var doc = new Document(new List<ISegment>());
            Assert.AreEqual(1, doc.SegmentsCount);
            Assert.IsNotNull(doc.FirstSegment);
            var segment = doc.FirstSegment;
            Assert.IsInstanceOfType(segment, typeof(EmptySegment));
            Assert.AreEqual(segment, doc.LastSegment);
            Assert.AreEqual(null, doc.Next(segment));
            Assert.AreEqual(null, doc.Prev(segment));
        }

        [TestMethod]
        public void Construction_1Segment_DocumentCorrectness()
        {
            var segment = MakeSegment(10);
            var doc = new Document(new List<ISegment> {segment});
            Assert.AreEqual(1, doc.SegmentsCount);
            Assert.AreEqual(segment, doc.FirstSegment);
            Assert.AreEqual(segment, doc.LastSegment);
            Assert.AreEqual(null, doc.Next(segment));
            Assert.AreEqual(null, doc.Prev(segment));
        }

        [TestMethod]
        public void Construction_ManySegment_DocumentCorrectness()
        {
            var segmentList = Enumerable.Range(1, 10).Select(MakeSegment).ToList();
            var doc = new Document(segmentList);
            Assert.AreEqual(segmentList.Count, doc.SegmentsCount);
            Assert.AreEqual(segmentList.First(), doc.FirstSegment);
            Assert.AreEqual(segmentList.Last(), doc.LastSegment);
            Assert.AreEqual(null, doc.Next(segmentList.Last()));
            for (var i = 0; i < segmentList.Count - 1; i++)
                Assert.AreEqual(segmentList[i + 1], doc.Next(segmentList[i]));

            Assert.AreEqual(null, doc.Prev(segmentList.First()));
            for (var i = 1; i < segmentList.Count; i++)
                Assert.AreEqual(segmentList[i - 1], doc.Prev(segmentList[i]));
        }
    }
}
