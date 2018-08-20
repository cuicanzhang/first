using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
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
using WXLogin;

namespace WeChat
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string uin="";
        public static WXService _wxSerivice;
        private Dictionary<string, Window> _chatWindowsDic;
        public MainWindow()
        {
            InitializeComponent();
            
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.ShowDialog();
            if (loginWindow.DialogResult != Convert.ToBoolean(1))
            {
                this.Close();
            }
            InitPulginAsync();
            InitUserData();
            OpenListening();

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _chatWindowsDic = new Dictionary<string, Window>();

            LoadPalyBoxSource();

        }
        private async void InitPulginAsync()
        {
            await Task.Run(() =>
            {
                var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugin\\");
                if (!Directory.Exists(path) || Directory.GetFiles(path).Length < 1) return;

                foreach (var item in Directory.GetFiles(path))
                {
                    if (System.IO.Path.GetExtension(item) != ".dll" && System.IO.Path.GetExtension(item) != ".exe") continue;

                    var tp = Assembly.LoadFrom(item).GetTypes().FirstOrDefault(o => o.Name == "Rule");
                    if (tp == null) continue;
                    var obj = (object)null;

                    Dispatcher.Invoke(() =>
                    {
                        obj = Activator.CreateInstance(tp);
                    });


                    tp.GetProperty("Me")?.SetValue(obj, new Tuple<string, string>(_wxSerivice.Me.NickName, _wxSerivice.Me.UserName));


                }
            });
        }
        private void InitUserData()
        {

            this.setPanel.Visibility = Visibility.Visible;
            this.recentList.ItemsSource = WXService.RecentContactList;
            this.allList.ItemsSource = WXService.AllContactList;

            WXService.RecentContactList.CollectionChanged += UpdateImage;
            WXService.AllContactList.CollectionChanged += UpdateImage;

            _wxSerivice.InitData();
            _wxSerivice.UpdateMsgToWxUsering += _wxSerivice_UpdateMsgToWxUsering;
        }
        private void OpenListening()
        {
            Task.Run(() =>
            {
                try
                {
                    // create msg handle assembly
                    var allFile = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory)
                    .Where(o => System.IO.Path.GetExtension(o) == ".dll");
                    foreach (var file in allFile)
                    {
                        var type = Assembly.LoadFrom(file).GetTypes()
                        .FirstOrDefault(o => o != typeof(IWXMsgHandle) && typeof(IWXMsgHandle).IsAssignableFrom(o));

                        if (type == null) continue;
                        _wxSerivice.MsgHandle = Activator.CreateInstance(type) as IWXMsgHandle;
                        break;
                    }

                    // begin Listen the msg
                    _wxSerivice.Listening(ListeningHandle);
                }
                catch (LoginOutException)
                {
                    MessageBox.Show("在其它地方登录！程序将退出！");
                    Environment.Exit(0);
                }
                catch (Exception e)
                {
                    Console.WriteLine("OpenListening: " + e.Message);
                }
            });
        }
        private void ListeningHandle(IEnumerable<WXMsg> msgs)
        {
            foreach (var item in msgs)
            {
                string reMsg;
                var msg = item.Msg;

                PrintLin($"[{item.FromNickName}] - [{item.ToNickName}] - {item.Time}");
                PrintLin($"Msg: {msg}");

                if (item.From.Equals(_wxSerivice.Me.UserName))
                {
                    // from me to other people
                    var toUser = Rule.Rules.Keys.FirstOrDefault(o => o.UserName == item.To);
                    if (toUser == null)
                    {
                        PrintLin(); continue;
                    }

                    var toRule = Rule.Rules[toUser];
                    if (toRule.Name == "Default")
                    {
                        PrintLin(); continue;
                    }
                    reMsg = toRule.FromMeInvoke(toUser.UserName, msg, item.Type, (int)toUser.UserType);
                }
                else
                {
                    PlayMsgMusic();

                    // find the specify user rule, if aleady store in Rule.Rules
                    var user = Rule.Rules.Keys.FirstOrDefault(o => o.UserName == item.From);
                    if (user == null)
                    {
                        PrintLin(); continue;
                    };
                    var rule = Rule.Rules[user];

                    // if rule equals "Default", just ignore it
                    if (rule.Name == "Default")
                    {
                        PrintLin(); continue;
                    }
                    reMsg = rule.Invoke(user.UserName, msg, item.Type, (int)user.UserType);
                }

                PrintLin($"ReMsg: {reMsg}");
                PrintLin();

                // if reMsg is null, skip it
                if (reMsg != null)
                {
                    _wxSerivice.SendMsg(reMsg, _wxSerivice.Me.UserName, item.From, 1);
                }
            }
        }
        private void UpdateImage(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (WXUserViewModel item in e.NewItems)
            {
                Task.Run(() =>
                {
                    var iconBytes = item.UserName.Contains("@@")
                        ? WXLogin.WXService.Instance.GetHeadImg(item.UserName)
                        : WXLogin.WXService.Instance.GetIcon(item.UserName);
                    item.BitMapImage = InternalHelp.ConvertByteToBitmapImage(iconBytes);
                });
            }
        }
        private bool _wxSerivice_UpdateMsgToWxUsering(WXUserViewModel user, WXMsg msg)
        {
            if (_chatWindowsDic.ContainsKey(user.UserName))
                msg.Readed = true;
            return true;
        }
        private void Print(string msg)
        {
            this.print.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.print.Text += msg;
                this.print.ScrollToEnd();
            }));
        }
        private void PrintLin(string msg = "")
        {
            Print(msg + "\r\n");
        }
        private void PlayMsgMusic()
        {
            // play music
            Dispatcher.InvokeAsync(() =>
            {
                playBox.Position = TimeSpan.FromSeconds(0);
                if (playBox.Source == null)
                {
                    LoadPalyBoxSource();
                }
                if (playBox.Source != null && isOpenMsgMusic.IsChecked == true) playBox.Play();
            });
        }
        private void LoadPalyBoxSource()
        {
            var musicFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources\\msg.mp3");

            if (File.Exists(musicFile))
            {
                Console.WriteLine("init music file: " + musicFile);
                playBox.Source = new Uri(musicFile);
            }
        }
        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var control = sender as Button;
            var user = control.GetBindingExpression(ContentProperty).DataItem as WXUserViewModel;

            //if (user.Messages.Count == 0) return;
            foreach (var item in user.Messages)
            {
                item.Readed = true;
            }
            user.DoPropertyChanged(nameof(user.Messages));

            if (_chatWindowsDic.ContainsKey(user.UserName))
            {
                _chatWindowsDic[user.UserName].Activate();
                return;
            }

            var chatWindow = new Chat(user);
            _chatWindowsDic.Add(user.UserName, chatWindow);
            chatWindow.Closed += (a, b) =>
            {
                _chatWindowsDic.Remove(user.UserName);
            };

            chatWindow.Show();
        }
        private void searchTb_KeyUp(object sender, KeyEventArgs e)
        {
            var control = sender.Cast<TextBox>();
            if (control.Text == string.Empty)
            {
                this.allList.ItemsSource = WXService.AllContactList;
            }

            this.allList.ItemsSource = WXService.AllContactList.Where(o => o.DisplayName.Contains(control.Text));
        }

        private void allSelect_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in WXService.AllContactList)
            {
                item.IsCheck = true;
            }
        }

        private void allSelect_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in WXService.AllContactList)
            {
                item.IsCheck = false;
            }
        }

        private void selectOther_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in WXService.AllContactList)
            {
                item.IsCheck = !(item.IsCheck ?? false);
            }
        }

    }
    public static class Extention
    {
        public static T Cast<T>(this object obj) where T : class
        {
            return obj as T;
        }
    }
}
