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

namespace WebServiceUtility
{
    public partial class ConvertFileToByte : Form
    {
        public ConvertFileToByte()
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
                string filename = Path.GetFileNameWithoutExtension(openFileDialog.FileName) + DateTime.Now.ToString("yyyy-dd-MM") + ".txt";
                //save to the application executable folder
                string path = Application.StartupPath + "\\" + filename;
                File.WriteAllText(path, Convert.ToBase64String(fileContent));
                MessageBox.Show("Conversion Successful. Filename : "
                                + filename
                                + "\nFile Path: "
                                + Application.StartupPath, "", MessageBoxButtons.OK);
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

        private void ConvertFileToByte_Load(object sender, EventArgs e)
        {

        }
    }
}
