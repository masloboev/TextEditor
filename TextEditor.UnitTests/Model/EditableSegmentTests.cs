using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextEditor.Model;

namespace TextEditor.UnitTests.Model
{
    [TestClass]
    public class EditableSegmentTests
    {
        [TestMethod]
        public void Constructor_AllDataProvided_ShouldFillAllFields()
        {
            var segment = new EmptySegment();
            Assert.AreEqual(0, segment.BeginPosition);
            Assert.AreEqual(false, segment.IsMonoWord);
            Assert.AreEqual(true, segment.EndsWithNewLine);
            Assert.AreEqual(0, segment.Length);
            CollectionAssert.AreEqual(new char[0], segment.RowData);
        }
    }
}
