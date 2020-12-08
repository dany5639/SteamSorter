using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamSorter
{
    public partial class Form1 : Form
    {
        private string path = @""; // need to auto get paths
        /// <summary>
        /// A collection of the games found in appmanifest files.
        /// </summary>
        private Dictionary<int, item> items;
        public class item
        {
            public string title;
            public long size;
        }
        public Form1()
        {
            InitializeComponent();

            items = new Dictionary<int, item>();

            toolStripStatusLabel1.Text = "Double click on any cell to uninstall a game.";

        }
        /// <summary>
        /// Read any text file and return as a list of strings, one string per line.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private static List<string> ReadTextFile(string filename)
        {
            var output = new List<string>();

            using (var reader = new StreamReader(filename))
            {
                var line = "";
                while (line != null)
                {
                    line = reader.ReadLine();
                    output.Add(line);
                }

                if (output.Last() == null)
                    output.RemoveAt(output.Count - 1);
            }

            return output;
        }

        /// <summary>
        /// Read every Steam appmanifest file to get the appid, game title, and game size on disk.
        /// </summary>
        private void ReadFiles()
        {
            var files = Directory.EnumerateFiles(path);

            foreach (var file in files)
            {
                if (!file.EndsWith(".acf"))
                    continue;

                // get appid from filename
                var appid_s = file.Split("_".ToCharArray());
                var appid_s2 = appid_s[1].Split(".".ToCharArray())[0];
                int.TryParse(appid_s2, System.Globalization.NumberStyles.Integer, null, out int appid);

                var item1 = new item();
                int collectedValues = 0;

                var text = ReadTextFile(file);
                foreach (var line in text)
                {
                    if (line.Contains("\"appid\""))
                    {
                        var line2 = line.Split("		".ToCharArray());
                        var line3 = line2[3].Split("\"".ToCharArray());
                        var line4 = line3[1];
                        int.TryParse(line4, System.Globalization.NumberStyles.Integer, null, out int appid_check);
                        if (appid_check != appid)
                            continue;
                            // throw new Exception("ERROR: filename appid missmatches the appid in the file.");

                        collectedValues++;

                    }

                    if (line.Contains("\"name\""))
                    {
                        var line2 = line.Split("		".ToCharArray());
                        var line3 = line2[3];
                        item1.title = line3;

                        collectedValues++;

                    }

                    if (line.Contains("\"SizeOnDisk\""))
                    {
                        var line2 = line.Split("		".ToCharArray());
                        var line3 = line2[3];
                        var line4 = line3.Substring(1, line3.Length - 2);
                        long.TryParse(line4, System.Globalization.NumberStyles.Integer, null, out long size);
                        item1.size = size;

                        collectedValues++;

                    }

                }

                if (collectedValues < 3)
                    continue;

                if (!items.ContainsKey(appid))
                    items.Add(appid, item1);

            }
        }
        /// <summary>
        /// Fill up the table with the found games: appid, title, size.
        /// </summary>
        private void PopulateList()
        {
            dataGridView1.Rows.Clear();

            int i = -1;
            foreach (var a in items)
            {
                i++;
                var size = $"{((a.Value.size / 1024 / 1024)):D6}";
                size = $"{size.Substring(0, 3)},{size.Substring(3,3)} GB";
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = $"{a.Key:D8}";
                dataGridView1.Rows[i].Cells[1].Value = size;
                dataGridView1.Rows[i].Cells[2].Value = a.Value.title;

            }
        }
        /// <summary>
        /// Double click on any row to call Steam and uninstall the game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count == 0)
                return;

            var rowindex = dataGridView1.SelectedCells[0].RowIndex;

            var a = dataGridView1.Rows[rowindex].Cells[0].Value;

            int.TryParse(a.ToString(), System.Globalization.NumberStyles.Integer, null, out int val);

            var path = $"steam://uninstall/{val}";

            Process.Start(path);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            path = folderBrowserDialog1.SelectedPath;

            ReadFiles();

            PopulateList();
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }
    }
}
