using Autofac;
using BililiveRecorder.Core;
using BililiveRecorder.FlvProcessor;
using NLog;
using NLog.Fluent;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace BililiveRecorder.UWP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainPage : Page, INotifyPropertyChanged
    {
        private const int MAX_LOG_ROW = 25;
        private const string LAST_WORK_DIR_FILE = "lastworkdir";

        private Autofac.IContainer Container { get; set; }
        private ILifetimeScope RootScope { get; set; }

        private IRecorder _recorder;
        public IRecorder Recorder
        {
            get => _recorder; set
            {
                _recorder = value;
                OnPropertyChanged(nameof(Recorder));
            }
        }

        private IRecordedRoom _current;
        public IRecordedRoom Current
        {
            get => _current;
            set
            {
                _current = value;
                OnPropertyChanged(nameof(Current));
            }
        }
        public ObservableCollection<string> Logs { get; set; } =
            new ObservableCollection<string>()
            {
                "当前版本：" + "1.0.0",
                "网站： https://rec.danmuji.org",
                "QQ群： 689636812",
            };

        public static void AddLog(string message) => _AddLog?.Invoke(message);
        private static Action<string> _AddLog;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<FlvProcessorModule>();
            builder.RegisterModule<CoreModule>();
            Container = builder.Build();
            RootScope = Container.BeginLifetimeScope("recorder_root");
            Recorder = RootScope.Resolve<IRecorder>();
            InitializeComponent();

            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool skip_ui = false;
            string workdir = string.Empty;
            //if (!skip_ui)
            //{
            //    try
            //    {
            //        workdir = File.ReadAllText(LAST_WORK_DIR_FILE);
            //    }
            //    catch (Exception) { }
            //    var wdw = new WorkDirectoryPage()
            //    {
            //        WorkPath = workdir,
            //    };

            //    if (wdw.ShowDialog() == true)
            //    {
            //        workdir = wdw.WorkPath;
            //    }
            //    else
            //    {
            //        Environment.Exit(-1);
            //        return;
            //    }
            //}

            //if (!Recorder.Initialize(workdir))
            //{
            //    if (!skip_ui)
            //    {
            //        MessageBox.Show("初始化错误", "录播姬", MessageBoxButton.OK, MessageBoxImage.Error);
            //    }
            //    Environment.Exit(-2);
            //    return;
            //}

            //NotifyIcon.Visibility = Visibility.Visible;
        }

        //private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    if (new TimedMessageBox { Title = "关闭录播姬？", Message = "确定要关闭录播姬吗？", CountDown = 10 }.ShowDialog() == true)
        //    {
        //        _AddLog = null;
        //        Recorder.Shutdown();
        //        try
        //        {
        //            File.WriteAllText(LAST_WORK_DIR_FILE, Recorder.Config.WorkDirectory);
        //        }
        //        catch (Exception) { }
        //    }
        //    else
        //    {
        //        e.Cancel = true;
        //    }
        //}

        private void TextBlock_MouseRightButtonUp(object sender, PointerRoutedEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(textBlock.Text);
            }
        }

        /// <summary>
        /// 触发回放剪辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clip_Click(object sender, RoutedEventArgs e)
        {
            var rr = _GetSenderAsRecordedRoom(sender);
            if (rr == null)
            {
                return;
            }

            Task.Run(() => rr.Clip());
        }

        /// <summary>
        /// 启用自动录制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnableAutoRec(object sender, RoutedEventArgs e)
        {
            var rr = _GetSenderAsRecordedRoom(sender);
            if (rr == null)
            {
                return;
            }

            Task.Run(() => rr.Start());
        }

        /// <summary>
        /// 禁用自动录制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisableAutoRec(object sender, RoutedEventArgs e)
        {
            var rr = _GetSenderAsRecordedRoom(sender);
            if (rr == null)
            {
                return;
            }

            Task.Run(() => rr.Stop());
        }

        /// <summary>
        /// 手动触发尝试录制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TriggerRec(object sender, RoutedEventArgs e)
        {
            var rr = _GetSenderAsRecordedRoom(sender);
            if (rr == null)
            {
                return;
            }

            Task.Run(() => rr.StartRecord());
        }

        /// <summary>
        /// 切断当前录制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CutRec(object sender, RoutedEventArgs e)
        {
            var rr = _GetSenderAsRecordedRoom(sender);
            if (rr == null)
            {
                return;
            }

            Task.Run(() => rr.StopRecord());
        }

        /// <summary>
        /// 删除当前房间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveRecRoom(object sender, RoutedEventArgs e)
        {
            var rr = _GetSenderAsRecordedRoom(sender);
            if (rr == null)
            {
                return;
            }

            Recorder.RemoveRoom(rr);
        }

        /// <summary>
        /// 全部直播间启用自动录制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnableAllAutoRec(object sender, RoutedEventArgs e)
        {
            Recorder.ToList().ForEach(rr => Task.Run(() => rr.Start()));
        }

        /// <summary>
        /// 全部直播间禁用自动录制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisableAllAutoRec(object sender, RoutedEventArgs e)
        {
            Recorder.ToList().ForEach(rr => Task.Run(() => rr.Stop()));
        }

        /// <summary>
        /// 添加直播间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddRoomidButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(AddRoomidTextBox.Text, out int roomid))
            {
                if (roomid > 0)
                {
                    Recorder.AddRoom(roomid);
                }
                else
                {
                    var dialog = new MessageDialog("房间号是大于0的数字！");
                    await dialog.ShowAsync();
                }
            }
            else
            {
                var dialog = new MessageDialog("房间号是数字！");
                await dialog.ShowAsync();
            }
            AddRoomidTextBox.Text = string.Empty;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingsWindow();
        }

        private async void ShowSettingsWindow()
        {
            var page = new SettingsPage(Recorder.Config);
            var dialog = new ContentDialog()
            {
                Content = page,
            };
            dialog.PrimaryButtonClick += (s, e) =>
            {
                page.Config.CopyPropertiesTo(Recorder.Config);
            };
            await dialog.ShowAsync();
        }

        private IRecordedRoom _GetSenderAsRecordedRoom(object sender) => (sender as Button)?.DataContext as IRecordedRoom;

        private void RoomList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Current = RoomList.SelectedItem as RecordedRoom;
        }

        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        //private void Taskbar_Quit_Click(object sender, RoutedEventArgs e)
        //{
        //    Close();
        //}

        //private void Window_StateChanged(object sender, EventArgs e)
        //{
        //    if (WindowState == WindowState.Minimized)
        //    {
        //        Hide();
        //        NotifyIcon.ShowBalloonTip("B站录播姬", "录播姬已最小化到托盘，左键单击图标恢复界面。", BalloonIcon.Info);
        //    }
        //}

        //private void Taskbar_Click(object sender, RoutedEventArgs e)
        //{
        //    Show();
        //    WindowState = WindowState.Normal;
        //    Topmost ^= true;
        //    Topmost ^= true;
        //    Focus();
        //}
    }
}
