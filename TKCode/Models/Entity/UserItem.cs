using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechQuickCode.Models.Entity
{
   public class UserItem
    {
       public string GUID { set; get; }
       public string Token { set; get; }
       public string UserNickName { set; get; }
       public string UserName { set; get; }
       public string UserPassword { set; get; }
    }
}
