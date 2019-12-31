using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace WebServiceUtility
{
    public partial class Encrypt : Form
    {
        string plaintext = "", Key = "", Iv = "", Encrypted = "";
        public static readonly int KeyBitSize = 256;
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
                    Encrypted = EncryptText(plaintext, ref Key, ref Iv);

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
        private string EncryptText(string PlainText, ref string Key, ref string Iv)
        {
            AesManaged aesManaged = new AesManaged();
            try
            {
                byte[] bytestoEncrypt = Encoding.ASCII.GetBytes(PlainText);
                aesManaged.KeySize = KeyBitSize;
                aesManaged.Mode = CipherMode.ECB;
                aesManaged.Padding = PaddingMode.PKCS7;

                aesManaged.GenerateIV();
                aesManaged.GenerateKey();

                Iv = Convert.ToBase64String(aesManaged.IV);
                Key = Convert.ToBase64String(aesManaged.Key);

                ICryptoTransform cryptoTransform = aesManaged.CreateEncryptor(aesManaged.Key, aesManaged.IV);
                byte[] resultArray =
      cryptoTransform.TransformFinalBlock(bytestoEncrypt, 0,
      bytestoEncrypt.Length);

                return Convert.ToBase64String(resultArray);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
