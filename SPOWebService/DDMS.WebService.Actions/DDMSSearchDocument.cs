using DDMS.WebService.Constants;
using DDMS.WebService.ExternalServices.Interfaces;
using DDMS.WebService.Models;
using SPOService.Helper;
using log4net;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security;

namespace DDMS.WebService.SPOActions
{
    public class DDMSSearchDocument : IDDMSSearchDocument
    {
        //Get the logger for the DDMSSearchDocument type specified
        private static readonly ILog Log = log4net.LogManager.GetLogger(typeof(DDMSSearchDocument));

        #region Public Member Functions
        /// <summary>
        /// Method to Search a document
        /// </summary>
        /// <param name="searchDocumentRequest">Request model for Search operation</param>
        /// <param name="LoggerId">Message Id used for logging information</param>
        /// <returns></returns>
        public SearchDocumentResponse DDMSSearch(SearchDocumentRequest searchDocumentRequest, string LoggerId)
        {
            SearchDocumentResponse searchDocumentResponse = new SearchDocumentResponse();
            SecureString secureString = null;
            try
            {
                Log.DebugFormat("In DDMSSearch method for MessageId - {0}", LoggerId);
                if (searchDocumentRequest.DocumentId != Guid.Empty)
                {
                    using (ClientContext clientContext = new ClientContext(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteURL)))
                    {
                        //Get SPO Credentials
                        secureString = new NetworkCredential("", CommonHelper.Decrypt(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPassword),
                               ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPasswordKey),
                               ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPasswordIv))).SecurePassword;
                        //Decrypt the user name and password information
                        String username = CommonHelper.Decrypt(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserName),
                                    ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserNameKey),
                                    ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserNameIv));

                        clientContext.Credentials = new SharePointOnlineCredentials(username, secureString);

                        if ((!string.IsNullOrEmpty(searchDocumentRequest.Version) && !string.IsNullOrWhiteSpace(searchDocumentRequest.Version)))
                            //if version number is passed in request object
                            searchDocumentResponse = SearchDocument(clientContext, searchDocumentRequest, LoggerId);
                    }
                }
                else
                {
                    Log.DebugFormat("In DDMSSearch method DocumentId is empty for MessageId - {0}", LoggerId);
                    searchDocumentResponse.ErrorMessage = string.Format(ErrorMessage.ValueEmpty, SpoConstants.DocumentId);
                }
                Log.DebugFormat("Out of DDMSSearch method for MessageId - {0}", LoggerId);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Exception in DDMSSearch method for MessageId - {0} :{1}", LoggerId, e.Message);
                searchDocumentResponse.ErrorMessage = e.Message;
            }
            return searchDocumentResponse;
        }
        /// <summary>
        /// Method to fetch all the versions metadata
        /// </summary>
        /// <param name="searchDocumentRequest">Request model for Search Operation</param>
        /// <param name="LoggerId">Message Id used for logging information</param>
        /// <returns></returns>
        public SearchDocumentAllMetaDataVersions DDMSSearchAllOldVersions(SearchDocumentRequest searchDocumentRequest, string LoggerId)
        {
            SearchDocumentAllMetaDataVersions searchDocumentAllMetaDataVersions = new SearchDocumentAllMetaDataVersions();
            //create a list of search response
            List<SearchDocumentAllVersionsResponse> listSearchDocumentResponse = new List<SearchDocumentAllVersionsResponse>();
            SecureString secureString = null;
            try
            {
                Log.DebugFormat("In DDMSSearchAllOldVersions method for MessageId - {0}", LoggerId);
                using (ClientContext clientContext = new ClientContext(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteURL)))
                {
                    //Get SPO credentials
                    secureString = new NetworkCredential("", CommonHelper.Decrypt(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPassword),
                               ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPasswordKey),
                               ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPasswordIv))).SecurePassword;

                    //Decrypt the user name and password information
                    String username = CommonHelper.Decrypt(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserName),
                                ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserNameKey),
                                ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserNameIv));

                    clientContext.Credentials = new SharePointOnlineCredentials(username, secureString);

                    //retrieve file based on DocumentId
                    Microsoft.SharePoint.Client.File file = clientContext.Web.GetFileById(searchDocumentRequest.DocumentId);

                    //load list item fields
                    ListItem listItem = file.ListItemAllFields;

                    //load all the list item versions
                    ListItemVersionCollection versions = listItem.Versions;
                    clientContext.Load(file);

                    //fetch the specific metadata
                    clientContext.Load(listItem, item => item[SpoConstants.Name],
                                             item => item[SpoConstants.DealerNumber],
                                             item => item[SpoConstants.RequestUser],
                                             item => item[SpoConstants.Version]);
                    clientContext.Load(versions);
                    clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);
                    Log.DebugFormat("In DDMSSearchAllOldVersions method after ExecuteQueryWithRetry for MessageId - {0}", LoggerId);
                    //loop through all versions and fetch metadata
                    foreach (var version in versions)
                    {
                        //assign the metadata of each version to search document response model
                        SearchDocumentAllVersionsResponse searchDocumentResponse = new SearchDocumentAllVersionsResponse();
                        foreach (var item in version.FieldValues)
                        {
                            if (item.Key == SpoConstants.Name && item.Value != null)
                                searchDocumentResponse.DocumentName = item.Value.ToString();
                            if (item.Key == SpoConstants.DealerNumber && item.Value != null)
                                searchDocumentResponse.DealerNumber = item.Value.ToString();
                            if (item.Key == SpoConstants.RequestUser && item.Value != null)
                                searchDocumentResponse.RequestUser = item.Value.ToString();
                            if (item.Key == SpoConstants.Version && item.Value != null)
                                searchDocumentResponse.Version = item.Value.ToString();
                        }
                        listSearchDocumentResponse.Add(searchDocumentResponse);
                    }
                }
                Log.DebugFormat("Out of DDMSSearchAllOldVersions method for MessageId - {0}", LoggerId);
                searchDocumentAllMetaDataVersions.SearchMetadata = listSearchDocumentResponse;
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Log.ErrorFormat("WebException in DDMSSearchAllOldVersions method for MessageId - {0} :{1}", LoggerId, e.Message);
                searchDocumentAllMetaDataVersions.ErrorMessage = ErrorMessage.RemoteName;
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in DDMSSearchAllOldVersions method for MessageId - {0} :{1}", LoggerId, ex.Message);
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                    searchDocumentAllMetaDataVersions.ErrorMessage = ErrorMessage.FileNotFound;
                else
                    searchDocumentAllMetaDataVersions.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in DDMSSearchAllOldVersions method for MessageId - {0} :{1}", LoggerId, ex.Message);
                searchDocumentAllMetaDataVersions.ErrorMessage = ex.Message;
            }
            return searchDocumentAllMetaDataVersions;
        }
        #endregion

        #region Private Member Functions
        /// <summary>
        /// Method to retrieve a document based on specific version
        /// </summary>
        /// <param name="clientContext">SPO Client Context</param>
        /// <param name="searchDocumentRequest">Request Model for Search Operation</param>
        /// <param name="LoggerId">Message Id used for logging information</param>
        /// <returns></returns>
        private SearchDocumentResponse SearchDocument(ClientContext clientContext, SearchDocumentRequest searchDocumentRequest, string LoggerId)
        {
            SearchDocumentResponse searchDocumentResponse = new SearchDocumentResponse();
            int requestedVersion = 0, currentResponseVersion = 0;
            try
            {
                Log.DebugFormat("In SearchDocument method before calling SearchDocumentCurrentVersion for MessageId - {0}", LoggerId);
                //Check if version passed in request is latest published version
                searchDocumentResponse = SearchDocumentCurrentVersion(clientContext, searchDocumentRequest, LoggerId);
                Log.DebugFormat("In SearchDocument method after calling SearchDocumentCurrentVersion for MessageId - {0}", LoggerId);
                if (string.IsNullOrEmpty(searchDocumentResponse.ErrorMessage))
                {
                    //Get the version in SPO format from request
                    requestedVersion = GetVersionNumber(searchDocumentRequest.Version, LoggerId);
                    //Get the version in SPO format from latest published document
                    currentResponseVersion = GetVersionNumber(searchDocumentResponse.Version, LoggerId);

                    if (requestedVersion <= currentResponseVersion)
                    {
                        Log.DebugFormat("Before calling SearchDocumentByVersion method - requested version is less than major version for MessageId - {0}", LoggerId);
                        //if the version passed in request is not the latest published version
                        if (requestedVersion != currentResponseVersion)
                            //Get the previous version document content
                            searchDocumentResponse = SearchDocumentByVersion(clientContext, searchDocumentRequest, requestedVersion, LoggerId);
                    }
                    else
                    {
                        searchDocumentResponse = null;
                        searchDocumentResponse = new SearchDocumentResponse() { ErrorMessage = ErrorMessage.GreaterVersionProvided };
                    }
                }
                Log.DebugFormat("Out SearchDocument method for MessageId - {0}", LoggerId);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in SearchDocument method for MessageId - {0} :{1}", LoggerId, ex.Message);
                searchDocumentResponse.ErrorMessage = ex.Message;
            }
            return searchDocumentResponse;
        }
        /// <summary>
        /// Method to retrieve the old version document content
        /// </summary>
        /// <param name="clientContext">SPO Client Context</param>
        /// <param name="searchDocumentRequest">Request model for search operation</param>
        /// <param name="requestedVersion">Specific version number to be fetched from SPO</param>
        /// <param name="LoggerId">Message Id used for logging information</param>
        /// <returns></returns>
        private SearchDocumentResponse SearchDocumentByVersion(ClientContext clientContext, SearchDocumentRequest searchDocumentRequest, int requestedVersion, string LoggerId)
        {
            SearchDocumentResponse searchDocumentResponse = new SearchDocumentResponse();
            try
            {
                Log.DebugFormat("In SearchDocumentByVersion method for MessageId - {0}", LoggerId);
                //Retrieve file based on DocumentId
                Microsoft.SharePoint.Client.File file = clientContext.Web.GetFileById(searchDocumentRequest.DocumentId);
                //Load list item version to fetch the previous version metadata
                ListItemVersion listItemVersions = file.ListItemAllFields.Versions.GetById(requestedVersion);
                //Get the content of specific file version
                FileVersion fileVersions = file.Versions.GetById(requestedVersion);
                ClientResult<Stream> clientResult = fileVersions.OpenBinaryStream();
                clientContext.Load(file);
                //Load specific metadata of the file
                clientContext.Load(listItemVersions, item => item[SpoConstants.Name],
                                                     item => item[SpoConstants.DealerNumber],
                                                     item => item[SpoConstants.RequestUser],
                                                     item => item[SpoConstants.Version]);
                clientContext.Load(fileVersions);
                clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount,
                                                    ExecuteQueryConstants.RetryDelayTime);
                Log.DebugFormat("In SearchDocumentByVersion method after ExecuteQueryWithRetry method for MessageId - {0}", LoggerId);
                using (MemoryStream mStream = new MemoryStream())
                {
                    if (clientResult != null)
                    {
                        clientResult.Value.CopyTo(mStream);
                        //Read the file content and assign to response model
                        searchDocumentResponse.DocumentContent = mStream.ToArray();

                        foreach (var item in listItemVersions.FieldValues)
                        {
                            if (item.Key == SpoConstants.Name && item.Value != null)
                                searchDocumentResponse.DocumentName = item.Value.ToString();
                            if (item.Key == SpoConstants.DealerNumber && item.Value != null)
                                searchDocumentResponse.DealerNumber = item.Value.ToString();
                            if (item.Key == SpoConstants.RequestUser && item.Value != null)
                                searchDocumentResponse.RequestUser = item.Value.ToString();
                            if (item.Key == SpoConstants.Version && item.Value != null)
                                searchDocumentResponse.Version = item.Value.ToString();
                        }
                    }
                }
                Log.DebugFormat("Out of SearchDocumentByVersion method for MessageId - {0}", LoggerId);
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Log.ErrorFormat("WebException in SearchDocumentByVersion method for MessageId - {0} :{1}", LoggerId, e.Message);
                searchDocumentResponse.ErrorMessage = ErrorMessage.RemoteName;
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in SearchDocumentByVersion method for MessageId - {0} :{1}", LoggerId, ex.Message);
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                    searchDocumentResponse.ErrorMessage = ErrorMessage.FileNotFound;
                else
                    searchDocumentResponse.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in SearchDocumentByVersion method for MessageId - {0} :{1}", LoggerId, ex.Message);
                searchDocumentResponse.ErrorMessage = ex.Message;
            }
            return searchDocumentResponse;

        }
        /// <summary>
        /// Method to retrieve document if it is latest published version
        /// </summary>
        /// <param name="clientContext">SPO Client Context</param>
        /// <param name="searchDocumentRequest">Request model for Search Operation</param>
        /// <param name="LoggerId">Message Id used for logging information</param>
        /// <returns></returns>
        private SearchDocumentResponse SearchDocumentCurrentVersion(ClientContext clientContext, SearchDocumentRequest searchDocumentRequest, string LoggerId)
        {
            SearchDocumentResponse searchDocumentResponse = new SearchDocumentResponse();
            try
            {
                Log.DebugFormat("In SearchDocumentCurrentVersion method for MessageId - {0}", LoggerId);
                //Retrieve file based on documentid
                Microsoft.SharePoint.Client.File file = clientContext.Web.GetFileById(searchDocumentRequest.DocumentId);
                //Fetch the list item fields
                ListItem oListItem = file.ListItemAllFields;
                clientContext.Load(file);
                //Load the specific metadata of the file
                clientContext.Load(oListItem, item => item[SpoConstants.Name],
                                             item => item[SpoConstants.DealerNumber],
                                             item => item[SpoConstants.RequestUser],
                                             item => item[SpoConstants.Version]);
                ClientResult<Stream> clientResult = file.OpenBinaryStream();
                clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);
                Log.DebugFormat("In SearchDocumentCurrentVersion after ExecuteQueryWithRetry method for MessageId - {0}", LoggerId);
                using (MemoryStream mStream = new MemoryStream())
                {
                    if (clientResult != null)
                    {
                        clientResult.Value.CopyTo(mStream);
                        //Read the file content and assign to response model
                        searchDocumentResponse.DocumentContent = mStream.ToArray();
                        searchDocumentResponse.DocumentName = file.Name.ToString();

                        //Add file metadata in search response
                        foreach (var item in oListItem.FieldValues)
                        {
                            if (item.Key == SpoConstants.Name && item.Value != null)
                                searchDocumentResponse.DocumentName = item.Value.ToString();
                            if (item.Key == SpoConstants.DealerNumber && item.Value != null)
                                searchDocumentResponse.DealerNumber = item.Value.ToString();
                            if (item.Key == SpoConstants.RequestUser && item.Value != null)
                                searchDocumentResponse.RequestUser = item.Value.ToString();
                            if (item.Key == SpoConstants.Version && item.Value != null)
                                searchDocumentResponse.Version = item.Value.ToString();
                        }
                    }
                }
                Log.DebugFormat("Out of SearchDocumentCurrentVersion method for MessageId - {0}", LoggerId);
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Log.ErrorFormat("WebException in SearchDocumentCurrentVersion method for MessageId - {0} :{1}", LoggerId, e.Message);
                searchDocumentResponse.ErrorMessage = ErrorMessage.RemoteName;
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in SearchDocumentCurrentVersion method for MessageId - {0} :{1}", LoggerId, ex.Message);
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                    searchDocumentResponse.ErrorMessage = ErrorMessage.FileNotFound;
                else
                    searchDocumentResponse.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in SearchDocumentCurrentVersion method for MessageId - {0} :{1}", LoggerId, ex.Message);
                searchDocumentResponse.ErrorMessage = ex.Message;
            }
            return searchDocumentResponse;

        }
        /// <summary>
        /// Method to convert version number into SPO format
        /// </summary>
        /// <param name="version">Version to convert</param>
        /// <param name="LoggerId">MessageId used for logging information</param>
        /// <returns></returns>
        private int GetVersionNumber(string version, string LoggerId)
        {
            string[] versions = null;
            int returnvalue = 0;
            try
            {
                Log.DebugFormat("In GetVersionNumber method for MessageId - {0}", LoggerId);
                versions = version.Split('.');
                if (versions.Length == 1)
                    returnvalue = (Convert.ToInt32(versions[0]) * 512);
                if (versions.Length == 2)
                    returnvalue = ((Convert.ToInt32(versions[0]) * 512) + (Convert.ToInt32(versions[1]) * 1));
                Log.DebugFormat("Out GetVersionNumber method - Version number for MessageId - {0} :{1}", LoggerId, version);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Exception in GetVersionNumber method for MessageId - {0} :{1}", LoggerId, e.Message);
            }
            return returnvalue;
        }
        #endregion
    }
}
