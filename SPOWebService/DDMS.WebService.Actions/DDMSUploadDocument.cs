using System;
using DDMS.WebService.Models;
using DDMS.WebService.Models.Common;
using DDMS.WebService.ExternalServices.Interfaces;
using Microsoft.SharePoint.Client;
using System.Configuration;
using System.Security;
using System.Net;
using log4net;
using SPOService.EncryptConfiguration;

namespace DDMS.WebService.SPOActions
{
    public class DDMSUploadDocument : IDDMSUploadDocument
    {
        private static readonly ILog Log = log4net.LogManager.GetLogger(typeof(DDMSUploadDocument));
        #region Public Member Functions 
        public UploadDocumentResponse DDMSUpload(UploadDocumentRequest uploadDocumentRequest)
        {
            UploadDocumentResponse uploadDocumentResponse = new UploadDocumentResponse();
            try
            {
                Log.Info("In DDMSUpload method");
                if (uploadDocumentRequest.DocumentId != Guid.Empty)
                    uploadDocumentResponse = UpdateDocument(uploadDocumentRequest);
                else
                    uploadDocumentResponse = UploadDocument(uploadDocumentRequest);
                Log.Info("Out of DDMSUpload method");
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Exception in DDMSUpload method :{0}", e.Message);
                uploadDocumentResponse.ErrorMessage = e.Message;
            }
            return uploadDocumentResponse;
        }
        #endregion

        #region Private Member Functions
        private UploadDocumentResponse UploadDocument(UploadDocumentRequest uploadDocumentRequest)
        {
            UploadDocumentResponse uploadDocumentResponse = new UploadDocumentResponse();
            SecureString secureString = null;
            try
            {
                Log.Info("In UploadDocument method");
                if (uploadDocumentRequest.DocumentContent != null && uploadDocumentRequest.DocumentContent.Length > 0 && uploadDocumentRequest.DocumentContent.Length <= SpoConstants.MaxFileSize)
                {
                    using (ClientContext clientContext = new ClientContext(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteURL)))
                    {
                        secureString = new NetworkCredential("", EncryptDecrypt.SPOPassword).SecurePassword;
                        clientContext.Credentials = new SharePointOnlineCredentials(EncryptDecrypt.SPOUserName, secureString);
                        if (TryGetFileByServerRelativeUrl(clientContext, uploadDocumentRequest))
                        {
                            Random random = new Random();
                            Log.Info("In UploadDocument method FileName already exists renaming the file");
                            uploadDocumentRequest.DocumentName = string.Concat(uploadDocumentRequest.DocumentName.Split('.')[0], DateTime.Now.ToString("yyyyMMddhhmmss"), random.Next(1000, 9999), '.', uploadDocumentRequest.DocumentName.Split('.')[1]);
                            Log.Debug("In UploadDocument method after renaming the file :" + uploadDocumentRequest.DocumentName);
                        }

                        Folder folder = clientContext.Web.GetFolderByServerRelativeUrl(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteURL) + "/" + ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOFolder));
                        FileCreationInformation fileCreationInformation = new FileCreationInformation
                        {
                            ContentStream = new System.IO.MemoryStream(uploadDocumentRequest.DocumentContent),
                            Url = uploadDocumentRequest.DocumentName,
                            Overwrite = false
                        };
                        File file = folder.Files.Add(fileCreationInformation);
                        clientContext.Load(folder);
                        clientContext.Load(file);
                        clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);

