using TextEditor.Attributes;
using TextEditor.Model;

namespace TextEditor.ViewModel
{
    /// <summary>
    /// TextViewModel interface
    /// </summary>
    public interface ITextViewModel
    {
        /// <summary>
        /// Initializes the viewmodel.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="contentWriter">The content writer.</param>
        /// <param name="moduleFactory">The module factory.</param>
        /// <param name="scrollBarViewModel">The ScrollBarViewModel.</param>
        void Init([NotNull] IDocument document, [NotNull] IContentWriter contentWriter,
            [NotNull] IModuleFactory moduleFactory, [NotNull] IScrollBarViewModel scrollBarViewModel);

        /// <summary>
        /// Gets the ScrollBarViewModel.
        /// </summary>
        IScrollBarViewModel ScrollBarViewModel { get; }
    }
}
