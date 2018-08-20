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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace License
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DayCB.ItemsSource = DayDic()["day"];
            DayCB.SelectedValue = "1";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //产生注册码
            int day = int.Parse(DayCB.SelectedValue.ToString());
            regCode.Text = RegInfo.CreateRegisterCode(mCode.Text, DateTime.Now.AddDays(day));
            Dictionary<string, string> regDic = new Dictionary<string,string>();
            regDic["regCode"]= RegInfo.CreateRegisterCode(mCode.Text, DateTime.Now.AddDays(day));
            regDic["overTime"] = DateTime.Now.AddDays(day).ToString();
            LicSerializable ls = new LicSerializable( EnumContainer.SerializType.WxBot);
            ls.Serializable(regDic);
        }
        private Dictionary<string, List<string>> DayDic()
        {
            Dictionary<string, List<string>> Dic = new Dictionary<string, List<string>>();

            List<string> monthList = new List<string>();
            List<string> dayList = new List<string>();
            for (int i = 0; i < 31; i++)
            {
                dayList.Add((i + 1).ToString());
            }
            Dic["day"] = dayList;
            return Dic;
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            //产生注册码
            int day = int.Parse(DayCB.SelectedValue.ToString());
            regCode.Text = RegInfo.CreateRegisterCode(mCode.Text, DateTime.Now.AddDays(day));
            Dictionary<string, string> regDic = new Dictionary<string, string>();
            regDic["regCode"] = RegInfo.CreateRegisterCode(mCode.Text, DateTime.Now.AddDays(day));
            regDic["overTime"] = DateTime.Now.AddMinutes(66).ToString();
            LicSerializable ls = new LicSerializable(EnumContainer.SerializType.WxBot);
            ls.Serializable(regDic);
        }
    }
}
