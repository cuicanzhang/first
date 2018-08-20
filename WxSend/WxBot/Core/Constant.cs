﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WxBot.Core
{
    class Constant
    {
        #region 系统属性

        public static string Uin = "";
        public static bool allowLogin =false;
        public static string overtime="";
        public static bool isReg = false;
        public static int WU_FILE_N = 0;

        public static string Version
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        #endregion

        #region 微信登录使用

        //获取会话ID的URL
        public static string _session_id_url = "https://login.weixin.qq.com/jslogin?appid=wx782c26e4c19acffb";
        //获取二维码的URL
        public static string _qrcode_url = "https://login.weixin.qq.com/qrcode/"; //后面增加会话id
        //生成一个一键登录qr
        public static string _Aloginqrcode_url = "https://login.weixin.qq.com/l/"; //后面增加会话id
        //判断二维码扫描情况   200表示扫描登录  201表示已扫描未登录  其它表示未扫描
        public static string _login_check_url = "https://login.weixin.qq.com/cgi-bin/mmwebwx-bin/login?loginicon=true&uuid="; //后面增加会话id


        //免扫码快速登陆URL
        public static string _webwxstatreport = "https://wx.qq.com/cgi-bin/mmwebwx-bin/webwxstatreport?fun=new&lang=zh_CN";
        #endregion

        #region 微信主体功能

        //微信初始化url
        public static string _init_url = "/cgi-bin/mmwebwx-bin/webwxinit?r=1377482058764";
        //获取好友头像
        //public static string _geticon_url = "https://wx2.qq.com";
        public static string _geticon_url = "/cgi-bin/mmwebwx-bin/webwxgeticon?username=";
        //获取群聊（组）头像
        //public static string _getheadimg_url = "https://wx2.qq.com/";
        public static string _getheadimg_url = "/";
        //获取好友列表
        public static string _getcontact_url = "/cgi-bin/mmwebwx-bin/webwxgetcontact";
        /// <summary>
        /// 批量获取联系人信息
        /// </summary>
        public static string _getbatcontact_url = "/cgi-bin/mmwebwx-bin/webwxbatchgetcontact?type=ex&r={0}&pass_ticket={1}";

        //同步检查url
        public static string _synccheck_url = "/cgi-bin/mmwebwx-bin/synccheck?sid={0}&uin={1}&synckey={2}&r={3}&skey={4}&deviceid={5}";
        //同步url
        public static string _sync_url = "/cgi-bin/mmwebwx-bin/webwxsync?sid=";
        //发送消息url
        public static string _sendmsg_url = "/cgi-bin/mmwebwx-bin/webwxsendmsg?sid=";
        //接收语音
        public static string _getmsgmp3_url = "/cgi-bin/mmwebwx-bin/webwxgetvoice?msgid=";
        //接收小视频
        public static string _getmsgvideo_url = "/cgi-bin/mmwebwx-bin/webwxgetvideo?msgid=";
        //接收图片
        public static string _getmsgimg_url = "/cgi-bin/mmwebwx-bin/webwxgetmsgimg?&MsgID=";

        public static string _uploadmedia_url = "/cgi-bin/mmwebwx-bin/webwxuploadmedia?f=json";
        //发送图片
        public static string _sendmsgimg_url = "/cgi-bin/mmwebwx-bin/webwxsendmsgimg?fun=async&f=json";

        //发送表情
        public static string _webwxsendemoticon_url = "/cgi-bin/mmwebwx-bin/webwxsendemoticon?fun=sys";
        #endregion
    }
}
