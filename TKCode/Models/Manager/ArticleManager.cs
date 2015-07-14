using STSdb4.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechQuickCode.App_Code;
using TechQuickCode.Models.Entity;
using Dapper;
using Qing;

namespace TechQuickCode.Models.Manager
{
    public class ArticleManager
    {
        public static readonly ArticleManager Instance = new ArticleManager();
        private IStorageEngine _SE = STSdb.FromFile(Config.ArticleDataPath);
        private ITable<string, ArticleContentItem> _Table = null;
        List<string> guids = new List<string>();
        ArticleManager()
        {
            _Table = _SE.OpenXTable<string, ArticleContentItem>("Articles");
        }
        public string GetArticleGUID()
        {
            string guid = Guid.NewGuid().ToString().Replace("-", "");
            guids.Add(guid);
            return guid;
        }
        public bool Update(string guid, ArticleItem ai, ArticleContentItem aci)
        {
            bool result = false;

            ai.GUID = guid;
            ai.UpdateTime = DateTime.Now;
            var imgs = Utils.PickupImgUrl(aci.ArticleHtml);
            if (imgs.Count > 0)
            {
                ai.ArticleHeadImage = imgs[0];
            }
            else
            {
                ai.ArticleHeadImage = "/content/images/logo.png";
            }
            ai.ArticleDescription = Utils.ReplaceHtmlTag(aci.ArticleHtml);

            try
            {
                var conn = MySQLConnectionPool.GetConnection();
                if (guids.Contains(guid))
                {
                    string insertSQL = "Insert Into `ArticleList` (`GUID`,`CreateTime`) values ('" + guid + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "');";
                    conn.Execute(insertSQL);
                    guids.Remove(guid);
                }
                string UpdateSql = ai.GetUpdateSQL("GUID", "ArticleList", "CreateTime");
                QLog.SendLog_Debug(UpdateSql, "ArticleManager");
                conn.Execute(UpdateSql);
                conn.GiveBack();
                _Table[guid] = aci;
                _SE.Commit();
                result = true;
            }
            catch (Exception ex)
            {
                QLog.SendLog_Debug(ex.Message, "ArticleManager");
            }
            return result;
        }

        internal ArticleItem GetArticleItem(string id)
        {
            ArticleItem ai = null;
            var conn = MySQLConnectionPool.GetConnection();
            try
            {
                ai = conn.Query<ArticleItem>("update ArticleList Set ReadCount=ReadCount+1 where  GUID='" + id + "'; select * from ArticleList where GUID='" + id + "';").SingleOrDefault();
            }
            catch (Exception ex)
            {
                QLog.SendLog(ex.ToString());
            }
            conn.GiveBack();

            return ai;
        }

        internal ArticleContentItem GetArticleContentItem(string id)
        {
            ArticleContentItem aci = null;
            _Table.TryGet(id, out aci);
            return aci;
        }

        internal Dictionary<string, List<string>> GetArticlePlateType()
        {
            var conn = MySQLConnectionPool.GetConnection();
            List<ArticlePlateTypeItem> aptis = conn.Query<ArticlePlateTypeItem>("select * from ArticlePlateType").ToList();
            var result = aptis.GroupBy(x => x.PlateName).ToDictionary(k => k.Key, v => v.Select(vv => vv.TypeName).ToList());
            conn.GiveBack();
            return result;
        }

        internal object GetStar(string ArticleID, string UserID)
        {
            return null;
        }

        /// <summary>
        /// 发布文章
        /// </summary>
        /// <param name="articleID"></param>
        /// <returns></returns>
        internal bool Publish(string articleID)
        {
            var conn = MySQLConnectionPool.GetConnection();
            string sql = string.Format("update `ArticleList` Set  Publish=1 where  GUID='{0}';", articleID);
           var result= conn.Execute(sql);
            conn.GiveBack();
            return result == 1 ? true : false;
        }


        internal List<TypeID> GetArticleType(string plateName)
        {
            var conn = MySQLConnectionPool.GetConnection();
            string sql = string.Format("select * from `ArticlePlateType` where PlateName='{0}';", plateName);
            List<TypeID> result = conn.Query<TypeID>(sql).ToList();
            conn.GiveBack();
            return result;
        }

