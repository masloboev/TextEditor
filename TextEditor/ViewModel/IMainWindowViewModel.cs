using TextEditor.Attributes;

namespace TextEditor.ViewModel
{
    /// <summary>
    /// MainViewModel interface
    /// </summary>
    public interface IMainWindowViewModel
    {
        /// <summary>
        /// Initializes the viewmodel.
        /// </summary>
        /// <param name="textViewModel">The TextViewModel.</param>
        void Init([NotNull] ITextViewModel textViewModel);

        /// <summary>
        /// The status.
        /// </summary>
        string Status { get; set; }

        /// <summary>
        /// The TextViewModel.
        /// </summary>
        ITextViewModel TextViewModel { get; }
    }
}
