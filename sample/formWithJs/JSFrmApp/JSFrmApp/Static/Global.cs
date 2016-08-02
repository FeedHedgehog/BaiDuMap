using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace JSFrmApp.Static
{
    /// <summary>
    /// 全局变量类
    /// </summary>
    public class Global
    {
        private static string _baseServerIP;

        /// <summary>
        /// 服务的基类地址
        /// </summary>
        public static string BaseServerIP
        {
            get
            {
                if (string.IsNullOrEmpty(_baseServerIP))
                {
                    System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    _baseServerIP = config.AppSettings.Settings["BaseServerAddress"].Value;
                }
                return _baseServerIP;
            }
            set { _baseServerIP = value; }
        }

        /// <summary>
        /// 项目信息的Url
        /// </summary>
        public static string ProjectInfoServer = BaseServerIP + "/server/sys/projectList.json";
    }
}
