using LinqToExcel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TechQuickCode.App_Code;
using TechQuickCode.Models.Entity;
using TechQuickCode.Models.Manager;
using TKCode.App_Code;
using TKCode.Models.Entity;

namespace TechQuickCode.Controllers
{
    public class UpLoadController : Controller
    {


        public string SimditorImges(HttpPostedFileBase ImageFileName)
        {

            bool uploadOK = false;
            string error = string.Empty;
            string Extension = System.IO.Path.GetExtension(ImageFileName.FileName);
            string fileName = Guid.NewGuid().ToString().Replace("-", "") + (string.IsNullOrEmpty(Extension) ? ".png" : Extension);
            string filePhysicalPath = Server.MapPath("~/ImageUpload/" + fileName);
            string pic = "";
            try
            {
                ImageFileName.SaveAs(filePhysicalPath);
                pic = "/ImageUpload/" + fileName;
                uploadOK = true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            dynamic jr = new ExpandoObject();
            jr.success = uploadOK ? 1 : 0;
            jr.file_path = pic;
            jr.message = error;
            return JsonConvert.SerializeObject(jr);
        }


        [HttpPost]
        public string Imges(HttpPostedFileBase imagefile)
        {

            bool uploadOK = false;
            string error = string.Empty;
            string fileName = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(imagefile.FileName);
            string filePhysicalPath = Server.MapPath("~/ImageUpload/" + fileName);
            string pic = "";
            try
            {
                imagefile.SaveAs(filePhysicalPath);
                pic = "/ImageUpload/" + fileName;
                uploadOK = true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            dynamic jr = new ExpandoObject();
            jr.success = uploadOK ? 1 : 0;
            jr.url = pic;
            jr.message = error;
            return JsonConvert.SerializeObject(jr);
        }
        [HttpPost]
        public string Heads(HttpPostedFileBase headfile, string uid)
        {

            bool uploadOK = false;
            string error = string.Empty;
            string color = "#66FFCC";
            string fileName = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(headfile.FileName);
            string filePhysicalPath = Server.MapPath("~/HeadImg/" + fileName);
            string pic = "";
            try
            {
                headfile.SaveAs(filePhysicalPath);
                pic = "/HeadImg/" + fileName;
                uploadOK = true;
                UserManager.Instance.UpdateUserHeadImg(uid, pic);
                //  color = PictureAnalysis.GetMostUsedColor(filePhysicalPath);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            dynamic jr = new ExpandoObject();
            jr.success = uploadOK ? 1 : 0;
            jr.url = pic;
            jr.color = color;
            jr.message = error;
            return JsonConvert.SerializeObject(jr);
        }

        [HttpPost]
        public string Files(HttpPostedFileBase AttachmentFile)
        {
            dynamic jr = new ExpandoObject();
            bool uploadOK = false;
            string error = string.Empty;
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff_") + AttachmentFile.FileName;
            string filePhysicalPath = Server.MapPath("~/Attribute/" + fileName);
            string filePath = "";
            try
            {
                AttachmentFile.SaveAs(filePhysicalPath);
                string extensionName = System.IO.Path.GetExtension(AttachmentFile.FileName).TrimStart('.').ToLower();
                filePath = "/Attribute/" + fileName;
                uploadOK = true;
                jr.fileName = AttachmentFile.FileName;
                jr.url = filePath;
                jr.type = AttachmentFile.ContentType;
                jr.length = Utils.ConvertBytes(AttachmentFile.ContentLength);
                jr.fontCode = Utils.GetFileLogoStr(extensionName);

                AttachmentItem ai = new AttachmentItem();
                ai.FileFontCode = jr.fontCode;
                ai.FileLength = jr.length;
                ai.FilePath = filePath;
                ai.FileType = jr.type;
                ai.FileName = AttachmentFile.FileName;
                string guid = AttachmentManager.Instance.Add(ai);
                if (string.IsNullOrEmpty(guid))
                {
                    Qing.QLog.SendLog_Exception("文件上传入库失败", "AttributeManager");
                    uploadOK = false;
                }
                else
                {
                    jr.id = guid;
                }

            }
            catch (Exception ex)
            {
                error = ex.Message;
                jr.message = error;
            }

            jr.success = uploadOK ? 1 : 0;
            return JsonConvert.SerializeObject(jr);
        }

        [HttpPost]
        public string ImportArticle(HttpPostedFileBase ArticleFile)
        {

            int count=0;
            try
            {
              
                string filepath=Server.MapPath("~/Attribute/_IA_" + DateTime.Now.ToString("yyyyMMddHHmmssfff_") + ArticleFile.FileName);
                ArticleFile.SaveAs(filepath);
                 var excelFile = new ExcelQueryFactory(filepath);
                 var data = excelFile.Worksheet<ImportArticleItem>();
                 count = data.Count();
                 foreach (var item in data)
                 {
                     ArticleItem ai = new ArticleItem();
                     ai.GUID = ArticleManager.Instance.GetArticleGUID();
                     ai.ArticleDescription = Utils.ReplaceHtmlTag(item.文档类容);
                     ai.ArticleHeadImage = "/Content/Images/logo.png";
                     ai.ArticlePlate = item.板块名称.Trim();
                     ai.ArticleTags = item.文档标签.Trim();
                     ai.ArticleTitle = item.文档标题.Trim();
                     ai.ArticleType = item.类别名称.Trim();
                     ai.ArticleTypeID = ArticleManager.Instance.GetOrCreateTypeID(ai.ArticlePlate, ai.ArticleType);
                     ai.Author = item.文档作者姓名.Trim();
                     ai.AuthorID = UserManager.Instance.GetUserByUserName(ai.Author);
                     ai.CreateTime = DateTime.Now;
                     ArticleContentItem  aci=new ArticleContentItem ();
                     aci.ArticleHtml= HttpUtility.UrlDecode(item.文档类容.Trim().Replace("\n","<br/>"));
                     aci.ArticleMarkdown="";
                     ArticleManager.Instance.UpdateArticle(ai.GUID, ai, aci);
                 }
              
            }
            catch (Exception ex)
            {

                Qing.QLog.SendLog(ex.Message);
                return "导入出错："+ex.Message;
            }

            return "导入成功 共" + count+"条";
        }

        public ActionResult ImportArticle()
        {
            UserManager.Instance.CheckLogin(Request, ViewBag);
            return View();
        }

    }
}
