using JSFrmApp.Tool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using JS.Model;

namespace JSFrmApp.LogicLayer
{
    public class Logic
    {
        private static Logic _instance;

        public static Logic Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Logic();

                    //_Helper.Proxy = new WebProxy("192.168.2.144", 8888);

                }

                return _instance;
            }
        }

        /// <summary>
        /// webBrowser请求的数据推送到Html网页中
        /// </summary>
        public void InvokePushDataToHtmlMethod(WebBrowser webBrowser)
        {
            string data = LogicLayer.Logic.Instance.LoginServer();
            if (webBrowser.Document != null)
            {
                Object[] objArray = new Object[1];
                objArray[0] = (Object)data;
                var oSum = webBrowser.Document.InvokeScript("PushDataToHtml", objArray);
                //if (oSum != null)
                //{
                //    MessageBox.Show(oSum.ToString());
                //}

            }
        }

        public string LoginServer()
        {
            string result = string.Empty;
            #region Login
            //ThreadPool.QueueUserWorkItem((o) =>
            //{
            //    BeginInvoke(new Action(() =>                 //UI是主线程，要想在线程里使用主线程的UI就要这样做
            //    {
            try
            {
                string url = Static.Global.BaseServerIP + "/login";

                IDictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("j_username", "admin");
                parameters.Add("j_password", "111111");
                parameters.Add("remember_me", "on");
                parameters.Add("j_type", "mobile");

                String xml = HttpHelper.Instance.Post(url, parameters, null, null, 2000, "", null, null, null);
                JObject jo = (JObject)JsonConvert.DeserializeObject(xml);

                string loginstate = jo["state"]["code"].ToString();
                if (loginstate == "1")
                {
                    result = GetDataFromServer();
                }

                else
                {
                    result = string.Empty;
                    MessageBox.Show("登陆失败，请重新登录！", "系统提示：");
                }
            }
            catch (NullReferenceException ex)
            {
                result = string.Empty;
            }
            catch (Exception ex)
            {
                result = string.Empty;
            }

            //    }));
            //});
            return result;
            #endregion

        }

        public string GetDataFromServer()
        {
            //获得多个项目信息
            string url = Static.Global.ProjectInfoServer;

            string projectInfo = string.Empty;
            try
            {
                HttpWebResponse myHttpWebResponse = HttpHelper.Instance.CreateGetHttpResponse(url, null, 20000,
                    null, HttpHelper.Instance.Cookies, null, null, null);
                projectInfo = new StreamReader(myHttpWebResponse.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
            JObject jo = (JObject)JsonConvert.DeserializeObject(projectInfo);
            string state = jo["state"]["code"].ToString();
            if (state.CompareTo("1") != 0)
            {
                return string.Empty;
            }
            else
            {
                return projectInfo;
            }
        }

        /// <summary>
        /// 获取要绑定的线条列表对象
        /// </summary>
        /// <returns></returns>
        public List<Line> GetLines()
        {
            string sourceJson = GetDataFromServer();

            List<Line> lines = new List<Line>();
            return lines;
        }
    }
}
