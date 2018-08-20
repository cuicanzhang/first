using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WxBot.License
{
    class LicSerializable
    {
        string fileName = string.Empty;
        public LicSerializable(LicEnumContainer.SerializType type)
        {
            try
            {
                var dir = Environment.CurrentDirectory;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                fileName = dir + "\\" + type.ToString() + ".dat";//文件名称与路径
                if (!File.Exists(fileName))
                {
                    Core.Constant.isReg = false;
                }
            }
            catch 
            {
                //写日志
                //MessageBox.Show("WxSerializable" + ex.Message);

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
            catch 
            {
                //写日志

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
            catch 
            {

                return null;
            }
        }
    }
}
