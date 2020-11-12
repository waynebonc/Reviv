using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Reviv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _BootFilePath;
        private readonly List<SysCfgItem> _SysCfg;
        private BackgroundWorker _CarveSysCfgWorker;
        private BoyerMoore _BoyerMoore;
        private byte[] _BootFileDump;
        public MainWindow()
        {
            InitializeComponent();

            _BoyerMoore = new BoyerMoore();

            _BootFilePath = null;

            _SysCfg = new List<SysCfgItem>
            {
                new SysCfgItem { Key = "SrNm", DisplayName = "Serial Number", IsHex = false },
                new SysCfgItem { Key = "WMac", DisplayName = "Wi-Fi MAC Address", IsHex = true },
                new SysCfgItem { Key = "BMac", DisplayName = "Bluetooth MAC Address", IsHex = true },
                new SysCfgItem { Key = "Mod#", DisplayName = "Model Number", IsHex = false },
                new SysCfgItem { Key = "Regn", DisplayName = "Region", IsHex = false },
                new SysCfgItem { Key = "RMd#", DisplayName = "Model", IsHex = false }
            };

            _CarveSysCfgWorker = new BackgroundWorker();
            _CarveSysCfgWorker.DoWork += CarveSysCfgWorker_DoWork;
            _CarveSysCfgWorker.RunWorkerCompleted += CarveSysCfgWorker_RunWorkerCompleted;
            // _CarveSysCfgWorker.WorkerSupportsCancellation = true;
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

        private void Riviv_Click(object sender, RoutedEventArgs e)
        {
            // Check file size
            if (new FileInfo(_BootFilePath).Length > (1024 * 1024 * 25))
            {
                MessageBox.Show(this, "File is too big. Maximum allowed is 25MB.", "Unsupported", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Read the file
            try
            {
                _BootFileDump = File.ReadAllBytes(_BootFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not read selected file.\nMessage: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate for carving eligibility
            _BoyerMoore.SetPattern(Encoding.ASCII.GetBytes(StringReverse("SCfg")));

            if (_BoyerMoore.Search(_BootFileDump) == -1)
            {
                MessageBox.Show(this, "This file does not contain SysCfg data.", "Unsupported", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Start carving
            if (!_CarveSysCfgWorker.IsBusy)
            {
                _CarveSysCfgWorker.RunWorkerAsync();
                Mouse.OverrideCursor = Cursors.Wait;
            }
        }

        private void CarveSysCfgWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            // Iterate all keys and attempt carving
            foreach (SysCfgItem item in _SysCfg)
            {
                byte[] keyBytes;

                try
                {
                    keyBytes = Encoding.ASCII.GetBytes(StringReverse(item.Key));
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        MessageBox.Show(this, $"Something went wrong.\nMessage: {ex.Message}");
                    }));

                    e.Cancel = true;
                    return;
                }

                _BoyerMoore.SetPattern(keyBytes);

                int index =_BoyerMoore.Search(_BootFileDump);

                if (index != -1)
                {
                    // Offset acquisition by key length
                    int i = index + item.Key.Length;

                    List<byte> buffer = new List<byte>();

                    // Limit carving to 50 bytes just in case this location is corrupted
                    while (_BootFileDump[i] != 0 && buffer.Count < 50)
                    {
                        buffer.Add(_BootFileDump[i]);
                        i++;
                    }

                    item.RawValue = buffer.ToArray();
                }
            }
        }

        private void CarveSysCfgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                GreetingPanel.Visibility = Visibility.Collapsed;
                ResultPanel.Visibility = Visibility.Visible;

                foreach (SysCfgItem item in _SysCfg)
                {
                    CarvedSysCfgGrid.RowDefinitions.Add(new RowDefinition());

                    Label name = new Label();
                    name.Content = item.DisplayName;
                    CarvedSysCfgGrid.Children.Add(name);
                    Grid.SetRow(name, CarvedSysCfgGrid.RowDefinitions.Count - 1);
                    Grid.SetColumn(name, CarvedSysCfgGrid.ColumnDefinitions.Count - 2);

                    Label value = new Label();

                    if (item.RawValue != null)
                    {
                        value.Content = item.IsHex ? ByteArrToString(item.RawValue) : Encoding.ASCII.GetString(item.RawValue);
                    }
                    else
                    {
                        value.Content = "Acquisition Failed";
                        value.Foreground = System.Windows.Media.Brushes.Red;
                    }

                    CarvedSysCfgGrid.Children.Add(value);
                    Grid.SetRow(value, CarvedSysCfgGrid.RowDefinitions.Count - 1);
                    Grid.SetColumn(value, CarvedSysCfgGrid.ColumnDefinitions.Count - 1);
                }
            }
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void StartOver_Click(object sender, RoutedEventArgs e)
        {
            ResultPanel.Visibility = Visibility.Collapsed;
            GreetingPanel.Visibility = Visibility.Visible;

            CarvedSysCfgGrid.Children.Clear();
            CarvedSysCfgGrid.RowDefinitions.RemoveRange(1, CarvedSysCfgGrid.RowDefinitions.Count - 1);
        }

        private void SaveTxt_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.FileName = $"{Path.GetFileNameWithoutExtension(_BootFilePath)}.txt";
            saveDialog.Filter = "Text File (*.txt)|*.txt|All Files (*.*)|*.*";

            if (saveDialog.ShowDialog() == true)
            {
                string exportText = "";

                foreach (SysCfgItem item in _SysCfg)
                {
                    if (item.RawValue != null)
                    {
                        exportText += $"{item.DisplayName}: {(item.IsHex ? ByteArrToString(item.RawValue) : Encoding.ASCII.GetString(item.RawValue))}";
                    }
                    else
                    {
                        exportText += "N/A";
                    }

                    if (!item.Key.GetHashCode().Equals(_SysCfg[_SysCfg.Count - 1].Key.GetHashCode()))
                    {
                        exportText += "\n";
                    }
                }

                try
                {
                    File.WriteAllText(saveDialog.FileName, exportText);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Could not export SysCfg.\nMessage: {ex.Message}");
                }
            }
        }

        private string StringReverse(string s)
        {
            char[] charArr = s.ToCharArray();
            Array.Reverse(charArr);
            return new string(charArr);
        }

        public string ByteArrToString(byte[] arr)
        {
            return BitConverter.ToString(arr).Replace("-", "");
        }
    }
}
