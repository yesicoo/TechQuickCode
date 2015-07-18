using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TKCode.Models.Entity
{
   public class LoginItem
    {
       public bool success { set; get; }
       public string  message { set; get; }
       public int count { set; get; }

       public List< LoginResult> results { set; get; }
    }

   public class LoginResult
   {
       public string id { set; get; }
       public string name { set; get; }
       public string mail { set; get; }
   }
}
