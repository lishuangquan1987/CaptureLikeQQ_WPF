using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Threading;

namespace CaptureLikeQQ_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CaptureForm captureForm = null;
        const int WM_HOTKEY = 0X0312;//热键触发
        private int id = 100;//热键Id
        private System.Windows.Forms.NotifyIcon notifiyIcon = null;
        bool isReady = false;
        public MainWindow()
        {
            InitializeComponent();
            //带系统资源加载完毕才出来
            while (!SystemRead())
            {
                Thread.Sleep(500);
            }
            InitNotifyIcon();
            this.SourceInitialized += MainWindow_SourceInitialized;
        }
        private bool SystemRead()
        {
            foreach (Process p in System.Diagnostics.Process.GetProcesses())
            {
                if (p.ProcessName.Contains("Idle"))
                    return true;
            }
            return false;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            if (User32Helper.RegisterHotKey(handle, id, User32Helper.KeyModifiers.Ctrl, Keys.B))
            {
                isReady = true;
                this.lbMsg.Content = "按Ctrl+B开始截图";
            }
            else
            {
                isReady = false;
                this.lbMsg.Content = "热键Ctrl+B被占用，热键截图功能无法使用！";
                this.lbMsg.Foreground = new SolidColorBrush(Colors.Red);
            }
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CaptureLikeQQ_WPF.Resources.Capture.ico");
            this.Icon = ImageHelper.GetImage(stream);
        }
        private void InitNotifyIcon()
        {
            notifiyIcon = new System.Windows.Forms.NotifyIcon();
            notifiyIcon.Visible = true;
            notifiyIcon.Text = "模拟QQ截图";
            //图标
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CaptureLikeQQ_WPF.Resources.Capture.ico");
            if (stream != null)
            {
                notifiyIcon.Icon = new System.Drawing.Icon(stream);
            }
            //菜单
            var menu = new System.Windows.Forms.ContextMenuStrip();
            var showMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text="显示"};
            var exitMenuItem = new System.Windows.Forms.ToolStripMenuItem() { Text="退出"};
            showMenuItem.Click += MenuItem_Click;
            exitMenuItem.Click += MenuItem_Click;           
            menu.Items.Add(showMenuItem);
            menu.Items.Add(exitMenuItem);
            notifiyIcon.ContextMenuStrip = menu;
            notifiyIcon.MouseDoubleClick += NotifiyIcon_MouseDoubleClick;
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item.Text == "显示")
            {
                this.Show();
            }
            else if (item.Text == "退出")
            {
                Environment.Exit(Environment.ExitCode);
            }
        }

        private void NotifiyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.Show();
            User32Helper.SetForegroundWindow(new WindowInteropHelper(this).Handle);
        }

        public static void SetAutoRun(string fileName, bool isAutoRun)
        {
            RegistryKey reg = null;
            try
            {
                if (!System.IO.File.Exists(fileName))
                    throw new Exception("该文件不存在!");
                String name = fileName.Substring(fileName.LastIndexOf(@"\") + 1);
                reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (reg == null)
                    reg = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                if (isAutoRun)
                    reg.SetValue(name, fileName);
                else
                    reg.SetValue(name, false);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {
                if (reg != null)
                    reg.Close();
            }

        }
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(WindowProc));
            }
        }
        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                //热键按下
                StartCaupture();
            }
            return IntPtr.Zero;
        }
        private void StartCaupture()
        {
            this.Hide();
            if (captureForm == null || captureForm.IsDisposed)
                captureForm = new CaptureForm();
            System.Drawing.Bitmap bmp = ImageHelper.GetImage(new System.Drawing.Point(0,0),
                new System.Drawing.Size(Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height));
            captureForm.BackgroundImage = bmp;
            captureForm.Show();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            if(isReady)
                notifiyIcon.ShowBalloonTip(5000, "提示", "按Ctrl+B可以开始截图", ToolTipIcon.Info);
            else
                notifiyIcon.ShowBalloonTip(5000, "错误", "热键Ctrl+B与其他程序冲突，后台截图功能无法启用", ToolTipIcon.Error);
            this.Hide();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void hlStartCapture_Click(object sender, RoutedEventArgs e)
        {
            StartCaupture();
        }

        private void hlAbout_Click(object sender, RoutedEventArgs e)
        {
            StartAnimation(this.gridMain, this.gridAbout);
        }

        private void hlHelp_Click(object sender, RoutedEventArgs e)
        {
            StartAnimation(this.gridMain, this.gridHelp);
        }

        private void hlHelpReturn_Click(object sender, RoutedEventArgs e)
        {
            RollBackAnimation(this.gridMain,this.gridHelp);
        }
        private void hlAboutReturn_Click(object sender, RoutedEventArgs e)
        {
            RollBackAnimation(this.gridMain, this.gridAbout);
        }
        private void StartAnimation(Grid gridLeft, Grid gridRight)
        {
            double w = gridLeft.ActualWidth;
            double h = gridLeft.ActualHeight;
            ThicknessAnimation ani = new ThicknessAnimation();
            ani.From = new Thickness(0, 0, 0, 0);
            ani.To = new Thickness(-w,0,w,0);
            ani.Duration = TimeSpan.FromMilliseconds(300);

            ThicknessAnimation ani1 = new ThicknessAnimation();
            ani1.From = new Thickness(w, 0, -w, 0);
            ani1.To = new Thickness(0);
            ani1.Duration = TimeSpan.FromMilliseconds(300);

            gridLeft.BeginAnimation(Grid.MarginProperty,ani);
            gridRight.BeginAnimation(Grid.MarginProperty, ani1);
        }

        private void RollBackAnimation(Grid gridLeft, Grid gridRight)
        {
            double w = gridLeft.ActualWidth;
            double h = gridLeft.ActualHeight;
            ThicknessAnimation ani = new ThicknessAnimation();
            ani.To = new Thickness(0, 0, 0, 0);
            ani.From = new Thickness(-w, 0, w, 0);
            ani.Duration = TimeSpan.FromMilliseconds(300);

            ThicknessAnimation ani1 = new ThicknessAnimation();
            ani1.To = new Thickness(w, 0, -w, 0);
            ani1.From = new Thickness(0);
            ani1.Duration = TimeSpan.FromMilliseconds(300);

            gridLeft.BeginAnimation(Grid.MarginProperty, ani);
            gridRight.BeginAnimation(Grid.MarginProperty, ani1);
        }

        private void cbAutoRun_Checked(object sender, RoutedEventArgs e)
        {
            SetAutoRun(System.Windows.Forms.Application.ExecutablePath,true);
        }

        private void cbAutoRun_Unchecked(object sender, RoutedEventArgs e)
        {
            SetAutoRun(System.Windows.Forms.Application.ExecutablePath, false);
        }
    }
}
