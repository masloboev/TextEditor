using System.Windows.Input;

namespace TextEditor.Commands
{
    /// <summary>
    ///     Application commands container
    /// </summary>
    public class TextControlCommands
    {
        public static RoutedUICommand LineDown { get; } = new RoutedUICommand(nameof(LineDown), nameof(LineDown), typeof(TextControlCommands));
        public static RoutedUICommand LineUp { get; }  = new RoutedUICommand(nameof(LineUp), nameof(LineUp), typeof(TextControlCommands));

        public static RoutedUICommand PageDown { get; } = new RoutedUICommand(nameof(PageDown), nameof(PageDown), typeof(TextControlCommands));
        public static RoutedUICommand PageUp { get; } = new RoutedUICommand(nameof(PageUp), nameof(PageUp), typeof(TextControlCommands));
    }
}
