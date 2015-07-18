using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TechQuickCode.Models.Entity;
using TechQuickCode.Models.Manager;

namespace TKCode.Controllers
{
    public class UserController : Controller
    {
        // GET: /User/Details/5

        public ActionResult Details(string id)
        {
            UserManager.Instance.CheckLogin(Request, ViewBag);
            if (id == null)
            {
                return View("Index");
            }
            UserItem user = UserManager.Instance.GetUserByGUID(id);
            if (user == null)
            {
                return View("404");
            }
            ViewBag.IsOwn = user.GUID == ViewBag.User.GUID;
            ViewBag.WacthUser = user;
            return View();
        }

        public string GetTypes(string id)
        {
            dynamic data = ArticleManager.Instance.GetTypesByUserID(id);
            return JsonConvert.SerializeObject(data);
        }

    }
}
