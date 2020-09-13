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

namespace GifOptimizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Gif file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "GIF Files|*.gif";
            openFileDialog.Title = "Select a GIF File";

            // Show th Dialog.
            // If the user clicked OK in the dialogue and a .GIF file was selected, open it.
            
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxPath.Text = openFileDialog.FileName;
                // show the gif image in the picture box
                using (var fs = new System.IO.FileStream(openFileDialog.FileName, System.IO.FileMode.Open,System.IO.FileAccess.Read))
                {
                    var ms = new System.IO.MemoryStream();
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    if (pictureBoxSource.Image != null)
                        pictureBoxSource.Image.Dispose();
                    pictureBoxSource.Image = Image.FromStream(ms);
                }
            }
        }

        // Load the last settings
        private void Form1_Load(object sender, EventArgs e)
        {
            comboBoxCompressLevel.Text = Properties.Settings.Default.Level;
            numericUpDownLossy.Value = Properties.Settings.Default.Lossy;
            numericUpDownColor.Value = Properties.Settings.Default.Color;
            textBoxWidth.Text = Properties.Settings.Default.Width.ToString();
            textBoxHeight.Text = Properties.Settings.Default.Height.ToString();
        }

        // Remember the settings
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // save our settings
            Properties.Settings.Default.Level = comboBoxCompressLevel.Text;
            Properties.Settings.Default.Lossy = Convert.ToInt32(numericUpDownLossy.Value);
            Properties.Settings.Default.Color = Convert.ToInt32(numericUpDownColor.Value);
            Properties.Settings.Default.Width = Convert.ToInt32(textBoxWidth.Text);
            Properties.Settings.Default.Height = Convert.ToInt32(textBoxHeight.Text);

            Properties.Settings.Default.Save();
        }

        // Optimization method
        private void Optimize()
        {
            string ExeToUse = string.Empty;

            if (Environment.Is64BitOperatingSystem == true)
                ExeToUse = "gifopt64.exe";
            else
                ExeToUse = "gifopt32.exe";

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.FileName = ExeToUse;

            string CompressValue = comboBoxCompressLevel.Text;

            StringBuilder ArgsString = new StringBuilder();

            ArgsString.Append(" -" + CompressValue + " ");

            // Append optional parameters
            if (numericUpDownLossy.Value > 0)
                ArgsString.Append(" --lossy=" + numericUpDownLossy.Value.ToString());

            if (numericUpDownColor.Value > 0)
                ArgsString.Append(" --colors=" + numericUpDownColor.Value.ToString());

            if (textBoxHeight.Text != "0" && textBoxWidth.Text != "0")
                ArgsString.Append(" --resize-fit " + textBoxWidth.Text + "X" + textBoxHeight.Text + " ");

            ArgsString.Append("\"" + textBoxPath.Text + "\"");
            ArgsString.Append(" -o" + "\"" + "temp.gif" + "\"");

            processStartInfo.Arguments = ArgsString.ToString();

            // Start the execution
            Process.Start(processStartInfo).WaitForExit();

            // show the gif image in the picture box
            using (var fs = new System.IO.FileStream("temp.gif", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var ms = new System.IO.MemoryStream();
                fs.CopyTo(ms);
                ms.Position = 0;
                if (pictureBoxTarget.Image != null)
                    pictureBoxTarget.Image.Dispose();
                pictureBoxTarget.Image = Image.FromStream(ms);
            }


        }

        private void buttonOptimize_Click(object sender, EventArgs e)
        {
            Optimize();
        }

        private void buttonSaveResult_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            DialogResult dialogResult = folderBrowserDialog.ShowDialog();

            if(dialogResult == DialogResult.OK)
            {
                File.Copy("temp.gif", folderBrowserDialog.SelectedPath + "\\Optimized.gif");
            }
        }
    }
}
