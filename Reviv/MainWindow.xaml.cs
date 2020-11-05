using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace Reviv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _BootFilePath;
        public MainWindow()
        {
            InitializeComponent();

            _BootFilePath = null;
        }

        private void SelectBootFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Binary Files (*.bin)|*.bin|Dump Files (*.dump)|*.dump|All Files (*.*)|*.*";
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (openFileDialog.ShowDialog() == true)
            {
                _BootFilePath = openFileDialog.FileName;

                BootFileLabel.Content = Path.GetFileName(_BootFilePath);
                Riviv.Visibility = Visibility.Visible;
            }
        }
    }
}
