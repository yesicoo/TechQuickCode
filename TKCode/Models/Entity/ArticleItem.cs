using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechQuickCode.Models.Entity
{
    public class ArticleItem:Qing.QEInterface
    {
        public string GUID { set; get; }
        public string ArticleTitle { set; get; }
        public string ArticlePlate { set; get; }
        public string ArticleType { set; get; }
        public int ArticleTypeID { set; get; }
        public string ArticleHeadImage { set; get; }
        public string ArticleDescription { set; get; }
        public string ArticleTags { set; get; }
        public string ArticleAttachments { set; get; }

        public string Author { set; get; }
        public string AuthorID { set; get; }
        public int ReadCount{set;get;}
        public int CommentCount { set; get; }
        public double Score { set; get; }

      //  public int Publish { set; get; }
        public DateTime CreateTime { set; get; }
        public DateTime UpdateTime { set; get; }
    }
}
