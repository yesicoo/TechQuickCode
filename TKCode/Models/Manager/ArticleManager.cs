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
                string UpdateSql = ai.GetUpdateSQL("GUID", "ArticleList", "CreateTime,Author,AuthorID");
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
                ai = conn.Query<ArticleItem>("select * from ArticleList where GUID='" + id + "';").SingleOrDefault();
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

        internal List<ArticleItem> GetArticleItems(string PlateName, string TypeName, int page)
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
            sb_sql.AppendFormat(" limit {0},{1}", (page - 1) * 10, 10);
            var conn = MySQLConnectionPool.GetConnection();
            var result = conn.Query<ArticleItem>(sb_sql.ToString()).ToList();
            conn.GiveBack();
            return result;
        }
    }
}
