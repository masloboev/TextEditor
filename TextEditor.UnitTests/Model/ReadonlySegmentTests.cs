using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextEditor.Model;

namespace TextEditor.UnitTests.Model
{
    [TestClass]
    public class ReadonlySegmentTests
    {
        [TestMethod]
        public void Constructor_AllDataProvided_ShouldFillAllFields()
        {
            var data = "0123".ToCharArray();
            var segment = new ReadonlySegment(data, 1, 2, true, true);
            Assert.AreEqual(1, segment.BeginPosition);
            Assert.AreEqual(true, segment.IsMonoWord);
            Assert.AreEqual(true, segment.EndsWithNewLine);
            Assert.AreEqual(2, segment.Length);
            Assert.AreSame(data, segment.RowData);
        }
    }
}
