using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using TextEditor.Attributes;
using TextEditor.Controls;
using TextEditor.Exceptions;
using TextEditor.Model;
using TextEditor.ViewModel;

namespace TextEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private CancellationTokenSource _cancellationTokenSource;
        private IProgress<string> _progress;

        private readonly IModuleFactory _moduleFactory = new ModuleFactory();
        private IMainWindowViewModel _mainWindowViewModel;
        private MainWindow _mainWindow;
        private string _path;
        private Task _loadingDocumentTask;

        /// <summary>
        /// Called when Application starts.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event data.</param>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            _path = e.Args.Length > 0 ? e.Args[0] : null;
            if (_path == null)
            { // Open file if no filepath provided
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() != true)
                { // Shutdown if file wasn't selected
                    Current.Shutdown();
                    return;
                }
                _path = openFileDialog.FileName;
            }

            _mainWindowViewModel = new MainWindowViewModel();
            _mainWindowViewModel.Init(_moduleFactory.MakeTextViewModel());

            _mainWindow = new MainWindow { DataContext = _mainWindowViewModel };
            _mainWindow.Closing += MainWindowOnClosing;
            _mainWindow.Show();

            _cancellationTokenSource = new CancellationTokenSource();
            _progress = new Progress<string>(p => _mainWindowViewModel.Status = p);

            _loadingDocumentTask = StartLoadAsync(_path);
        }

        /// <summary>
        /// Main windwo closing callback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="cancelEventArgs">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void MainWindowOnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            // Try cancel loading
            _cancellationTokenSource?.Cancel();
            // if task is in progress then it will be cancelled and I close the application there
            _loadingDocumentTask.ContinueWith(task => Current.Shutdown());
            cancelEventArgs.Cancel = !_loadingDocumentTask.IsCompleted;
        }

        /// <summary>
        /// Starts the load asynchronous.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>
        /// async task
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        [return: NotNull]
        private async Task StartLoadAsync([NotNull] string filePath)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            IDocument document;
            try
            {
                using (var fileStream = new StreamReader(filePath))
                {
                    document = await _moduleFactory.MakeDocumentBuilder().LoadAsync(_moduleFactory, fileStream, _cancellationTokenSource.Token, _progress);
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(_mainWindow, $"We have problem with file:\n{_path}\n{e.Message}", "Error"
                    , MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }
            catch (TooBigWordException e)
            {
                MessageBox.Show(_mainWindow, $"We have problem with file:\n{_path}\n{e.Message}", "Error"
                    , MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show(_mainWindow, $"File:\n{_path}\nis too big", "Error"
                    , MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }
            catch (OperationCanceledException)
            {
                Current.Shutdown();
                return;
            }
            catch (Exception e)
            {
                MessageBox.Show(_mainWindow, $"{e.Message}\n when opening file:\n{_path}", "Error"
                    , MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            _mainWindowViewModel.Status = "";
            _mainWindowViewModel.TextViewModel.Init(document, _moduleFactory.MakeContentWriter(), _moduleFactory, _moduleFactory.MakeScrollBarViewModel());
            _progress = null;
        }
    }
}