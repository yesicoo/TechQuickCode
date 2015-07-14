using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TechQuickCode.App_Code
{
    public static class Utils
    {
        public static string GetFileLogoStr(string fileType)
        {
            string fontCode = "";
            switch (fileType)
            {
                case "pdf":
                    fontCode = "&#xe615;";
                    break;
                case "doc":
                case "docx":
                case "dot":
                case "rtf":
                case "wps":
                case "wpt":
                    fontCode = "&#xe60f;";
                    break;
                case "xls":
                case "xlsx":
                case "et":
                case "ett":
                case "xlt":
                case "cvs":
                    fontCode = "&#xe611;";
                    break;
                case "bmp":
                case "jpg":
                case "jpeg":
                case "png":
                case "gif":
                    fontCode = "&#xe617;";
                    break;
                case "zip":
                    fontCode = "&#xe616;";
                    break;
                case "apk":
                    fontCode = "&#xe618;";
                    break;
                case "rar":
                    fontCode = "&#xe612;";
                    break;
                case "html":
                case "cs":
                case "js":
                case "css":
                case "less":
                case "sass":
                case "xml":
                case "java":
                case "xmal":
                case "json":
                    fontCode = "&#xe614;";
                    break;

                case "dps":
                case "dpt":
                case "ppt":
                case "pot":
                case "pps":
                    fontCode = "&#xe613;";
                    break;
                case "exe":
                case "bat":
                    fontCode = "&#xe60d;";
                    break;

                default:
                    fontCode = "&#xe60e;";
                    break;
            }
            return fontCode;
        }

        public static string  ConvertBytes(long bytes)
        {
            int unit = 1024;

            if (bytes < unit) return bytes + " B";

            int exp = (int)(Math.Log(bytes) / Math.Log(unit));

            return String.Format("{0:F1} {1}B", bytes / Math.Pow(unit, exp), "KMGTPE"[exp - 1]);
        }
        public static List<string> PickupImgUrl(string html)
        {
            Regex regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);
            MatchCollection matches = regImg.Matches(html);
            List<string> lstImg = new List<string>();
            foreach (Match match in matches)
            {
                lstImg.Add(match.Groups["imgUrl"].Value);
            }
            return lstImg;
        }

        public static string ReplaceHtmlTag(string html, int length = 120)
        {
            string strText = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
            strText = System.Text.RegularExpressions.Regex.Replace(strText, "&[^;]+;", "");
            if (length > 0 && strText.Length > length)
                return strText.Substring(0, length)+"......";
            return strText;
        }
    

    }
}
