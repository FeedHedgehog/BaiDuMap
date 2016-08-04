using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace JSFrmApp.Tool
{
    /// <summary>
    /// Http的帮助类
    /// </summary>
    public partial class HttpHelper
    {
        private static HttpHelper _instance;

        public static HttpHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HttpHelper();

                    //_Helper.Proxy = new WebProxy("192.168.2.144", 8888);

                }

                return _instance;
            }
        }

        public System.Net.WebProxy Proxy { get; set; }

        public bool Debug { get; set; }

        public CookieCollection Cookies
        {
            get { return _cookies; }
        }

        public void ClearCookies()
        {
            _cookies = new CookieCollection();
        }

        CookieCollection _cookies = new CookieCollection();

        private static readonly string DefaultUserAgent =
            "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";



        #region 测试
        /// <summary>  
        /// 使用Post方法获取字符串结果  
        /// </summary>  
        /// <param name="url"></param>  
        /// <param name="formItems">Post表单内容</param>  
        /// <param name="cookieContainer"></param>  
        /// <param name="timeOut">默认20秒</param>  
        /// <param name="encoding">响应内容的编码类型（默认utf-8）</param>  
        /// <returns></returns>  
        public string PostForm(string url, List<FormItemModel> formItems, CookieContainer cookieContainer = null, string refererUrl = null, Encoding encoding = null, int timeout = 300)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                #region 初始化请求对象

                request.Method = "POST";
                request.Timeout = timeout * 1000;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.KeepAlive = true;
                request.UserAgent =
                    "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36";
                if (!string.IsNullOrEmpty(refererUrl))
                    request.Referer = refererUrl;
                if (cookieContainer != null)
                    request.CookieContainer = cookieContainer;
                else
                {
                    request.CookieContainer = new CookieContainer();
                    request.CookieContainer.Add(Cookies);
                }


                #endregion

                string boundary = "----" + DateTime.Now.Ticks.ToString("x"); //分隔符  
                request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
                //请求流  
                var postStream = new MemoryStream();

                #region 处理Form表单请求内容

                //是否用Form上传文件  
                var formUploadFile = formItems != null && formItems.Count > 0;
                if (formUploadFile)
                {
                    //文件数据模板  
                    string fileFormdataTemplate =
                        "\r\n--" + boundary +
                        "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" +
                        "\r\nContent-Type: application/octet-stream" +
                        "\r\n\r\n";
                    //文本数据模板  
                    string dataFormdataTemplate =
                        "\r\n--" + boundary +
                        "\r\nContent-Disposition: form-data; name=\"{0}\"" +
                        "\r\n\r\n{1}";
                    foreach (var item in formItems)
                    {
                        string formdata = null;
                        if (item.IsFile)
                        {
                            //上传文件  
                            formdata = string.Format(
                                fileFormdataTemplate,
                                item.Key, //表单键  
                                item.FileName);
                        }
                        else
                        {
                            //上传文本  
                            formdata = string.Format(
                                dataFormdataTemplate,
                                item.Key,
                                item.Value);
                        }

                        //统一处理  
                        byte[] formdataBytes = null;
                        //第一行不需要换行  
                        if (postStream.Length == 0)
                            formdataBytes = Encoding.UTF8.GetBytes(formdata.Substring(2, formdata.Length - 2));
                        else
                            formdataBytes = Encoding.UTF8.GetBytes(formdata);
                        postStream.Write(formdataBytes, 0, formdataBytes.Length);

                        //写入文件内容  
                        if (item.FileContent != null && item.FileContent.Length > 0)
                        {
                            using (var stream = item.FileContent)
                            {
                                byte[] buffer = new byte[40960];
                                int bytesRead = 0;
                                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    postStream.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                    }
                    //结尾  
                    var footer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                    postStream.Write(footer, 0, footer.Length);

                }
                else
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                }

                #endregion

                request.ContentLength = postStream.Length;

                #region 输入二进制流

                if (postStream != null)
                {
                    postStream.Position = 0;
                    //直接写入流  
                    Stream requestStream = request.GetRequestStream();

                    byte[] buffer = new byte[40960];
                    int bytesRead = 0;
                    while ((bytesRead = postStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                    }
                    postStream.Close(); //关闭文件访问  
                }

                #endregion

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (cookieContainer != null)
                {
                    response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
                }

                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader myStreamReader = new StreamReader(responseStream, encoding ?? Encoding.UTF8))
                    {
                        string retString = myStreamReader.ReadToEnd();
                        return retString;
                    }
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        public class FormItemModel
        {
            /// <summary>  
            /// 表单键，request["key"]  
            /// </summary>  
            public string Key { set; get; }
            /// <summary>  
            /// 表单值,上传文件时忽略，request["key"].value  
            /// </summary>  
            public string Value { set; get; }
            /// <summary>  
            /// 是否是文件  
            /// </summary>  
            public bool IsFile
            {
                get
                {
                    if (FileContent == null || FileContent.Length == 0)
                        return false;

                    if (FileContent != null && FileContent.Length > 0 && string.IsNullOrWhiteSpace(FileName))
                        throw new Exception("上传文件时 FileName 属性值不能为空");
                    return true;
                }
            }
            /// <summary>  
            /// 上传的文件名  
            /// </summary>  
            public string FileName { set; get; }
            /// <summary>  
            /// 上传的文件内容  
            /// </summary>  
            public Stream FileContent { set; get; }
        }

        #endregion

        /// <summary>  
        /// 创建GET方式的HTTP请求  
        /// </summary>  
        /// <param name="url">请求的URL</param>  
        /// <param name="timeout">请求的超时时间</param>  
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>  
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>  
        /// <returns></returns>  
        public HttpWebResponse CreateGetHttpResponse(string url, IDictionary<string, string> parameters, int? timeout = 300, string userAgent = "",
            CookieCollection cookies = null
            , string Referer = "", Dictionary<string, string> headers = null,
            string contentType = "application/x-www-form-urlencoded")
        {
            HttpWebRequest request = null;
            HttpWebResponse v = null;
            string retvalue = "";
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    throw new ArgumentNullException("url");
                }
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                        new RemoteCertificateValidationCallback(CheckValidationResult);
                    request = WebRequest.Create(url) as HttpWebRequest;
                    request.ProtocolVersion = HttpVersion.Version10;
                }
                else
                {
                    request = WebRequest.Create(url) as HttpWebRequest;
                }

                if (Proxy != null)
                {
                    request.Proxy = Proxy;
                }

                request.Method = "GET";
                request.Headers["Pragma"] = "no-cache";
                request.Accept = "text/html, application/xhtml+xml, */*";
                //request.Headers["Accept-Language"] = "en-US,en;q=0.5";
                request.Headers.Add("Accept-Language", "zh-CN");

                request.ContentType = contentType;

                request.UserAgent = DefaultUserAgent;
                request.Referer = Referer;

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }
                if (!string.IsNullOrEmpty(userAgent))
                {
                    request.UserAgent = userAgent;
                }
                if (timeout.HasValue)
                {
                    request.Timeout = timeout.Value * 1000;
                }
                if (cookies != null)
                {
                    request.CookieContainer = new CookieContainer();
                    request.CookieContainer.Add(cookies);
                }
                else
                {
                    request.CookieContainer = new CookieContainer();
                    request.CookieContainer.Add(Cookies);
                }

                if (!(parameters == null || parameters.Count == 0))
                {
                    var buffer = CreateParameter(parameters);


                    UTF8Encoding requestEncoding = new UTF8Encoding();

                    byte[] data = requestEncoding.GetBytes(buffer.ToString());
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }

                v = request.GetResponse() as HttpWebResponse;
                v.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                Cookies.Add(request.CookieContainer.GetCookies(request.RequestUri));
            }
            catch (Exception)
            {
                return null;
            }

            return v;
        }

        public HttpWebResponse MyCreateGetHttpResponse(string url, string parameters, int? timeout = 300, string userAgent = "",
            CookieCollection cookies = null
            , string Referer = "", Dictionary<string, string> headers = null,
            string contentType = "application/x-www-form-urlencoded")
        {
            if (Debug)
            {
                Console.Write("Start Get Url:{0}    ", url);
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            HttpWebRequest request;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }

            if (Proxy != null)
            {
                request.Proxy = Proxy;
            }

            request.Method = "GET";
            request.Headers["Pragma"] = "no-cache";
            request.Accept = "text/html, application/xhtml+xml, */*";
            //request.Headers["Accept-Language"] = "en-US,en;q=0.5";
            request.Headers.Add("Accept-Language", "zh-CN");

            request.ContentType = contentType;

            request.UserAgent = DefaultUserAgent;
            request.Referer = Referer;

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value * 1000;
            }
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            else
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(Cookies);
            }
            //如果需要POST数据  
            if (!string.IsNullOrEmpty(parameters))
            {
                if (parameters.EndsWith("&"))
                {
                    parameters = parameters.Remove(parameters.Length - 1);
                }
                UTF8Encoding requestEncoding = new UTF8Encoding();
                byte[] data = requestEncoding.GetBytes(parameters);
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }

            var v = request.GetResponse() as HttpWebResponse;
            v.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
            Cookies.Add(request.CookieContainer.GetCookies(request.RequestUri));
            //Cookies.Add(request.CookieContainer.GetCookies(new Uri("https://" + new Uri(url).Host)));
            //Cookies.Add(v.Cookies);

            if (Debug)
            {
                Console.WriteLine("OK");
            }

            return v;
        }

        /// <summary>  
        /// 创建POST方式的HTTP请求  
        /// </summary>  
        /// <param name="url">请求的URL</param>  
        /// <param name="parameters">随同请求POST的参数名称及参数值字典</param>  
        /// <param name="timeout">请求的超时时间</param>  
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>  
        /// <param name="requestEncoding">发送HTTP请求时所用的编码</param>  
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>  
        /// <returns></returns>  
        public HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, string> parameters,
            Encoding requestEncoding, int? timeout = 300, string userAgent = "", CookieCollection cookies = null,
            string Referer = "", Dictionary<string, string> headers = null,
            string contentType = "application/x-www-form-urlencoded")
        {
            if (Debug)
            {
                Console.Write("Start Post Url:{0}  ", url);

                foreach (KeyValuePair<string, string> keyValuePair in parameters)
                {
                    Console.Write(",{0}:{1}", keyValuePair.Key, keyValuePair.Value);
                }
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            if (requestEncoding == null)
            {
                requestEncoding = new UTF8Encoding();
            }
            HttpWebRequest request = null;
            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }


            if (Proxy != null)
            {
                request.Proxy = Proxy;
            }

            request.Method = "POST";
            request.Accept = "text/html, application/xhtml+xml, application/json, text/javascript, */*; q=0.01";
            request.Referer = Referer;
            //request.Headers["Accept-Language"] = "en-US,en;q=0.5";
            request.UserAgent = DefaultUserAgent;
            request.ContentType = contentType;
            request.Headers["Pragma"] = "no-cache";
            request.Headers.Add("Accept-Language", "zh-CN");

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    //request.Headers.Add(header.Key, header.Value
                    request.Headers[header.Key] = header.Value;
                }
            }

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            else
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(Cookies);
            }

            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            else
            {
                request.UserAgent = DefaultUserAgent;
            }

            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value * 1000;
            }

            request.Expect = string.Empty;

            //如果需要POST数据  
            if (!(parameters == null || parameters.Count == 0))
            {
                var buffer = CreateParameter(parameters);
                byte[] data = requestEncoding.GetBytes(buffer.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            var v = request.GetResponse() as HttpWebResponse;
            v.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
            Cookies.Add(request.CookieContainer.GetCookies(request.RequestUri));
            //Cookies.Add(request.CookieContainer.GetCookies(new Uri("https://" + new Uri(url).Host)));            
            //Cookies.Add(v.Cookies);

            if (Debug)
            {
                Console.WriteLine("OK");
            }

            return v;
        }

        /// <summary>  
        /// 创建POST方式的HTTP请求  
        /// </summary>  
        /// <param name="url">请求的URL</param>  
        /// <param name="parameters">随同请求POST的参数名称及参数值字典</param>  
        /// <param name="timeout">请求的超时时间</param>  
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>  
        /// <param name="requestEncoding">发送HTTP请求时所用的编码</param>  
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>  
        /// <returns></returns>  
        public HttpWebResponse CreatePostHttpResponse(string url, string parameters, Encoding requestEncoding,
            int? timeout = 300, string userAgent = "", CookieCollection cookies = null, string Referer = "",
            Dictionary<string, string> headers = null, string contentType = "application/x-www-form-urlencoded")
        {
            if (Debug)
            {
                Console.Write("Start Post Url:{0} ,parameters:{1}  ", url, parameters);
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            if (requestEncoding == null)
            {
                requestEncoding = new UTF8Encoding();
            }
            HttpWebRequest request = null;
            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }

            if (Proxy != null)
            {
                request.Proxy = Proxy;
            }

            request.Method = "POST";
            request.Headers.Add("Accept-Language", "zh-CN,en-GB;q=0.5");
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.Referer = Referer;
            request.UserAgent = DefaultUserAgent;
            request.ContentType = contentType;
            request.Headers["Pragma"] = "no-cache";

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            else
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(Cookies);
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }


            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            else
            {
                request.UserAgent = DefaultUserAgent;
            }

            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value * 1000;
            }

            request.Expect = string.Empty;

            //如果需要POST数据  
            if (!string.IsNullOrEmpty(parameters))
            {
                byte[] data = requestEncoding.GetBytes(parameters);
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }

            var v = request.GetResponse() as HttpWebResponse;

            Cookies.Add(request.CookieContainer.GetCookies(new Uri("http://" + new Uri(url).Host)));
            Cookies.Add(request.CookieContainer.GetCookies(new Uri("https://" + new Uri(url).Host)));
            Cookies.Add(v.Cookies);

            if (Debug)
            {
                Console.WriteLine("OK");
            }


            return v;
        }

        /// <summary>  
        /// 创建POST方式的HTTP请求  
        /// </summary>  
        /// <param name="url">请求的URL</param>  
        /// <param name="parameters">随同请求POST的参数名称及参数值字典</param>  
        /// <param name="timeout">请求的超时时间</param>  
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>  
        /// <param name="requestEncoding">发送HTTP请求时所用的编码</param>  
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>  
        /// <returns></returns>  
        public HttpWebResponse CreatePostModelFileHttpResponse(string url, string parameters, List<string> keys, List<string> filePaths, int? timeout = 300,
            string userAgent = "", CookieCollection cookies = null, string Referer = "",
            Dictionary<string, string> headers = null, string contentType = "application/x-www-form-urlencoded")
        {

            if (Debug)
                Console.Write("Start Post Url:{0}  ", url);

            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            Encoding requestEncoding = new UTF8Encoding();

            HttpWebRequest request = null;
            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }


            if (Proxy != null)
            {
                request.Proxy = Proxy;
            }

            string strBoundary = "----------" + DateTime.Now.Ticks.ToString("x");

            //request.Method = "POST";
            // This is important, otherwise the whole file will be read to memory anyway...  
            //request.AllowWriteStreamBuffering = false;
            //request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            //request.Referer = Referer;
            //request.KeepAlive = true;
            //request.Headers["Accept-Language"] = "en-US,en;q=0.5";
            //request.UserAgent = DefaultUserAgent;
            ////request.ContentType = contentType;
            //request.Headers["Pragma"] = "no-cache";
            //request.ContentType = "multipart/form-data; boundary=" + strBoundary;

            request.Method = "POST";
            //request.Timeout = timeOut;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.KeepAlive = true;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36";


            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            else
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(Cookies);
            }


            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            else
            {
                request.UserAgent = DefaultUserAgent;
            }

            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value * 1000;
            }

            request.Expect = string.Empty;
            WebResponse webRespon = null;
            var postStream = new MemoryStream();
            //第一个是文字的上传
            //如果需要POST数据  
            if (!string.IsNullOrEmpty(parameters))
            {
                byte[] data = requestEncoding.GetBytes(parameters);
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }

            //这是图片，视频的上传

            for (int Index = 0; Index < keys.Count; Index++)
            {
                if (!string.IsNullOrEmpty(filePaths[Index]))
                {
                    using (FileStream fs = new FileStream(filePaths[Index], FileMode.Open, FileAccess.Read))
                    {
                        string formdata = null;

                        string fileFormdataTemplate =
                           "\r\n--" + strBoundary +
                           "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" +
                           "\r\nContent-Type: application/octet-stream" +
                           "\r\n\r\n";

                        //上传文件  
                        formdata = string.Format(
                            fileFormdataTemplate,
                            keys[Index], //表单键  
                            fs.Name);

                        //统一处理  
                        byte[] formdataBytes = null;
                        //第一行不需要换行  
                        if (postStream.Length == 0)
                            formdataBytes = Encoding.UTF8.GetBytes(formdata.Substring(2, formdata.Length - 2));
                        else
                            formdataBytes = Encoding.UTF8.GetBytes(formdata);
                        postStream.Write(formdataBytes, 0, formdataBytes.Length);

                        //写入文件内容   每次上传4K
                        int bufferLength = 4096;
                        byte[] buffer = new byte[bufferLength];
                        byte[] buffer1 = new Byte[checked((uint)Math.Min(4096, (int)fs.Length))];
                        int bytesRead = 0;
                        while ((bytesRead = fs.Read(buffer, 0, buffer1.Length)) != 0)
                            postStream.Write(buffer, 0, bytesRead);
                    }
                    //结尾  
                    var footer = Encoding.UTF8.GetBytes("\r\n--" + strBoundary + "--\r\n");
                    postStream.Write(footer, 0, footer.Length);
                }
            }
            var v = request.GetResponse() as HttpWebResponse;

            Cookies.Add(request.CookieContainer.GetCookies(new Uri("http://" + new Uri(url).Host)));
            Cookies.Add(request.CookieContainer.GetCookies(new Uri("https://" + new Uri(url).Host)));
            Cookies.Add(v.Cookies);

            if (Debug)
            {
                Console.WriteLine("OK");
            }

            //如果需要POST数据  
            //if (!string.IsNullOrEmpty(filePath))
            //{
            //    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            //    {
            //        BinaryReader r = new BinaryReader(fs);

            //        //请求头部信息 
            //        StringBuilder sb = new StringBuilder();
            //        sb.Append("--");
            //        sb.Append(strBoundary);
            //        sb.Append("\r\n");
            //        sb.Append("Content-Disposition: form-data; name=\"");
            //        sb.Append("file");
            //        sb.Append("\"; filename=\"");
            //        sb.Append(fs.Name);
            //        sb.Append("\"");
            //        sb.Append("\r\n");
            //        sb.Append("Content-Type: ");
            //        sb.Append("application/octet-stream");
            //        sb.Append("\r\n");
            //        sb.Append("\r\n");

            //        string strPostHeader = sb.ToString();
            //        byte[] postHeaderBytes = Encoding.UTF8.GetBytes(strPostHeader);

            //        byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + strBoundary + "\r\n");

            //        long length = fs.Length + postHeaderBytes.Length + boundaryBytes.Length;
            //        long filelength = fs.Length;

            //        request.ContentLength = length;

            //        try
            //        {
            //           //每次上传4K
            //            int bufferLength = 4096;
            //            byte[] buffer = new byte[bufferLength];
            //            long offset = 0;

            //            //开始上传时间 
            //            DateTime startTime = DateTime.Now;
            //            int size = r.Read(buffer, 0, bufferLength);
            //            Stream postStream = request.GetRequestStream();
            //            //发送头部消息
            //            postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
            //            //发送文字参数
            //            byte[] data = requestEncoding.GetBytes(parameters);
            //            postStream.Write(data, 0, data.Length);
            //            //发送文件

            //            byte[] buffer1 = new Byte[checked((uint)Math.Min(4096, (int)fs.Length))];
            //            int bytesRead = 0;
            //            while ((bytesRead = fs.Read(buffer, 0, buffer1.Length)) != 0)
            //                postStream.Write(buffer, 0, bytesRead);


            //            //添加尾部时间
            //            postStream.Write(boundaryBytes, 0, strBoundary.Length);
            //            postStream.Close();

            //            //获取服务器端的响应
            //            webRespon = request.GetResponse();
            //            Stream s = webRespon.GetResponseStream();
            //            StreamReader sr = new StreamReader(s);
            //            //读取服务器端返回的消息
            //            string serverMsg = sr.ReadLine();
            //            //hwr = JSSerialize.Deserialize<HttpWebRequestReturn>(serverMsg);
            //            s.Close();
            //            sr.Close();
            //        }
            //        finally
            //        {
            //            fs.Close();
            //            r.Close();
            //        }
            //    }
            //}
            //var v = webRespon as HttpWebResponse;
            //v.Cookies = request.CookieContainer.GetCookies(request.RequestUri);

            //Cookies.Add(v.Cookies);

            //if (Debug)
            //{
            //    Console.WriteLine("OK");
            //}

            return v;
        }
        /// <summary>  
        /// 创建POST方式的HTTP请求  
        /// </summary>  
        /// <param name="url">请求的URL</param>  
        /// <param name="parameters">随同请求POST的参数名称及参数值字典</param>  
        /// <param name="timeout">请求的超时时间</param>  
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>  
        /// <param name="requestEncoding">发送HTTP请求时所用的编码</param>  
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>  
        /// <returns></returns>  
        public HttpWebResponse CreatePostFileHttpResponse(string url, string filePath, int? timeout = 300,
            string userAgent = "", CookieCollection cookies = null, string Referer = "",
            Dictionary<string, string> headers = null, string contentType = "application/x-www-form-urlencoded")
        {
            if (Debug)
            {
                Console.Write("Start Post Url:{0}  ", url);

            }

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            HttpWebRequest request = null;
            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }


            if (Proxy != null)
            {
                request.Proxy = Proxy;
            }

            request.Method = "POST";
            // This is important, otherwise the whole file will be read to memory anyway...  
            request.AllowWriteStreamBuffering = false;
            request.Accept = "text/html, application/xhtml+xml, application/json, text/javascript, */*; q=0.01";
            request.Referer = Referer;
            request.Headers["Accept-Language"] = "en-US,en;q=0.5";
            request.UserAgent = DefaultUserAgent;
            request.ContentType = contentType;
            request.Headers["Pragma"] = "no-cache";

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            else
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(Cookies);
            }


            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            else
            {
                request.UserAgent = DefaultUserAgent;
            }

            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value * 1000;
            }

            request.Expect = string.Empty;

            //如果需要POST数据  
            if (!string.IsNullOrEmpty(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    BinaryReader r = new BinaryReader(fs);

                    //时间戳 
                    string strBoundary = "----------" + DateTime.Now.Ticks.ToString("x");
                    byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + strBoundary + "\r\n");

                    //请求头部信息 
                    StringBuilder sb = new StringBuilder();
                    sb.Append("--");
                    sb.Append(strBoundary);
                    sb.Append("\r\n");
                    sb.Append("Content-Disposition: form-data; name=\"");
                    sb.Append("file");
                    sb.Append("\"; filename=\"");
                    sb.Append(fs.Name);
                    sb.Append("\"");
                    sb.Append("\r\n");
                    sb.Append("Content-Type: ");
                    sb.Append("application/octet-stream");
                    sb.Append("\r\n");
                    sb.Append("\r\n");
                    string strPostHeader = sb.ToString();
                    byte[] postHeaderBytes = Encoding.UTF8.GetBytes(strPostHeader);


                    request.ContentType = "multipart/form-data; boundary=" + strBoundary;
                    long length = fs.Length + postHeaderBytes.Length + boundaryBytes.Length;

                    request.ContentLength = length;

                    //开始上传时间 
                    DateTime startTime = DateTime.Now;

                    byte[] filecontent = new byte[fs.Length];

                    fs.Read(filecontent, 0, filecontent.Length);

                    using (Stream stream = request.GetRequestStream())
                    {

                        //发送请求头部消息 
                        stream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

                        stream.Write(filecontent, 0, filecontent.Length);

                        //添加尾部的时间戳 
                        stream.Write(boundaryBytes, 0, boundaryBytes.Length);
                    }
                }
            }
            var v = request.GetResponse() as HttpWebResponse;
            v.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
            //Cookies.Add(request.CookieContainer.GetCookies(new Uri("http://" + new Uri(url).Host)));
            //Cookies.Add(request.CookieContainer.GetCookies(new Uri("https://" + new Uri(url).Host)));

            //ClearCookies();
            Cookies.Add(v.Cookies);

            if (Debug)
            {
                Console.WriteLine("OK");
            }

            return v;
        }

        public HttpWebResponse CreatePostFileHttpResponse(string url, Encoding requestEncoding,
            Tuple<string, string, string[], string[]> tuple, int? timeout = 300,
            string userAgent = "", CookieCollection cookies = null, string Referer = "",
            Dictionary<string, string> headers = null, string contentType = "application/x-www-form-urlencoded")
        {
            if (Debug)
            {
                Console.Write("Start Post Url:{0}  ", url);

            }
            if (requestEncoding == null)
            {
                requestEncoding = new UTF8Encoding();
            }
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            HttpWebRequest request = null;
            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }


            if (Proxy != null)
            {
                request.Proxy = Proxy;
            }

            request.Method = "POST";
            // This is important, otherwise the whole file will be read to memory anyway...  
            request.AllowWriteStreamBuffering = false;
            request.Accept = "text/html, application/xhtml+xml, application/json, text/javascript, */*; q=0.01";
            request.Referer = Referer;
            request.Headers["Accept-Language"] = "en-US,en;q=0.5";
            request.UserAgent = DefaultUserAgent;
            request.ContentType = contentType;
            request.Headers["Pragma"] = "no-cache";

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            else
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(Cookies);
            }


            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            else
            {
                request.UserAgent = DefaultUserAgent;
            }

            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value * 1000;
            }

            request.Expect = string.Empty;

            IDictionary<string, string> parameters = new Dictionary<string, string>();
            //parameters.Add("projectId", tuple.Item1.ToString());
            //parameters.Add("type", tuple.Item2.ToString());

            string paramData = "projectId=" + tuple.Item1.ToString() + "&" + "type=" + tuple.Item2.ToString() + "&";
            for (int i = 0; i < tuple.Item3.Length; i++)
            {
                //如果需要POST数据  
                if (!string.IsNullOrEmpty(tuple.Item3[i]))
                {
                    using (FileStream fs = new FileStream(tuple.Item3[i], FileMode.Open, FileAccess.Read))
                    {
                        BinaryReader r = new BinaryReader(fs);

                        //时间戳 
                        string strBoundary = "----------" + DateTime.Now.Ticks.ToString("x");
                        byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + strBoundary + "\r\n");

                        //请求头部信息 
                        StringBuilder sb = new StringBuilder();
                        sb.Append("--");
                        sb.Append(strBoundary);
                        sb.Append("\r\n");
                        sb.Append("Content-Disposition: form-data; name=\"");
                        sb.Append("file");
                        sb.Append("\"; filename=\"");
                        sb.Append(fs.Name);
                        sb.Append("\"");
                        sb.Append("\r\n");
                        sb.Append("Content-Type: ");
                        sb.Append("application/octet-stream");
                        sb.Append("\r\n");
                        sb.Append("\r\n");
                        string strPostHeader = sb.ToString();
                        byte[] postHeaderBytes = Encoding.UTF8.GetBytes(strPostHeader);


                        request.ContentType = "multipart/form-data; boundary=" + strBoundary;
                        long length = fs.Length + postHeaderBytes.Length + boundaryBytes.Length;

                        request.ContentLength = length;
                        //开始上传时间 
                        DateTime startTime = DateTime.Now;
                        byte[] filecontent = new byte[fs.Length];
                        fs.Read(filecontent, 0, filecontent.Length);


                        //paramData
                        string file1 = "file=" + System.Text.Encoding.Default.GetString(filecontent) + "&memo=" + tuple.Item4[i].ToString() + "&";
                        paramData = paramData + file1;
                        //parameters.Add("file", filecontent.ToString());
                        //parameters.Add("memo", tuple.Item4[i].ToString());
                    }
                }
            }
            paramData = paramData.Remove(paramData.Length - 1);

            HttpWebResponse v = null;
            try
            {
                //var buffer = CreateParameter(parameters);
                var buffer = paramData;
                byte[] data = requestEncoding.GetBytes(buffer.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    //发送请求头部消息 
                    //stream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                    stream.Write(data, 0, data.Length);
                    //添加尾部的时间戳 
                    //stream.Write(boundaryBytes, 0, boundaryBytes.Length);
                }

                v = request.GetResponse() as HttpWebResponse;
                v.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                //Cookies.Add(request.CookieContainer.GetCookies(new Uri("http://" + new Uri(url).Host)));
                //Cookies.Add(request.CookieContainer.GetCookies(new Uri("https://" + new Uri(url).Host)));

                //ClearCookies();
                Cookies.Add(v.Cookies);

                if (Debug)
                {
                    Console.WriteLine("OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {

            }

            return v;
        }

        /// <summary>
        /// 用&符号，隔开多个参数
        /// </summary>
        /// <param name="parameters">多个参数</param>
        /// <returns>返回字符串</returns>
        public static string CreateParameter(IDictionary<string, string> parameters)
        {
            StringBuilder buffer = new StringBuilder();
            foreach (string key in parameters.Keys)
            {
                buffer.AppendFormat("&{0}={1}", key, parameters[key]);
            }
            return buffer.ToString().TrimStart('&');
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors errors)
        {
            return true; //总是接受  
        }



        public string Post(string url, IDictionary<string, string> parameters, Encoding requestEncoding,
            Encoding responseEncoding, int? timeout = 300, string userAgent = "", CookieCollection cookies = null,
            string Referer = "", Dictionary<string, string> headers = null,
            string contentType = "application/x-www-form-urlencoded")
        {
            HttpWebResponse response = CreatePostHttpResponse(url, parameters, requestEncoding, timeout, userAgent,
                cookies, Referer, headers, contentType);

            try
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }


        public T Post<T>(string url, IDictionary<string, string> parameters, Encoding requestEncoding,
            Encoding responseEncoding, int? timeout = 300, string userAgent = "", CookieCollection cookies = null,
            string Referer = "", Dictionary<string, string> headers = null,
            string contentType = "application/x-www-form-urlencoded")
        {
            return
                JsonConvert.DeserializeObject<T>(Post(url, parameters, requestEncoding, responseEncoding, timeout,
                    userAgent, cookies, Referer, headers, contentType));
        }


        public string Post(string url, string parameters, Encoding requestEncoding, Encoding responseEncoding,
            int? timeout = 300, string userAgent = "", CookieCollection cookies = null, string Referer = "",
            Dictionary<string, string> headers = null, string contentType = "application/x-www-form-urlencoded")
        {
            HttpWebResponse response = CreatePostHttpResponse(url, parameters, requestEncoding, timeout, userAgent,
                cookies, Referer, headers, contentType);

            try
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public T Post<T>(string url, string parameters, Encoding requestEncoding, Encoding responseEncoding,
            int? timeout = 300, string userAgent = "", CookieCollection cookies = null, string Referer = "",
            Dictionary<string, string> headers = null, string contentType = "application/x-www-form-urlencoded")
        {
            return
                JsonConvert.DeserializeObject<T>(Post(url, parameters, requestEncoding, responseEncoding, timeout,
                    userAgent, cookies, Referer, headers, contentType));
        }

        public string PostFile(string url, string filePath, Encoding responseEncoding,
            int? timeout = 300, string userAgent = "", CookieCollection cookies = null, string Referer = "",
            Dictionary<string, string> headers = null, string contentType = "application/x-www-form-urlencoded")
        {
            HttpWebResponse response = CreatePostFileHttpResponse(url, filePath, timeout, userAgent, cookies, Referer,
                headers, contentType);

            try
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), responseEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string Get(string url, Encoding responseEncoding, int? timeout = 300, string userAgent = "",
            CookieCollection cookies = null
            , string Referer = "", Dictionary<string, string> headers = null,
            string contentType = "application/x-www-form-urlencoded")
        {
            HttpWebResponse response = CreateGetHttpResponse(url, null, timeout, userAgent, cookies, Referer, headers,
                contentType);

            try
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return null;
            }
        }

        public T Get<T>(string url, Encoding responseEncoding, int? timeout = 300, string userAgent = "",
            CookieCollection cookies = null, string Referer = "", Dictionary<string, string> headers = null,
            string contentType = "application/x-www-form-urlencoded")
        {
            return
                JsonConvert.DeserializeObject<T>(Get(url, responseEncoding, timeout, userAgent, cookies, Referer,
                    headers, contentType));
        }

        public byte[] GetFile(string url, out Dictionary<string, string> header, int? timeout = 300,
            string userAgent = "", CookieCollection cookies = null, string Referer = "",
            Dictionary<string, string> headers = null)
        {
            HttpWebResponse response = CreateGetHttpResponse(url, null, timeout, userAgent, cookies, Referer, headers);

            header = new Dictionary<string, string>();

            foreach (string key in response.Headers.AllKeys)
            {
                header.Add(key, response.Headers[key]);
            }

            try
            {
                System.IO.Stream st = response.GetResponseStream();

                byte[] by = new byte[response.ContentLength];

                st.Read(by, 0, by.Length);

                st.Close();

                return by;
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        public Stream GetStream(string url, int? timeout = 300, string userAgent = "", CookieCollection cookies = null,
            string Referer = "", Dictionary<string, string> headers = null)
        {
            HttpWebResponse response = CreateGetHttpResponse(url, null, timeout, userAgent, cookies, Referer, headers);

            return response.GetResponseStream();
        }


        //public static string PostWebRequest(string postUrl, string paramData, Encoding dataEncode)
        //{
        //    string ret = string.Empty;
        //    try
        //    {
        //        CookieContainer cookies = new CookieContainer();
        //        byte[] byteArray = dataEncode.GetBytes(paramData); //转化
        //        HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
        //        myHttpWebRequest.Method = "POST";
        //        myHttpWebRequest.ContentType = "text/xml";
        //        myHttpWebRequest.Timeout = 20 * 1000; //连接超时
        //        myHttpWebRequest.Accept = "*/*";
        //        myHttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0;)";
        //        cookies.Add(EBIMWorksCommLib.LoginDB.Cookie);
        //        myHttpWebRequest.CookieContainer = cookies; //使用已经保存的cookies 方法一

        //        myHttpWebRequest.ContentLength = byteArray.Length;
        //        Stream newStream = myHttpWebRequest.GetRequestStream();
        //        newStream.Write(byteArray, 0, byteArray.Length);//写入参数
        //        newStream.Close();
        //        HttpWebResponse response = (HttpWebResponse)myHttpWebRequest.GetResponse();
        //        ret = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
        //        response.Close();
        //        newStream.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        XtraMessageBox.Show(ex.Message);
        //    }
        //    return ret;
        //}

    }
}
