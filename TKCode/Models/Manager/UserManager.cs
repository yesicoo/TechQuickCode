using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechQuickCode.Models.Entity;
using Qing;
using Dapper;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using TKCode.Models.Entity;

namespace TechQuickCode.Models.Manager
{
    public class UserManager
    {
        public static readonly UserManager Instance = new UserManager();
        List<UserItem> Users = new List<UserItem>();

        public UserManager()
        {

            var conn = MySQLConnectionPool.GetConnection();
            Users = conn.Query<UserItem>("select * from `UserList`").ToList();
            QLog.SendLog("Users Count:" + Users.Count);
            conn.GiveBack();
        }

        public string Login(string UserName, string UserPassword)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            string result = wc.DownloadString(string.Format("http://120.209.198.188:10108/module/Events/Action.aspx?action=Login&uid={0}&pwd={1}", HttpUtility.UrlEncode(UserName), HttpUtility.UrlEncode(UserPassword)));
            LoginItem json_Result = JsonConvert.DeserializeObject<LoginItem>(result);
            if (!json_Result.success)
            {
                return "Error";
            }
            UserItem user = Users.Where(x => x.GUID == json_Result.results[0].id).FirstOrDefault();
            string token = System.Guid.NewGuid().ToString().Replace("-", "");
            if (user != null)
            {
                user.Token = token;
                new System.Threading.Thread(() =>
                {
                    var conn = MySQLConnectionPool.GetConnection();
                    conn.Execute("update UserList set Token='" + token + "' where GUID='" + user.GUID + "';");
                    conn.GiveBack();
                }).Start();
                return token;
            }
            else
            {
                user = new UserItem();
                user.GUID = json_Result.results[0].id;
                user.UserNickName = json_Result.results[0].name;
                user.UserEmail = json_Result.results[0].mail;
                user.UserHeadImg = "/Content/Images/head.jpg";
                user.Token = token;
                Users.Add(user);
                string sql = user.GetInsertSQL("UserList");
                var conn = MySQLConnectionPool.GetConnection();
                conn.Execute(sql);
                conn.GiveBack();
                return token;
            }
        }

        internal void CheckLogin(System.Web.HttpRequestBase Request, dynamic ViewBag)
        {

            try
            {
                var cookie = Request.Cookies["Token"];
                if (cookie == null)
                {
                    ViewBag.Login = false;
                }
                else
                {
                    UserItem user = Users.Where(x => x.Token == cookie.Value).FirstOrDefault();
                    if (user == null)
                    {
                        ViewBag.Login = false;
                    }
                    else
                    {
                        ViewBag.Login = true;
                        ViewBag.UserName = user.UserNickName;
                        ViewBag.User = user;
                    }
                }
            }
            catch (Exception e)
            {
                QLog.SendLog(e.Message);
            }

        }

        internal UserItem GetUserByToken(string token)
        {
            UserItem user = Users.Where(x => x.Token == token).FirstOrDefault();
            return user;
        }

        internal UserItem GetUserByGUID(string id)
        {
            UserItem user = Users.Where(x => x.GUID == id).FirstOrDefault();
            return user;
        }
        internal string  GetUserByUserName(string userName)
        {
            UserItem user = Users.Where(x => x.UserNickName == userName).FirstOrDefault();
            if (user == null)
            {
                return "NoPeople";
            }
            else
            {
                return user.GUID;
            }
           
        }

        internal bool UpdateUserHeadImg(string id,string headimg)
        {
            UserItem user = Users.Where(x => x.GUID == id).FirstOrDefault();
            user.UserHeadImg = headimg;
            string sql = user.GetUpdateSQL("GUID", "UserList", "UserHeadImg", true);
            var conn = MySQLConnectionPool.GetConnection();
            conn.Execute(sql);
            conn.GiveBack();
            return true;
        }

        internal List<UserActive> GetUserActives(string uid, int page, int count = 1)
        {
            List<UserActive> userActives = new List<UserActive>();
            var conn = MySQLConnectionPool.GetConnection();
            userActives = conn.Query<UserActive>(string.Format("select * from `UserActive` where UserID='{0}' order by CreateTime Desc limit {1},{2}", uid, (page - 1) * count, count)).ToList();
            conn.GiveBack();
            return userActives;
        }
    }
}
