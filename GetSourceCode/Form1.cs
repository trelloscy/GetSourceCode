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

        private void button1_Click(object sender, EventArgs e)
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
                result = match.Groups[1].Value;
            }

            return result;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }
    }
}
