using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Qing
{
    public static class EntityUtils
    {
        /// <summary>
        /// 生成更新SQL
        /// </summary>
        /// <typeparam name="T">实体对象</typeparam>
        /// <param name="t">实体对象</param>
        /// <param name="PrimaryKey">主键</param>
        /// <param name="tableName">表名</param>
        /// <param name="clumns">字段名称</param>
        /// <param name="exclude">是否包含</param>
        /// <returns></returns>
        public static string GetUpdateSQL<T>(this T t, string PrimaryKeys, string tableName, string clumns = "", bool exclude = false) where T : QEInterface
        {
            if (t == null) { return null; }
            Dictionary<string, string> Primarys = new Dictionary<string, string>();
            List<string> excludeColumns = new List<string>();
            if (!string.IsNullOrEmpty(clumns))
            {
                excludeColumns = clumns.Split(',').ToList();
            }
            if (!string.IsNullOrEmpty(PrimaryKeys))
            {
                PrimaryKeys.Split(',').ToList().ForEach(x =>
                {
                    Primarys.Add(x, null);
                });
            }
            PropertyInfo[] pros = t.GetType().GetProperties();
            StringBuilder sb_SQL = new StringBuilder(string.Format("Update `{0}` set ", tableName));
            foreach (var item in pros)
            {
                if (Primarys.ContainsKey(item.Name))
                {

                    var value = item.GetValue(t, null).ToString();
                    Primarys[item.Name] = value;
                    continue;
                }
                if (exclude == excludeColumns.Contains(item.Name))
                {
                    var value = item.GetValue(t, null);
                    if (value != null)
                    {
                        sb_SQL.Append(string.Format("`{0}`='{1}',", item.Name, value.ToString()));
                    }
                }
            }
            StringBuilder whereStr = new StringBuilder("where 1=1 ");
            foreach (var item in Primarys)
            {
                whereStr.Append(string.Format("and `{0}`='{1}' ", item.Key, item.Value));
            }
            string sql = sb_SQL.ToString().TrimEnd(',') + whereStr.ToString() + ";";
            return sql;
        }

        public static string GetUpdateSQL<T>(this IEnumerable<T> ts, string PrimaryKeys, string tableName, string clumns = "", bool exclude = false) where T : QEInterface
        {
            if (ts == null) { return null; }
            if (ts.Count() == 0) { return null; }

            Dictionary<string, string> Primarys = new Dictionary<string, string>();
            List<string> excludeColumns = new List<string>();
            if (!string.IsNullOrEmpty(clumns))
            {
                excludeColumns = clumns.Split(',').ToList();
            }
            if (!string.IsNullOrEmpty(PrimaryKeys))
            {
                PrimaryKeys.Split(',').ToList().ForEach(x =>
                {
                    Primarys.Add(x, null);
                });
            }
            var _t = ts.FirstOrDefault();
            PropertyInfo[] pros = _t.GetType().GetProperties();
            StringBuilder sb_SQL = new StringBuilder();
            foreach (var t in ts)
            {
                StringBuilder sb_SingleSQL = new StringBuilder(string.Format("Update `{0}` set ", tableName));
                foreach (var item in pros)
                {
                    if (Primarys.ContainsKey(item.Name))
                    {

                        var value = item.GetValue(t, null).ToString();
                        Primarys[item.Name] = value;
                        continue;
                    }
                    if (exclude == excludeColumns.Contains(item.Name))
                    {
                        var value = item.GetValue(t, null);
                        if (value != null)
                        {
                            sb_SingleSQL.Append(string.Format("`{0}`='{1}',", item.Name, value.ToString()));
                        }
                    }
                }
                StringBuilder whereStr = new StringBuilder("where 1=1 ");
                foreach (var item in Primarys)
                {
                    whereStr.Append(string.Format("and `{0}`='{1}' ", item.Key, item.Value));
                }
                string sql = sb_SingleSQL.ToString().TrimEnd(',') + whereStr.ToString() + ";";
                sb_SQL.Append(sql);
            }
            return sb_SQL.ToString();
        }

        public static string GetInsertSQL<T>(this T t, string tableName, string clumns = "", bool exclude = false) where T : QEInterface
        {
            if (t == null) { return null; }
            List<string> excludeColumns = new List<string>();
            if (!string.IsNullOrEmpty(clumns))
            {
                excludeColumns = clumns.Split(',').ToList();
            }

            PropertyInfo[] pros = t.GetType().GetProperties();
            string InsertSQL = "Insert into `{0}` ({1}) values({2});";
            StringBuilder sb_Columns = new StringBuilder();
            StringBuilder sb_Values = new StringBuilder();
            foreach (var item in pros)
            {

                if (exclude == excludeColumns.Contains(item.Name))
                {
                    var value = item.GetValue(t, null);
                    if (value != null)
                    {
                        sb_Columns.Append(string.Format("`{0}`,", item.Name));
                        sb_Values.Append(string.Format("'{0}',", value.ToString()));
                    }
                }
            }

            string sql = string.Format(InsertSQL, tableName, sb_Columns.ToString().TrimEnd(','), sb_Values.ToString().TrimEnd(','));
            return sql;
        }

        public static string GetInsertSQL<T>(this IEnumerable<T> ts, string tableName, string clumns = "", bool exclude = false) where T : QEInterface
        {
            List<string> excludeColumns = new List<string>();
            if (!string.IsNullOrEmpty(clumns))
            {
                excludeColumns = clumns.Split(',').ToList();
            }

            var _t = ts.FirstOrDefault();


            PropertyInfo[] pros = _t.GetType().GetProperties();
            StringBuilder sb_Columns = new StringBuilder();
            foreach (var item in pros)
            {
                if (exclude == excludeColumns.Contains(item.Name))
                {
                    sb_Columns.Append(string.Format("`{0}`,", item.Name));
                }
            }

            StringBuilder sb_SQL=new StringBuilder (string.Format("Insert into `{0}` ({1}) values ",tableName,sb_Columns.ToString().TrimEnd(',')));

           
            foreach (var t in ts)
            {
                StringBuilder sb_Values = new StringBuilder("(");
                foreach (var item in pros)
                {

                    if (exclude == excludeColumns.Contains(item.Name))
                    {
                        var value = item.GetValue(t, null);
                        if (value != null)
                        {
                            sb_Values.Append(string.Format("'{0}',", value.ToString()));
                        }else{
                             sb_Values.Append(string.Format("'{0}',", ""));
                        }
                    }
                }
                string values=sb_Values.ToString().TrimEnd(',')+"),";
                sb_SQL.Append(values);
            }



            string sql = sb_SQL.ToString().TrimEnd(',')+";";
            return sql;
        }
    }
}
