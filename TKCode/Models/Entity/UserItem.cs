using Qing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechQuickCode.Models.Entity
{
   public class UserItem:QEInterface
    {
       public string GUID { set; get; }
       public string Token { set; get; }
       public string UserNickName { set; get; }
       public string UserHeadColor { set; get; }
       public string UserHeadImg { set; get; }
       public string UserEmail { set; get; }
    }
}
