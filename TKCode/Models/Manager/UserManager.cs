using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechQuickCode.Models.Entity;
using Qing;
using Dapper;

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
            UserItem user = Users.Where(x => x.UserName == UserName && x.UserPassword == UserPassword).FirstOrDefault();
            if (user != null)
            {
                string token = System.Guid.NewGuid().ToString().Replace("-", "");
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
                return "Error";
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
    }
}
