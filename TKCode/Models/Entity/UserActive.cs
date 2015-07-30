using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TKCode.Models.Entity
{
    public class UserActive:Qing.QEInterface
    {
       
        public string UserID { set; get; }
        public string TitleHtml { set; get; }
        public string Content { set; get; }
        public DateTime CreateTime { set; get; }
    }
}
