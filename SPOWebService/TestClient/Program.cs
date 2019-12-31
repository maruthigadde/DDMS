using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DDMS.WebService;
using DDMS.WebService.SPOActions;
using System.Security.Cryptography;
using System.Configuration;
using log4net;
using log4net.Repository.Hierarchy;
using log4net.Layout;
using log4net.Appender;
using log4net.Core;
using System.Net;
using Newtonsoft.Json;


namespace TestClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            string operation = "", path = "", dealerNumber = "", requestUser = "", version = "", plaintext = "", Key = "", Iv = "", Encrypted = "";
            Guid DocumentID = Guid.Empty;
            try
            {
                Console.WriteLine("Please select the operation\r\n 1.Upload\r\n 2.Search\r\n 3.Delete");
                operation = Console.ReadLine();
                switch (operation.ToUpper())
                {
                    case "UPLOAD":
                        Console.WriteLine("\r\n Enter the DocumentID :");
                        Guid.TryParse(Console.ReadLine(), out DocumentID);
                        Console.WriteLine("\r\n Enter the File path for read :");
                        path = Console.ReadLine();
                        Console.WriteLine("\r\n Enter the DealerNumber :");
                        dealerNumber = Console.ReadLine();
                        Console.WriteLine("\r\n Enter the RequestUser :");
                        requestUser = Console.ReadLine();
                        UploadDocument(DocumentID, path, dealerNumber, requestUser);
                        break;
                    case "SEARCH":
                        Console.WriteLine("\r\n Enter the DocumentID :");
                        Guid.TryParse(Console.ReadLine(), out DocumentID);
                        if (DocumentID != Guid.Empty)
                        {
                            Console.WriteLine("\r\n Enter version number :");
                            version = Console.ReadLine();
                            SearchDocument(DocumentID, version);
                        }
                        else
                            Console.WriteLine("\r\n Document Id is in valid");

                        break;
                    case "DELETE":
                        Console.WriteLine("\r\n Enter the DocumentID :");
                        Guid.TryParse(Console.ReadLine(), out DocumentID);
                        if (DocumentID != Guid.Empty)
                        {
                            Console.WriteLine("\r\n Enter version number :");
                            version = Console.ReadLine();
                            DeleteDocument(DocumentID, version);
                        }
                        else
                            Console.WriteLine("\r\n Document Id is in valid");

                        break;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();

            //try
            //{
            //    Encrypted = EncryptDecrypt.Encrypt(plaintext, ref Key, ref Iv);
            //    Console.WriteLine("Encrypted Text :" + Encrypted);
            //    Console.WriteLine("Encryption Key :" + Key);
            //    Console.WriteLine("Encryption Iv :" + Iv);
            //    Console.ReadLine();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Error in Encrypt :" + ex.Message);
            //}
        }

        private static void UploadDocument(Guid documentId, string filepath, string dealerNumber, string requestUser)
        {
            FileStream fileStream = null;
            Byte[] fileContent = null;
            UploadDocumentResponse hondaUploadDocumentResponse = null;
            try
            {
                //string docPath = @"C:\Users\manjunathyadav.k\Desktop\Test.txt";
                fileStream = File.OpenRead(filepath);
                fileContent = new byte[Convert.ToInt32(fileStream.Length)];
                fileStream.Read(fileContent, 0, Convert.ToInt32(fileStream.Length));

                UploadDocumentRequest objHondaUpload = new UploadDocumentRequest
                {
                    DocumentId = documentId,
                    DocumentContent = fileContent,
                    DocumentName = Path.GetFileName(filepath),
                    DealerNumber = dealerNumber,
                    RequestUser = requestUser
                };

                HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ConfigurationManager.AppSettings.Get("ApiURL") + "/api/DDMS");
                myHttpWebRequest.Headers.Add("messageId", Guid.NewGuid().ToString());
                myHttpWebRequest.Headers.Add("siteId", "DDMS");
                myHttpWebRequest.Headers.Add("businessId", "DDMS Documents");
                myHttpWebRequest.Headers.Add("collectedTimestamp", DateTime.Now.ToString());
                myHttpWebRequest.ContentType = "application/json";
                myHttpWebRequest.Method = "POST";
                myHttpWebRequest.Timeout = 3000000;
                string sMessage = JsonConvert.SerializeObject(objHondaUpload);
                using (var streamWriter = new StreamWriter(myHttpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(sMessage);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                Stream responseStream = myHttpWebResponse.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);
                string pageContent = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                responseStream.Close();
                hondaUploadDocumentResponse = JsonConvert.DeserializeObject<UploadDocumentResponse>(pageContent);

                var response = JsonConvert.DeserializeObject(pageContent);
                Console.WriteLine("StatusCode :" + myHttpWebResponse.StatusCode);
                Console.WriteLine("Response :" + response);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Upload :" + e.Message);
            }
        }

        private static void SearchDocument(Guid guid, string version)
        {
            HttpWebRequest myHttpWebRequest = null;
            try
            {
                if (string.IsNullOrEmpty(version) || string.IsNullOrWhiteSpace(version))
                    myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ConfigurationManager.AppSettings.Get("ApiURL") + "/api/DDMS/" + guid);
                else
                    myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ConfigurationManager.AppSettings.Get("ApiURL") + "/api/DDMS/" + guid + "/" + version + "/");
                myHttpWebRequest.Headers.Add("messageId", Guid.NewGuid().ToString());
                myHttpWebRequest.Headers.Add("siteId", "DDMS");
                myHttpWebRequest.Headers.Add("businessId", "DDMS Documents");
                myHttpWebRequest.Headers.Add("collectedTimestamp", DateTime.Now.ToString());
                myHttpWebRequest.ContentType = "application/json";
                myHttpWebRequest.Method = "GET";
                myHttpWebRequest.Timeout = 3000000;

                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                Stream responseStream = myHttpWebResponse.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);
                string pageContent = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                responseStream.Close();

                var response = JsonConvert.DeserializeObject(pageContent);
                Console.WriteLine("Response :" + response);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Search :" + e.Message);
            }
        }

        private static void DeleteDocument(Guid guid, string version)
        {
            HttpWebRequest myHttpWebRequest = null;
            try
            {
                if (string.IsNullOrEmpty(version) || string.IsNullOrWhiteSpace(version))
                    myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ConfigurationManager.AppSettings.Get("ApiURL") + "/api/DDMS/" + guid);
                else
                    myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ConfigurationManager.AppSettings.Get("ApiURL") + "/api/DDMS/" + guid + "/" + version + "/");
                myHttpWebRequest.Headers.Add("messageId", Guid.NewGuid().ToString());
                myHttpWebRequest.Headers.Add("siteId", "DDMS");
                myHttpWebRequest.Headers.Add("businessId", "DDMS Documents");
                myHttpWebRequest.Headers.Add("collectedTimestamp", DateTime.Now.ToString());

                myHttpWebRequest.ContentType = "application/json";
                myHttpWebRequest.Method = "DELETE";
                myHttpWebRequest.Timeout = 3000000;

                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                Stream responseStream = myHttpWebResponse.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);
                string pageContent = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                responseStream.Close();

                var response = JsonConvert.DeserializeObject(pageContent);
                Console.WriteLine("Response :" + response);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Delete :" + e.Message);
            }
        }
    }
}
