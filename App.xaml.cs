using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;


namespace ElapsedTime
{
    public class Settings
    {
        private long _jpegQuality = 20;
        ///<value>0~100，保存时JPEG图片的清晰度</value>
        public long JpegQuality
        {
            get => _jpegQuality;
            set
            {
                _jpegQuality = value;
                /*if (value < 0) { _jpegQuality = 0; }
                else if (value > 100) { _jpegQuality = 100; }
                else { _jpegQuality = value; }*/
            }
        }
        public string ExeIconFolderPath { set; get; } = string.Empty;
        public string ScreenshotFolderPath { set; get; } = string.Empty;
        public bool Screenshot { set; get; } = true;
        public int ScreenshotInterval_ms { set; get; } = 240_000;
        /// <value>永远大于等于0</value>
        public int GetTopWindowInterval_s { set; get; } = 1;
        public List<int> RefreshListBoxIntervalList_s = [];
        /// <value>永远大于等于0</value>
        public int RefreshListBoxSelectedIndex { set; get; } = 0;
        /// <summary>
        /// 还原为默认设置
        /// </summary>
        public void Default()
        {
            ScreenshotFolderPath = "*CurrentFolder*/screenshot";
            ExeIconFolderPath = "*CurrentFolder*/exe_icon";
            GetTopWindowInterval_s = 1;
            Screenshot = true;
            ScreenshotInterval_ms = 1000 * 60 * 4;
            JpegQuality = 10;
            ReplaceCurrentFolder(AppDomain.CurrentDomain.BaseDirectory);
            RefreshListBoxSelectedIndex = 1;
            RefreshListBoxIntervalList_s = [1, 3, 10, 0];
        }
        public void ReplaceCurrentFolder(string currentFolder)
        {
            ExeIconFolderPath = ExeIconFolderPath.Replace("*CurrentFolder*", currentFolder);
            ScreenshotFolderPath = ScreenshotFolderPath.Replace("*CurrentFolder*", currentFolder);
        }
    }
    public class ExeItem : INotifyPropertyChanged
    {
        public string Name { get; set; } = string.Empty;
        private string _exePath = string.Empty;
        public string ExePath
        {
            get => _exePath;
            set
            {
                _exePath = value;
                Name = Path.GetFileNameWithoutExtension(ExePath);
            }
        }
        public string IconPath { get; set; } = String.Empty;
        private int _percentage = 0;
        public int Percentage
        {
            get => _percentage;
            set
            {
                _percentage = value;
                OnPropertyChanged(nameof(Percentage));
            }
        }
        private string _timeText = string.Empty;
        public string TimeText
        {
            get => _timeText;
            set
            {
                _timeText = value;
                OnPropertyChanged(nameof(TimeText));
            }
        }
        public int Seconds { get; set; } = 0;
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 将总秒数转换为“x小时x分钟x秒”的形式
        /// </summary>
        public static string SecondToTime(int second)
        {
            string time = "";
            int hour = second / 3600;
            int minute = second % 3600 / 60;
            if (hour != 0) { time = hour.ToString() + "小时"; }
            if (minute != 0) { time = time + minute.ToString() + "分钟"; }
            time = time + (second % 60).ToString() + "秒";
            return time;
        }
    }
    /*    public class AccurateTimer
        {
            public AccurateTimer()
            {
                _timer = new(new TimerCallback(TimerProc), null, -1, 0);
                //系统睡眠事件
                SystemEvents.PowerModeChanged += PowerModeChangedEventHandler;
                IsRunning = false;
            }
            private readonly System.Threading.Timer _timer;
            private DateTime _beginTime;
            private long _times = 0;
            public event EventHandler? Tick;
            public bool IsRunning { get; private set; }
            private int _interval_ms;
            public int Interval_ms
            {
                get => _interval_ms;
                set
                {
                    if (_interval_ms != value)
                    {
                        _interval_ms = value;
                        if (IsRunning == true)
                        {
                            Start();
                        }
                    }
                }
            }
            private void PowerModeChangedEventHandler(object sender, PowerModeChangedEventArgs e)
            {
                switch (e.Mode)
                {
                    //睡眠恢复后从_begingTime开始计算的理论等待时间就错了
                    case PowerModes.Resume: //1.操作系统即将从挂起状态继续。
                        if (IsRunning == true)
                        {
                            _beginTime = DateTime.Now;
                            _times = 0;
                            _timer.Change(Interval_ms, 0);
                        }
                        break;
                    //case PowerModes.StatusChange: //2.一个电源模式状态的通知事件已由操作系统引发。 这可能指示电池电力不足或正在充电、电源正由交流电转换为电池或相反，或系统电源状态的其他更改。
                    //    break;
                    case PowerModes.Suspend: //3.操作系统即将挂起。
                        _timer.Change(-1, 0);
                        break;
                }
            }
            private void TimerProc(object? state)
            {
                Tick?.Invoke(this, EventArgs.Empty);
                // 补偿误差
                _times++;
                int timerInterval = (int)(Interval_ms * (_times + 1) - (DateTime.Now - _beginTime).TotalMilliseconds);
                //Debug.WriteLine($"{_times}=={timerInterval}");
                _timer.Change((timerInterval > 0 ? timerInterval : 1), 0);
            }
            public void Pause()
            {
                IsRunning = false;
                _timer.Change(-1, 0);
            }
            public void Start()
            {
                IsRunning = true;
                _beginTime = DateTime.Now;
                _times = 0;
                _timer.Change(Interval_ms, 0);
            }
        }*/
    class AccurateDispatcherTimer
    {
        public AccurateDispatcherTimer()
        {
            _dispatcherTimer = new();
            _dispatcherTimer.Tick += Timer_Tick;
            //系统睡眠事件
            SystemEvents.PowerModeChanged += PowerModeChangedEventHandler;
            IsRunning = false;
        }

