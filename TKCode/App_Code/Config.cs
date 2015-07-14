using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace TechQuickCode.App_Code
{
    public static class Config
    {
        public static int LogPort = int.Parse(ConfigurationManager.AppSettings["LogPort"]);
        public static string MySQLConStr = ConfigurationManager.AppSettings["MySQLConStr"];
        public static string ArticleDataPath ;
        public static string[] Plates = { "技术片段", "项目框架", "实用工具", "案例库", "问答"};
        public static string ArticleItemStr = "<div class='ArticleItem'><div class='ArticleImg pull-left'><img src='~/Content/Image/Lazyr.js.jpg' width='100%' height='100%' /></div><div class='ArticleCard'><div class='ArticleTitle'><a>Lazyr.js – 延迟加载图片（Lazy Loading）</a></div><div class='ArticleAttributes'><span><code>teemo</code></span><span>发布于6天前</span><span>&nbsp;</span><span>分类：</span><span><code>前端开发</code></span><span>&nbsp;</span><span>阅读：(</span><span>131</span><span>)</span><span>&nbsp;</span><span>评论：(</span><span>9</span><span>)</span><span>&nbsp;</span><span>评级：(</span><span>★★★★★</span><span>)</span></div><div class='ArticleContent'><span>Lazyr.js是一个小的、快速的、现代的、相互间无依赖的图片延迟加载库。通过延迟加载图片，让图片出现在（或接近)）视窗才加载来提高页面打开速度。这个库通过保持最少选项并最大化速度...</span></div><div class='ArticleTags'><span class='btn btn-success'>JavaScript</span></div></div></div>";
    }
}
