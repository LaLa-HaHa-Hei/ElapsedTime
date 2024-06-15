using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace ElapsedTime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ExeItem> ExeItemList { get; set; } = ((App)App.Current).ExeItemList;
        private readonly App _app = (App)App.Current;
        readonly DispatcherTimer _refreshListBoxTimer = new();
        public bool CloseWindow = false;
        public Dispatcher UiDispatcher = Dispatcher.CurrentDispatcher;
        private SortDescription _sortDescription;
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            _refreshListBoxTimer.Interval = TimeSpan.FromSeconds(_app.GlobalSettings.RefreshListBoxIntervalList_s[_app.GlobalSettings.RefreshListBoxSelectedIndex]);
            _refreshListBoxTimer.Tick += (sender, arg) => { RefreshListBox(); };
            RefreshListBoxIntervalComboBox.ItemsSource = _app.GlobalSettings.RefreshListBoxIntervalList_s;
            TimeListBox.ItemsSource = _app.ExeItemList;
            _sortDescription = new SortDescription("Percentage", ListSortDirection.Descending);
        }

        private void RefreshListBox()
        {
            TotalTimeTextBlock.Text = "总时间：" + ExeItem.SecondToTime(_app.TotalSecond);
            if (_app.TotalSecond == 0) { return; } // 防止0做除数
            foreach (var item in _app.ExeItemList)
            {
                item.Percentage = item.Seconds * 100 / _app.TotalSecond;
                item.TimeText = ExeItem.SecondToTime(item.Seconds);
            }
            TimeListBox.Items.SortDescriptions.Add(_sortDescription);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshListBox();

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CloseWindow == true)
            {
                e.Cancel = false;
            }
            else
            {
                Hide();
                _refreshListBoxTimer.Stop();
                e.Cancel = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_app.GlobalSettings.RefreshListBoxIntervalList_s[_app.GlobalSettings.RefreshListBoxSelectedIndex] > 0)
            {
                _refreshListBoxTimer.Start();
            }
            RefreshListBoxIntervalComboBox.SelectedIndex = _app.GlobalSettings.RefreshListBoxSelectedIndex;
        }

        private void RefreshListBoxIntervalComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if ((int)RefreshListBoxIntervalComboBox.SelectedValue <= 0)
            {
                _refreshListBoxTimer.Stop();
            }
            else
            {
                _refreshListBoxTimer.Interval = TimeSpan.FromSeconds((int)RefreshListBoxIntervalComboBox.SelectedValue);
                _refreshListBoxTimer.Start();
            }
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                RefreshListBox();
                _refreshListBoxTimer.Start();
            }
            else
            {
                _refreshListBoxTimer.Stop();
            }
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void OpenInstallationDirectoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", $"/select,{System.Windows.Forms.Application.ExecutablePath}");
        }

        /*private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)//没用，不触发
        {
            Debug.WriteLine("=================");
            var failedImage = (System.Windows.Controls.Image)sender;
            BitmapImage fallbackImage = new BitmapImage();
            fallbackImage.BeginInit();
            fallbackImage.UriSource = new Uri("pack://application:,,,/ElapsedTime;component/img/unknowfile.png", UriKind.Absolute);
            fallbackImage.CacheOption = BitmapCacheOption.OnLoad; // 加载完立即释放资源
            fallbackImage.EndInit();
            failedImage.Source = fallbackImage;
        }*/
    }
}

