using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using WebServiceClient.Models;

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
            Byte[] fileContent = null;
            try
            {
                using (FileStream fileStream = File.OpenRead(filepath))
                {
                    fileContent = new byte[Convert.ToInt32(fileStream.Length)];
                    fileStream.Read(fileContent, 0, Convert.ToInt32(fileStream.Length));
                }

                UploadDocumentRequest uploadRequest = new UploadDocumentRequest
                {
                    DocumentId = documentId,
                    DocumentContent = fileContent,
                    DocumentName = Path.GetFileName(filepath),
                    DealerNumber = dealerNumber,
                    RequestUser = requestUser
                };


                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(
                                                            ConfigurationManager.AppSettings.Get("ServiceURL"));
                httpWebRequest.Headers.Add(Constants.Authorization,
                    Constants.AuthenticationType
                    + Convert.ToBase64String(Encoding.Default.GetBytes(ConfigurationManager.AppSettings.Get("UserName")
                                                                       + ":"
                                                                       + ConfigurationManager.AppSettings.Get("Password"))));

                httpWebRequest.Headers.Add(Constants.MessageId, Guid.NewGuid().ToString());
                httpWebRequest.Headers.Add(Constants.SiteId, HeaderValueConstants.SiteId);
                httpWebRequest.Headers.Add(Constants.BusinessId, HeaderValueConstants.BusinessId);
                httpWebRequest.Headers.Add(Constants.CollectedTimeStamp, DateTime.Now.ToString());
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 180000;
                string sMessage = JsonConvert.SerializeObject(uploadRequest);
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(sMessage);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);
                string pageContent = myStreamReader.ReadToEnd();

                myStreamReader.Close();
                responseStream.Close();
                //Deserialize response content
                var response = JsonConvert.DeserializeObject(pageContent);
                Console.WriteLine("StatusCode :" + httpWebResponse.StatusCode);
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
            HttpWebRequest httpWebRequest = null;
            try
            {
                if (string.IsNullOrEmpty(version) || string.IsNullOrWhiteSpace(version))
                    httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ConfigurationManager.AppSettings.Get("ServiceURL")
                                                                                                             + guid);
                else
                    httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ConfigurationManager.AppSettings.Get("ServiceURL")
                                                                             + guid
                                                                             + "/"
                                                                             + version
                                                                             + "/");
                httpWebRequest.Headers.Add(Constants.Authorization,
                    Constants.AuthenticationType
                    + Convert.ToBase64String(Encoding.Default.GetBytes(ConfigurationManager.AppSettings.Get("UserName")
                                                                       + ":"
                                                                       + ConfigurationManager.AppSettings.Get("Password"))));
                httpWebRequest.Headers.Add(Constants.MessageId, Guid.NewGuid().ToString());
                httpWebRequest.Headers.Add(Constants.SiteId, HeaderValueConstants.SiteId);
                httpWebRequest.Headers.Add(Constants.BusinessId, HeaderValueConstants.BusinessId);
                httpWebRequest.Headers.Add(Constants.CollectedTimeStamp, DateTime.Now.ToString());
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";
                httpWebRequest.Timeout = 180000;

                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);
                string pageContent = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                responseStream.Close();

                Console.WriteLine("StatusCode :" + httpWebResponse.StatusCode);
                var response = JObject.Parse(pageContent);

                string downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                                                                            + ConfigurationManager.AppSettings.Get("DownloadPath"));

                if (!Directory.Exists(downloadPath))
                    Directory.CreateDirectory(downloadPath);
                if (response.ContainsKey("documentContent"))
                    if (!string.IsNullOrEmpty((string)response["documentContent"]))
                    {
                        File.WriteAllBytes(downloadPath + "\\" + response["documentName"], Convert.FromBase64String(response["documentContent"].ToString()));
                        Console.WriteLine("File Downloaded to Path :" + downloadPath + "\\" + response["documentName"]);
                    }

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
            HttpWebRequest httpWebRequest = null;
            try
            {
                if (string.IsNullOrEmpty(version) || string.IsNullOrWhiteSpace(version))
                    httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ConfigurationManager.AppSettings.Get("ServiceURL")
                                                                             + guid);
                else
                    httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(ConfigurationManager.AppSettings.Get("ServiceURL")
                                                                             + guid
                                                                             + "/"
                                                                             + version
                                                                             + "/");
                httpWebRequest.Headers.Add(Constants.Authorization,
                    Constants.AuthenticationType
                    + Convert.ToBase64String(Encoding.Default.GetBytes(ConfigurationManager.AppSettings.Get("UserName")
                                                                       + ":"
                                                                       + ConfigurationManager.AppSettings.Get("Password"))));
                httpWebRequest.Headers.Add(Constants.MessageId, Guid.NewGuid().ToString());
                httpWebRequest.Headers.Add(Constants.SiteId, HeaderValueConstants.SiteId);
                httpWebRequest.Headers.Add(Constants.BusinessId, HeaderValueConstants.BusinessId);
                httpWebRequest.Headers.Add(Constants.CollectedTimeStamp, DateTime.Now.ToString());

                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "DELETE";
                httpWebRequest.Timeout = 180000;

                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);
                string pageContent = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                responseStream.Close();

                Console.WriteLine("StatusCode :" + httpWebResponse.StatusCode);
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
