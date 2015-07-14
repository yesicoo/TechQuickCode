using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using TechQuickCode.App_Code;

namespace TKCode
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new Engine());

            System.IO.DirectoryInfo di_ImageUpload = new System.IO.DirectoryInfo(Server.MapPath("~/") + "ImageUpload/");
            di_ImageUpload.Create();
            System.IO.DirectoryInfo di_Attribute = new System.IO.DirectoryInfo(Server.MapPath("~/") + "Attribute/");
            di_Attribute.Create();
            Config.ArticleDataPath = Server.MapPath("~/") + "Article.db";
            Qing.QLog.Init();
            Qing.QLog.StartWS(Config.LogPort);

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}