using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WxBot.License;

namespace WxBot.Tools
{
    class Regedit
    {
        private static string RegistFileName = "WxBot";
        /// <summary>
        /// 创建一个test注册表项,下面包含OpenLog，和SaveLog两个子项
        /// </summary>
        public static void CreateRegistFile()
        {
            //SOFTWARE在LocalMachine分支下
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
            RegistryKey Wxbot = key.CreateSubKey("WxBot");
            RegistryKey reg = Wxbot.CreateSubKey("reg");


        }

        /// <summary>
        /// 将path写入OPenLog子项
        /// </summary>
        public static void writeToRegistFile(string value)
        {
            RegistryKey key = Registry.CurrentUser;
            RegistryKey software = key.OpenSubKey("SOFTWARE", true);
            RegistryKey test = software.OpenSubKey(RegistFileName, true);
            RegistryKey OpenPath = test.OpenSubKey("reg", true);
            //"name"是该键值的name，相当于一个别名，可自行设置
            OpenPath.SetValue("regCode", value);
        }

        /// <summary>
        /// 判断注册表项是否存在
        /// </summary>
        /// <returns>bool</returns>
        private bool IsRegeditItemExist()
        {
            string[] subkeyNames;
            RegistryKey key = Registry.LocalMachine;
            RegistryKey software = key.OpenSubKey("SOFTWARE");
            subkeyNames = software.GetSubKeyNames();
            //在这里我是判断test表项是否存在
            foreach (string keyName in subkeyNames)
            {
                if (keyName == RegistFileName)
                {
                    key.Close();
                    return true;
                }
            }
            key.Close();
            return false;
        }

        /// <summary>
        /// 判断该路径是否已经存在
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static bool IsRegeditKeyExit(string name)
        {
            string[] saveSubkeyNames;

            RegistryKey key = Registry.LocalMachine;
            RegistryKey software = key.OpenSubKey("SOFTWARE", true);
            RegistryKey test = software.OpenSubKey(RegistFileName, true);
            RegistryKey Savekey = test.OpenSubKey("WxBot\\reg", true);

            //获取该子项下的所有键值的名称saveSubkeyNames 
            saveSubkeyNames = Savekey.GetSubKeyNames();
            foreach (string keyName in saveSubkeyNames)
            {
                if (keyName == name)
                {
                    key.Close();
                    return false;
                }
            }
            key.Close();
            return true;
        }
        public static string getRegCode()
        {
            RegistryKey key = Registry.CurrentUser;
            RegistryKey reg = key.OpenSubKey("SOFTWARE\\WxBot\\reg", true);
            if (reg != null)
            {
                if (reg.GetValue("regCode") != null)
                {
                    return reg.GetValue("regCode").ToString();
                }
            }
            key.Close();
            return "";
           
        }
        public static Dictionary<string, string> getRegCodeS()
        {
            LicSerializable s = new LicSerializable(LicEnumContainer.SerializType.WxBot);
            var WxBot = (Dictionary<string, string>)s.DeSerializable();
            return WxBot;

        }

    }
}
