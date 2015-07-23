using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TechQuickCode.App_Code;
using TechQuickCode.Models.Entity;
using TechQuickCode.Models.Manager;
using TKCode.App_Code;

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

        public string Heads(HttpPostedFileBase headfile)
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
                color = PictureAnalysis.GetMostUsedColor(pic);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            dynamic jr = new ExpandoObject();
            jr.success = uploadOK ? 1 : 0;
            jr.url = pic;
            jr.color = pic;
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
    }
}