        private readonly System.Windows.Threading.DispatcherTimer _dispatcherTimer;
        private DateTime _beginTime;
        private long _times = 0;
        public event EventHandler? Tick;
        public bool IsRunning { get; private set; }
        private int _interval_ms;
        public int Interval_ms
        {
            get => _interval_ms;
            set
            {
                if (_interval_ms != value)
                {
                    _interval_ms = value;
                    _beginTime = DateTime.Now;
                    _times = 0;
                }
            }
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            Tick?.Invoke(this, EventArgs.Empty);
            // 补偿误差
            _times++;
            TimeSpan timeSpan = DateTime.Now - _beginTime;
            double timerInterval = Interval_ms * (_times + 1) - timeSpan.TotalMilliseconds;
            //Debug.WriteLine(timerInterval.ToString());
            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(timerInterval > 0 ? timerInterval : 1);
        }
        public void Start()
        {
            IsRunning = true;
            _beginTime = DateTime.Now;
            _times = 0;
            _dispatcherTimer.Start();
        }
        private void PowerModeChangedEventHandler(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                //睡眠恢复后从_begingTime开始计算的理论等待时间就错了
                case PowerModes.Resume: //1.操作系统即将从挂起状态继续。
                    if (IsRunning == true)
                    {
                        _beginTime = DateTime.Now;
                        _times = 0;
                        Start();
                    }
                    break;
                //case PowerModes.StatusChange: //2.一个电源模式状态的通知事件已由操作系统引发。 这可能指示电池电力不足或正在充电、电源正由交流电转换为电池或相反，或系统电源状态的其他更改。
                //    break;
                case PowerModes.Suspend: //3.操作系统即将挂起。
                    Pause();
                    break;
            }
        }
        public void Pause()
        {
            _dispatcherTimer.Stop();
        }
    }
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        [LibraryImport("user32.dll")]
        private static partial IntPtr GetForegroundWindow();
        [LibraryImport("user32.dll")]
        public static partial int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        public System.Windows.Forms.NotifyIcon TrayNotifyIcon; // 如果在此处new，会导致托盘变虚，窗口也变虚有黑框
        public ContextMenuStrip TrayContextMenuStrip;
        public Settings GlobalSettings = new();
        public int TotalSecond = 0;
        private readonly DateTime _today = DateTime.Now; // 记录开始运行时的日期，在经过一天后重启程序
        readonly AccurateDispatcherTimer _getTopWindowTimer = new();
        private readonly System.Threading.Timer _screenshotTimer;
        public ObservableCollection<ExeItem> ExeItemList { get; set; } = [];
        // 创建Encoder参数对象来指定JPEG的质量
        private readonly EncoderParameters _encoderParams = new(1);
        // 获取JPEG编码器信息
        private readonly ImageCodecInfo _jpegCodec = GetEncoderInfo("image/jpeg");
        public App()
        {
            _screenshotTimer = new(new TimerCallback(Screenshot), null, -1, 0); // 等加载完_settings后才能开始
            TrayNotifyIcon = new NotifyIcon();
            TrayContextMenuStrip = new();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            InitializeSettings();
            InitializeTray();

            _screenshotTimer.Change(0, GlobalSettings.ScreenshotInterval_ms);
            _getTopWindowTimer.Tick += TimerTick_GetTopwindow;
            _getTopWindowTimer.Interval_ms = GlobalSettings.GetTopWindowInterval_s * 1000;
            _getTopWindowTimer.Start();

            //ExeItemList.Add(new() { ExePath = "Others", IconPath = "pack://application:,,,/ElapsedTime;component/img/unknowfile.png" }); ;
            _encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, GlobalSettings.JpegQuality); // quality 是0-100之间的数值，100为最高质量，0为最低质量
        }


        private void InitializeSettings()
        {
            GlobalSettings.Default();
            if (!Directory.Exists(GlobalSettings.ExeIconFolderPath)) { Directory.CreateDirectory(GlobalSettings.ExeIconFolderPath); }
            if (!Directory.Exists(GlobalSettings.ScreenshotFolderPath + "/" + _today.ToString("yyyy-MM-dd"))) { Directory.CreateDirectory(GlobalSettings.ScreenshotFolderPath + "/" + _today.ToString("yyyy-MM-dd")); }
        }
        private void InitializeTray()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream? stream = assembly.GetManifestResourceStream("ElapsedTime.img.2_32.ico");
            if (stream != null) { TrayNotifyIcon.Icon = new Icon(stream); }
            else
            {
                Bitmap bitmap = new(16, 16);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    SolidBrush blueBrush = new(Color.FromArgb(255, 236, 161));
                    graphics.FillRectangle(blueBrush, 0, 0, 16, 16);
                }
                TrayNotifyIcon.Icon = System.Drawing.Icon.FromHandle(bitmap.GetHicon());
                bitmap.Dispose();
            }
            TrayNotifyIcon.MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (Current.MainWindow == null)
                    {
                        Current.MainWindow = new MainWindow();
                        Current.MainWindow.Show();
                        Current.MainWindow.Activate();
                    }
                    else
                    {
                        if (Current.MainWindow.Visibility == Visibility.Visible)
                        {
                            Current.MainWindow.Hide();
                        }
                        else
                        {
                            Current.MainWindow.Show();
                            Current.MainWindow.Activate();
                        }
                    }
                }
            };

            /*ToolStripMenuItem closeWindowItem = new("销毁窗口");
            closeWindowItem.Click += (sender, arg) =>
            {
                if (Current.MainWindow != null)
                {
                    Current.MainWindow.Hide();
                    (Current.MainWindow as MainWindow).CloseWindow = true;
                    Current.MainWindow.Close();
                }
            };
            contextMenu.Items.Add(closeWindowItem);*/

            ToolStripMenuItem restartItem = new("重启程序");
            restartItem.Font = new Font(restartItem.Font.FontFamily.Name, 9F);
            restartItem.Click += (sender, arg) =>
            {
                Process.Start(System.Windows.Forms.Application.ExecutablePath);
                TrayNotifyIcon.Visible = false;
                Current.Shutdown();
            };
            TrayContextMenuStrip.Items.Add(restartItem);

            ToolStripMenuItem selfStartingItem = new("开机自启动");
            selfStartingItem.Font = new Font(selfStartingItem.Font.FontFamily.Name, 9F);
            selfStartingItem.CheckOnClick = true;
            //检测是否已经开机自启动
            RegistryKey? runKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
            if (runKey?.GetValue("ElapsedTime") != null)
            { selfStartingItem.Checked = true; }
            selfStartingItem.Click += (sender, arg) =>
            {
                RegistryKey? runKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                //if (runKey == null) { System.Windows.MessageBox.Show("失败，无法获取开机自启动注册表", "错误"); return; }
                if (selfStartingItem.Checked == false) // 按下后已经改变了打钩状态，现在没钩说明按之前有钩
                {
                    runKey?.DeleteValue("ElapsedTime", false);
                }
                else
                {
                    runKey?.SetValue("ElapsedTime", System.Windows.Forms.Application.ExecutablePath);
                }
            };
            TrayContextMenuStrip.Items.Add(selfStartingItem);

            ToolStripMenuItem screenshotItem = new("截图");
            screenshotItem.Font = new Font(screenshotItem.Font.FontFamily.Name, 9F);
            screenshotItem.Checked = true;
            screenshotItem.CheckOnClick = true;
            screenshotItem.Click += (sender, arg) =>
            {
                if (GlobalSettings.Screenshot == true) { GlobalSettings.Screenshot = false;/* screenshotItem.Text = "开始截图";*/ }
                else { GlobalSettings.Screenshot = true; /*screenshotItem.Text = "暂停截图";*/ }
            };
            TrayContextMenuStrip.Items.Add(screenshotItem);

            ToolStripMenuItem openInstallationDirectoryItem = new("安装目录");
            openInstallationDirectoryItem.Font = new Font(openInstallationDirectoryItem.Font.FontFamily.Name, 9F);
            openInstallationDirectoryItem.Click += (sender, arg) =>
            {
                Process.Start("explorer.exe", $"/select,{System.Windows.Forms.Application.ExecutablePath}");
            };
            TrayContextMenuStrip.Items.Add(openInstallationDirectoryItem);

            ToolStripMenuItem exitItem = new("退出");
            exitItem.Font = new Font(exitItem.Font.FontFamily.Name, 9F);
            exitItem.Click += (sender, arg) =>
            {
                TrayNotifyIcon.Visible = false;
                Current.Shutdown();
            };
            TrayContextMenuStrip.Items.Add(exitItem);

            TrayNotifyIcon.ContextMenuStrip = TrayContextMenuStrip;
            TrayNotifyIcon.ContextMenuStrip.Opening += ContextMenuStripOnOpening;

            TrayNotifyIcon.Visible = true;
        }
        /// <summary>
        /// 默认的菜单是在左上角弹出，但是一般放到右上角好
        /// </summary>
        private void ContextMenuStripOnOpening(object? sender, CancelEventArgs cancelEventArgs)
        {
            System.Drawing.Point p = Cursor.Position;
            p.Y -= TrayNotifyIcon?.ContextMenuStrip?.Height ?? 27 * 5;
            TrayNotifyIcon?.ContextMenuStrip?.Show(p);
        }
        private void CheckDate()
        {
            if (DateTime.Now.Date > _today.Date)
            {
                Process.Start(System.Windows.Forms.Application.ExecutablePath);
                TrayNotifyIcon.Visible = false;
                Shutdown();
            }
        }
        private void Screenshot(object? state)
        {
            if (GlobalSettings.Screenshot != true) { return; }
            Rectangle bounds = Screen.GetBounds(System.Drawing.Point.Empty);
            using Bitmap bitmap = new(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // 将屏幕内容绘制到Bitmap上  
                g.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, bounds.Size);
            }
            bitmap.Save($"{GlobalSettings.ScreenshotFolderPath}/{_today:yyyy-MM-dd}/{DateTime.Now:HH：mm}.jpeg", _jpegCodec, _encoderParams);
        }
        /// <summary>
        /// 获取当前焦点窗口并记录时间
        /// </summary>
        private void TimerTick_GetTopwindow(object? sender, EventArgs e)
        {
            CheckDate();
            TotalSecond += GlobalSettings.GetTopWindowInterval_s;
            IntPtr activeWindowHandle = GetForegroundWindow();
            _ = GetWindowThreadProcessId(activeWindowHandle, out int pid);
            if (pid == IntPtr.Zero) { return; }
            // 获取程序路径
            string? filePath;
            try
            {
                filePath = Process.GetProcessById(pid).MainModule?.FileName.ToLower();
            }
            catch (System.ComponentModel.Win32Exception) { return; }
            if (filePath == null) { return; }

            var item = ExeItemList.Where(x => x.ExePath == filePath).FirstOrDefault();
            //是否已经使用过该程序
            if (item != null)
            {
                item.Seconds += GlobalSettings.GetTopWindowInterval_s;
            }
            else
            {
                ExeItem exeItem = new()
                {
                    ExePath = filePath,
                    IconPath = $"{GlobalSettings.ExeIconFolderPath}/{filePath.Replace("\\", "$").Replace(":", "")}.png",
                    Seconds = GlobalSettings.GetTopWindowInterval_s,
                };
                if (!File.Exists(exeItem.IconPath)) // 不存在图标则获取
                {
                    Icon? icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                    if (icon != null)
                    {
                        using Bitmap bitmap = icon.ToBitmap();
                        bitmap.Save(exeItem.IconPath, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    else
                    {
                        exeItem.IconPath = "pack://application:,,,/ElapsedTime;component/img/unknowfile.png";
                    }
                }

                if (Current.MainWindow == null)
                {
                    exeItem.TimeText = ExeItem.SecondToTime(exeItem.Seconds);
                    exeItem.Percentage = exeItem.Seconds * 100 / (TotalSecond != 0 ? TotalSecond : exeItem.Seconds);
                    ExeItemList.Add(exeItem);
                }
                else
                {
                    // System.ArgumentNullException:“Value cannot be null.Arg_ParamName_Name”
                    ((MainWindow)Current.MainWindow).UiDispatcher.Invoke(() =>
                    {
                        exeItem.TimeText = ExeItem.SecondToTime(exeItem.Seconds);
                        exeItem.Percentage = exeItem.Seconds * 100 / (TotalSecond != 0 ? TotalSecond : exeItem.Seconds);
                        ExeItemList.Add(exeItem);
                    });
                }
            }
        }
        // 获取指定文件扩展名对应的编码器
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == mimeType)
                    return codec;
            }
            throw new Exception($"No encoder for mime type: {mimeType}");
        }

    }
}
