using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WxBot.Core.Entity
{
    class PassTicketEntity
    {
        public string WxSid { get; set; }

        public string WxUin { get; set; }

        public string SKey { get; set; }

        public string PassTicket { get; set; }
        public string WxHost { get; set; }
    }
}
