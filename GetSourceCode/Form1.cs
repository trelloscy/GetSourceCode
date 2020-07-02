using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetSourceCode {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        public string InstagramURL { get; set; } = "https://www.instagram.com";
        public string OutputCSV { get; set; } = $@"C:\Users\koliost\Desktop\followers statistics\follower statistics {DateTime.Now.ToString("yyyyMMddHHmmss")}.csv";

        private void btnSearch_Click(object sender, EventArgs e) {
            try {
                var profileUrl = $"{InstagramURL}/{txtUsername.Text}/";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(profileUrl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
                    richTextBox1.Text = sr.ReadToEnd();
                }

                var username = RegexCheck("\"full_name\":\"(.*?)\",");
                ParseDetails(out string followers, out string following, out string postCount, out string lastPostDateFormatted);

                // Show fields on form
                textBox1.Text = username;
                textBox2.Text = followers;
                textBox3.Text = following;
                textBox4.Text = postCount;
                textBox5.Text = lastPostDateFormatted;

                // Show profile picture
                var profilePictureURL = RegexCheck("\"profile_pic_url\":\"(.*?)\"");
                pictureBox1.Load(profilePictureURL);

                // Make picture round
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddEllipse(0, 0, pictureBox1.Height, pictureBox1.Height);
                pictureBox1.Region = new Region(path);

            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private string RegexCheck(string pattern) {
            string result = "";

            var match = Regex.Match(richTextBox1.Text, pattern);
            if (match.Success && match.Groups.Count > 1) {
                //result = match.Groups[1].Value.Replace("\\u0026", "&");
                result = Regex.Unescape(match.Groups[1].Value);
            }

            return result;
        }
        private string RegexCheckLargest(string pattern) {

            string result = "";

            var match = Regex.Matches(richTextBox1.Text, pattern).Cast<Match>().OrderByDescending(x => x.Value).FirstOrDefault();
            if (match != null && match.Success && match.Groups.Count > 1) {
                result = Regex.Unescape(match.Groups[1].Value);
            }

            return result;
        }
        private Task<string> RegexCheckAll(string source, string pattern) {
            string result = "";

            return Task.Run(() => {
                var matches = Regex.Matches(source, pattern);
                foreach (Match match in matches) {
                    var text = match.Groups[1].Value;
                    text = "{" + text.Replace("&#92;\"", "'") + "}";
                    JToken parsedJson = JToken.Parse(text);
                    var beautified = parsedJson.ToString(Formatting.Indented);
                    result += beautified + Environment.NewLine;
                }

                return result;
            });
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape) {
                Close();
            }
        }

        private async void btnParse_Click(object sender, EventArgs e) {
            try {
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

                if (string.IsNullOrEmpty(richTextBox2.Text)) {
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
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private async void btnParseFollowers_Click(object sender, EventArgs e) {

            if (string.IsNullOrEmpty(txtFollowersSource.Text)) {
                MessageBox.Show("No follower usernames specified");
                return;
            }

            // Parse followers list into array
            var followersList = txtFollowersSource.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int counter = 0;
            lblCounter.Text = $"Completed: 0/{followersList.Length}";
            btnParseFollowers.Enabled = false;

            using (var stream = File.CreateText(OutputCSV)) {

                // Write headers
                var headers = "Username,Followers,Following,Post Count,Last Post Date,Profile URL";
                stream.WriteLine(headers);

                // Parse each follower, get post count and latest date, then delay for x seconds
                foreach (var username in followersList) {

                    try {
                        var profileUrl = $"{InstagramURL}/{username}/";
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(profileUrl);
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
                            richTextBox1.Text = sr.ReadToEnd();
                        }

                        ParseDetails(out string followers, out string following, out string postCount, out string lastPostDateFormatted);

                        string csvRow = $"{username},{followers},{following},{postCount},{lastPostDateFormatted},{profileUrl}";
                        stream.WriteLine(csvRow);

                        // Finally, wait 300-600ms before moving to the next
                        await Task.Delay(new Random().Next(5000, 10000));
                        lblCounter.Text = $"Completed: {++counter}/{followersList.Length}";

                    } catch (Exception ex) {
                        MessageBox.Show(ex.Message);
                    }

                } // End of foreach
            } // End of using streamwriter

            btnParseFollowers.Enabled = true;

            if (followersList.Length > 0) {
                btnOpenCSV.Visible = true;
            }
        }

        private void ParseDetails(out string followers, out string following, out string postCount, out string lastPostDateFormatted) {
            followers = RegexCheck("\"edge_followed_by\":{\"count\":(\\d*)}");
            following = RegexCheck("\"edge_follow\":{\"count\":(\\d*)}");
            postCount = RegexCheck("\"edge_owner_to_timeline_media\":{\"count\":(\\d*),");
            var latestTimestamp = RegexCheckLargest("\"taken_at_timestamp\":(\\d*)");
            lastPostDateFormatted = "N/A";

            // New
            if (!string.IsNullOrEmpty(postCount)
                && postCount != "0"
                && !string.IsNullOrEmpty(latestTimestamp)
                && double.TryParse(latestTimestamp, out double timestamp)) {

                // Add the timestamp (number of seconds since the Epoch) to be converted
                lastPostDateFormatted = new DateTime(1970, 1, 1, 0, 0, 0, 0)
                                   .AddSeconds(timestamp)
                                   .ToLocalTime()
                                   .ToString("yyyy-MM-ddTHH:mm:ss");
            }
        }

        private void btnOpenCSV_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start(OutputCSV);
        }
    }
}
