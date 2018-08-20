using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WxBot.Tools
{
    class Tools
    {
        public static void WriteLog(string strLog)
        {
            string sFilePath = System.Environment.CurrentDirectory + "\\log";
            string sFileName = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            sFileName = sFilePath + "\\" + sFileName; //文件的绝对路径
            if (!Directory.Exists(sFilePath))//验证路径是否存在
            {
                Directory.CreateDirectory(sFilePath);
                //不存在则创建
            }
            FileStream fs;
            StreamWriter sw;
            try
            {
                if (File.Exists(sFileName)) //验证文件是否存在，有则追加，无则创建
                {

                    fs = new FileStream(sFileName, FileMode.Append, FileAccess.Write);
                    fs.Close();
                    fs.Dispose();
                }
            }
            catch
            {
                fs = new FileStream(sFileName, FileMode.Append, FileAccess.Write);
                sw = new StreamWriter(fs);
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   -----   " + strLog);
                sw.Close();
                fs.Close();
                fs.Dispose();
            }
        }

        public static long FileSize(string filePath)
        {
            long temp = 0;

            //判断当前路径所指向的是否为文件
            if (File.Exists(filePath) == false)
            {
                string[] str1 = Directory.GetFileSystemEntries(filePath);
                foreach (string s1 in str1)
                {
                    temp += FileSize(s1);
                }
            }
            else
            {

                //定义一个FileInfo对象,使之与filePath所指向的文件向关联,

                //以获取其大小
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            return temp;
        }
        public static  DateTime ConvertStringToDateTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

    }
}
