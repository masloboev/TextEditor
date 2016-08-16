using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextEditor.SupportModel;

namespace TextEditor.UnitTests.SupportModel
{
    [TestClass]
    public class RowTests
    {
        [TestMethod]
        public void Length_OneSymbol_Should1()
        {
            var row = new Row("0".ToCharArray(), 0, 0, true, false);
            Assert.AreEqual(1, row.Length);
        }
        [TestMethod]
        public void Length_TwoSymbolEndPosition_Should1()
        {
            var row = new Row("01".ToCharArray(), 0, 0, true, false);
            Assert.AreEqual(1, row.Length);
        }
        [TestMethod]
        public void Length_TwoSymbolBeginPosition_Should1()
        {
            var row = new Row("01".ToCharArray(), 1, 1, true, false);
            Assert.AreEqual(1, row.Length);
        }
        [TestMethod]
        public void Length_TwoSymbol_Should2()
        {
            var row = new Row("01".ToCharArray(), 0, 1, true, false);
            Assert.AreEqual(2, row.Length);
        }

        [TestMethod]
        public void Constructor_AllDataProvided_ShouldFillAllFields()
        {
            var data = "0123".ToCharArray();
            var row = new Row(data, 1, 2, true, true);
            Assert.AreEqual(1, row.BeginPosition);
            Assert.AreEqual(true, row.IsMonoWord);
            Assert.AreEqual(true, row.EndsWithNewLine);
            Assert.AreEqual(2, row.Length);
            Assert.AreSame(data, row.RowData);
        }
    }
}
