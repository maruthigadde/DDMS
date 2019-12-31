using DDMS.WebService.Models;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace WebServiceClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = string.Empty;
            try
            {
                Console.WriteLine("Please select the operation\n 1.Upload\n 2.Search\n 3.Delete\n");
                input = Console.ReadLine();

                if (input.ToUpper() == "SEARCH" || input.ToUpper() == "UPLOAD" || input.ToUpper() == "DELETE")
                    DDMSAPI(input);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }

        }

        private static void DDMSAPI(string operation)
        {
            string path = string.Empty,
                   dealerNumber = string.Empty,
                   requestUser = string.Empty,
                   version = string.Empty;
            Guid DocumentID = Guid.Empty;
            switch (operation.ToUpper())
            {
                case "UPLOAD":
                    Console.WriteLine("Enter the DocumentID (Press enter without a value if it is a new upload) :");
                    Guid.TryParse(Console.ReadLine(), out DocumentID);
                    Console.WriteLine("Enter the File path for read :");
                    path = Console.ReadLine();
                    Console.WriteLine("Enter the DealerNumber :");
                    dealerNumber = Console.ReadLine();
                    Console.WriteLine("Enter the RequestUser :");
                    requestUser = Console.ReadLine();
                    Console.WriteLine("Uploading Document....");
                    UploadDocument(DocumentID, path, dealerNumber, requestUser);
                    break;
                case "SEARCH":
                    Console.WriteLine("Enter the DocumentID :");
                    Guid.TryParse(Console.ReadLine(), out DocumentID);
                    if (DocumentID != Guid.Empty)
                    {
                        Console.WriteLine("Enter version number(Press enter without a value for all versions metadata):");
                        version = Console.ReadLine();
                        Console.WriteLine("Retrieving document.....");
                        SearchDocument(DocumentID, version);
                    }
                    else
                        Console.WriteLine("Document Id is in valid");

                    break;
                case "DELETE":
                    Console.WriteLine("Enter the DocumentID :");
                    Guid.TryParse(Console.ReadLine(), out DocumentID);
                    if (DocumentID != Guid.Empty)
                    {
                        Console.WriteLine("Enter version number(Press enter to delete entire document):");
                        version = Console.ReadLine();
                        Console.WriteLine("Deleting Document....");
                        DeleteDocument(DocumentID, version);
                    }
                    else
                        Console.WriteLine("Document Id is in valid");

                    break;
            }
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


                HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(
                                                            ConfigurationManager.AppSettings.Get("ServiceURL") + "/api/DDMS");
                myHttpWebRequest.Headers.Add("Authorization", "Basic "
                                                              + Convert.ToBase64String(Encoding.Default.GetBytes(ConfigurationManager.AppSettings.Get("UserName")
                                                                                                                 + ":"
                                                                                                                 + ConfigurationManager.AppSettings.Get("Password"))));

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

            catch (WebException webex)
            {
                WebResponse errResp = webex.Response;
                using (Stream respStream = errResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream);
                    string text = reader.ReadToEnd();
                    Console.WriteLine("Response :" + text);
                }
            }
            Console.WriteLine("To continue, select: \n 1.Upload\n 2.Search\n 3.Delete\n");
            string input = Console.ReadLine();

            if (input.ToUpper() == "SEARCH" || input.ToUpper() == "UPLOAD" || input.ToUpper() == "DELETE")
                DDMSAPI(input);

        }

        private static void SearchDocument(Guid guid, string version)
        {
            HttpWebRequest myHttpWebRequest = null;
            try
            {
                if (string.IsNullOrEmpty(version) || string.IsNullOrWhiteSpace(version))
                    myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ConfigurationManager.AppSettings.Get("ServiceURL")
                                                                             + "/api/DDMS/"
                                                                             + guid);
                else
                    myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ConfigurationManager.AppSettings.Get("ServiceURL")
                                                                             + "/api/DDMS/"
                                                                             + guid
                                                                             + "/"
                                                                             + version
                                                                             + "/");
                myHttpWebRequest.Headers.Add("Authorization", "Basic "
                                                              + Convert.ToBase64String(Encoding.Default.GetBytes(ConfigurationManager.AppSettings.Get("UserName")
                                                                                                                 + ":"
                                                                                                                 + ConfigurationManager.AppSettings.Get("Password"))));
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
            catch (WebException webex)
            {
                WebResponse errResp = webex.Response;
                using (Stream respStream = errResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream);
                    string text = reader.ReadToEnd();
                    Console.WriteLine("Response :" + text);
                }
            }
            Console.WriteLine("To continue, select: \n 1.Upload\n 2.Search\n 3.Delete\n");
            string input = Console.ReadLine();

            if (input.ToUpper() == "SEARCH" || input.ToUpper() == "UPLOAD" || input.ToUpper() == "DELETE")
                DDMSAPI(input);
        }

        private static void DeleteDocument(Guid guid, string version)
        {
            HttpWebRequest myHttpWebRequest = null;
            try
            {
                if (string.IsNullOrEmpty(version) || string.IsNullOrWhiteSpace(version))
                    myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ConfigurationManager.AppSettings.Get("ServiceURL")
                                                                             + "/api/DDMS/"
                                                                             + guid);
                else
                    myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ConfigurationManager.AppSettings.Get("ServiceURL")
                                                                             + "/api/DDMS/"
                                                                             + guid
                                                                             + "/"
                                                                             + version
                                                                             + "/");
                myHttpWebRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(ConfigurationManager.AppSettings.Get("UserName")
                                                                                                                           + ":"
                                                                                                                           + ConfigurationManager.AppSettings.Get("Password"))));
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
            catch (WebException webex)
            {
                WebResponse errResp = webex.Response;
                using (Stream respStream = errResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream);
                    string text = reader.ReadToEnd();
                    Console.WriteLine("Response :" + text);
                }
            }
            Console.WriteLine("To continue, select: \n 1.Upload\n 2.Search\n 3.Delete\n");
            string input = Console.ReadLine();

            if (input.ToUpper() == "SEARCH" || input.ToUpper() == "UPLOAD" || input.ToUpper() == "DELETE")
                DDMSAPI(input);
        }
    }
}
