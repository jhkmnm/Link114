using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CsharpHttpHelper;
using System.Text.RegularExpressions;
using System.Configuration;
using System.IO;

namespace Link114
{
    public partial class Form1 : Form
    {
        string getPath = "http://www.link114.cn/get.php?title&{0}&64113";
        Random rdm = new Random();

        public Form1()
        {
            InitializeComponent();

            txtKeyword.Text = LoadConfig()[0];
        }

        private string GetTitle(string url)
        {
            var item = new HttpItem()
            {
                URL = string.Format(getPath, url), 
                Method = "get",
                Encoding = Encoding.UTF8
            };

            HttpHelper http = new HttpHelper();
            HttpResult htmlr = http.GetHtml(item);
            string title = "";
            switch (htmlr.Html)
            {
                case "-1":
                    title = "失败:重查";
                    break;
                case "-2":
                    title = "此功能升级中";
                    break;
                case "-3":
                    title = "已停用";
                    break;
                case "-4":
                    title = "不支持域名后辍";
                    break;
                default:
                    title = GetRemovePrefixString(htmlr.Html, "200:");
                    break;
            }
            return title;// string.Format("{0}   {1}", url, title);
        }

        ///<summary>
        /// 移除前缀字符串
        ///</summary>
        ///<param name="val">原字符串</param>
        ///<param name="str">前缀字符串</param>
        ///<returns></returns>
        private string GetRemovePrefixString(string val, string str)
        {
            string strRegex = @"^(" + str + ")";
            return Regex.Replace(val, strRegex, "");
        }

        /// <summary>
        /// 本地保存登录的账号和密码
        /// </summary>
        public void SaveConfig()
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings["KeyWord"].Value = txtKeyword.Text;
            configuration.Save();
        }

        /// <summary>
        /// 读取本地的账号和密码
        /// </summary>
        public string[] LoadConfig()
        {
            string[] str = new string[2];
            str[0] = ConfigurationManager.AppSettings["KeyWord"];
            return str;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            var urls = txtUrl.Text.Replace("\n", "").Split('\r');
            List<string> resultAll = new List<string>();
            List<string> resultOK = new List<string>();
            string strRegex = @"(" + txtKeyword.Text.Replace(",", "|").Replace("，", "|") + ")";
            foreach (string url in urls)
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    var title = GetTitle(url);
                    resultAll.Add(string.Format("{0}   {1}", url, title));
                    if (Regex.IsMatch(title, strRegex))
                    {
                        resultOK.Add(string.Format("{0}   {1}", url, title));
                    }
                }
            }

            if(resultAll.Count > 0)
                Write(resultAll, "网址和标题" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            if (resultOK.Count > 0)
                Write(resultOK, "导出的文件" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

            MessageBox.Show("保存成功");
            button1.Enabled = true;
        }

        private void Write(List<string> list, string fileName)
        {
            using (SaveFileDialog file = new SaveFileDialog())
            {
                file.FileName = fileName;
                if(file.ShowDialog() == DialogResult.OK)
                {
                    string path = file.FileName;
                    FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
                    StreamWriter sw = new StreamWriter(fs);
                    //开始写入
                    foreach (string str in list)
                        sw.WriteLine(str);
                    //清空缓冲区
                    sw.Flush();
                    //关闭流
                    sw.Close();
                    fs.Close();
                }
            }
        }

        private void btnImportKeyword_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog file = new OpenFileDialog())
            {
                if (file.ShowDialog() == DialogResult.OK)
                {
                    txtKeyword.Text = "";
                    StreamReader sr = new StreamReader(file.FileName, Encoding.Default);
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        txtKeyword.Text += line.ToString();
                    }
                    SaveConfig();
                }
            }
        }

        private void btnImportUrl_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog file = new OpenFileDialog())
            {
                if (file.ShowDialog() == DialogResult.OK)
                {
                    txtUrl.Text = "";
                    StreamReader sr = new StreamReader(file.FileName, Encoding.Default);
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        txtUrl.Text += line.ToString() + "\r\n";
                    }
                }
            }
        }
    }
}
