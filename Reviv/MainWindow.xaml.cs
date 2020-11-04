using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace Reviv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _DumpFilePath;
        public MainWindow()
        {
            InitializeComponent();

            _DumpFilePath = null;
        }

        private void SelectBootFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Binary Files (*.bin)|*.bin|Dump Files (*.dump)|*.dump|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                _DumpFilePath = openFileDialog.FileName;

                DumpFileLabel.Content = Path.GetFileName(_DumpFilePath);
                Riviv.Visibility = Visibility.Visible;
            }
        }
    }
}
