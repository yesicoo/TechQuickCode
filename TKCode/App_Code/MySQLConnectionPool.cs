using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechQuickCode.App_Code;

namespace Qing
{

    public static class MySQLConnectionPool
    {
        private static int size = 1;//池中连接数
        private static int useCount = 0;//已经使用的连接数
        private static ArrayList pool = new ArrayList();//连接保存的集合
       
        #region 获取池中的连接
        public static MySqlConnection GetConnection()
        {
            lock (pool)
            {
                MySqlConnection tmp = null;
                //可用连接数量大于0
                if (pool.Count > 0)
                {
                    //取第一个可用连接
                    tmp = (MySqlConnection)pool[0];
                    //在可用连接中移除此链接
                    pool.RemoveAt(0);
                    //不成功
                    if (!isUserful(tmp))
                    {
                        //可用的连接数据已去掉一个
                        useCount--;
                        tmp = GetConnection();
                    }
                }
                else
                {
                    //可使用的连接小于连接数量
                    if (useCount <= size)
                    {
                        try
                        {
                            //创建连接
                            tmp = CreateConnection(tmp);
                        }
                        catch (Exception e)
                        {
                            Qing.QLog.SendLog_Exception(e.Message, "MySQLConnectionPool");
                        }
                    }
                }
                //连接为null
                if (tmp == null)
                {
                    //达到最大连接数递归调用获取连接否则创建新连接
                    if (useCount <= size)
                    {
                        tmp = GetConnection();
                    }
                    else
                    {
                        tmp = CreateConnection(tmp);
                    }
                }
                return tmp;
            }
        }
        #endregion

        #region 创建连接
        private static MySqlConnection CreateConnection(MySqlConnection tmp)
        {
            //创建连接
            MySqlConnection conn = new MySqlConnection(Config.MySQLConStr);
            conn.Open();
            //可用的连接数加上一个
            useCount++;
            tmp = conn;
            return tmp;
        }
        #endregion

        #region 加连接回到池中
        public static void GiveBack(this MySqlConnection con)
        {
            lock (pool)
            {
                if (con != null)
                {
                    //将连接添加在连接池中
                    pool.Add(con);
                }
            }
        }
        #endregion

        #region 目的保证所创连接成功,测试池中连接
        private static bool isUserful(MySqlConnection con)
        {
            //主要用于不同用户
            bool result = true;
            if (con != null)
            {
                string sql = "select 1";//随便执行对数据库操作
                MySqlCommand cmd = new MySqlCommand(sql, con);
                try
                {
                    cmd.ExecuteScalar().ToString();
                }
                catch
                {
                    result = false;
                }
            }
            return result;
        }
        #endregion
    }
}
