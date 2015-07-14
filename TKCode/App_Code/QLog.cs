/*
                   _ooOoo_
                  o8888888o
                  88" . "88
                  (| -_- |)
                  O\  =  /O
               ____/`---'\____
             .'  \\|     |//  `.
            /  \\|||  :  |||//  \
           /  _||||| -:- |||||-  \
           |   | \\\  -  /// |   |
           | \_|  ''\---/''  |   |
           \  .-\__  `-`  ___/-. /
         ___`. .'  /--.--\  `. . __
      ."" '<  `.___\_<|>_/___.'  >'"".
     | | :  `- \`.;`\ _ /`;.`/ - ` : | |
     \  \ `-.   \_ __\ /__ _/   .-` /  /
======`-.____`-.___\_____/___.-`____.-'======
                   `=---='
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
         佛祖保佑       永无BUG

 佛曰:    
         写字楼里写字间，写字间里程序员；    
         程序人员写程序，又拿程序换酒钱。    
         酒醒只在网上坐，酒醉还来网下眠；    
         酒醉酒醒日复日，网上网下年复年。    
         但愿老死电脑间，不愿鞠躬老板前；    
         奔驰宝马贵者趣，公交自行程序员。    
         别人笑我忒疯癫，我笑自己命太贱；    
         不见满街漂亮妹，哪个归得程序员？
*/

#region 文件描述
//---------------------------------------------------------------------------------------------
// 文 件 名: QLog.cs
// 作    者：XuQing
// 邮    箱：Code@XuQing.me
// 创建时间：2015/5/12 16:11:43
// 描    述：
// 版    本：Version 1.0
//---------------------------------------------------------------------------------------------
// 历史更新纪录
//---------------------------------------------------------------------------------------------
// 版    本：   Version 1.0        修改时间： 2015/06/04  修改人：     XuQing      
// 修改内容：修改日志记录目录，兼容Linux平台
//---------------------------------------------------------------------------------------------
// 本文件内代码如果没有特殊说明均遵守MIT开源协议 http://opensource.org/licenses/mit-license.php
//---------------------------------------------------------------------------------------------
#endregion
using Fleck;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Qing
{
    public static class QLog
    {
        static bool IsWs = false;
        static ConcurrentQueue<string> q_log = new ConcurrentQueue<string>();
        static System.Timers.Timer t_ClearLog = new System.Timers.Timer(1000 * 60 * 10);
        static System.Timers.Timer t_Writelog = new System.Timers.Timer(1000);
        static int LogCycle = 0;
        static string DirPath = string.Empty;
        static string BasePath = System.AppDomain.CurrentDomain.BaseDirectory;
        static bool isRun = false;
        static List<IWebSocketConnection> Wss = new List<IWebSocketConnection>();
        static WebSocketServer server;
        /// <summary>
        /// 日志级别
        /// 1-4 对应 Nomal,Exception,Debug,ShowOnly，向下包括
        /// 默认4，全部打印。 设置为0，不打印日志。
        /// </summary>
        public static int LogLevel = 4;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="path">日志文件目录（默认\log\）</param>
        /// <param name="logCycle">日志文件有效期（默认十五天）</param>
        public static void Init(string path = "/log/", int logCycle = 15)
        {
            DirPath = BasePath.TrimEnd('\\').TrimEnd('/') + path;
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(DirPath);
            di.Create();
            t_Writelog.Elapsed += t_Writelog_Elapsed;
            t_Writelog.Start();
            if (logCycle > 0)
            {
                LogCycle = logCycle;
                t_ClearLog.Elapsed += t_ClearLog_Elapsed;
                t_ClearLog.Start();
            }
        }
        /// <summary>
        /// 日志写入文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void t_Writelog_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (isRun) { return; }
            isRun = true;
            int i = 0;
            StringBuilder sb_log = new StringBuilder();
            //最多50条写一次 减少io操作
            while (q_log.Count > 0 && i < 50)
            {
                string log = string.Empty;
                if (q_log.TryDequeue(out log))
                {
                    sb_log.Append(log);
                }
            }
            using (StreamWriter sw = File.AppendText(DirPath + DateTime.Today.ToString("yy_MM_dd") + "_log.txt"))
            {
                sw.Write(sb_log.ToString());
            }
            isRun = false;
        }
        /// <summary>
        /// 清理日志文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void t_ClearLog_Elapsed(object sender, ElapsedEventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(DirPath);
            foreach (FileInfo fi in di.GetFiles())
            {
                if (fi.CreationTime < DateTime.Now.AddDays(-LogCycle))
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch (Exception ex)
                    {

                        SendLog("删除历史日志文件出错。\r\n" + ex.Message, E_LogType.Exception);
                    }
                }
            }
        }

        public static void StartWS(int port)
        {

            server = new WebSocketServer("ws://0.0.0.0:" + port.ToString());
            server.Start(socket =>
            {
                socket.OnOpen = () => { try { Wss.Add(socket); } catch (Exception) { } };
                socket.OnClose = () => { try { Wss.Remove(socket); } catch (Exception) { } };
                socket.OnMessage = m => { SendLog(m); };
                
            });
            IsWs = true;
        }

        public static void SendLog(this string sLog, string tag, E_LogType logType = E_LogType.Nomal)
        {
            SendLog("[" + tag + "] " + sLog, logType);
        }
        public static void SendLog_Debug(this string sLog, string tag)
        {
            SendLog("[" + tag + "] " + sLog, E_LogType.Debug);
        }
        public static void SendLog_Exception(this string sLog, string tag)
        {
            SendLog("[" + tag + "] " + sLog, E_LogType.Exception);
        }
       
        

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="sLog">日志内容</param>
        /// <param name="logType">日志类型（默认普通）</param>
        public static void SendLog(this string sLog, E_LogType logType = E_LogType.Nomal)
        {
            if ((int)logType > LogLevel)
            {
                return;
            }
            new Thread(() =>
            {
                StringBuilder log = new StringBuilder(DateTime.Now.ToString("HH:mm:ss"));
                switch (logType)
                {
                    case E_LogType.ShowOnly:
                        log.Append(" ").Append(sLog).AppendLine();
                        Console.Write(log);
                        break;
                    case E_LogType.Nomal:
                        log.Append(" ").Append(sLog).AppendLine();
                        Console.Write(log);
                        q_log.Enqueue(log.ToString());
                        break;
                    case E_LogType.Debug:
                        log.Append(" [Debug]").Append(sLog).AppendLine();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(log);
                        q_log.Enqueue(log.ToString());
                        break;
                    case E_LogType.Exception:
                        log.Append(" [Exception]").Append(sLog).AppendLine();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(log);
                        q_log.Enqueue(log.ToString());
                        break;
                }
                Console.ResetColor();
                if (IsWs)
                {
                    Wss.ForEach(x => { x.Send(log.ToString()); });
                }
            }).Start();
        }
    }
    public enum E_LogType
    {
        /// <summary>
        /// 普通日志
        /// </summary>
        Nomal = 1,
        /// <summary>
        ///异常日志
        ///</summary>
        Exception = 2,
        /// <summary>
        ///调试日志
        ///</summary>
        Debug = 3,
        /// <summary>
        /// 仅显示(不保存到日志文件)
        /// </summary>
        ShowOnly = 4
    }
}
