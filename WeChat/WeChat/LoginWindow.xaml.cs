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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WXLogin;

namespace WeChat
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        private WXService _wxSerivice;
        public LoginWindow()
        {
            InitializeComponent();
            Window_Loaded();

        }
        public LoginWindow(string arg) : this()
        {
            //this._arg = arg;
        }
        private void Window_Loaded()
        {
            //_chatWindowsDic = new Dictionary<string, Window>();

            //LoadPalyBoxSource();

            Init();
        }

        private async void Init()
        {
            
                // wechat login
                var status = await InitLoginAsync();

                if (!status)
                {
                    // login fail
                    MessageBox.Show("登录失败,请稍等再试!");
                    return;
                }
            MainWindow._wxSerivice= _wxSerivice;
            this.DialogResult = Convert.ToBoolean(1);
            this.Close();

        }
        private async Task<bool> InitLoginAsync()
        {
            this.loginLable.Content = "二维码获取中...";

            await Task.Run(() =>
            {
                // start login
                var ls = new LoginService();

                if (ls.LoginCheck() == null)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.codeImage.Source = InternalHelp.ConvertByteToBitmapImage(ls.GetQRCode());
                        this.loginLable.Content = "请扫描二维码";
                    }));
                }

                // if the user scan and click login button
                while (true)
                {
                    var loginResult = ls.LoginCheck();

                    if (loginResult is byte[])
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.codeImage.Source = InternalHelp.ConvertByteToBitmapImage(loginResult as byte[]);
                            this.loginLable.Content = "请点击手机上的登陆按钮!";
                        }));
                    }
                    else if (loginResult is string)
                    {
                        if (ls.GetSidUid(loginResult as string)) break;

                        MessageBox.Show("当前环境存在异常！登录失败！");
                        Environment.Exit(0);
                    }
                }
            });

            _wxSerivice = WXService.Instance;
            return _wxSerivice != null;
        }
    }
}
