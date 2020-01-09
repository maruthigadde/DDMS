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
        public DeleteDocumentResponse DDMSDelete(DeleteDocumentRequest deleteDocumentRequest)
        {
            DeleteDocumentResponse deleteDocumentResponse = new DeleteDocumentResponse();
            SecureString secureString = null;
            try
            {
                if (deleteDocumentRequest.DocumentId != Guid.Empty)
                {
                    Log.Info("In DDMSDelete method");
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
                            DeleteByVersion(clientContext, deleteDocumentRequest);
                        if ((string.IsNullOrEmpty(deleteDocumentRequest.Version) || string.IsNullOrWhiteSpace(deleteDocumentRequest.Version)))
                            DeleteAllversions(clientContext, deleteDocumentRequest);
                    }
                    Log.Info("Out of DDMSDelete method");
                }
                else
                {
                    deleteDocumentResponse.ErrorMessage = string.Format(ErrorMessage.ValueEmpty, SpoConstants.DocumentId);
                }
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Log.ErrorFormat("WebException in DDMSDelete method :{0}", e.Message);
                deleteDocumentResponse.ErrorMessage = ErrorMessage.RemoteName;
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in DDMSDelete method :{0}", ex.Message);
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                    deleteDocumentResponse.ErrorMessage = ErrorMessage.FileNotFound;
                else
                    deleteDocumentResponse.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in DDMSDelete method :{0}", ex.Message);
                deleteDocumentResponse.ErrorMessage = ex.Message;
            }
            return deleteDocumentResponse;
        }
        #endregion

        #region Private Member Functions
        private void DeleteAllversions(ClientContext clientContext, DeleteDocumentRequest deleteDocumentRequest)
        {
            try
            {
                Log.Info("In DeleteAllversions method");
                File file = clientContext.Web.GetFileById(deleteDocumentRequest.DocumentId);
                clientContext.Load(file);
                ClientResult<Guid> recycle = file.Recycle();
                clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);
                Log.Info("In DeleteAllversions after ExecuteQueryWithRetry - Document Deleted: " + recycle.Value.ToString());
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Exception in DeleteAllversions method :{0}", e.Message.ToString());
                throw;
            }
        }
        private void DeleteByVersion(ClientContext clientContext, DeleteDocumentRequest deleteDocumentRequest)
        {
            try
            {
                Log.Info("In DeleteByVersion method");
                File file = clientContext.Web.GetFileById(deleteDocumentRequest.DocumentId);
                FileVersionCollection fileVersions = file.Versions;
                clientContext.Load(file);
                clientContext.Load(fileVersions);
                fileVersions.RecycleByLabel(deleteDocumentRequest.Version);
                clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);
                Log.DebugFormat("In DeleteByVersion after ExecuteQueryWithRetry - Document Deleted for Version {0}", deleteDocumentRequest.Version);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Exception in DeleteByVersion method :{0}", e.Message);
                throw;
            }
        }
        #endregion
    }
}
