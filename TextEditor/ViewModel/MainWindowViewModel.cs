using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TextEditor.ViewModel
{
    /// <summary>
    ///     Viewmodel for main window
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private TextViewModel _textViewModel = new TextViewModel();

        /// <summary>
        ///     TextViewModel property
        /// </summary>
        public TextViewModel TextViewModel
        {
            get { return _textViewModel; }
            set { _textViewModel = value; OnPropertyChanged(); }
        }

        /// <summary>
        ///     Status. Used for displaing loading progress
        /// </summary>
        private string _status;

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
