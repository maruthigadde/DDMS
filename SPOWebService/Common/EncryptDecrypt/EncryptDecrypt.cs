using DDMS.WebService.Models.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SPOService.EncryptConfiguration
{
    [ExcludeFromCodeCoverage]
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
    }
}