                        if (file.Exists)
                        {
                            uploadDocumentResponse.DocumentId = file.UniqueId;
                            Log.Info("In UploadDocument method - after uploading file to SPO DocumentId :" + uploadDocumentResponse.DocumentId);
                            if (!SaveOrUpdateMetaData(clientContext, folder, file, uploadDocumentRequest, uploadDocumentResponse, SpoConstants.OverRideExistingVersion))
                            {
                                uploadDocumentResponse.DocumentId = Guid.Empty;
                                uploadDocumentResponse.Version = string.Empty;
                            }
                        }
                    }
                }
                else
                {
                    Log.DebugFormat("In UploadDocument method - {0}", ErrorMessage.MaxFileSizeContentReached);
                    uploadDocumentResponse.ErrorMessage = ErrorMessage.MaxFileSizeContentReached;
                }
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Log.ErrorFormat("Error in UploadDocument method :{0}", e.Message);
                uploadDocumentResponse.ErrorMessage = ErrorMessage.RemoteName;
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.MessageLengthLimitExceeded)
            {
                Log.ErrorFormat("WebException in UploadDocument method :{0}", e.Message);
                uploadDocumentResponse.ErrorMessage = WebExceptionStatus.MessageLengthLimitExceeded.ToString();
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in UploadDocument method :{0}", ex.Message);
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                    uploadDocumentResponse.ErrorMessage = ErrorMessage.FileNotFound;
                else
                    uploadDocumentResponse.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in UploadDocument method :{0}", ex.Message);
                uploadDocumentResponse.ErrorMessage = ex.Message;
            }
            return uploadDocumentResponse;
        }
        private UploadDocumentResponse UpdateDocument(UploadDocumentRequest uploadDocumentRequest)
        {
            UploadDocumentResponse uploadDocumentResponse = new UploadDocumentResponse();
            SecureString secureString = null;
            try
            {
                Log.Info("In UpdateDocument method");
                using (ClientContext clientContext = new ClientContext(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteURL)))
                {
                    secureString = new NetworkCredential("", EncryptDecrypt.SPOPassword).SecurePassword;
                    clientContext.Credentials = new SharePointOnlineCredentials(EncryptDecrypt.SPOUserName, secureString);
                    Folder folder = clientContext.Web.GetFolderByServerRelativeUrl(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteURL) + "/" + ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOFolder));

                    File file = clientContext.Web.GetFileById(uploadDocumentRequest.DocumentId);
                    clientContext.Load(file);
                    clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);
                    if (uploadDocumentRequest.DocumentContent != null && uploadDocumentRequest.DocumentContent.Length > 0 && uploadDocumentRequest.DocumentContent.Length <= SpoConstants.MaxFileSize)
                    {
                        if (file.Name.Split('.')[1].ToUpper() == uploadDocumentRequest.DocumentName.Split('.')[1].ToUpper())
                        {
                            if (file.Name.Split('.')[0].ToUpper() != uploadDocumentRequest.DocumentName.Split('.')[0].ToUpper())
                                uploadDocumentRequest.DocumentName = file.Name;
                            FileCreationInformation fileCreationInformation = new FileCreationInformation
                            {
                                ContentStream = new System.IO.MemoryStream(uploadDocumentRequest.DocumentContent),
                                Url = uploadDocumentRequest.DocumentName,
                                Overwrite = true
                            };
                            file = folder.Files.Add(fileCreationInformation);
                            clientContext.Load(folder);
                            clientContext.Load(file);
                            clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);
                            Log.Info("In UpdateDocument method after updating the document in SPO");
                            if (!SaveOrUpdateMetaData(clientContext, folder, file, uploadDocumentRequest, uploadDocumentResponse, SpoConstants.OverRideExistingVersion))
                            {
                                uploadDocumentResponse.DocumentId = Guid.Empty;
                                uploadDocumentResponse.Version = string.Empty;
                                uploadDocumentResponse.ErrorMessage = ErrorMessage.UpDateFailed;
                            }
                        }
                        else
                        {
                            uploadDocumentResponse.DocumentId = Guid.Empty;
                            uploadDocumentResponse.Version = string.Empty;
                            uploadDocumentResponse.ErrorMessage = ErrorMessage.DifferentFileExtension;
                        }
                    }
                    else
                    {
                        if (uploadDocumentRequest.DocumentContent == null || uploadDocumentRequest.DocumentContent.Length == 0)
                        {
                            Log.DebugFormat("In UploadDocument method - DocumentContent is null or DocumentContent length is zero");
                            Log.DebugFormat("In UploadDocument method - Update only MetaData");
                            if (!SaveOrUpdateMetaData(clientContext, folder, file, uploadDocumentRequest, uploadDocumentResponse, !SpoConstants.OverRideExistingVersion))
                            {
                                Log.Debug("In UploadDocument method - Update only MetaData failed :" + uploadDocumentResponse.ErrorMessage);
                                uploadDocumentResponse.DocumentId = Guid.Empty;
                                uploadDocumentResponse.Version = string.Empty;
                                uploadDocumentResponse.ErrorMessage = ErrorMessage.UpDateFailed;
                            }
                        }
                        if (uploadDocumentRequest.DocumentContent.Length >= SpoConstants.MaxFileSize)
                        {
                            Log.DebugFormat("In UploadDocument method - {0}", ErrorMessage.MaxFileSizeContentReached);
                            uploadDocumentResponse.ErrorMessage = ErrorMessage.MaxFileSizeContentReached;
                            uploadDocumentResponse.DocumentId = Guid.Empty;
                            uploadDocumentResponse.Version = string.Empty;
                        }
                    }
                }
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Log.ErrorFormat("WebException in UpdateDocument method :{0}", e.Message);
                uploadDocumentResponse.ErrorMessage = ErrorMessage.RemoteName;
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.MessageLengthLimitExceeded)
            {
                Log.ErrorFormat("WebException in UpdateDocument method :{0}", e.Message);
                uploadDocumentResponse.ErrorMessage = WebExceptionStatus.MessageLengthLimitExceeded.ToString();
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in UpdateDocument method :{0}", ex.Message);
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                    uploadDocumentResponse.ErrorMessage = ErrorMessage.FileNotFound;
                else
                    uploadDocumentResponse.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in UpdateDocument method :{0}", ex.Message);
                uploadDocumentResponse.ErrorMessage = ex.Message;
            }
            return uploadDocumentResponse;
        }
        private static bool TryGetFileByServerRelativeUrl(ClientContext clientContext, UploadDocumentRequest uploadDocumentRequest)
        {
            bool fileAlreadyExistStatus = false;
            try
            {
                Log.Info("In TryGetFileByServerRelativeUrl method");
                File file = clientContext.Web.GetFileByUrl(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteURL) + "/" + ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOFolder) + "/" + uploadDocumentRequest.DocumentName);
                clientContext.Load(file);
                clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);
                if (file.Exists)
                    fileAlreadyExistStatus = true;
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in TryGetFileByServerRelativeUrl method :{0}", ex.Message);
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                {
                    fileAlreadyExistStatus = false;
                }
                else
                    throw;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in TryGetFileByServerRelativeUrl method :{0}", ex.Message);
                fileAlreadyExistStatus = false;
            }
            return fileAlreadyExistStatus;
        }
        private bool SaveOrUpdateMetaData(ClientContext clientContext, Folder folder, File file, UploadDocumentRequest uploadDocumentRequest, UploadDocumentResponse uploadDocumentResponse, bool overRideExistingVersion)
        {
            bool bReturnvalue = false;
            try
            {
                Log.Info("In SaveOrUpdateMetaData method");
                List objList = clientContext.Web.Lists.GetByTitle(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOFolder));
                ListItem objListItem = objList.GetItemByUniqueId(file.UniqueId);

                if (!string.IsNullOrWhiteSpace(uploadDocumentRequest.DocumentName))
                    objListItem[SpoConstants.Title] = uploadDocumentRequest.DocumentName;
                if (!string.IsNullOrWhiteSpace(uploadDocumentRequest.DealerNumber))
                    objListItem[SpoConstants.DealerNumber] = uploadDocumentRequest.DealerNumber;
                if (!string.IsNullOrWhiteSpace(uploadDocumentRequest.RequestUser))
                    objListItem[SpoConstants.RequestUser] = uploadDocumentRequest.RequestUser;

                if (overRideExistingVersion)
                    objListItem.UpdateOverwriteVersion();
                else
                    objListItem.Update();

                clientContext.Load(file);
                clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);
                uploadDocumentResponse.DocumentId = file.UniqueId;
                uploadDocumentResponse.Version = file.UIVersionLabel;
                Log.DebugFormat("In SaveOrUpdateMetaData method DocumentId :{0} Version :{1}", uploadDocumentResponse.DocumentId, uploadDocumentResponse.Version);
                bReturnvalue = true;
                Log.Info("Out of SaveOrUpdateMetaData method");
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Log.ErrorFormat("WebException in SaveOrUpdateMetaData method :{0}", e.Message);
                bReturnvalue = false;
                uploadDocumentResponse.ErrorMessage = ErrorMessage.RemoteName;
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in SaveOrUpdateMetaData method :{0}", ex.Message);
                bReturnvalue = false;
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                    uploadDocumentResponse.ErrorMessage = ErrorMessage.FileNotFound;
                if (ex.ServerErrorTypeName == ErrorException.ArgumentException)
                    uploadDocumentResponse.ErrorMessage = ex.Message;
                if (ex.ServerErrorTypeName == ErrorException.SpFieldValueException)
                    uploadDocumentResponse.ErrorMessage = ex.ServerErrorValue + ErrorMessage.FieldValueNotValid;
                else
                    uploadDocumentResponse.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in SaveOrUpdateMetaData method :{0}", ex.Message);
                uploadDocumentResponse.ErrorMessage = ex.Message;
            }
            return bReturnvalue;
        }
        #endregion
    }
}
