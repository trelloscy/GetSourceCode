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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetSourceCode
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                String url = $"https://www.instagram.com/{txtUsername.Text}/";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    richTextBox1.Text = sr.ReadToEnd();
                }

                textBox1.Text = RegexCheck("\"full_name\":\"(.*?)\",");
                textBox2.Text = RegexCheck("\"edge_followed_by\":{\"count\":(\\d*)}");
                textBox3.Text = RegexCheck("\"edge_follow\":{\"count\":(\\d*)}");
                textBox4.Text = RegexCheck("\"edge_owner_to_timeline_media\":{\"count\":(\\d*),");
                pictureBox1.Load(RegexCheck("\"profile_pic_url\":\"(.*?)\""));

                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddEllipse(0, 0, pictureBox1.Height, pictureBox1.Height);
                pictureBox1.Region = new Region(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string RegexCheck(string pattern)
        {
            string result = "";

            var match = Regex.Match(richTextBox1.Text, pattern);
            if (match.Success && match.Groups.Count > 1)
            {
                //result = match.Groups[1].Value.Replace("\\u0026", "&");
                result = Regex.Unescape(match.Groups[1].Value);
            }

            return result;
        }
        private Task<string> RegexCheckAll(string source, string pattern)
        {
            string result = "";

            return Task.Run(() =>
            {
                var matches = Regex.Matches(source, pattern);
                foreach (Match match in matches)
                {
                    var text = match.Groups[1].Value;
                    text = "{" + text.Replace("&#92;\"", "'") + "}";
                    JToken parsedJson = JToken.Parse(text);
                    var beautified = parsedJson.ToString(Formatting.Indented);
                    result += beautified + Environment.NewLine;
                }

                return result;
            });
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private async void btnParse_Click(object sender, EventArgs e)
        {
            try
            {
                richTextBox2.Text = Clipboard.GetText();

                //String url = "https://www.linkedin.com/me/profile-views/urn:li:wvmp:summary/";
                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                //using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                //{
                //    richTextBox2.Text = await sr.ReadToEndAsync();
                //}

                //textBox1.Text = RegexCheck("\"full_name\":\"(.*?)\",");
                //textBox2.Text = RegexCheck("\"edge_followed_by\":{\"count\":(\\d*)}");
                //textBox3.Text = RegexCheck("\"edge_follow\":{\"count\":(\\d*)}");
                //textBox4.Text = RegexCheck("\"edge_owner_to_timeline_media\":{\"count\":(\\d*),");
                //pictureBox1.Load(RegexCheck("\"profile_pic_url\":\"(.*?)\""));

                //System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                //path.AddEllipse(0, 0, pictureBox1.Height, pictureBox1.Height);
                //pictureBox1.Region = new Region(path);

                if (string.IsNullOrEmpty(richTextBox2.Text))
                {
                    MessageBox.Show("No source code specified");
                    return;
                }

                progressBar1.Style = ProgressBarStyle.Marquee;
                string sourceCode = richTextBox2.Text.Replace("&quot;", "\"");
                //richTextBox2.Text = await RegexCheckAll(sourceCode, "({\"data\":.*})");
                var result = await RegexCheckAll(sourceCode, "(\"firstName\":\".*?),\"objectUrn\"");
                await Task.Delay(1500);
                richTextBox2.Text = result;
                //richTextBox2.ScrollToCaret();
                progressBar1.Style = ProgressBarStyle.Blocks;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
