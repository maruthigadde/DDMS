using System;
using System.IO;
using System.Collections.Generic;
using DDMS.WebService.Models.Honda;
using DDMS.WebService.Models.Common;
using DDMS.WebService.ExternalServices.Interfaces;
using Microsoft.SharePoint.Client;
using System.Configuration;
using System.Security;
using System.Net;
using log4net;
using EncryptConfiguration;

namespace DDMS.WebService.SPOActions
{
    public class DDMSSearchDocument : IDDMSSearchDocument
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DDMSSearchDocument));
        #region Public Member Functions
        public SearchDocumentResponse DDMSSearch(SearchDocumentRequest searchDocumentRequest)
        {
            SearchDocumentResponse searchDocumentResponse = new SearchDocumentResponse();
            SecureString secureString = null;
            try
            {
                Log.Info("In DDMSSearch method");
                if (searchDocumentRequest.DocumentId != Guid.Empty)
                {
                    using (ClientContext clientContext = new ClientContext(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteURL)))
                    {
                        secureString = new NetworkCredential("", EncryptDecrypt.SPOPassword).SecurePassword;
                        clientContext.Credentials = new SharePointOnlineCredentials(EncryptDecrypt.SPOUserName, secureString);

                        if ((!string.IsNullOrEmpty(searchDocumentRequest.Version) && !string.IsNullOrWhiteSpace(searchDocumentRequest.Version)))
                            searchDocumentResponse = SearchDocument(clientContext, searchDocumentRequest);
                    }
                }
                else
                {
                    Log.Info("In DDMSSearch method DocumentId is empty");
                    searchDocumentResponse.ErrorMessage = string.Format(ErrorMessage.ValueEmpty, SpoConstants.DocumentId);
                }
                Log.Info("Out of DDMSSearch method");
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Exception in DDMSSearch method :{0}", e.Message);
                searchDocumentResponse.ErrorMessage = e.Message;
            }
            return searchDocumentResponse;
        }
        public List<SearchDocumentAllVersionsResponse> DDMSSearchAllOldVersions(SearchDocumentRequest searchDocumentRequest)
        {
            List<SearchDocumentAllVersionsResponse> listSearchDocumentResponse = new List<SearchDocumentAllVersionsResponse>();
            SecureString secureString = null;
            try
            {
                Log.Info("In DDMSSearchAllOldVersions method");
                using (ClientContext clientContext = new ClientContext(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteURL)))
                {
                    secureString = new NetworkCredential("", EncryptDecrypt.SPOPassword).SecurePassword;
                    clientContext.Credentials = new SharePointOnlineCredentials(EncryptDecrypt.SPOUserName, secureString);

                    Microsoft.SharePoint.Client.File file = clientContext.Web.GetFileById(searchDocumentRequest.DocumentId);
                    ListItem listItem = file.ListItemAllFields;
                    ListItemVersionCollection versions = listItem.Versions;
                    clientContext.Load(file);
                    clientContext.Load(listItem);
                    clientContext.Load(versions);
                    clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);
                    Log.Info("In DDMSSearchAllOldVersions method after ExecuteQueryWithRetry");
                    foreach (var version in versions)
                    {
                        SearchDocumentAllVersionsResponse searchDocumentResponse = new SearchDocumentAllVersionsResponse();
                        foreach (var item in version.FieldValues)
                        {
                            if (item.Key == SpoConstants.Title && item.Value != null)
                                searchDocumentResponse.DocumentName = item.Value.ToString();
                            if (item.Key == SpoConstants.DealerNumber && item.Value != null)
                                searchDocumentResponse.DealerNumber = item.Value.ToString();
                            if (item.Key == SpoConstants.RequestUser && item.Value != null)
                                searchDocumentResponse.RequestUser = item.Value.ToString();
                            if (item.Key == SpoConstants.DocumentumId && item.Value != null)
                                searchDocumentResponse.DocumentumId = item.Value.ToString();
                            if (item.Key == SpoConstants.DocumentumVersion && item.Value != null)
                                searchDocumentResponse.DocumentumVersion = item.Value.ToString();
                            if (item.Key == SpoConstants.Version && item.Value != null)
                                searchDocumentResponse.Version = item.Value.ToString();
                        }
                        listSearchDocumentResponse.Add(searchDocumentResponse);
                    }
                }
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Log.ErrorFormat("WebException in DDMSSearchAllOldVersions method :{0}", e.Message);
                throw new Exception(ErrorMessage.RemoteName);
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in DDMSSearchAllOldVersions method :{0}", ex.Message);
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                    throw new Exception(ErrorMessage.FileNotFound);
                else
                    throw new Exception(ex.Message);
            }
            catch (ArgumentException e)
            {
                Log.ErrorFormat("ArgumentException in DDMSSearchAllOldVersions method :{0}", e.Message);
                throw new Exception(e.Message);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in DDMSSearchAllOldVersions method :{0}", ex.Message);
                throw new Exception(ex.Message);
            }
            return listSearchDocumentResponse;
        }
        #endregion

        #region Private Member Functions
        private SearchDocumentResponse SearchDocument(ClientContext clientContext, SearchDocumentRequest searchDocumentRequest)
        {
            SearchDocumentResponse searchDocumentResponse = new SearchDocumentResponse();
            int requestedVersion = 0, currentResponseVersion = 0;
            try
            {
                Log.Info("In SearchDocument method before calling SearchDocumentCurrentVersion");
                searchDocumentResponse = SearchDocumentCurrentVersion(clientContext, searchDocumentRequest);
                Log.DebugFormat("In SearchDocument method after calling SearchDocumentCurrentVersion");
                if (string.IsNullOrEmpty(searchDocumentResponse.ErrorMessage))
                {
                    requestedVersion = GetVersionNumber(searchDocumentRequest.Version);
                    currentResponseVersion = GetVersionNumber(searchDocumentResponse.Version);

                    if (requestedVersion <= currentResponseVersion)
                    {
                        Log.Info("Before calling SearchDocumentByVersion method - requested version is less than major version");
                        if (requestedVersion != currentResponseVersion)
                            searchDocumentResponse = SearchDocumentByVersion(clientContext, searchDocumentRequest, requestedVersion);
                    }
                    else
                    {
                        searchDocumentResponse = null;
                        searchDocumentResponse = new SearchDocumentResponse() { ErrorMessage = ErrorMessage.GreaterVersionProvided };
                    }
                }
                Log.Info("Out SearchDocument method");
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Log.ErrorFormat("WebException in SearchDocument method :{0}", e.Message);
                searchDocumentResponse.ErrorMessage = ErrorMessage.RemoteName;
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in SearchDocument method :{0}", ex.Message);
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                    searchDocumentResponse.ErrorMessage = ErrorMessage.FileNotFound;
                else
                    searchDocumentResponse.ErrorMessage = ex.Message;
            }
            catch (ArgumentException e)
            {
                Log.ErrorFormat("ArgumentException in SearchDocument method :{0}", e.Message);
                searchDocumentResponse.ErrorMessage = e.Message;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in SearchDocument method :{0}", ex.Message);
                searchDocumentResponse.ErrorMessage = ex.Message;
            }
            return searchDocumentResponse;
        }
        private SearchDocumentResponse SearchDocumentByVersion(ClientContext clientContext, SearchDocumentRequest searchDocumentRequest, int requestedVersion)
        {
            SearchDocumentResponse searchDocumentResponse = new SearchDocumentResponse();
            try
            {
                Log.Info("In SearchDocumentByVersion method");
                List list = clientContext.Web.Lists.GetByTitle(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOFolder));
                ListItem listItem = list.GetItemByUniqueId(searchDocumentRequest.DocumentId);
                ListItemVersion listItemVersions = listItem.Versions.GetById(requestedVersion);
                FileVersion fileVersions = listItem.File.Versions.GetById(requestedVersion);
                ClientResult<Stream> clientResult = fileVersions.OpenBinaryStream();
                clientContext.Load(list);
                clientContext.Load(listItem);
                clientContext.Load(listItemVersions);
                clientContext.Load(fileVersions);
                clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);
                using (MemoryStream mStream = new MemoryStream())
                {
                    if (clientResult != null)
                    {
                        clientResult.Value.CopyTo(mStream);
                        searchDocumentResponse.DocumentContent = mStream.ToArray();

                        foreach (var item in listItemVersions.FieldValues)
                        {
                            if (item.Key == SpoConstants.Title && item.Value != null)
                                searchDocumentResponse.DocumentName = item.Value.ToString();
                            if (item.Key == SpoConstants.DealerNumber && item.Value != null)
                                searchDocumentResponse.DealerNumber = item.Value.ToString();
                            if (item.Key == SpoConstants.RequestUser && item.Value != null)
                                searchDocumentResponse.RequestUser = item.Value.ToString();
                            if (item.Key == SpoConstants.DocumentumId && item.Value != null)
                                searchDocumentResponse.DocumentumId = item.Value.ToString();
                            if (item.Key == SpoConstants.DocumentumVersion && item.Value != null)
                                searchDocumentResponse.DocumentumVersion = item.Value.ToString();
                            if (item.Key == SpoConstants.Version && item.Value != null)
                                searchDocumentResponse.Version = item.Value.ToString();
                        }
                    }
                }
                Log.Info("Out of SearchDocumentByVersion method");
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Log.ErrorFormat("WebException in SearchDocumentByVersion method :{0}", e.Message);
                searchDocumentResponse.ErrorMessage = ErrorMessage.RemoteName;
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in SearchDocumentByVersion method :{0}", ex.Message);
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                    searchDocumentResponse.ErrorMessage = ErrorMessage.FileNotFound;
                else
                    searchDocumentResponse.ErrorMessage = ex.Message;
            }
            catch (ArgumentException e)
            {
                Log.ErrorFormat("ArgumentException in SearchDocumentByVersion method :{0}", e.Message);
                searchDocumentResponse.ErrorMessage = e.Message;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in SearchDocumentByVersion method :{0}", ex.Message);
                searchDocumentResponse.ErrorMessage = ex.Message;
            }
            return searchDocumentResponse;

        }
        private SearchDocumentResponse SearchDocumentCurrentVersion(ClientContext clientContext, SearchDocumentRequest searchDocumentRequest)
        {
            SearchDocumentResponse searchDocumentResponse = new SearchDocumentResponse();
            try
            {
                Log.Info("In SearchDocumentCurrentVersion method");
                Microsoft.SharePoint.Client.File file = clientContext.Web.GetFileById(searchDocumentRequest.DocumentId);
                ListItem oListItem = file.ListItemAllFields;
                clientContext.Load(file);
                clientContext.Load(oListItem);
                ClientResult<Stream> clientResult = file.OpenBinaryStream();
                clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);
                using (MemoryStream mStream = new MemoryStream())
                {
                    if (clientResult != null)
                    {
                        clientResult.Value.CopyTo(mStream);
                        searchDocumentResponse.DocumentContent = mStream.ToArray();
                        searchDocumentResponse.DocumentName = file.Name.ToString();

                        foreach (var item in oListItem.FieldValues)
                        {
                            if (item.Key == SpoConstants.Title && item.Value != null)
                                searchDocumentResponse.DocumentName = item.Value.ToString();
                            if (item.Key == SpoConstants.DealerNumber && item.Value != null)
                                searchDocumentResponse.DealerNumber = item.Value.ToString();
                            if (item.Key == SpoConstants.RequestUser && item.Value != null)
                                searchDocumentResponse.RequestUser = item.Value.ToString();
                            if (item.Key == SpoConstants.DocumentumId && item.Value != null)
                                searchDocumentResponse.DocumentumId = item.Value.ToString();
                            if (item.Key == SpoConstants.DocumentumVersion && item.Value != null)
                                searchDocumentResponse.DocumentumVersion = item.Value.ToString();
                            if (item.Key == SpoConstants.Version && item.Value != null)
                                searchDocumentResponse.Version = item.Value.ToString();
                        }
                    }
                }
                Log.Info("Out of SearchDocumentCurrentVersion method");
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Log.ErrorFormat("WebException in SearchDocumentCurrentVersion method :{0}", e.Message);
                searchDocumentResponse.ErrorMessage = ErrorMessage.RemoteName;
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in SearchDocumentCurrentVersion method :{0}", ex.Message);
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                    searchDocumentResponse.ErrorMessage = ErrorMessage.FileNotFound;
                else
                    searchDocumentResponse.ErrorMessage = ex.Message;
            }
            catch (ArgumentException e)
            {
                Log.ErrorFormat("ArgumentException in SearchDocumentCurrentVersion method :{0}", e.Message);
                searchDocumentResponse.ErrorMessage = e.Message;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in SearchDocumentCurrentVersion method :{0}", ex.Message);
                searchDocumentResponse.ErrorMessage = ex.Message;
            }
            return searchDocumentResponse;

        }
        private int GetVersionNumber(string version)
        {
            string[] versions = null;
            int returnvalue = 0;
            try
            {
                Log.Info("In GetVersionNumber method");
                versions = version.Split('.');
                if (versions.Length == 1)
                    returnvalue = (Convert.ToInt32(versions[0]) * 512);
                if (versions.Length == 2)
                    returnvalue = ((Convert.ToInt32(versions[0]) * 512) + (Convert.ToInt32(versions[1]) * 1));
                Log.DebugFormat("Out GetVersionNumber method - Version number :{0}", returnvalue);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Exception in GetVersionNumber method :{0}", e.Message);
            }
            return returnvalue;
        }
        #endregion
    }
}
