using DDMS.WebService.Models.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EncryptConfiguration
{
    public class EncryptDecrypt
    {
        public static readonly int KeyBitSize = 256;
        //public static string SPOSiteID = "";
        public static string SPOUserName = "";
        public static string SPOPassword = "";

        public static void Initialize()
        {
            //SPOSiteID = Decrypt(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteId), ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteIdKey), ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteIdIv));
            SPOUserName = Decrypt(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserName), ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserNameKey), ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserNameIv));
            SPOPassword = Decrypt(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPassword), ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPasswordKey), ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPasswordIv));
        }
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

        public static string Encrypt(string PlainText, ref string Key, ref string Iv)
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
