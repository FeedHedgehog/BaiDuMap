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
        private string _htmlPath = Application.StartupPath + "/html/EBIM.html";
        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //webBrowser1.AllowNavigation = true;
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
            button1.Click += Button1_Click;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            LogicLayer.Logic.Instance.InvokePushDataToHtmlMethod(webBrowser1);
        }


        public void GetNavigateToU3DMessage(string message)
        {
            MessageBox.Show(message, "client code");
            //this.Controls.Clear();
            //var dsd = new UserControl1();
            //dsd.Dock = DockStyle.Fill;

            //this.Controls.Add(dsd);
        }


    }
}
