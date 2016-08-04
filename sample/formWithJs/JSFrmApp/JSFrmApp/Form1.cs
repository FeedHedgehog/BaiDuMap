using JSFrmApp.Tool;
using JSFrmApp.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace JSFrmApp
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public partial class Form1 : Form
    {
        private string _htmlPath = Application.StartupPath + "/html/EBIMDemo.html";
        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            UiInitialize();

            #region Event Register

            btnLoad.ItemClick += BtnLoad_ItemClick;

            #endregion
        }

        #region Events Response

        private void BtnLoad_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            LogicLayer.Logic.Instance.InvokePushDataToHtmlMethod(webBrowser1);
        }

        #endregion

        #region Public ExtentionMethods

        /// <summary>
        /// 前台Html中js调用的方法,网页操作cs窗体
        /// </summary>
        /// <param name="message"></param>
        public void GetNavigateToU3DMessage(string message)
        {
            MessageBox.Show(message, "client code");
            //this.Controls.Clear();
            //var dsd = new UserControl1();
            //dsd.Dock = DockStyle.Fill;

            //this.Controls.Add(dsd);
        }

        #endregion

        #region Methods

        private void UiInitialize()
        {
            webBrowser1.AllowNavigation = true;
            webBrowser1.Url = new Uri(_htmlPath);
            webBrowser1.AllowWebBrowserDrop = false;
            webBrowser1.IsWebBrowserContextMenuEnabled = false;
            webBrowser1.WebBrowserShortcutsEnabled = false;
            webBrowser1.ObjectForScripting = this;
            // Uncomment the following line when you are finished debugging.
            webBrowser1.ScriptErrorsSuppressed = true;

            //webBrowser1.DocumentText =
            //  "<html><head><script>" +
            //  "function test(message) { alert(message); }" +
            //  "</script></head><body><button " +
            //  "onclick=\"window.external.Test('called from script code')\">" +
            //  "call client code from script code</button>" +
            //  "</body></html>";  
        }

        #endregion
    }
}
