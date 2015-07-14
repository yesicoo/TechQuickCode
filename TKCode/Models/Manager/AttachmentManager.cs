using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechQuickCode.Models.Entity;
using Qing;
using Dapper;
using System.Web;

namespace TechQuickCode.Models.Manager
{
    public class AttachmentManager
    {
        public static readonly AttachmentManager Instance = new AttachmentManager();

        public string Add(AttachmentItem ai)
        {
            ai.GUID = Guid.NewGuid().ToString().Replace("-", "");
            ai.UpLoadTime = DateTime.Now;
            string sql = ai.GetInsertSQL("AttachmentList");
            var conn = MySQLConnectionPool.GetConnection();
            var result = conn.Execute(sql);
            conn.GiveBack();
            if (result > 0)
            {
                return ai.GUID;
            }
            else
            {
                return null;
            }
        }
        public AttachmentItem GetAttachmentItem(string GUID)
        {
            var conn = MySQLConnectionPool.GetConnection();
            var result = conn.Query<AttachmentItem>("select * from AttachmentList where GUID='" + GUID + "'").SingleOrDefault();
            conn.GiveBack();
            return result;
        }
        public List<AttachmentItem> GetAttachmentItems(string GUIDs)
        {
            string acs = HttpUtility.UrlDecode(GUIDs).TrimEnd(',').Replace(",", "','");
            var conn = MySQLConnectionPool.GetConnection();
            var result = conn.Query<AttachmentItem>("select GUID,FileName,FileFontCode,FileLength,DownLoadCount from AttachmentList where GUID in ('" + acs + "')").ToList();
            conn.GiveBack();
            return result;
        }

        internal string GetAttachmentPath(string ID)
        {
            var conn = MySQLConnectionPool.GetConnection();
            var result = conn.ExecuteScalar(string.Format("update AttachmentList set DownLoadCount=DownLoadCount+1 where GUID='{0}';select FilePath from AttachmentList where GUID='{0}';", ID));
            conn.GiveBack();
            return result != null ? result.ToString() : "";
        }

        internal void AddPlateType(string PlateName, string TypeName)
        {
            var conn = MySQLConnectionPool.GetConnection();
            var id = conn.ExecuteScalar(string.Format("select id from `ArticlePlateType` where `PlateName`='{0}' and TypeName= '{1}';", PlateName, TypeName));
            if (id == null)
            {
                var result = conn.Execute(string.Format("INSERT INTO `ArticlePlateType` (`PlateName`, `TypeName`) VALUES ('{0}', '{1}');", PlateName, TypeName));
            }
            conn.GiveBack();
        }
    }
}
