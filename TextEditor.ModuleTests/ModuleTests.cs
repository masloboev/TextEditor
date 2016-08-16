using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextEditor.Attributes;
using TextEditor.UnitTests.Utils;
using TextEditor.ViewModel;

namespace TextEditor.ModuleTests
{
    [TestClass]
    public class ModuleTests
    {
        private ModuleFactory _moduleFactory;
        private MainWindowViewModel _mainWindowViewModel;
        private TextViewModel _textViewModel;
        private ScrollBarViewModel _scrollBarViewModel;
        private ContentWriter _contentWriter;

        [TestInitialize]
        public void Initialize()
        {
            _moduleFactory = new ModuleFactory();

            _mainWindowViewModel = (MainWindowViewModel)_moduleFactory.MakeMainWindowViewModel();
            _textViewModel = (TextViewModel)_moduleFactory.MakeTextViewModel();
            _scrollBarViewModel = (ScrollBarViewModel) _moduleFactory.MakeScrollBarViewModel();
            _contentWriter = (ContentWriter)_moduleFactory.MakeContentWriter();
            _mainWindowViewModel.Init(_textViewModel);

            _textViewModel.RowsCount = 10;
            _textViewModel.SymbolsInRowCount = 20;
        }

        [TestMethod]
        public async Task EmptyDocument_NoErrorsEmptyContent()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var progress = new Progress<string>(p => _mainWindowViewModel.Status = p);
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("")))
            using (var streamReader = new StreamReader(stream))
            {
                var document = await _moduleFactory.MakeDocumentBuilder().LoadAsync(_moduleFactory, streamReader, cancellationTokenSource.Token, progress);
                _mainWindowViewModel.Status = "";
                _textViewModel.Init(document, _moduleFactory.MakeContentWriter(), _moduleFactory, _scrollBarViewModel);
            }

            Assert.AreEqual("", _textViewModel.Content);
            Assert.IsFalse(_scrollBarViewModel.IsEnabled);
        }

        [TestMethod]
        public async Task ShortDocument_NoErrorsAllContentInView()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var progress = new Progress<string>(p => _mainWindowViewModel.Status = p);
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("Very\nShort document\n")))
            using (var streamReader = new StreamReader(stream))
            {
                var document = await _moduleFactory.MakeDocumentBuilder().LoadAsync(_moduleFactory, streamReader, cancellationTokenSource.Token, progress);
                _mainWindowViewModel.Status = "";
                _textViewModel.Init(document, _moduleFactory.MakeContentWriter(), _moduleFactory, _scrollBarViewModel);
            }

            Assert.AreEqual("Very¶\r\nShort·document¶↓\r\n", _textViewModel.Content);
            Assert.IsFalse(_scrollBarViewModel.IsEnabled);
        }

        /// <summary>
        /// Verifies that content is all visible in viewport.
        /// </summary>
        /// <param name="content">The content.</param>
        private void VerifyViewportContentSize([NotNull] string content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var contentStrings = content.Replace("\r\n", "\n").Split('\n');
            Assert.AreEqual(_textViewModel.RowsCount, contentStrings.Length - 1);
            Assert.AreEqual("", contentStrings[contentStrings.Length - 1]);
            // verify each string length
            foreach (var s in contentStrings)
                Assert.IsTrue(s.Length <= _textViewModel.SymbolsInRowCount);
        }

        /// <summary>
        /// Recover original text from content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="firstLineLength">Length of the first content line.</param>
        /// <returns>original text</returns>
        [return: NotNull]
        private string GetContentText([NotNull] string content, out int firstLineLength)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var sb = new StringBuilder(content);
            sb.Replace("   →", "\t");
            sb.Replace("  →", "\t");
            sb.Replace(" →", "\t");
            sb.Replace("→", "\t");
            sb.Replace("⇃", "");
            sb.Replace("↓", "");
            sb.Replace("⇣", "");

            firstLineLength = sb.ToString().IndexOf("\r\n", StringComparison.Ordinal);

            sb.Replace("\r\n", "");
            sb.Replace("·", " ");
            sb.Replace("¶", "\n");

            return sb.ToString();
        }

        /// <summary>
        /// Initializes the module with long text.
        /// </summary>
        /// <returns>
        /// Async task
        /// </returns>
        [return: NotNull]
        private async Task<string> InitializeLongTextAsync()
        {
            var text = new TextGenerator().GenerateText(new[] { "x", "ABCD EFGHIJKLMNOPQRSTUVWXYZ", "0123456789 02468", " ", "defghijklmnopq", "rstuvwyz", "\t", "\n" });

            var cancellationTokenSource = new CancellationTokenSource();
            var progress = new Progress<string>(p => _mainWindowViewModel.Status = p);
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(text)))
            using (var streamReader = new StreamReader(stream))
            {
                var document = await _moduleFactory.MakeDocumentBuilder().LoadAsync(_moduleFactory, streamReader, cancellationTokenSource.Token, progress);
                _mainWindowViewModel.Status = "";
                _textViewModel.Init(document, _contentWriter, _moduleFactory, _scrollBarViewModel);
            }
            return text;
        }

        [TestMethod]
        public async Task LongDocument_ScrollDownByLinesContentInView()
        {
            var text = await InitializeLongTextAsync();
            // await scrollbar calculation done
            var layout = await _scrollBarViewModel.SegmentsRowsLayoutProvider.GetAsync(_textViewModel.SymbolsInRowCount - _contentWriter.ReservedSymbolsCount, null);
            Assert.IsTrue(_scrollBarViewModel.IsEnabled);
            Assert.IsTrue(layout.TotalRowsCount > 0);

            var originalTextOffset = 0;
            string contentText = null;

            for (var i = 0L; i <= layout.TotalRowsCount - _textViewModel.RowsCount; i++)
            {
                Assert.AreEqual(i, _scrollBarViewModel.Value);
                var content = _textViewModel.Content;

                Assert.IsNotNull(content);
                VerifyViewportContentSize(content);

                int firstLineLength;
                contentText = GetContentText(content, out firstLineLength);
                Assert.IsTrue(firstLineLength > 0);

                Assert.IsTrue(text.Length - originalTextOffset >= contentText.Length);
                Assert.AreEqual(text.Substring(originalTextOffset, contentText.Length), contentText);

                originalTextOffset += firstLineLength;

                if (i % 2 == 0)
                    _textViewModel.LineDown();
                else
                    _scrollBarViewModel.Value++;
            }

            Assert.AreEqual(_scrollBarViewModel.Maximum, _scrollBarViewModel.Value);

            Assert.IsNotNull(contentText);
            Assert.IsTrue(text.EndsWith(contentText));
        }

        [TestMethod]
        public async Task LongDocument_ScrollUpByLinesAfterResizeContentInView()
        {
            var text = await InitializeLongTextAsync();
            var layout1 = await _scrollBarViewModel.SegmentsRowsLayoutProvider.GetAsync(_textViewModel.SymbolsInRowCount - _contentWriter.ReservedSymbolsCount, null);
            _textViewModel.SymbolsInRowCount += 10;
            _textViewModel.SymbolsInRowCount += 10;
            var layout2 = await _scrollBarViewModel.SegmentsRowsLayoutProvider.GetAsync(_textViewModel.SymbolsInRowCount - _contentWriter.ReservedSymbolsCount, null);

            //Should be less rows
            Assert.IsTrue(layout2.TotalRowsCount < layout1.TotalRowsCount);

            Assert.IsTrue(_scrollBarViewModel.IsEnabled);
            Assert.IsTrue(layout2.TotalRowsCount > 0);

            Assert.IsTrue(_scrollBarViewModel.Maximum > 0);
            _scrollBarViewModel.Value = _scrollBarViewModel.Maximum;

            var content = _textViewModel.Content;
            int firstLineLength;
            var contentText = GetContentText(content, out firstLineLength);
            Assert.IsTrue(text.EndsWith(contentText));
            var originalTextOffset = text.Length - contentText.Length;

            for (var i = layout2.TotalRowsCount - _textViewModel.RowsCount - 1; i > 0; i--)
            {
                if (i % 2 == 0)
                    _textViewModel.LineUp();
                else
                    _scrollBarViewModel.Value--;

                Assert.AreEqual(i, _scrollBarViewModel.Value);
                content = _textViewModel.Content;

                Assert.IsNotNull(content);
                VerifyViewportContentSize(content);
                
                contentText = GetContentText(content, out firstLineLength);
                Assert.IsTrue(firstLineLength > 0);

                originalTextOffset -= firstLineLength;
                Assert.IsTrue(originalTextOffset > 0);

                Assert.IsTrue(text.Length - originalTextOffset >= contentText.Length);
                Assert.AreEqual(text.Substring(originalTextOffset, contentText.Length), contentText);
            }

            _textViewModel.LineUp();
            Assert.AreEqual(0, _scrollBarViewModel.Value);
            content = _textViewModel.Content;

            Assert.IsNotNull(content);
            VerifyViewportContentSize(content);
            contentText = GetContentText(content, out firstLineLength);
            Assert.IsTrue(text.StartsWith(contentText));
        }

        [TestMethod]
        public async Task LongDocument_ScrollDownByPagesContentInView()
        {
            var text = await InitializeLongTextAsync();
            var layout = await _scrollBarViewModel.SegmentsRowsLayoutProvider.GetAsync(_textViewModel.SymbolsInRowCount - _contentWriter.ReservedSymbolsCount, null);
            Assert.IsTrue(_scrollBarViewModel.IsEnabled);
            Assert.IsTrue(layout.TotalRowsCount > 0);

            var originalTextOffset = 0;
            string contentText;
            string content;
            int firstLineLength;

            for (var i = 0L; i <= layout.TotalRowsCount - _textViewModel.RowsCount; i += _textViewModel.RowsCount)
            {
                Assert.AreEqual(i, _scrollBarViewModel.Value);
                content = _textViewModel.Content;

                Assert.IsNotNull(content);
                VerifyViewportContentSize(content);

                contentText = GetContentText(content, out firstLineLength);

                Assert.IsTrue(text.Length - originalTextOffset >= contentText.Length);
                Assert.AreEqual(text.Substring(originalTextOffset, contentText.Length), contentText);

                originalTextOffset += contentText.Length;

                _textViewModel.PageDown();
            }

            Assert.AreEqual(_scrollBarViewModel.Maximum, _scrollBarViewModel.Value);

            content = _textViewModel.Content;

            Assert.IsNotNull(content);
            VerifyViewportContentSize(content);

            contentText = GetContentText(content, out firstLineLength);
            Assert.IsTrue(text.EndsWith(contentText));
        }

        [TestMethod]
        public async Task LongDocument_ScrollUpByPagesContentInView()
        {
            var text = await InitializeLongTextAsync();
            var layout = await _scrollBarViewModel.SegmentsRowsLayoutProvider.GetAsync(_textViewModel.SymbolsInRowCount - _contentWriter.ReservedSymbolsCount, null);
            Assert.IsTrue(_scrollBarViewModel.IsEnabled);
            Assert.IsTrue(layout.TotalRowsCount > 0);

            Assert.IsTrue(_scrollBarViewModel.Maximum > 0);
            _scrollBarViewModel.Value = _scrollBarViewModel.Maximum;

            var content = _textViewModel.Content;
            int firstLineLength;
            var contentText = GetContentText(content, out firstLineLength);
            Assert.IsTrue(text.EndsWith(contentText));
            var originalTextOffset = text.Length - contentText.Length;

            for (var i = layout.TotalRowsCount - _textViewModel.RowsCount - _textViewModel.RowsCount; i > 0; i -= _textViewModel.RowsCount)
            {
                _textViewModel.PageUp();

                Assert.AreEqual(i, _scrollBarViewModel.Value);
                content = _textViewModel.Content;

                Assert.IsNotNull(content);
                VerifyViewportContentSize(content);

                contentText = GetContentText(content, out firstLineLength);
                Assert.IsTrue(firstLineLength > 0);

                originalTextOffset -= contentText.Length;
                Assert.IsTrue(originalTextOffset > 0);

                Assert.IsTrue(text.Length - originalTextOffset >= contentText.Length);
                Assert.AreEqual(text.Substring(originalTextOffset, contentText.Length), contentText);
            }

            _textViewModel.PageUp();
            Assert.AreEqual(0, _scrollBarViewModel.Value);
            content = _textViewModel.Content;

            Assert.IsNotNull(content);
            VerifyViewportContentSize(content);
            contentText = GetContentText(content, out firstLineLength);
            Assert.IsTrue(text.StartsWith(contentText));
        }
    }
}
