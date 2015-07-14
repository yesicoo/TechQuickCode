using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TKCode.Models.Entity
{
    public class JResult
    {
        public bool Success { set; get; }
        public string ErrorMsg { set; get; }
        public dynamic Data { set; get; }
    }
}
