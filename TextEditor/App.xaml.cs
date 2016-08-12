using System;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
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
        private Progress<string> _progress;

        MainWindowViewModel _mainWindowViewModel;
        MainWindow _mainWindow;
        private string _path;

        private void OnStartup(object sender, StartupEventArgs e)
        {
            _path = e.Args.Length > 0 ? e.Args[0] : null;
            if (_path == null)
            {
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() != true)
                {
                    Current.Shutdown();
                    return;
                }
                _path = openFileDialog.FileName;
            }

            _mainWindowViewModel = new MainWindowViewModel();
            _mainWindow = new MainWindow {DataContext = _mainWindowViewModel};
            _mainWindow.Show();

            _cancellationTokenSource = new CancellationTokenSource();
            _progress = new Progress<string>(p => _mainWindowViewModel.Status = p);
            StartLoadAsync(_path);
        }

        private async void StartLoadAsync(string filePath)
        {
            Document document;
            try
            {
                document = await new FileDocumentBuilder().LoadAsync(filePath, _cancellationTokenSource.Token, _progress);
            }
            catch (System.IO.IOException e)
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
            catch (Exception e)
            {
                MessageBox.Show(_mainWindow, $"{e.Message}\n when opening file:\n{_path}", "Error"
                    , MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }

            _mainWindowViewModel.Status = "";
            _mainWindowViewModel.TextViewModel.Init(document, new RowWriter());
            _cancellationTokenSource = null;
            _progress = null;
        }
    }
}