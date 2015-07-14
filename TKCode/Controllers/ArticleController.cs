using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Qing;
using TechQuickCode.Models.Manager;
using TechQuickCode.Models.Entity;
using System.Dynamic;
using TechQuickCode.App_Code;
using Newtonsoft.Json;
using Dapper;
using System.Text;
using TKCode.Models.Entity;

namespace TechQuickCode.Controllers
{
    public class ArticleController : Controller
    {
        string LogTag = "ArticleController";
        #region 文章操作
        #region 获取文章
        /// <summary>
        /// 获取文章
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Details(string id)
        {
            UserManager.Instance.CheckLogin(Request, ViewBag);
            ViewBag.Title = "页面找不到了....";
            bool find = false;
            if (!string.IsNullOrEmpty(id))
            {
                ArticleContentItem aci = ArticleManager.Instance.GetArticleContentItem(id);
                if (aci != null)
                {
                    ViewBag.Tags = new List<string>();
                    ArticleItem ai = ArticleManager.Instance.GetArticleItem(id);
                    ViewBag.Title = ai.ArticleTitle;
                    ViewBag.ArticleItem = ai;
                    if (!string.IsNullOrEmpty(ai.ArticleTags))
                    {
                        ViewBag.Tags = ai.ArticleTags.Split(new string[] { ",", "，", ";", "；" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                    ViewBag.ArticleContentItem = aci;
                    find = true;
                }
            }
            if (find)
            {

                return View();
            }
            else
            {
                return View("404");
            }
        }

        public string ArticleHTMLList(string PlateName, string TypeName, int Page)
        {
            List<ArticleItem> ArticleItems = ArticleManager.Instance.GetArticleItems(PlateName, TypeName, Page);
            StringBuilder sb = new StringBuilder();
            foreach (var item in ArticleItems)
            {
                sb.AppendFormat("<div class='ArticleItem'><div class='ArticleImg pull-left'><img src='{0}' width='100%' height='100%' /></div><div class='ArticleCard'><div class='ArticleTitle'><a href='/Article/Details/{1}' target='_blank'>{2}</a></div><div class='ArticleAttributes'><span><code>{3}</code></span><span>发布于:{4}</span><span>&nbsp;</span><span>分类：</span><span><a>{5}</a>&nbsp;/&nbsp;<a>{6}</a></span><span>&nbsp;</span><span>阅读：(</span><span>{7}</span><span>)</span><span>&nbsp;</span><span>评论：(</span><span>{8}</span><span>)</span><span>&nbsp;</span><span>评级：(</span><span>{9}分</span><span>)</span></div><div class='ArticleContent'><span>{10}</span></div><div class='ArticleTags'>",
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
            return sb.ToString();
        }

        #endregion

        #region 创建文章
        public ActionResult Create()
        {
            UserManager.Instance.CheckLogin(Request, ViewBag);
            if (ViewBag.Login)
            {
                string guid = ArticleManager.Instance.GetArticleGUID();
                ViewBag.GUID = guid;
                ViewBag.Title = "创建新的文章";
                return View();
            }
            else
            {
                return Redirect("/Home/Login");
            }
        }
        #endregion

        #region 编辑文章
        [HttpPost]
        public string Edit(string id, FormCollection collection)
        {

            var acticleItem = new ArticleItem();
            acticleItem.Author = ViewBag.UserName;
            var acticleContentItem = new ArticleContentItem();
            TryUpdateModel<ArticleItem>(acticleItem, collection);
            TryUpdateModel<ArticleContentItem>(acticleContentItem, collection);
            acticleContentItem.ArticleHtml = HttpUtility.UrlDecode(acticleContentItem.ArticleHtml);
            acticleContentItem.ArticleMarkdown = HttpUtility.UrlDecode(acticleContentItem.ArticleMarkdown);
            bool result = ArticleManager.Instance.Update(id, acticleItem, acticleContentItem);

            dynamic re = new ExpandoObject();
            re.success = result ? 1 : 0;
            re.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return JsonConvert.SerializeObject(re);

        }
        #endregion

        [HttpPost]
        public string Publish(string ArticleID)
        {
            JResult jr = new JResult();
            try
            {
                var r = ArticleManager.Instance.Publish(ArticleID);
                jr.Success = r;
            }
            catch (Exception ex)
            {
                jr.ErrorMsg = ex.Message;
                QLog.SendLog_Exception(ex.ToString(), LogTag);
            }
            return JsonConvert.SerializeObject(jr);
        }

        #endregion

        #region 列表
        [HttpGet]
        public ActionResult List(string id)
        {
            UserManager.Instance.CheckLogin(Request, ViewBag);
            if (string.IsNullOrEmpty(id))
            {
                return View("404");
            }
            else if (!Config.Plates.Contains(id))
            {
                return View("404");
            }
            else
            {
                ViewBag.PlateName = id;
                ViewBag.Title = id + "-TechQuick'Code";
                ViewBag.TypeID = "";
                if (Request.Params.AllKeys.Contains("Type"))
                {
                    ViewBag.TypeID = Request["Type"];
                }
                return View();
            }
        }
        #endregion

        #region 获取附件列表
        [HttpPost]
        public string GetAttachments(string AttachmentGuids)
        {
            var result = AttachmentManager.Instance.GetAttachmentItems(AttachmentGuids);
            return JsonConvert.SerializeObject(result);
        }
        #endregion

        #region 添加板块和分类
        [HttpPost]
        public bool AddPlateType(string PlateName, string TypeName)
        {
            new System.Threading.Thread(() =>
            {
                AttachmentManager.Instance.AddPlateType(PlateName, TypeName);
            }).Start();
            return true;
        }
        #endregion

        #region 获取板块和分类
        [HttpPost]
        public string GetPlateType()
        {
            var data = ArticleManager.Instance.GetArticlePlateType();
            StringBuilder sb = new StringBuilder();
            foreach (var item in data)
            {
                sb.AppendFormat("<optgroup label='{0}'>", item.Key);
                foreach (var option in item.Value)
                {
                    sb.AppendFormat("<option>{0}</option>", option);
                }
                sb.Append("<option>--新增--</option>");
                sb.Append("</optgroup>");
            }
            return sb.ToString();
        }

        [HttpPost]
        public string GetType(string PlateName)
        {
            QLog.SendLog(PlateName);
            List<TypeID> types = ArticleManager.Instance.GetArticleType(PlateName);
            return JsonConvert.SerializeObject(types);
        }
        #endregion

        #region 评星和评论
        #region 获取评分
        [HttpPost]
        public string GetStar(string ArticleID, string UserID)
        {
            var result = ArticleManager.Instance.GetStar(ArticleID, UserID);
            return null;
        }
        #endregion



        #endregion
    }
}
