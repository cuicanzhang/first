using System;
using System.Windows;
using System.Windows.Media.Imaging;
using WxBot.Http;
using System.IO;
using System.Windows.Input;

namespace WxBot
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            MainWindow_Loaded();
        }
        private void Login_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch { }
        }
        LoginService ls = new LoginService();
        private void MainWindow_Loaded()
        {
            QRCode.Source = null;
            ((Action)(delegate ()
            {
                MemoryStream qrcode = ls.GetQRCode();
                if (qrcode != null)
                {
                    this.Dispatcher.Invoke((Action)delegate ()
                    {
                        QRCode.Source = BitmapFrame.Create(qrcode, BitmapCreateOptions.None, BitmapCacheOption.Default);
                    });
                }
                else
                {
                    MessageBox.Show("获取二维码失败！");
                }
                object login_result = null;
                    while (true)
                    {
                        login_result = ls.LoginCheck();
                        if (login_result is MemoryStream)
                        {
                            this.Dispatcher.Invoke((Action)delegate ()
                            {
                                QRCode.Source = BitmapFrame.Create(login_result as MemoryStream, BitmapCreateOptions.None, BitmapCacheOption.Default); ;
                            });
                        }
                        if (login_result is string)  //已完成登录
                        {
                            //访问登录跳转URL

                            var uin = ls.GetSidUid(login_result as string);
                            this.Dispatcher.Invoke((Action)delegate ()
                            {
                                Core.Constant.Uin = ls.GetSidUid(login_result as string);
                                Core.Constant.allowLogin = true;
                                this.DialogResult = Convert.ToBoolean(1);
                                this.Close();
                            });
                            break;
                        }
                    }
                })).BeginInvoke(null, null);
            }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
