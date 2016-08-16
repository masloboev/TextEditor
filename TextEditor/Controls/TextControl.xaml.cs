using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TextEditor.Commands;

namespace TextEditor.Controls
{
    /// <summary>
    /// Interaction logic for TextControl.xaml
    /// </summary>
    public partial class TextControl
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public TextControl()
        {
            var typeface = new Typeface(new FontFamily("Courier New"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            var formattedText = new FormattedText("0", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, 12, Brushes.Black);
            RowHeight = formattedText.Height;
            _letterWidth = formattedText.Width;

            InitializeComponent();
        }

        /// <summary>
        ///     Height of the one line of text in control
        /// </summary>
        public double RowHeight { get; }

        /// <summary>
        ///     Width of the one text letter in control
        /// </summary>
        private readonly double _letterWidth;

        /// <summary>
        ///     The method that is called when the text control changes size
        /// </summary>
        /// <param name="sender">control that changes size. Text control</param>
        /// <param name="e">The event data.</param>
        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e) => UpdateSize();

        /// <summary>
        ///     Updates text control size in letters.
        ///     Changes will be transferred to TextViewModel
        /// </summary>
        private void UpdateSize()
        {
            SymbolsInRowCount = (int)(ContentItem.ActualWidth / _letterWidth);
            RowsCount = (int)(ContentItem.ActualHeight / RowHeight);
        }

        /// <summary>
        ///     Dependency property for control height in letters.
        /// </summary>
        public static readonly DependencyProperty RowsCountProperty =
            DependencyProperty.Register(nameof(RowsCount), typeof(int), typeof(TextControl), new FrameworkPropertyMetadata());
        
        /// <summary>
        ///     Dependency property wrapper for control.
        /// </summary>
        public int RowsCount
        {
            get { return (int)GetValue(RowsCountProperty); }
            set { SetValue(RowsCountProperty, value); }
        }

        /// <summary>
        ///     Dependency property for control width in letters.
        /// </summary>
        public static readonly DependencyProperty SymbolsInRowCountProperty =
            DependencyProperty.Register(nameof(SymbolsInRowCount), typeof(int), typeof(TextControl), new FrameworkPropertyMetadata());

        /// <summary>
        ///     Dependency property wrapper for control.
        /// </summary>
        public int SymbolsInRowCount
        {
            get { return (int)GetValue(SymbolsInRowCountProperty); }
            set { SetValue(SymbolsInRowCountProperty, value); }
        }

        /// <summary>
        ///     The method that is called when the text control is loaded
        /// </summary>
        /// <param name="sender">control that changes size. Text control</param>
        /// <param name="e">The event data.</param>
        private void TextControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateSize();
            var window = Window.GetWindow(this);
            if (window != null)
            { // we make subscription this way because no childs raises preview input events
                window.KeyDown += TextControl_OnKeyDown;
                window.MouseWheel += TextBlockList_OnMouseWheel;
            }
        }

        /// <summary>
        ///     The method that is called when key is down in the text control
        /// </summary>
        /// <param name="sender">control that changes size. Text control</param>
        /// <param name="e">The event data.</param>
        private void TextControl_OnKeyDown(object sender, KeyEventArgs e)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (e.Key)
            {
                case Key.Down:
                    e.Handled = true;
                    TextControlCommands.LineDown.Execute(null, this);
                    break;
                case Key.Up:
                    e.Handled = true;
                    TextControlCommands.LineUp.Execute(null, this);
                    break;
                case Key.PageDown:
                    e.Handled = true;
                    TextControlCommands.PageDown.Execute(null, this);
                    break;
                case Key.PageUp:
                    e.Handled = true;
                    TextControlCommands.PageUp.Execute(null, this);
                    break;
            }
        }

        /// <summary>
        ///     The method that is called when mouse wheels in the text control
        /// </summary>
        /// <param name="sender">control that changes size. Text control</param>
        /// <param name="e">The event data.</param>
        private void TextBlockList_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                TextControlCommands.LineUp.Execute(null, this);
            else
                TextControlCommands.LineDown.Execute(null, this);
            e.Handled = true;
        }
    }
}
