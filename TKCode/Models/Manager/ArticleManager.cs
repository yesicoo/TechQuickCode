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
        List<ArticlePlateTypeItem> PlateTypeItems = new List<ArticlePlateTypeItem>();
        List<string> guids = new List<string>();

        #region 初始化
        ArticleManager()
        {
            _Table = _SE.OpenXTable<string, ArticleContentItem>("Articles");
            var conn = MySQLConnectionPool.GetConnection();
            PlateTypeItems = conn.Query<ArticlePlateTypeItem>("select * from ArticlePlateType").ToList();
            conn.GiveBack();

        }
        #endregion

        #region 获取唯一标识
        public string GetArticleGUID()
        {
            string guid = Guid.NewGuid().ToString().Replace("-", "");
            guids.Add(guid);
            return guid;
        }
        #endregion

        #region 更新文章
        public bool UpdateArticle(string guid, ArticleItem ai, ArticleContentItem aci)
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
                ai.ArticleHeadImage = "/Content/Images/logo.png";
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
                ai.ArticleTypeID = GetTypeID(ai.ArticlePlate, ai.ArticleType);
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
        #endregion

        #region 读取文章列表信息
        internal ArticleItem GetArticleItem(string id)
        {
            ArticleItem ai = null;
            var conn = MySQLConnectionPool.GetConnection();
            try
            {
                ai = conn.Query<ArticleItem>(string.Format("update ArticleList Set ReadCount=ReadCount+1 where  GUID='{0}'; select * from ArticleList where GUID='{0}';", id)).SingleOrDefault();
            }
            catch (Exception ex)
            {
                QLog.SendLog(ex.ToString());
            }
            conn.GiveBack();

            return ai;
        }
        #endregion

        #region 获取文章正文内容
        internal ArticleContentItem GetArticleContentItem(string id)
        {
            ArticleContentItem aci = null;
            _Table.TryGet(id, out aci);
            return aci;
        }

        #endregion

        #region 发布文章
        /// <summary>
        /// 发布文章
        /// </summary>
        /// <param name="articleID"></param>
        /// <returns></returns>
        internal bool Publish(string articleID)
        {
            var conn = MySQLConnectionPool.GetConnection();
            string sql = string.Format("update `ArticleList` Set  Publish=1,CreateTime='{1}' where  GUID='{0}';", articleID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            var result = conn.Execute(sql);
            conn.GiveBack();
            return result == 1 ? true : false;
        }
        #endregion

        #region 分类获取板块文章数据
        internal List<ArticleItem> GetArticleItemsByPlate(string PlateName,int page, int count = 10)
        {
            StringBuilder sb_sql = new StringBuilder("select * from ArticleList where Publish=1");
            if (PlateName != "All")
            {
                sb_sql.Append(" and ArticlePlate='").Append(PlateName).Append("'");
            }
            sb_sql.AppendFormat(" order by CreateTime desc  limit {0},{1}", (page - 1) * count, count);
            var conn = MySQLConnectionPool.GetConnection();
            var result = conn.Query<ArticleItem>(sb_sql.ToString()).ToList();
            conn.GiveBack();
            return result;
        }

        #endregion


        #region 分页获取文章数据
        internal List<ArticleItem> GetArticleItems(string PlateName, string TypeName, int page, int count = 10)
        {
            StringBuilder sb_sql = new StringBuilder("select * from ArticleList where Publish=1 and ArticlePlate='").Append(PlateName).Append("'");
            switch (TypeName)
            {
                case "最新":
                    sb_sql.Append(" order by CreateTime desc ");
                    break;
                case "热门":
                    sb_sql.Append(" order by ReadCount desc ");
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
        #endregion

        #region 转HTML格式输出文章列表
        internal string GetHtmlList(string PlateName, string TypeName, int Page, int count)
        {
            string result = "";
            List<ArticleItem> ArticleItems = GetArticleItems(PlateName, TypeName, Page, count);
            switch (PlateName)
            {

                case "案例库":
                    if (ArticleItems.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < ArticleItems.Count; i++)
                        {
                            var ai = ArticleItems[i];
                            sb.AppendFormat("<div class='ExampleTitle'><div class='pull-left  text'><span class='Index-{0}'>{0}</span><a href='/Article/Details/{1}' target='_blank'>{2}</a></div><div class='pull-right author'> <span class='pull-left'><a href='/User/Details/{3}' target='_blank'>{4}</a></span><span class='pull-right'>{5}</span></div><div style='clear: both;'></div></div>", i + 1, ai.GUID, ai.ArticleTitle, ai.AuthorID, ai.Author, ai.CreateTime.ToString("yyyy-MM-dd"));
                        }
                        result = sb.ToString();
                    }
                    else
                    {
                        result = " <div style='height:140px;text-align:center;padding-top:40px;'> 凸(艹皿艹 ) &nbsp;&nbsp;&nbsp;&nbsp;在数据库翻了翻，一篇文章都没有......</div>";
                    }
                    break;
                case "实用工具":
                    if (ArticleItems.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < ArticleItems.Count; i++)
                        {
                            var ai = ArticleItems[i];
                            sb.AppendFormat("<div class='Tool'><div> <span class='ToolTitle'>{0}</span></div> <div><img src='{1}' title='VS 2010'></div> <div class='About'>{2}</div> <div> <button class='btn' onclick=\"ToDetaDetails('{3}')\">详细介绍</button> <button class='btn btn-warning'onclick=\"ToDownLoad('{4}')\">立即下载</button> </div> </div>"
                                , ai.ArticleTitle, ai.ArticleHeadImage, ai.ArticleDescription, ai.GUID, ai.ArticleAttachments.TrimEnd(','));
                        }
                        result = sb.ToString();
                    }
                    else
                    {
                        result = " <div style='height:140px;text-align:center;padding-top:40px;'> 凸(艹皿艹 ) &nbsp;&nbsp;&nbsp;&nbsp;在数据库翻了翻，一篇文章都没有......</div>";
                    }

                    break;
                default:
                    if (ArticleItems.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var item in ArticleItems)
                        {
                            sb.AppendFormat("<div class='ArticleItem'><div class='ArticleImg pull-left'><img src='{0}' width='100%' height='100%' /></div><div class='ArticleCard'><div class='ArticleTitle'><a href='/Article/Details/{1}' target='_blank'>{2}</a></div><div class='ArticleAttributes'><span><a  href='/User/Details/{12}' target='_blank'>{3}</a></span><span>发布于:{4}</span><span>&nbsp;</span><span>分类：</span><span><a href='/Article/List/{5}'>{5}</a>&nbsp;/&nbsp;<a  href='/Article/List/{5}?Type={11}'>{6}</a></span><span>&nbsp;</span><span>阅读：(</span><span>{7}</span><span>)</span><span>&nbsp;</span><span>评论：(</span><span>{8}</span><span>)</span><span>&nbsp;</span><span>评级：(</span><span>{9}分</span><span>)</span></div><div class='ArticleContent'><span>{10}</span></div><div class='ArticleTags'>",
                                 item.ArticleHeadImage, item.GUID, item.ArticleTitle, item.Author, item.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"), item.ArticlePlate, item.ArticleType, item.ReadCount, item.CommentCount, item.Score, item.ArticleDescription, item.ArticleTypeID, item.AuthorID);
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
                        result = sb.ToString();
                    }
                    else
                    {
                        result = " <div style='height:280px;text-align:center;padding-top:80px;'> 凸(艹皿艹 ) &nbsp;&nbsp;&nbsp;&nbsp;在数据库翻了翻，一篇文章都没有......</div>";
                    }
                    break;
            }
            return result;
        }
        #endregion

        #region 评论
        internal object GetStar(string ArticleID, string UserID)
        {
            return null;
        }
        #endregion

        #region 分类与类型
        internal Dictionary<string, List<ArticlePlateTypeItem>> GetArticlePlateType()
        {
            var result = PlateTypeItems.GroupBy(x => x.PlateName).ToDictionary(k => k.Key, v => v.ToList());
            return result;
        }

        internal string AddPlateType(string PlateName, string TypeName)
        {
            string id = "";
            var item = PlateTypeItems.Where(x => x.PlateName == PlateName && x.TypeName == TypeName).FirstOrDefault();
            if (item == null)
            {
                var conn = MySQLConnectionPool.GetConnection();
                var result = conn.ExecuteScalar(string.Format("INSERT INTO `ArticlePlateType` (`PlateName`, `TypeName`) VALUES ('{0}', '{1}');SELECT LAST_INSERT_ID();", PlateName, TypeName));
                conn.GiveBack();
                ArticlePlateTypeItem apit = new ArticlePlateTypeItem();
                apit.ID = result.ToString();
                apit.PlateName = PlateName;
                apit.TypeName = TypeName;
                apit.Count = 0;
                PlateTypeItems.Add(apit);
                id = apit.ID;
            }
            else
            {
                id = item.ID;
            }
            return id;
        }
        internal dynamic GetArticleTypesByPlateName(string plateName)
        {

            var result = PlateTypeItems.Where(x => x.PlateName == plateName).Select(x => new { x.ID, x.TypeName });
            return result;
        }

        internal int GetTypeID(string plateName, string typeName)
        {
            var item = PlateTypeItems.Where(x => x.PlateName == plateName && x.TypeName == typeName).FirstOrDefault();
            if (item == null)
            {
                return -1;
            }
            else
            {
                return Convert.ToInt32(item.ID);
            }
        }

        internal dynamic GetTypesByUserID(string uid)
        {
            var conn = MySQLConnectionPool.GetConnection();
            dynamic result = conn.Query<dynamic>("SELECT ArticlePlate,COUNT(*) As Count  FROM ArticleList where Publish=1 and AuthorID='" + uid + "' GROUP BY ArticlePlate;");
            conn.GiveBack();
            return result;
        }
        #endregion


        internal List<ArticleItem> GetArticleItemsByPlateForUser(string PlateName, int page, string uid, int count)
        {
            StringBuilder sb_sql = new StringBuilder("select * from ArticleList where  AuthorID='" + uid + "'");
            if (PlateName != "All")
            {
                sb_sql.Append(" and ArticlePlate='").Append(PlateName).Append("'");
            }
            sb_sql.AppendFormat(" order by CreateTime desc  limit {0},{1}", (page - 1) * count, count);
            var conn = MySQLConnectionPool.GetConnection();
            var result = conn.Query<ArticleItem>(sb_sql.ToString()).ToList();
            conn.GiveBack();
            return result;
        }
    }
}
