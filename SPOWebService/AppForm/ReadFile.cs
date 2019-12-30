using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormApp
{
    public partial class ReadFile : Form
    {
        public ReadFile()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FileStream fileStream = null;
            byte[] fileContent = null;
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Files (*.doc;*.ppt;*.xls;*.pdf;*.jpg;*.gif;*.png;*.tiff;*.tif;|*.doc;*.ppt;*.xls;*.pdf;*.jpg;*.gif;*.png;*.tiff;*.tif|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    fileStream = File.OpenRead(openFileDialog.FileName);
                    txtSourcePath.Text = openFileDialog.FileName;
                    fileContent = new byte[Convert.ToInt32(fileStream.Length)];
                    fileStream.Read(fileContent, 0, Convert.ToInt32(fileStream.Length));
                }

                string path = Directory.GetCurrentDirectory() + DateTime.Now.ToString("yyyy-dd-MM") + Path.GetFileNameWithoutExtension(openFileDialog.FileName) + ".txt";
                File.WriteAllText(path, Convert.ToBase64String(fileContent));
                MessageBox.Show("File Read Successful", "", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtSourcePath.Text = "";
        }
    }
}
