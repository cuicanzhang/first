﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using WxBot.Core;
using WxBot.Core.Entity;
using WxBot.Http;

namespace WxBot
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        private const int WS_SHOWNORMAL = 1;
        private static Process RuningInstance()
        {
            Process currentProcess = Process.GetCurrentProcess();
            Process[] Processes = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (Process process in Processes)
            {
                if (process.Id != currentProcess.Id)
                {
                    if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == currentProcess.MainModule.FileName)
                    {
                        return process;
                    }
                }
            }
            return null;
        }
        public static void HandleRunningInstance(Process instance)
        {
            ShowWindowAsync(instance.MainWindowHandle, WS_SHOWNORMAL); //显示，可以注释掉
            SetForegroundWindow(instance.MainWindowHandle);            //放到前端
        }



        public MainWindow()
        {
            Process process = RuningInstance();
            if (process == null)
            {
                InitializeComponent();
                delFile();
                MainWindow_Loaded();

            }
            else
            {
                //MessageBox.Show("应用程序已经在运行中。。。");
                HandleRunningInstance(process);
                //System.Threading.Thread.Sleep(1000);
                System.Environment.Exit(1);
            }


        }
        private void Main_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch { }
        }

        public void MainWindow_Loaded()
        {
            if (Core.Constant.allowLogin != true)
            {
                if (License.CheckReg.isRegS(Tools.Regedit.getRegCodeS()))
                {
                    LoginWindow loginWindow = new LoginWindow();
                    loginWindow.ShowDialog();
                    if (loginWindow.DialogResult != Convert.ToBoolean(1))
                    {
                        this.Close();
                    }
                    else
                    {
                        MainWindow_Loaded();
                    }
                }
                else
                {
                    CheckRegWindow checkReg = new CheckRegWindow();
                    checkReg.ShowDialog();
                    if (checkReg.DialogResult != Convert.ToBoolean(1))
                    {
                        this.Close();
                    }
                    else
                    {

                        MainWindow_Loaded();
                    }
                }
            }

            else if (Core.Constant.allowLogin == true)
            {
                DoLogic();
            }


        }


        string uin;
        string forwardUser = "";
        List<string> dGroup = new List<string>();
        bool forward = false;
        int MaxSelected = 1;
        public void DoLogic()
        {
            sCB.IsEnabled = false;
            smCB.IsEnabled = false;
            overTimeTB.Text = "授权时间截止至 [ " + Core.Constant.overtime + " ]";
            //dLV.IsEnabled = false;
            uin = Core.Constant.Uin;
            List<Object> _contact_all = new List<object>();


            ((Action)(delegate ()
            {

                LoginCore.InitCookie(uin);
                string sid = LoginCore.GetPassTicket(uin).WxSid;
                string host = LoginCore.GetPassTicket(uin).WxHost;
                WXService wxs = new WXService
                {
                    Uin = uin,
                    Sid = sid,
                    DeviceID = "e" + LoginCore.GenerateCheckCode(15),
                    BaseUrl = "https://" + LoginCore.GetPassTicket(uin).WxHost,
                    PushUrl = "https://webpush." + LoginCore.GetPassTicket(uin).WxHost,
                    UploadUrl = "https://file." + LoginCore.GetPassTicket(uin).WxHost
                };
                JObject init_result = wxs.WxInit();  //初始化
                var partUsers = new List<WXUser>();
                if (init_result != null)
                {
                    this.Dispatcher.BeginInvoke(((Action)delegate ()  //等待结束
                    {
                        supLB.Content = "加载个人信息...";

                    }));
                    var _me = new WXUser
                    {
                        uin = wxs.Uin,
                        UserName = init_result["User"]["UserName"].ToString(),
                        City = "",
                        HeadImgUrl = init_result["User"]["HeadImgUrl"].ToString(),
                        NickName = init_result["User"]["NickName"].ToString(),
                        Province = "",
                        PYQuanPin = init_result["User"]["PYQuanPin"].ToString(),
                        RemarkName = init_result["User"]["RemarkName"].ToString(),
                        RemarkPYQuanPin = init_result["User"]["RemarkPYQuanPin"].ToString(),
                        Sex = init_result["User"]["Sex"].ToString(),
                        Signature = init_result["User"]["Signature"].ToString(),
                    };
                    forwardUser = _me.UserName;
                    partUsers.Add(_me);
                    this.Dispatcher.BeginInvoke((Action)(delegate ()  //等待结束
                    {
                        //var aa = wxs.BaseUrl + _me.HeadImgUrl;
                        //headImage.Source = new BitmapImage(new Uri(aa));
                        headImage.Source = BitmapFrame.Create(wxs.GetIcon(_me.UserName, uin), BitmapCreateOptions.None, BitmapCacheOption.Default);
                    }));
                    this.Dispatcher.BeginInvoke(((Action)delegate ()  //等待结束
                    {
                        supLB.Content = "加载最近联系人...";

                    }));
                    foreach (JObject contact in init_result["ContactList"])  //部分好友名单
                    {
                        WXUser user = new WXUser();
                        user.uin = wxs.Uin;
                        user.UserName = contact["UserName"].ToString();
                        user.City = contact["City"].ToString();
                        user.HeadImgUrl = contact["HeadImgUrl"].ToString();
                        user.NickName = contact["NickName"].ToString();
                        user.Province = contact["Province"].ToString();
                        user.PYQuanPin = contact["PYQuanPin"].ToString();
                        user.RemarkName = contact["RemarkName"].ToString();
                        user.RemarkPYQuanPin = contact["RemarkPYQuanPin"].ToString();
                        user.Sex = contact["Sex"].ToString();
                        user.Signature = contact["Signature"].ToString();
                        partUsers.Add(user);
                    }


                    var _syncKey = new Dictionary<string, string>();
                    foreach (JObject synckey in init_result["SyncKey"]["List"])  //同步键值
                    {
                        _syncKey.Add(synckey["Key"].ToString(), synckey["Val"].ToString());
                    }
                    //保存最新key
                    LoginCore.SyncKey(uin, _syncKey);
                    this.Dispatcher.BeginInvoke(((Action)delegate ()  //等待结束
                    {
                        supLB.Content = "初始化联系人...";

                    }));
                    WxContact _contact = new WxContact(uin);  //记住此处不适合再开线程
                    _contact.InitContact(partUsers); //初始联系人

                    Dictionary<string, string> Groups = new Dictionary<string, string>();
                    foreach (var g in _contact.GetGroupUserNames())
                    {
                        Groups.Add(g, _contact.GetGroupMemberNames(g).NickName);
                    }
                    this.Dispatcher.BeginInvoke((Action)(delegate ()  //等待结束
                    {
                        sCB.ItemsSource = Groups;
                        sCB.DisplayMemberPath = "Value";
                        sCB.SelectedValuePath = "Key";
                        smCB.DisplayMemberPath = "Value";
                        smCB.SelectedValuePath = "Key";
                        supLB.Content = "进入监听模式...";
                        sCB.IsEnabled = true;
                        smCB.IsEnabled = true;
                        //dLV.IsEnabled = true;
                    }));
                    string sync_flag = null;
                    JObject sync_result;
                    sync_flag = wxs.WxSyncCheck();  //同步检查
                    DateTime lastCheckTs = DateTime.Now;
                    while (true)
                    {


                        if (sync_flag != null)
                        {
                            var retcode = sync_flag.ToString().Split(new string[] { "\"" }, StringSplitOptions.None)[1];
                            var selector = sync_flag.ToString().Split(new string[] { "\"" }, StringSplitOptions.None)[3];
                            //this.Dispatcher.BeginInvoke((Action)(delegate ()
                            //{
                            //supLB.Content = "{retcode:" + retcode + " selector:" + selector + "}";

                            //}));
                            if (retcode == "1100")
                            {
                                this.Dispatcher.BeginInvoke((Action)(delegate ()
                                {
                                    headBorder.BorderBrush = Brushes.Red;
                                }));
                                MessageBox.Show("你在手机上登出了微信，债见");
                                break;
                            }
                            if (retcode == "1101")
                            {
                                this.Dispatcher.BeginInvoke((Action)(delegate ()
                                {
                                    headBorder.BorderBrush = Brushes.Red;
                                }));
                                MessageBox.Show("你在其他地方登录了 WEB 版微信，债见");
                                break;
                            }
                            else if (retcode == "0")
                            {
                                this.Dispatcher.BeginInvoke((Action)(delegate ()
                                {
                                    headBorder.BorderBrush = Brushes.GreenYellow;
                                }));
                                if (selector == "2")
                                {
                                    sync_result = wxs.WxSync();  //进行同步
                                    lastCheckTs = DateTime.Now;
                                    if (sync_result != null)
                                    {
                                        handleMsg(sync_result);
                                    }
                                    //this.Dispatcher.BeginInvoke((Action)(delegate ()
                                    //{
                                    //supLB.Content = "";
                                    //}));
                                }
                                else if (selector == "0")
                                {
                                    //this.Dispatcher.BeginInvoke((Action)(delegate ()
                                    //{
                                    //supLB.Content = "";
                                    //}));

                                    Thread.Sleep(500);
                                }
                                else
                                {
                                    sync_result = wxs.WxSync();  //进行同步
                                    this.Dispatcher.BeginInvoke((Action)(delegate ()
                                    {
                                        supLB.Content = "最后同步：{retcode:" + retcode + " selector:" + selector + "}" ;
                                    }));
                                    //this.Dispatcher.BeginInvoke((Action)(delegate ()
                                    //{
                                    //chatText.AppendText("\n" + selector + "\n-------" + sync_result.ToString() + "-------\n");
                                    //}));
                                }
                            }
                            sync_flag = null;
                            this.Dispatcher.BeginInvoke((Action)(delegate ()
                            {
                                headBorder.BorderBrush = Brushes.Blue;
                            }));
                        }

                        if ((DateTime.Now - lastCheckTs).Seconds < 23)
                        {
                            var sleep = (DateTime.Now - lastCheckTs).Seconds;

                            Thread.Sleep(1000);

                            sync_flag = wxs.WxSyncCheck();  //同步检查
                        }
                        else
                        {
                            sync_flag = wxs.WxSyncCheck();
                            lastCheckTs = DateTime.Now;

                        }
                        this.Dispatcher.BeginInvoke((Action)(delegate ()
                        {
                            sleepTB.Text = ((DateTime.Now - lastCheckTs).Seconds).ToString();
                            supLB.Content = "最后同步："+DateTime.Now.ToLongTimeString().ToString();
                        }));

                    }
                }

            })).BeginInvoke(null, null);
        }
        private void handleMsg(JObject sync_result)
        {
            if (sync_result["AddMsgCount"] != null && sync_result["AddMsgCount"].ToString() != "0")
            {
                WXService wxs = new WXService
                {
                    Uin = uin,
                    Sid = LoginCore.GetPassTicket(uin).WxSid,
                    DeviceID = "e" + LoginCore.GenerateCheckCode(15),
                    BaseUrl = "https://" + LoginCore.GetPassTicket(uin).WxHost,
                    PushUrl = "https://webpush." + LoginCore.GetPassTicket(uin).WxHost,
                    UploadUrl = "https://file." + LoginCore.GetPassTicket(uin).WxHost
                };
                foreach (JObject m in sync_result["AddMsgList"])
                {
                    string from = m["FromUserName"].ToString();
                    string to = m["ToUserName"].ToString();
                    string content = m["Content"].ToString();
                    string MsgId = m["MsgId"].ToString();
                    string type = m["MsgType"].ToString();//语音视频标识
                    if (type == "1")
                    {
                        Dispatcher.BeginInvoke(((Action)delegate ()
                        {
                            //wxs.SendMsg(content, from, to, 1, uin, sid);
                            //MessageBox.Show(content);
                            //chatText.AppendText("[" + type + "]" + from + "->" + to + " : " + content + "\n");
                            //chatText.AppendText("[" + type + "]:" + content + "\n");
                            if (forward == true)
                            {
                                if (from.Contains("@@"))
                                {

                                    if (from == sCB.SelectedValue.ToString())
                                    {
                                        string[] sArray = Regex.Split(content, ":<br/>", RegexOptions.IgnoreCase);
                                        var aa = sCB.SelectedValue.ToString();
                                        if (sArray[0] == smCB.SelectedValue.ToString())
                                        {
                                            wxs.SendMsg(sArray[1], forwardUser, dCB.SelectedValue.ToString(), int.Parse(type));
                                            supLB.Content = DateTime.Now.ToLongTimeString().ToString() + "：" + sArray[1];
                                        }
                                        /*
                                        if (sArray[0] == smCB.SelectedValue.ToString())
                                            foreach (var g in dGroup)
                                            {
                                                wxs.SendMsg(sArray[1], forwardUser, g, int.Parse(type));
                                                //Thread.Sleep(100);
                                            }
                                        */
                                    }
                                }
                                //chatText.AppendText("[" + msg.Type + "]" + wxc.GetNickName(from) + "->" + wxc.GetNickName(to) + " : " + content + "\n");
                                //chatText.PageDown();
                                //chatText.AppendText("\nmsg:                 "+sync_result["AddMsgList"].ToString());
                                //debugTextBox.AppendText(m.ToString());
                            }


                        }));
                    }
                    else if (type == "3")//图片
                    {
                        string sFilePath = Environment.CurrentDirectory + "\\IMG";
                        Dispatcher.BeginInvoke(((Action)delegate ()
                        {
                            if (forward == true)
                            {
                                if (from.Contains("@@"))
                                {

                                    if (from == sCB.SelectedValue.ToString())
                                    {
                                        string[] sArray = Regex.Split(content, ":<br/>", RegexOptions.IgnoreCase);
                                        if (sArray[0] == smCB.SelectedValue.ToString())
                                        {
                                            wxs.SendMsgImg("", sArray[1], forwardUser, dCB.SelectedValue.ToString(), int.Parse(type));
                                            supLB.Content = DateTime.Now.ToLongTimeString().ToString() + "：转发了图片";
                                        }

                                        /*  
                                      foreach (var g in dGroup)
                                          {
                                              //wxs.SendMsgImg(ClientMediaId, forwardUser, g, int.Parse(type), uin, sid);
                                              wxs.SendMsgImg("", sArray[1], forwardUser, g, int.Parse(type));
                                              //Thread.Sleep(300);
                                          }
                                          */
                                    }
                                }
                                //chatText.AppendText("[" + msg.Type + "]" + wxc.GetNickName(from) + "->" + wxc.GetNickName(to) + " : " + content + "\n");
                                //chatText.PageDown();
                                //chatText.AppendText("\nmsg:                 "+sync_result["AddMsgList"].ToString());
                                //debugTextBox.AppendText(m.ToString());
                            }
                        }));
                    }
                    else if (type == "47")//动态表情
                    {
                        Dispatcher.BeginInvoke(((Action)delegate ()
                        {
                            if (forward == true)
                            {
                                if (from.Contains("@@"))
                                {
                                    //chatText.AppendText("[" + type + "]" + from + "->" + to + " : " + content + "\n");

                                    var aa = sCB.SelectedValue.ToString();
                                    if (from == sCB.SelectedValue.ToString())
                                    {
                                        string[] sArray = Regex.Split(content, ":<br/>", RegexOptions.IgnoreCase);
                                        if (sArray[0] == smCB.SelectedValue.ToString())
                                        {
                                            Regex reg = new Regex(@"md5=.(.*).\slen");
                                            Match match = reg.Match(sArray[1]);
                                            var aaa = match.Groups[1].Value;
                                            var bb = forwardUser;
                                            wxs.SendEmoticon(match.Groups[1].Value, forwardUser, dCB.SelectedValue.ToString(), int.Parse(type));
                                            supLB.Content = DateTime.Now.ToLongTimeString().ToString() + "：转发了表情";
                                        }
                                        /*
                                            foreach (var g in dGroup)
                                            {
                                                Regex reg = new Regex(@"md5=.(.*).\slen");
                                                Match match = reg.Match(sArray[1]);
                                                var aaa = match.Groups[1].Value;
                                                var bb = forwardUser;
                                                wxs.SendEmoticon(match.Groups[1].Value, forwardUser, g, int.Parse(type));
                                                //Thread.Sleep(100);
                                            }
                                            */
                                    }
                                }
                            }
                        }));
                    }
                    else
                    {
                        //this.Dispatcher.BeginInvoke((Action)(delegate ()
                        //{
                        //supLB.Content = DateTime.Now.ToLongTimeString().ToString() + "：消息同步中";

                        //}));
                    }
                }
            }
            /*
            else
            {
                var _syncKey = new Dictionary<string, string>();
                foreach (JObject synckey in sync_result["SyncKey"]["List"])  //同步键值
                {
                    _syncKey.Add(synckey["Key"].ToString(), synckey["Val"].ToString());
                }
                //保存最新key
                LoginCore.SyncKey(uin, _syncKey);
            }
            */
        }
        private void sCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(delegate ()  //等待结束
            {
                dGroup.Clear();
                //ObservableCollection<WxGroup> dGroups = new ObservableCollection<WxGroup>();
                Dictionary<string, string> dGroups = new Dictionary<string, string>();
                Dictionary<string, string> GroupsMembers = new Dictionary<string, string>();
                WxContact wxc = new WxContact(uin);
                foreach (var gm in wxc.GetGroupMemberNames(sCB.SelectedValue.ToString()).MemberUserNames)
                {
                    GroupsMembers.Add(gm.UserName, gm.NickName);
                }
                smCB.ItemsSource = GroupsMembers;
                //smCB.DisplayMemberPath = "Value";
                //smCB.SelectedValue = "Key";

                foreach (var g in wxc.GetGroupUserNames())
                {
                    if (g != sCB.SelectedValue.ToString())
                    {
                        dGroups.Add(wxc.GetOnLineGroupMember(g).UserName, wxc.GetOnLineGroupMember(g).NickName);
                    }

                    //dGroups.Add(new WxGroup { NickName = g, });
                }
                dCB.ItemsSource = dGroups;
                dCB.DisplayMemberPath = "Value";
                dCB.SelectedValuePath = "Key";
                //dLV.ItemsSource = dGroups;



            }));
        }

        private void smCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            forwardBtn.IsEnabled = true;
        }
        private void dGroup_Click(object sender, RoutedEventArgs e)
        {
            dGroup.Clear();
            RadioButton rb = sender as RadioButton;
            string UserName = rb.Tag.ToString();
            if (rb.IsChecked == true && dGroup.Count < MaxSelected)
            {
                dGroup.Add(UserName);
            }

            if (dGroup.Count == 0)
            {
                forwardBtn.Content = "启用";
                forward = false;
            }
        }

        private void dLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //if (dGroup.Count > 0)
            if (dCB.SelectedValue != null)
            {
                if (forward == false)
                {
                    forward = true;
                    forwardBtn.Content = "关闭";
                }
                else
                {
                    forward = false;
                    forwardBtn.Content = "启用";
                }
            }
            else
            {
                MessageBox.Show("选择至少一个目标群后可开启");
            }

        }
        private void delFile()
        {
            var dir = Environment.CurrentDirectory + "\\data";
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir,true);
            }
        }
        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
         
        }

        private void minBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
