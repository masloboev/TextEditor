using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TextEditor.ViewModel;

namespace TextEditor.UnitTests.ViewModel
{
    [TestClass]
    public class MainWindowViewModelTests
    {
        private Mock<ITextViewModel> _textViewModelMock;

        private MainWindowViewModel _mainWindowViewModel;

        private HashSet<string> _changedProperties;

        [TestInitialize]
        public void Initialize()
        {
            _textViewModelMock = new Mock<ITextViewModel>();
            _changedProperties = new HashSet<string>();

            _mainWindowViewModel = new MainWindowViewModel();
            _mainWindowViewModel.Init(_textViewModelMock.Object);

            _mainWindowViewModel.PropertyChanged += (sender, args) => _changedProperties.Add(args.PropertyName);
        }

        [TestMethod]
        public void Init_Initialization_CheckInitialization()
        {
            Assert.AreSame(_textViewModelMock.Object, _mainWindowViewModel.TextViewModel);
        }

        [TestMethod]
        public void PropertyChanged_Status_ShouldRaiseStatusChanged()
        {
            _mainWindowViewModel.Status = "status";

            Assert.AreEqual("status", _mainWindowViewModel.Status);
            Assert.IsFalse(_changedProperties.Contains(nameof(MainWindowViewModel.TextViewModel)));
            Assert.IsTrue(_changedProperties.Contains(nameof(MainWindowViewModel.Status)));
        }

        [TestMethod]
        public void PropertyChanged_TextViewModel_ShouldRaiseTextViewModelChanged()
        {            
            var newTextViewModelMock = new Mock<ITextViewModel>();
            _mainWindowViewModel.TextViewModel = newTextViewModelMock.Object;

            Assert.AreSame(newTextViewModelMock.Object, _mainWindowViewModel.TextViewModel);
            Assert.IsTrue(_changedProperties.Contains(nameof(MainWindowViewModel.TextViewModel)));
            Assert.IsFalse(_changedProperties.Contains(nameof(MainWindowViewModel.Status)));
        }
    }
}
