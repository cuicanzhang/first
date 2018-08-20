using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WxBot.Core
{
    class WxSerializable
    {
        private string _uin = string.Empty;
        string fileName = string.Empty;
        public WxSerializable(string uin, WxBot.Core.Entity.EnumContainer.SerializType type)
        {
            try
            {
                this._uin = uin;
                var dir = Environment.CurrentDirectory + "\\data\\" + this._uin;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                fileName = dir + "\\" + type.ToString() + ".dat";//文件名称与路径
                if (!File.Exists(fileName))
                {
                    using (var stream = File.Create(fileName))
                    {
                        stream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                //写日志
                //MessageBox.Show("WxSerializable" + ex.Message);
                Tools.Tools.WriteLog(ex.ToString());
                //Tools.WriteLog(ex.ToString());
            }
        }

        public void Serializable(object obj)
        {
            try
            {
                using (Stream fStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
                {
                    BinaryFormatter binFormat = new BinaryFormatter();//创建二进制序列化器
                    binFormat.Serialize(fStream, obj);
                    fStream.Close();
                }
            }
            catch (Exception ex)
            {
                //写日志
                Tools.Tools.WriteLog(ex.ToString());
            }
        }

        public object DeSerializable()
        {
            try
            {
                using (Stream fStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
                {
                    BinaryFormatter binFormat = new BinaryFormatter();//创建二进制序列化器
                    if (fStream.Length > 0)
                    {
                        object obj = binFormat.Deserialize(fStream);//反序列化对象
                        fStream.Close();
                        return obj;
                    }
                    else
                        return null;
                }
            }
            catch (Exception exobj)
            {
                //MessageBox.Show("DeSerializable" + exobj.ToString());
                Tools.Tools.WriteLog(exobj.ToString());
                return null;
            }
        }
    }
}
