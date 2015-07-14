using Qing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechQuickCode.Models.Entity
{
    public class AttachmentItem : QEInterface
    {
        public string GUID { set; get; }
        public string FileName { set; get; }
        public string FilePath { set; get; }
        public string FileType { set; get; }
        public string FileFontCode { set; get; }
        public string FileLength { set; get; }
        public int DownLoadCount { set; get; }
        public DateTime UpLoadTime { set; get; }
             
    }
}
