using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;

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
                new SysCfgItem { Key = "SrNm", DisplayName = "Serial Number", IsHex = false},
                new SysCfgItem { Key = "WMac", DisplayName = "Wi-Fi MAC Address", IsHex = true},
                new SysCfgItem { Key = "BMac", DisplayName = "Bluetooth MAC Address", IsHex = true}
            };

            _CarveSysCfgWorker = new BackgroundWorker();
            _CarveSysCfgWorker.DoWork += CarveSysCfgWorker_DoWork;
            _CarveSysCfgWorker.ProgressChanged += CarveSysCfgWorker_ProgressChanged;
            _CarveSysCfgWorker.RunWorkerCompleted += CarveSysCfgWorker_RunWorkerCompleted;
            _CarveSysCfgWorker.WorkerReportsProgress = true;
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
            // Read the file
            _BootFileDump = File.ReadAllBytes(_BootFilePath);

            // Validate for carving eligibility
            _BoyerMoore.SetPattern(Encoding.ASCII.GetBytes(StringReverse("SCfg")));

            if (_BoyerMoore.Search(_BootFileDump) == -1)
            {
                MessageBox.Show(this, "This file does not contain SysCfg data.", "Unable", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Start carving
            if (!_CarveSysCfgWorker.IsBusy)
            {
                _CarveSysCfgWorker.RunWorkerAsync();
            }
        }

        private void CarveSysCfgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Iterate all keys and attempt carving
            foreach (SysCfgItem item in _SysCfg)
            {
                byte[] keyBytes = Encoding.ASCII.GetBytes(StringReverse(item.Key));

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

        private void CarveSysCfgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            CarveProgress.Value = e.ProgressPercentage;
        }

        private void CarveSysCfgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private string StringReverse(string s)
        {
            char[] charArr = s.ToCharArray();
            Array.Reverse(charArr);
            return new string(charArr);
        }
    }
}
