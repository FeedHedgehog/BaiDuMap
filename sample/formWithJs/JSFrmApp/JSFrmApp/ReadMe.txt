首先在js中定义被c#调用的方法:
function  Messageaa(message)
{
      alert(message);
}

在c#调用js方法Messageaa
private void button1_Click(object sender, EventArgs e)
{
    // 调用JavaScript的messageBox方法，并传入参数
    object[] objects = new object[1];
    objects[0] = "c#diao javascript";
    webBrowser1.Document.InvokeScript("Messageaa", objects);
}

用JS调用C#函数的方法：
复制代码
首先在c#中定义被js调用的方法：
public void MyMessageBox(string message)
{
    MessageBox.Show(message);
}

在js中调用c#方法：
<!-- 调用C#方法 -->
<button onclick="window.external.MyMessageBox('javascript访问C#代码')" >
javascript访问C#代码</button>