using System;
using DDMS.WebService.Models;
using SPOService.Helper;
using DDMS.WebService.Constants;
using DDMS.WebService.ExternalServices.Interfaces;
using Microsoft.SharePoint.Client;
using System.Configuration;
using System.Security;
using System.Net;
using log4net;

namespace DDMS.WebService.SPOActions
{
    public class DDMSDeleteDocument : IDDMSDeleteDocument
    {
        private static readonly ILog Log = log4net.LogManager.GetLogger(typeof(DDMSDeleteDocument));
        #region Public Member Functions
        /// <summary>
        /// Method to delete a document based on documentid or versionnumber
        /// </summary>
        /// <param name="deleteDocumentRequest">Request model for Delete Operation</param>
        /// <param name="LoggerId">Message Id used for Logging</param>
        /// <returns></returns>
        public DeleteDocumentResponse DDMSDelete(DeleteDocumentRequest deleteDocumentRequest, string LoggerId)
        {
            DeleteDocumentResponse deleteDocumentResponse = new DeleteDocumentResponse();
            SecureString secureString = null;
            try
            {
                if (deleteDocumentRequest.DocumentId != Guid.Empty)
                {
                    Log.DebugFormat("In DDMSDelete method for MessageId - {0} DocumentId :{1}", LoggerId, deleteDocumentRequest.DocumentId.ToString());
                    using (ClientContext clientContext = new ClientContext(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteURL)))
                    {
                        secureString = new NetworkCredential("", CommonHelper.Decrypt(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPassword),
                               ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPasswordKey),
                               ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPasswordIv))).SecurePassword;

                        String username = CommonHelper.Decrypt(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserName),
                                    ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserNameKey),
                                    ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserNameIv));

                        clientContext.Credentials = new SharePointOnlineCredentials(username, secureString);
                        if (!(string.IsNullOrEmpty(deleteDocumentRequest.Version) || string.IsNullOrWhiteSpace(deleteDocumentRequest.Version)))
                            //Invoke method to delete a specific version
                            DeleteByVersion(clientContext, deleteDocumentRequest, LoggerId);
                        if ((string.IsNullOrEmpty(deleteDocumentRequest.Version) || string.IsNullOrWhiteSpace(deleteDocumentRequest.Version)))
                            //Invoke method to delete all versions
                            DeleteAllversions(clientContext, deleteDocumentRequest, LoggerId);
                    }
                    Log.DebugFormat("Out of DDMSDelete method for MessageId - {0} DocumentId :{1}", LoggerId, deleteDocumentRequest.DocumentId.ToString());
                }
                else
                {
                    deleteDocumentResponse.ErrorMessage = string.Format(ErrorMessage.ValueEmpty, SpoConstants.DocumentId);
                }
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Log.ErrorFormat("WebException in DDMSDelete method for MessageId - {0} :{1}", LoggerId, e.Message);
                deleteDocumentResponse.ErrorMessage = ErrorMessage.RemoteName;
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in DDMSDelete method for MessageId - {0} :{1}", LoggerId, ex.Message);
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                    deleteDocumentResponse.ErrorMessage = ErrorMessage.FileNotFound;
                else
                    deleteDocumentResponse.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in DDMSDelete method for MessageId - {0} :{1}", LoggerId, ex.Message);
                deleteDocumentResponse.ErrorMessage = ex.Message;
            }
            return deleteDocumentResponse;
        }
        #endregion

        #region Private Member Functions
        /// <summary>
        /// Method to delete entire file
        /// </summary>
        /// <param name="clientContext">SPO Client Context</param>
        /// <param name="deleteDocumentRequest">Request model for Delete Operation</param>
        /// <param name="LoggerId">MessageId will be used for logging information</param>
        private void DeleteAllversions(ClientContext clientContext, DeleteDocumentRequest deleteDocumentRequest, string LoggerId)
        {
            try
            {
                Log.DebugFormat("In DeleteAllversions method for MessageId - {0} DocumentId :{1}", LoggerId, deleteDocumentRequest.DocumentId.ToString());
                //retrieve the file based on DocumentId
                File file = clientContext.Web.GetFileById(deleteDocumentRequest.DocumentId);
                clientContext.Load(file);
                
                //recycle the file - returns the GUID of the file in Recycle Bin
                ClientResult<Guid> recycle = file.Recycle();
                clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);
                Log.DebugFormat("In DeleteAllversions after ExecuteQueryWithRetry for MessageId - {0} - Document Deleted RecycleId : {1}", LoggerId, recycle.Value.ToString());
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Exception in DeleteAllversions method for MessageId - {0} :{1}", LoggerId, e.Message.ToString());
                throw;
            }
        }
        /// <summary>
        /// Method to delete specific version based on version number
        /// </summary>
        /// <param name="clientContext">SPO Client Context</param>
        /// <param name="deleteDocumentRequest">Request model for delete operation</param>
        /// <param name="LoggerId">MessageId will be used for logging information</param>
        private void DeleteByVersion(ClientContext clientContext, DeleteDocumentRequest deleteDocumentRequest, string LoggerId)
        {
            try
            {
                Log.DebugFormat("In DeleteByVersion method for MessageId - {0} DocumentId :{1}", LoggerId, deleteDocumentRequest.DocumentId.ToString());

                //retreive the file based on DocumentId
                File file = clientContext.Web.GetFileById(deleteDocumentRequest.DocumentId);

                //load file version collection
                FileVersionCollection fileVersions = file.Versions;
                clientContext.Load(file);
                clientContext.Load(fileVersions);
                //recycle file based on version label
                fileVersions.RecycleByLabel(deleteDocumentRequest.Version);
                clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);
                Log.DebugFormat("In DeleteByVersion after ExecuteQueryWithRetry for MessageId - {0} - Document Deleted for Version {1}", LoggerId, deleteDocumentRequest.Version);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Exception in DeleteByVersion method for MessageId - {0} :{1}", LoggerId, e.Message);
                throw;
            }
        }
        #endregion
    }
}