        internal int GetTypeID(string plateName,string typeName)
        {
            var conn = MySQLConnectionPool.GetConnection();
            string sql = string.Format("select * from `ArticlePlateType` where PlateName='{0}' and TypeName='{1}';", plateName, typeName);
            var id = conn.ExecuteScalar(sql);
            conn.GiveBack();
            if (id == null)
            {
                return -1;
            }
            else
            {
                return Convert.ToInt32(id);
            }
        }

        internal List<ArticleItem> GetArticleItems(string PlateName, string TypeName, int page,int count=10)
        {
            StringBuilder sb_sql = new StringBuilder("select * from ArticleList where Publish=1 and ArticlePlate='").Append(PlateName).Append("'");
            switch (TypeName)
            {
                case "最新":
                    sb_sql.Append(" order by CreateTime desc ");
                    break;
                case "热门":
                    sb_sql.Append(" order by readCount desc ");
                    break;
                case "精华":
                    sb_sql.Append(" order by Score desc ");
                    break;
                default:
                    sb_sql.AppendFormat(" and ArticleType='{0}' order by CreateTime desc ", TypeName);
                    break;
            }
            sb_sql.AppendFormat(" limit {0},{1}", (page - 1) * count, count);
            var conn = MySQLConnectionPool.GetConnection();
            var result = conn.Query<ArticleItem>(sb_sql.ToString()).ToList();
            conn.GiveBack();
            return result;
        }

       

        internal string GetHtmlList(string PlateName, string TypeName, int Page, int count)
        {
            string result = "";
            List<ArticleItem> ArticleItems = GetArticleItems(PlateName, TypeName, Page, count);
            switch (PlateName)
            {
                case "技术片段":
                case "项目框架":
                    if (ArticleItems.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var item in ArticleItems)
                        {
                            sb.AppendFormat("<div class='ArticleItem'><div class='ArticleImg pull-left'><img src='{0}' width='100%' height='100%' /></div><div class='ArticleCard'><div class='ArticleTitle'><a href='/Article/Details/{1}' target='_blank'>{2}</a></div><div class='ArticleAttributes'><span><code>{3}</code></span><span>发布于:{4}</span><span>&nbsp;</span><span>分类：</span><span><a href='/Article/List/{5}'>{5}</a>&nbsp;/&nbsp;<a>{6}</a></span><span>&nbsp;</span><span>阅读：(</span><span>{7}</span><span>)</span><span>&nbsp;</span><span>评论：(</span><span>{8}</span><span>)</span><span>&nbsp;</span><span>评级：(</span><span>{9}分</span><span>)</span></div><div class='ArticleContent'><span>{10}</span></div><div class='ArticleTags'>",
                                 item.ArticleHeadImage, item.GUID, item.ArticleTitle, item.Author, item.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"), item.ArticlePlate, item.ArticleType, item.ReadCount, item.CommentCount, item.Score, item.ArticleDescription);
                            if (!string.IsNullOrEmpty(item.ArticleTags))
                            {
                                string[] Tags = item.ArticleTags.Split(new string[] { ",", "，", ";", "；" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var tag in Tags)
                                {
                                    sb.AppendFormat("<span class='btn btn-success'>{0}</span>", tag);
                                }
                            }
                            sb.Append("</div></div></div>");
                        }
                        result= sb.ToString();
                    }
                    else
                    {
                        result= " <div style='height:280px;text-align:center;padding-top:80px;'> 凸(艹皿艹 ) &nbsp;&nbsp;&nbsp;&nbsp;在数据库翻了翻，一篇文章都没有......</div>";
                    }
                    break;
                case "案例库":
                    if (ArticleItems.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < ArticleItems.Count; i++)
                        {
                            var ai = ArticleItems[i];
                            sb.AppendFormat("<div class='ExampleTitle'><div class='pull-left  text'><span class='Index-{0}'>{0}</span><a href='/Article/Details/{1}' target='_blank'>{2}</a></div><div class='pull-right author'> <span class='pull-left'><a href='/User/Details/{3}' target='_blank'>{4}</a></span><span class='pull-right'>{5}</span></div><div style='clear: both;'></div></div>", i+1, ai.GUID, ai.ArticleTitle, ai.AuthorID, ai.Author, ai.CreateTime.ToString("yyyy-MM-dd"));
                        }
                        result = sb.ToString();
                    }
                    else
                    {
                        result = " <div style='height:140px;text-align:center;padding-top:40px;'> 凸(艹皿艹 ) &nbsp;&nbsp;&nbsp;&nbsp;在数据库翻了翻，一篇文章都没有......</div>";
                    }
                    break;
                case "实用工具":
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}
