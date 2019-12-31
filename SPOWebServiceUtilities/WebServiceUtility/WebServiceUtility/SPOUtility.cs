using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebServiceUtility
{
    public partial class SPOUtility : Form
    {
        public SPOUtility()
        {
            InitializeComponent();
            ConvertFileToByte readFile = new ConvertFileToByte();
            AddNewTab(readFile);
            Encrypt encrypt = new Encrypt();
            AddNewTab(encrypt);
            
        }
        private void AddNewTab(Form frm)
        {
            TabPage tab = new TabPage(frm.Text);
            tab.Height = frm.Height;
            tab.Width = frm.Width;
            frm.TopLevel = false;
            frm.AutoSize = true;
            frm.Parent = tab;
            frm.Location = new Point((tab.Width - frm.Width) / 2, (tab.Height - frm.Height) / 2);
            frm.Visible = true;
            tabControl1.TabPages.Add(tab);
            tabControl1.SelectedTab = tab;
            tabControl1.Size = tabControl1.Size + new Size(frm.Width, frm.Height);
        }
    }
}
