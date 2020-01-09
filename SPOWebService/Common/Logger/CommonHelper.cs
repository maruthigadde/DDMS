using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace SPOService.Helper
{
    [ExcludeFromCodeCoverage]
    public class CommonHelper
    {
        public static readonly int KeyBitSize = 256;
        public static string Decrypt(string Encryptedstring, string Key, string Iv)
        {
            AesManaged tdes = new AesManaged();
            byte[] bytestoDecrypt = null;
            try
            {
                bytestoDecrypt = Convert.FromBase64String(Encryptedstring);
                tdes.KeySize = KeyBitSize;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cryptoTransform = tdes.CreateDecryptor(Convert.FromBase64String(Key), Convert.FromBase64String(Iv));
                byte[] resultArray = cryptoTransform.TransformFinalBlock(bytestoDecrypt, 0, bytestoDecrypt.Length);

                return Encoding.ASCII.GetString(resultArray);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
