using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TextEditor.Attributes;

namespace TextEditor.ViewModel
{
    /// <summary>
    ///     Viewmodel for main window
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged, IMainWindowViewModel
    {
        /// <summary>
        /// The TextViewModel
        /// </summary>
        private ITextViewModel _textViewModel;

        /// <summary>
        /// Initializes the viewmodel.
        /// </summary>
        /// <param name="textViewModel">The TextViewModel.</param>
        /// <remarks>Separated to method due to designer</remarks>
        public void Init([NotNull] ITextViewModel textViewModel)
        {
            if (textViewModel == null) throw new ArgumentNullException(nameof(textViewModel));

            _textViewModel = textViewModel;
        }
        /// <summary>
        ///     TextViewModel property
        /// </summary>
        public ITextViewModel TextViewModel
        {
            get { return _textViewModel; }
            set { _textViewModel = value; OnPropertyChanged(); }
        }

        /// <summary>
        ///     Status. Used for displaing loading progress
        /// </summary>
        private string _status;

        /// <summary>
        /// Status. Used for displaing loading progress
        /// </summary>
        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged(); }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
