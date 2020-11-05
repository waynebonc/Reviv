using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public MainWindow()
        {
            InitializeComponent();

            _BootFilePath = null;

            _SysCfg = new List<SysCfgItem>
            {
                new SysCfgItem { Key = "SrNm", DisplayName = "Serial Number", IsHex = false},
                new SysCfgItem { Key = "WMac", DisplayName = "Wi-Fi MAC Address", IsHex = true},
                new SysCfgItem { Key = "BMac", DisplayName = "Bluetooth MAC Address", IsHex = true}
            };
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
            byte[] dump = File.ReadAllBytes(_BootFilePath);

            byte[] syscfgItem = Encoding.ASCII.GetBytes(StringReverse("SrNm"));

            var bm = new BoyerMoore();

            bm.SetPattern(syscfgItem);

            var index = bm.Search(dump);
        }

        private string StringReverse(string s)
        {
            char[] charArr = s.ToCharArray();
            Array.Reverse(charArr);
            return new string(charArr);
        }
    }
}
