using EncryptConfiguration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormApp
{
    public partial class Encrypt : Form
    {
        string plaintext = "", Key = "", Iv = "", Encrypted = "";

        public Encrypt()
        {
            InitializeComponent();
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            try
            {
                plaintext = txtStringtoEncrypt.Text;
                if (plaintext.Length > 0 && !string.IsNullOrEmpty(plaintext) && !string.IsNullOrWhiteSpace(plaintext))
                {
                    Encrypted = EncryptDecrypt.Encrypt(plaintext, ref Key, ref Iv);

                    txtEncryptedString.Text = Encrypted;
                    txtEncryptedKey.Text = Key;
                    txtEncryptedIv.Text = Iv;
                    MessageBox.Show("Encryption Successful");
                }
                else
                {
                    btnReset_Click(sender, e);
                    MessageBox.Show("Please enter the text to encrypt");
                    txtStringtoEncrypt.Focus();
                }
            }
            catch (Exception ex)
            {
                btnReset_Click(sender, e);
                MessageBox.Show("Error in Encryption :" + ex.Message);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            plaintext = string.Empty;
            Key = string.Empty;
            Iv = string.Empty;
            Encrypted = string.Empty;
            txtStringtoEncrypt.Text = "";
            txtEncryptedString.Text = "";
            txtEncryptedKey.Text = "";
            txtEncryptedIv.Text = "";
        }
    }
}
