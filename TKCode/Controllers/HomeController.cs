using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TechQuickCode.Models.Manager;

namespace TKCode.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        
        public ActionResult Index()
        {
            UserManager.Instance.CheckLogin(Request, ViewBag);
            return View();
        }

        public ActionResult Login(string id)
        {
            ViewBag.IsNav = id == "Nav";
            UserManager.Instance.CheckLogin(Request, ViewBag);
            return View();
        }

        public bool Logining(string UserName, string UserPassword)
        {
            string result = UserManager.Instance.Login(UserName, UserPassword);
            if (result == "Error")
            {
                return false;
            }
            else
            {
                Response.Cookies.Add(new HttpCookie("Token") { Name = "Token", Value = result, Expires = DateTime.Now.AddDays(7) });
                return true;
            }

        }
    }
}
