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

        public ActionResult NickDetails(string NickName)
        {
            UserManager.Instance.CheckLogin(Request, ViewBag);
            if (NickName == null)
            {
                //如何到首页呢
                return View("Index");
            }
            else
            {

                ViewBag.Title = "页面找不到了....";
                bool find = false;
                ArticleContentItem aci = ArticleManager.Instance.GetArticleContentItem(NickName);
                if (aci != null)
                {
                    ViewBag.Tags = new List<string>();
                    ArticleItem ai = ArticleManager.Instance.GetArticleItem(NickName);
                    ViewBag.Title = ai.ArticleTitle;
                    ViewBag.ArticleItem = ai;
                    if (!string.IsNullOrEmpty(ai.ArticleTags))
                    {
                        ViewBag.Tags = ai.ArticleTags.Split(new string[] { ",", "，", ";", "；" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                    ViewBag.ArticleContentItem = aci;
                    find = true;
                }
                if (find)
                {
                    return View("Details");
                }
                else
                {
                    return View("404");
                }
            }
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
        public ActionResult Edit(string id)
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


        [HttpPost]
        public string Edit(string id, FormCollection collection)
        {

            dynamic re = new ExpandoObject();
            var cookie = Request.Cookies["Token"];
            if (cookie == null)
            {
                re.success = 0;
                re.Error = "登录失效";
            }
            else
            {
                var acticleItem = new ArticleItem();
                var user = UserManager.Instance.GetUserByToken(cookie.Value);
                var acticleContentItem = new ArticleContentItem();
                TryUpdateModel<ArticleItem>(acticleItem, collection);
                TryUpdateModel<ArticleContentItem>(acticleContentItem, collection);
                acticleItem.Author = user.UserNickName;
                acticleItem.AuthorID = user.GUID;
                acticleContentItem.ArticleHtml = HttpUtility.UrlDecode(acticleContentItem.ArticleHtml);
                acticleContentItem.ArticleMarkdown = HttpUtility.UrlDecode(acticleContentItem.ArticleMarkdown);
                bool result = ArticleManager.Instance.UpdateArticle(id, acticleItem, acticleContentItem);
                re.success = result ? 1 : 0;
                re.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

            return JsonConvert.SerializeObject(re);

        }
        #endregion

        #region 发布
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
        public string ArticleHTMLList(string PlateName, string TypeName, int Page, int count = 10)
        {
            string HtmlStr = ArticleManager.Instance.GetHtmlList(PlateName, TypeName, Page, count);
            return HtmlStr;
        }

        public string ArticleItemListByPlate(string PlateName, int Page, int count = 10)
        {
            var listItem = ArticleManager.Instance.GetArticleItemsByPlate(PlateName, Page, count);
            var newItmes = listItem.Select(x => new {x.GUID,x.ArticleTitle, x.ArticleDescription, x.ArticlePlate, x.ArticleType, x.ArticleTypeID, CreateTime = x.CreateTime.ToString("yyyy-MM-dd HH:mm:ss") });
            return JsonConvert.SerializeObject(newItmes);
        }

        public string ArticleItemListByPlateForUser(string PlateName, int Page,string Token,int count = 10)
        {

            var listItem = ArticleManager.Instance.GetArticleItemsByPlateForUser(PlateName, Page,Token, count);
            var newItmes = listItem.Select(x => new { x.GUID, x.ArticleTitle, x.ArticleDescription, x.ArticlePlate, x.ArticleType, x.ArticleTypeID, CreateTime = x.CreateTime.ToString("yyyy-MM-dd HH:mm:ss") });
            return JsonConvert.SerializeObject(newItmes);
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
        public void AddPlateType(string PlateName, string TypeName)
        {
            ArticleManager.Instance.AddPlateType(PlateName, TypeName);
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
                    sb.AppendFormat("<option>{0}</option>", option.TypeName);
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
            var result = ArticleManager.Instance.GetArticleTypesByPlateName(PlateName);
            return JsonConvert.SerializeObject(result);
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
