using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;

namespace SimpleHash
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region private fields

        private string __FileName;
        private int __Percent;
        private bool __CalculationInProgress;
        private string __PercentUi;
        private bool __SelectFileEnable;

        private CancellationTokenSource __TokenSource;
        private string __HashTypeUi;
        private string __HashUi;
        private Visibility __ResultVisibility;

        #endregion

        #region properties

        public string FileName
        {
            get { return __FileName; }
            set
            {
                __FileName = value;
                OnPropertyChanged();
            }
        }

        public int Percent
        {
            get { return __Percent; }
            set
            {
                __Percent = value;

                PercentUi = __Percent + "%";
                OnPropertyChanged();
            }
        }

        public string HashTypeUi
        {
            get { return __HashTypeUi; }
            set
            {
                __HashTypeUi = value;
                OnPropertyChanged();
            }
        }

        public string HashUi
        {
            get { return __HashUi; }
            set
            {
                __HashUi = value;
                OnPropertyChanged();
            }
        }

        public string PercentUi
        {
            get { return __PercentUi; }
            set
            {
                __PercentUi = value;
                OnPropertyChanged();
            }
        }

        public bool CalculationInProgress
        {
            get { return __CalculationInProgress; }
            set
            {
                __CalculationInProgress = value;

                SelectFileEnable = !__CalculationInProgress;

                OnPropertyChanged();
            }
        }

        public bool SelectFileEnable
        {
            get { return __SelectFileEnable; }
            set
            {
                __SelectFileEnable = value;
                OnPropertyChanged();
            }
        }

        public Visibility ResultVisibility
        {
            get { return __ResultVisibility; }
            set
            {
                __ResultVisibility = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region constructors

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            Loaded += MainWindow_Loaded;
            Drop += Window_OnDrop;
        }

        #endregion

        #region private methods

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CalculationInProgress = false;

            foreach (var type in ((HashType[])Enum.GetValues(typeof(HashType))))
                ComboBoxHashes.Items.Add(type.GetString());

            ComboBoxHashes.SelectedIndex = 0;
            ResultVisibility = Visibility.Collapsed;
        }

        private void Window_OnDrop(object sender, DragEventArgs e)
        {
            if (CalculationInProgress)
                return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 0)
                    ProcessFile(files[0]);
            }
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
                ProcessFile(openFileDialog.FileName);
        }

        private async void ProcessFile(string path)
        {
            var hashType = (ComboBoxHashes.SelectedItem as string).GetHashTypeByString();

            HashTypeUi = hashType.GetString();

            TextBlockLogo.Visibility = Visibility.Collapsed;
            CalculationInProgress = true;
            ResultVisibility = Visibility.Collapsed;

            var hash = string.Empty;
            var errorMessage = string.Empty;

            UpdateFileName(path);

            __TokenSource = new CancellationTokenSource();

            var token = __TokenSource.Token;

            var callBack = new Action<int>(p => Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                Percent = p;
            })));

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    hash = Hash.Calculate(path, hashType, callBack, token);
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                }
            });

            CalculationInProgress = false;

            HashUi = hash;

            if (string.IsNullOrEmpty(errorMessage))
            {
                if (!string.IsNullOrEmpty(hash))
                    ResultVisibility = Visibility.Visible;
                else
                    TextBlockLogo.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show(errorMessage, "Error");
                TextBlockLogo.Visibility = Visibility.Collapsed;
            }

        }

        private void UpdateFileName(string path)
        {
            try
            {
                var fileInfo = new FileInfo(path);
                FileName = fileInfo.Name;
            }
            catch
            {
            }
        }

        private void AbortClick(object sender, RoutedEventArgs e)
        {
            if (__TokenSource != null)
                __TokenSource.Cancel();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
