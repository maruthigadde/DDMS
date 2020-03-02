using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DeleteApp
{
    public class Program
    {
        public static DataTable dtUniqueId = new DataTable();
        public static DataTable dtResponse = new DataTable();
        public static string SourcePath = string.Empty;
        public static string UserName = string.Empty;
        public static string Password = string.Empty;
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Enter the credentials for webservice authentication :");
                Console.WriteLine(@"Username (Domain\Username) :");
                UserName = Console.ReadLine();
                Console.WriteLine("Password :");
                Password = Console.ReadLine();
                if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password))
                    ReadFile();
                else
                    Console.WriteLine("Please enter proper username and password");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :" + ex.Message);
            }
            Console.ReadLine();
        }

        public static void ReadFile()
        {
            try
            {
                Console.WriteLine("Enter the File path (.csv) for fetching UniqueId's :");
                SourcePath = Console.ReadLine();
                if (!string.IsNullOrEmpty(SourcePath))
                {
                    if (Path.GetExtension(SourcePath).ToUpper() == ".CSV")
                    {
                        dtUniqueId = ReadExcelCSV(SourcePath);
                        Console.WriteLine("File read successful.");
                        Delete();
                    }
                    else
                    {
                        Console.WriteLine("Invalid file extension");
                        ReadFile();
                    }
                }
                else
                {
                    Console.WriteLine("Invalid File path to read");
                    ReadFile();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :" + ex.Message);
            }
        }

        public static DataTable ReadExcelCSV(string fileName)
        {
            DataTable dtexcel = new DataTable();
            string[] csvData = null;
            try
            {
                csvData = File.ReadAllText(fileName).Split('\n');
                for (int i = 0; i < csvData.Length; i++)
                {
                    csvData[i] = csvData[i].Replace("\r", "");
                    if (!string.IsNullOrEmpty(csvData[i]))
                    {
                        string[] row = csvData[i].Split(',');
                        if (i != 0)
                            dtexcel.Rows.Add();
                        for (int j = 0; j < row.Length; j++)
                        {
                            if (i == 0)
                            {
                                dtexcel.Columns.Add(row[j]);
                            }
                            else
                            {
                                dtexcel.Rows[i - 1][j] = row[j];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dtexcel = null;
                Console.WriteLine("Error in File read :" + ex.Message);
            }
            return dtexcel;
        }

        public static void WriteToCSV(string fileName, DataTable dtDataTable)
        {
            try
            {
                StreamWriter sw = new StreamWriter(fileName, false);

                for (int i = 0; i < dtDataTable.Rows.Count; i++)
                {
                    for (int j = 0; j < dtDataTable.Columns.Count; j++)
                    {
                        dtDataTable.Rows[i][j] = dtDataTable.Rows[i][j].ToString().Replace("\n\r", "");
                        dtDataTable.Rows[i][j] = dtDataTable.Rows[i][j].ToString().Replace("\r\n", "");
                        dtDataTable.Rows[i][j] = dtDataTable.Rows[i][j].ToString().Replace(" ", "");
                    }
                }

                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    sw.Write(dtDataTable.Columns[i]);
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
                foreach (DataRow dr in dtDataTable.Rows)
                {
                    for (int i = 0; i < dtDataTable.Columns.Count; i++)
                    {
                        if (!Convert.IsDBNull(dr[i]))
                        {
                            sw.Write(dr[i].ToString());
                        }
                        if (i < dtDataTable.Columns.Count - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);
                }
                sw.Close();
                Console.WriteLine("All the responses are written to file path :" + fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in writing response to CSV :" + ex.Message, "Warning");
            }
        }

        private static void Delete()
        {
            string response = "";
            try
            {
                Console.WriteLine("Deleting files... ");
                if (dtUniqueId.Rows.Count > 0)
                {
                    for (int i = 0; i < dtUniqueId.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            dtResponse.Columns.Add("UniqueId");
                            dtResponse.Columns.Add("Response");
                        }
                        Console.WriteLine("Deleting Document - " + dtUniqueId.Rows[i]["OldUniqueId"]);
                        response = DeleteDocument(Guid.Parse(dtUniqueId.Rows[i]["OldUniqueId"].ToString()), "");

                        dtResponse.Rows.Add(dtUniqueId.Rows[i]["OldUniqueId"], response);
                    }
                    WriteToCSV(Path.GetDirectoryName(SourcePath) + "\\" +
                               Path.GetFileNameWithoutExtension(SourcePath) + "-Response" +
                               Path.GetExtension(SourcePath),
                               dtResponse);
                }
                else
                {
                    Console.WriteLine("No records to delete");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Delete :" + ex.Message);
            }
        }

        private static string DeleteDocument(Guid guid, string version)
        {
            HttpWebRequest httpWebRequest = null;
            var response = "";
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
                    + Convert.ToBase64String(Encoding.Default.GetBytes(UserName
                                                                       + ":"
                                                                       + Password)));
                httpWebRequest.Headers.Add(Constants.MessageId, Guid.NewGuid().ToString());
                httpWebRequest.Headers.Add(Constants.SiteId, HeaderValueConstants.SiteId);
                httpWebRequest.Headers.Add(Constants.BusinessId, HeaderValueConstants.BusinessId);
                httpWebRequest.Headers.Add(Constants.CollectedTimeStamp, DateTime.Now.ToString());

                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "DELETE";
                httpWebRequest.Timeout = 180000;
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertificates);

                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);
                string pageContent = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                responseStream.Close();

                response = JsonConvert.DeserializeObject(pageContent).ToString();
            }
            catch (WebException webex)
            {
                WebResponse errResp = webex.Response;
                using (Stream respStream = errResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream);
                    response = reader.ReadToEnd();
                }
            }
            return response;
        }

        private static bool AcceptAllCertificates(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
