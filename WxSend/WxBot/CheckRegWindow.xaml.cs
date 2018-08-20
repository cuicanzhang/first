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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WxBot
{
    /// <summary>
    /// CheckReg.xaml 的交互逻辑
    /// </summary>
    public partial class CheckRegWindow : Window
    {
        public CheckRegWindow()
        {
            InitializeComponent();
            CheckRegWindow_Loaded();
        }
        void CheckRegWindow_Loaded()
        {
            code.Text= License.RegInfo.GetMachineCode();
        }

        private void CheckReg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch { }
        }

        private void RegBtn_Click(object sender, RoutedEventArgs e)
        {
            if (regCode.Text.Trim() != "")
            {
                DateTime time = DateTime.Now;
                Dictionary<string, string> WxBot = new Dictionary<string, string>();
                WxBot["WxBot"] = regCode.Text;
                if (License.CheckReg.isRegS(WxBot))
                {
                    msg.Visibility = Visibility.Visible;
                    msg.Content = "提示：注册成功！初始化...";
                    msg.Foreground = Brushes.Green;
                    code.IsEnabled = false;
                    regCode.IsEnabled = false;
                    regBtn.IsEnabled = false;

                    Tools.Regedit.CreateRegistFile();
                    Tools.Regedit.writeToRegistFile(regCode.Text);

                    this.DialogResult = Convert.ToBoolean(1);
                }
                else
                {
                    msg.Visibility = Visibility.Visible;
                    msg.Content = "提示：注册码无效，注册失败！";
                    msg.Foreground = Brushes.Red;
                }
            }     
        }
        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
