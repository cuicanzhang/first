using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WxBot.Core;

namespace WxBot.Http
{
    class WXService
    {
        public string Uin { get; set; }
        public string Sid { get; set; }
        public string DeviceID { get; set; }
        public string BaseUrl { get; set; }
        public string PushUrl { get; set; }
        public string UploadUrl { get; set; }

        public JObject WxInit()
        {
            string init_json = "{{\"BaseRequest\":{{\"Uin\":\"{0}\",\"Sid\":\"{1}\",\"Skey\":\"{2}\",\"DeviceID\":\"{3}\"}}}}";

            if (Uin != null && Sid != null)
            {
                string pass_ticket = LoginCore.GetPassTicket(Uin).PassTicket; ;//这个位置过来了
                string skey = LoginCore.GetPassTicket(Uin).SKey; ;
                init_json = string.Format(init_json, Uin, Sid, skey, DeviceID);
                var url = BaseUrl + Constant._init_url + "&pass_ticket=" + pass_ticket;
                byte[] bytes = HttpService.SendPostRequest(url, init_json, Uin);
                string init_str = Encoding.UTF8.GetString(bytes);
                JObject init_result = JsonConvert.DeserializeObject(init_str) as JObject;

                return init_result;
            }
            else
            {
                return null;
            }

        }
        /// <summary>
        /// 获取头像
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public MemoryStream GetIcon(string username, string uin = "")
        {
            try
            {
                byte[] bytes = HttpService.SendGetRequest(BaseUrl + Constant._geticon_url + username, uin);
                return new MemoryStream(bytes);
            }

            catch (Exception ex)
            {
                //MessageBox.Show("GetIcon" + ex.Message);
                Tools.Tools.WriteLog(ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <returns></returns>
        public JObject GetContact()
        {
            byte[] bytes = HttpService.SendGetRequest(BaseUrl + Constant._getcontact_url, Uin);
            string contact_str = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject(contact_str) as JObject;
        }
        public JObject BatGetContact(List<string> groupUserName)
        {
            var entity = LoginCore.GetPassTicket(Uin);
            var _jstr = string.Empty;
            foreach (var username in groupUserName)
            {
                _jstr += string.Format("{{{{\"UserName\":\"{0}\",\"ChatRoomId\":\"\"}}}},",
                    username, "");
            }
            string json = "{{" +
                "\"BaseRequest\":{{\"Uin\":{0}," +
                "\"Sid\":\"{1}\"," +
                "\"Skey\":\"{2}\"," +
                "\"DeviceID\":\"{4}\"}}," +
                "\"Count\":{3}," +
                "\"List\":[" +
                    _jstr.TrimEnd(',') +
                    "]" +
                "}}";
            try
            {
                json = string.Format(json, Uin, Sid, entity.SKey, groupUserName.Count, DeviceID);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("BatGetContact" + ex.Message);
                Tools.Tools.WriteLog(ex.ToString());
                //写日志

            }

            string url = string.Format(BaseUrl + Constant._getbatcontact_url, HttpService.GetTimeStamp(), entity.PassTicket);
            byte[] bytes = HttpService.SendPostRequest(url, json, Uin);
            string contact_str = Encoding.UTF8.GetString(bytes);

            return JsonConvert.DeserializeObject(contact_str) as JObject;
        }
        public string WxSyncCheck()
        {
            string sync_key = "";
            try
            {
                var _syncKey = LoginCore.GetSyncKey(Uin);
                foreach (KeyValuePair<string, string> p in _syncKey)
                {
                    sync_key += p.Key + "_" + p.Value + "%7C";
                }
                sync_key = sync_key.TrimEnd('%', '7', 'C');

                var entity = LoginCore.GetPassTicket(Uin);
                if (Sid != null && Uin != null)
                {
                    var _synccheck_url = string.Format(PushUrl + Constant._synccheck_url, Sid, Uin, sync_key, (long)(DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds, entity.SKey.Replace("@", "%40"), DeviceID);

                    byte[] bytes = HttpService.SendGetRequest(_synccheck_url + "&_=" + DateTime.Now.Ticks, Uin);

                    if (bytes != null)
                    {
                        //string contact_str = Encoding.UTF8.GetString(bytes);
                        return Encoding.UTF8.GetString(bytes);
                        //string retcode = contact_str.ToString().Split(new string[] { "\"" }, StringSplitOptions.None)[1];
                        //string selector = contact_str.ToString().Split(new string[] { "\"" }, StringSplitOptions.None)[3];
                        //string[]rs= { retcode, selector };
                        //return contact_str;
                        //return new string[]{ retcode, selector };
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("WxSyncCheck" + ex.Message);
                Tools.Tools.WriteLog(ex.ToString());
                return "";
                //return null;
            }
        }
        /// <summary>
        /// 微信同步
        /// </summary>
        /// <returns></returns> 
        public JObject WxSync()
        {
            var entity = LoginCore.GetPassTicket(Uin);
            string sync_json = "{{\"BaseRequest\" : {{\"DeviceID\":\"{6}\",\"Sid\":\"{1}\", \"Skey\":\"{5}\", \"Uin\":\"{0}\"}},\"SyncKey\" : {{\"Count\":{2},\"List\": [{3}]}},\"rr\" :{4}}}";
            string sync_keys = "";
            var _syncKey = LoginCore.GetSyncKey(Uin);
            foreach (KeyValuePair<string, string> p in _syncKey)
            {
                sync_keys += "{\"Key\":" + p.Key + ",\"Val\":" + p.Value + "},";
            }
            sync_keys = sync_keys.TrimEnd(',');
            sync_json = string.Format(sync_json, this.Uin, this.Sid, _syncKey.Count, sync_keys, (long)(DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds, entity.SKey, DeviceID);

            if (this.Sid != null && this.Uin != null)
            {
                byte[] bytes = HttpService.SendPostRequest(BaseUrl + Constant._sync_url + this.Sid + "&lang=zh_CN&skey=" + entity.SKey + "&pass_ticket=" + entity.PassTicket, sync_json, this.Uin);
                string sync_str = Encoding.UTF8.GetString(bytes);
                if (sync_str == null)
                {
                    return null;
                }
                JObject sync_resul = JsonConvert.DeserializeObject(sync_str) as JObject;
                // Dictionary<string, string> ss = new Dictionary<string, string>();
                if (sync_resul["SyncKey"]["Count"].ToString() != "1")
                {
                    _syncKey.Clear();
                    foreach (JObject key in sync_resul["SyncKey"]["List"])
                    {
                        _syncKey.Add(key["Key"].ToString(), key["Val"].ToString());
                    }
                }

                return sync_resul;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        public void SendMsg(string msg, string from, string to, int type)
        {
            string msg_json = "{{" +
            "\"BaseRequest\":{{" +
                "\"DeviceID\" : \"{10}\"," +
                "\"Sid\" : \"{0}\"," +
                "\"Skey\" : \"{6}\"," +
                "\"Uin\" : \"{1}\"" +
            "}}," +
            "\"Msg\" : {{" +
                "\"ClientMsgId\" : {8}," +
                "\"Content\" : \"{2}\"," +
                "\"FromUserName\" : \"{3}\"," +
                "\"LocalID\" : {9}," +
                "\"ToUserName\" : \"{4}\"," +
                "\"Type\" : {5}" +
            "}}," +
            "\"rr\" : {7}" +
            "}}";
            var entity = LoginCore.GetPassTicket(Uin);
            if (Sid != null && Uin != null)
            {
                //((long)(DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds) * 10000
                msg_json = string.Format(msg_json, Sid, Uin, msg, from, to, type, entity.SKey, DateTime.Now.Millisecond, DateTime.Now.Millisecond, DateTime.Now.Millisecond, DeviceID);
                byte[] bytes = HttpService.SendPostRequest(BaseUrl + Constant._sendmsg_url + Sid + "&lang=zh_CN&pass_ticket=" + entity.PassTicket, msg_json, Uin);
                string send_result = Encoding.UTF8.GetString(bytes);
                var aa = send_result;
            }
            //((Action)delegate()
            //{
            //    //存储发送消息
            //    var b = WxOperateLogDal.AddWxsendmsglog(Uin, from, msg,to,from);
            //}).BeginInvoke(null, null);
        }
        /// <summary>
        /// 发送图片
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        public void SendMsgImg(string MediaId, string msg, string from, string to, int type)
        {
            string msg_json = "{{" +
            "\"BaseRequest\":{{" +
                "\"DeviceID\" : \"{0}\"," +
                "\"Sid\" : \"{1}\"," +
                "\"Skey\" : \"{2}\"," +
                "\"Uin\" : \"{3}\"" +
            "}}," +
            "\"Msg\" : {{" +
                "\"ClientMsgId\" : {4}," +
                "\"Content\" : \"{5}\"," +
                "\"FromUserName\" : \"{6}\"," +
                "\"LocalID\" : {7}," +
                "\"MediaId\" : \"{8}\"," +
                "\"ToUserName\" : \"{9}\"," +
                "\"Type\" : {10}" +
            "}}," +
            "\"rr\" : {11}" +
            "}}";
            var entity = LoginCore.GetPassTicket(Uin);
            if (Sid != null && Uin != null)
            {
                //((long)(DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds) * 10000
                msg_json = string.Format(msg_json, DeviceID, Sid, entity.SKey, Uin, DateTime.Now.Millisecond, msg, from, DateTime.Now.Millisecond, MediaId, to, type, DateTime.Now.Millisecond);
                byte[] bytes = HttpService.SendPostRequest(BaseUrl + Constant._sendmsgimg_url + "&pass_ticket=" + entity.PassTicket, msg_json, Uin);
                string send_result = Encoding.UTF8.GetString(bytes);
                var aa = send_result;
            }
            //((Action)delegate()
            //{
            //    //存储发送消息
            //    var b = WxOperateLogDal.AddWxsendmsglog(Uin, from, msg,to,from);
            //}).BeginInvoke(null, null);
        }
        /// <summary>
        /// 发送表情
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        public void SendEmoticon(string EMoticonMd5, string from, string to, int type)
        {
            string msg_json = "{{" +
            "\"BaseRequest\":{{" +
                "\"DeviceID\" : \"{0}\"," +
                "\"Sid\" : \"{1}\"," +
                "\"Skey\" : \"{2}\"," +
                "\"Uin\" : \"{3}\"" +
            "}}," +
            "\"Msg\" : {{" +
                "\"ClientMsgId\" : {4}," +
                "\"EMoticonMd5\" : \"{5}\"," +
                "\"FromUserName\" : \"{6}\"," +
                "\"LocalID\" : {7}," +
                "\"ToUserName\" : \"{8}\"," +
                "\"Type\" : {9}" +
            "}}," +
            "\"rr\" : {10}" +
            "}}";
            var entity = LoginCore.GetPassTicket(Uin);
            if (Sid != null && Uin != null)
            {
                //((long)(DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds) * 10000
                msg_json = string.Format(msg_json, DeviceID, Sid, entity.SKey, Uin, DateTime.Now.Millisecond, EMoticonMd5, from, DateTime.Now.Millisecond, to, type, DateTime.Now.Millisecond);
                byte[] bytes = HttpService.SendPostRequest(BaseUrl + Constant._webwxsendemoticon_url + "&pass_ticket=" + entity.PassTicket, msg_json, Uin);
                string send_result = Encoding.UTF8.GetString(bytes);
                var aa = send_result;
            }
            //((Action)delegate()
            //{
            //    //存储发送消息
            //    var b = WxOperateLogDal.AddWxsendmsglog(Uin, from, msg,to,from);
            //}).BeginInvoke(null, null);
        }
        public string UploadMedia(string url, string picpath, string mediaName,  string sid, string DeviceID,string fromUser,string toUser)
        {
            string result = string.Empty;
            var entity = LoginCore.GetPassTicket(Uin);
            var webwx_data_ticket = "";

            var lastModified = new FileInfo(picpath).LastWriteTime;
            var dt = lastModified.ToString("r").Replace(",", "") + lastModified.ToString("zzz").Replace(":", "");
            var lastModifiedDate=dt.Split(' ')[0].ToString() + " "
                          + dt.Split(' ')[2].ToString() + " "
                          + dt.Split(' ')[1].ToString() + " "
                          + dt.Split(' ')[3].ToString() + " "
                          + dt.Split(' ')[4].ToString() + " "
                          + dt.Split(' ')[5].ToString() + " (中国标准时间)";



            var fileSize = Tools.Tools.FileSize(picpath);
            var chunkSize = 512 * 1024;
            var chunks = Math.Ceiling((double)fileSize / (double)chunkSize);
            var clientMediaID = DateTime.Now.Millisecond;
            var fileMd5 = HttpService.GetMD5(picpath);
            var fs = new FileStream(picpath, FileMode.Open,FileAccess.Read);
            var length = fs.Length;
            var current = 0;
            var bReader = new BinaryReader(fs);
            byte[] data;
            var byteCount = 512 * 1024;
            int i= 0;
            for (; current <= length; current = current + byteCount)
            //for (int i = 0; i < chunks; i++)
            {
                CookieContainer _cookiesContainer = HttpService.CookiesContainerDic[Uin] ;
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Accept = "*/*";
                request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
                request.ContentType = "multipart/form-data; boundary=----WebKitFormBoundarya4gGLw"+i+"MjIJx7nyC";
                request.KeepAlive = true;
                //request.ProtocolVersion = HttpVersion.Version11;
                request.Timeout = 25000;
                request.AllowAutoRedirect = true;
                request.Host = "file." + LoginCore.GetPassTicket(Uin).WxHost;
                request.Headers.Add("Origin", "https://" + LoginCore.GetPassTicket(Uin).WxHost);
                request.Referer = "https://" + LoginCore.GetPassTicket(Uin).WxHost + "/?&lang=zh_CN";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.82 Safari/537.36";
                request.Method = "POST";
                //ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
                if (!string.IsNullOrEmpty(Uin))
                {                   
                    request.CookieContainer = _cookiesContainer;  //启用cookie
                    foreach (var c in HttpService.GetAllCookies(_cookiesContainer))
                    {
                        Tools.Tools.WriteLog(c.ToString());
                        if (c.ToString().Contains("webwx_data_ticket"))
                        {
                            webwx_data_ticket = c.ToString().Split(new string[] { "=" }, StringSplitOptions.None)[1];
                        }
                    }

                }
                //ServicePointManager.ServerCertificateValidationCallback  += RemoteCertificateValidate;
            
                using (var ms = new MemoryStream())
                {
                    StringBuilder sb = new StringBuilder();
                    string uploadmediarequest = "{\"UploadType\":2," +
                        "\"BaseRequest\":{" +
                        "\"Uin\":" + Uin + "," +
                        "\"Sid\":\"" + sid + "\"," +
                        "\"Skey\":\"" + entity.SKey + "\"," +
                        "\"DeviceID\":\"" + DeviceID + "\"}," +
                        "\"ClientMediaId\":" + clientMediaID+ "," +
                        "\"TotalLen\":" + length + "," +
                        "\"StartPos\":0," +
                        "\"DataLen\":" + length + "," +
                        "\"FileMd5\":\"" + fileMd5 + "\"," +
                        "\"FromUserName\":\"" + fromUser + "\"," +
                        "\"ToUserName\":\"" + toUser + "\"," +
                        "\"MediaType\":4}";

                    var boundary = "------WebKitFormBoundarya4gGLw" + i + "MjIJx7nyC\r\n";

                    var lastBoundary = "------WebKitFormBoundarya4gGLw" + i + "MjIJx7nyC--\r\n";

                    var enterStr = "\r\n";

                    sb.Append(boundary);
                    sb.Append("Content-Disposition: form-data; name=\"id\"\r\n\r\n");
                    sb.Append("WU_FILE_" + Core.Constant.WU_FILE_N + enterStr);

                    sb.Append(boundary);
                    sb.Append("Content-Disposition: form-data; name=\"name\"" + enterStr + enterStr);
                    sb.Append(mediaName.Substring(mediaName.LastIndexOf("\\") + 1) + enterStr);

                    sb.Append(boundary);
                    sb.Append("Content-Disposition: form-data; name=\"type\"" + enterStr + enterStr);
                    sb.Append("image/jpeg\r\n");


                    sb.Append(boundary);
                    sb.Append("Content-Disposition: form-data; name=\"lastModifiedDate\"" + enterStr + enterStr);
                    //sb.Append("Wed Dec 30 2015 15:24:14 GMT+0800 (中国标准时间)" + enterStr);
                    sb.Append(lastModifiedDate + enterStr);



                    sb.Append(boundary);
                    sb.Append("Content-Disposition: form-data; name=\"mediatype\"" + enterStr + enterStr);
                    sb.Append("pic" + enterStr);


                    sb.Append(boundary);
                    sb.Append("Content-Disposition: form-data; name=\"size\"\r\n\r\n");
                    sb.Append(fileSize + enterStr);

                    if (chunks > 1)
                    {
                        sb.Append(boundary);
                        sb.Append("Content-Disposition: form-data; name=\"chunks\"\r\n\r\n");
                        sb.Append(chunks + enterStr);
                        sb.Append(boundary);
                        sb.Append("Content-Disposition: form-data; name=\"chunk\"\r\n\r\n");
                        sb.Append(i + enterStr);
                    }

                    sb.Append(boundary);
                    sb.Append("Content-Disposition: form-data; name=\"uploadmediarequest\"\r\n\r\n");
                    sb.Append(uploadmediarequest + enterStr);

                    sb.Append(boundary);
                    sb.Append("Content-Disposition: form-data; name=\"webwx_data_ticket\"\r\n\r\n");
                    sb.Append(webwx_data_ticket + enterStr);

                    sb.Append(boundary);
                    sb.Append("Content-Disposition: form-data; name=\"pass_ticket\"\r\n\r\n");
                    sb.Append(entity.PassTicket + enterStr);

                    sb.Append(boundary);
                    sb.Append("Content-Disposition: form-data; name=\"filename\"; filename=\"" + picpath.Substring(picpath.LastIndexOf("\\") + 1) + "\"\r\n");
                    sb.Append("Content-Type: application/octet-stream\r\n\r\n");

                    var bytes1 = Encoding.UTF8.GetBytes(sb.ToString());
                    ms.Write(bytes1, 0, bytes1.Length);

                    sb.Clear();
                    sb = null;
                    fs.Seek(current, SeekOrigin.Current);
                    if (current + byteCount > length)
                        {
                            data = new byte[Convert.ToInt64((length - current))];
                            bReader.Read(data, 0, Convert.ToInt32((length - current)));
                        }
                        else
                        {
                            data = new byte[byteCount];
                            bReader.Read(data, 0, byteCount);
                            
                        }
                        ms.Write(data, 0, data.Length);

                    var bytes2 = Encoding.UTF8.GetBytes(lastBoundary);
                    ms.Write(bytes2, 0, bytes2.Length);

                    using (var dataStream = request.GetRequestStream())
                    {
                        var bytes3 = ms.ToArray();
                        dataStream.Write(bytes3, 0, bytes3.Length);
                    }
                    
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        {
                            result = reader.ReadToEnd();
                            
                        }
                    }
                    
                    
                }
                i++;
            }
            fs.Close();
            JObject _resul = JsonConvert.DeserializeObject(result) as JObject;
            var result1 = _resul["MediaId"].ToString();
            return result1;          
        }
        public static bool CheckValidationResult(object sender, X509Certificate certificate,
X509Chain chain, SslPolicyErrors errors)
        {  // 总是接受  
            return true;
        }
    }
}
