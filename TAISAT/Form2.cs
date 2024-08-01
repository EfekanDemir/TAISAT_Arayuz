using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TAISAT
{

    public partial class Form2 : Form
    {
        //Video Kaydı Yolu Değişkeni
        public static string videorecordpath = @"";
        

        public Form2()
        {
            InitializeComponent();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void form2_Cancel_Button_Click(object sender, EventArgs e)
        {
            videorecordpath = null;
            Close();
        }

        private void button_browseVideoFolderToSave_Click(object sender, EventArgs e)
        {

            try
            {
                FolderBrowserDialog browser = new FolderBrowserDialog();
                if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    videorecordpath = browser.SelectedPath;
                    string[] str = videorecordpath.Split('\\');
                    textBox_videoFolderToSave.Text = "..\\" + str[str.Length - 1];
                    textBox_videoFolderToSave.ReadOnly = true;
                }
            }
            catch (Exception) { MessageBox.Show("button_browseVideoFolderToSave_Click"); }
        }

        private void form2_Save_Button_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
