using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WxBot.License
{
    
    class CheckReg
    {
        public static bool isReg(string regCode)
        {
            //取机器码
            string mCode = RegInfo.GetMachineCode();
            //产生注册码
            //string regCode = RegInfo.CreateRegisterCode(mCode, DateTime.Now.AddMonths(1));
            DateTime time = DateTime.Now;
            bool resu = RegInfo.CheckRegister(regCode, ref time);
            if (resu)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool isRegS(Dictionary<string, string> WxBot)
        {
            if (WxBot != null)
            {
                //取机器码
                string mCode = RegInfo.GetMachineCode();
                //产生注册码
                //string regCode = RegInfo.CreateRegisterCode(mCode, DateTime.Now.AddMonths(1));
                DateTime time = DateTime.Now;
                bool resu = RegInfo.CheckRegister(WxBot["regCode"], WxBot["overTime"]);
                if (resu)
                {
                    Core.Constant.overtime = WxBot["overTime"];
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            
        }
    }
}
