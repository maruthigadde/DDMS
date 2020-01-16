using DDMS.WebService.Constants;
using DDMS.WebService.ExternalServices.Interfaces;
using DDMS.WebService.Models;
using log4net;
using Microsoft.SharePoint.Client;
using SPOService.Helper;
using System;
using System.Configuration;
using System.Net;
using System.Security;

namespace DDMS.WebService.SPOActions
{
    public class DDMSUploadDocument : IDDMSUploadDocument
    {
        private static readonly ILog Log = log4net.LogManager.GetLogger(typeof(DDMSUploadDocument));
        #region Public Member Functions 
        /// <summary>
        /// Method to upload document to SPO library
        /// </summary>
        /// <param name="uploadDocumentRequest">Request Model format for Upload functionality</param>
        /// <param name="LoggerId">MessageId will be used for logging information</param>
        /// <returns></returns>
        public UploadDocumentResponse DDMSUpload(UploadDocumentRequest uploadDocumentRequest, string LoggerId)
        {
            UploadDocumentResponse uploadDocumentResponse = new UploadDocumentResponse();
            try
            {
                Log.DebugFormat("In DDMSUpload method for MessageId - {0}", LoggerId);
                if (uploadDocumentRequest.DocumentId != Guid.Empty)
                   //if documentid is passed in request, an existing document will be updated
                    uploadDocumentResponse = UpdateDocument(uploadDocumentRequest, LoggerId);
                else
                    //if documentid is empty or not passed in request, a new document is uploaded
                    uploadDocumentResponse = UploadDocument(uploadDocumentRequest, LoggerId);
                Log.DebugFormat("Out of DDMSUpload method for MessageId - {0}", LoggerId);
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
        /// <summary>
        /// Method to upload a new document
        /// </summary>
        /// <param name="uploadDocumentRequest">Request Model format for Upload functionality</param>
        /// <param name="LoggerId">MessageId will be used for logging information</param>
        /// <returns></returns>
        private UploadDocumentResponse UploadDocument(UploadDocumentRequest uploadDocumentRequest, string LoggerId)
        {
            UploadDocumentResponse uploadDocumentResponse = new UploadDocumentResponse();
            SecureString secureString = null;
            try
            {
                Log.DebugFormat("In UploadDocument method for MessageId - {0}", LoggerId);
                if (uploadDocumentRequest.DocumentContent != null && uploadDocumentRequest.DocumentContent.Length > 0 && uploadDocumentRequest.DocumentContent.Length <= Convert.ToInt32(ConfigurationManager.AppSettings.Get(SpoConstants.MaxFileSize)))
                {
                    using (ClientContext clientContext = new ClientContext(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteURL)))
                    {
                        //Get SPO Credentials for authentication
                        secureString = new NetworkCredential("", CommonHelper.Decrypt(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPassword),
                               ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPasswordKey),
                               ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPasswordIv))).SecurePassword;

                        String username = CommonHelper.Decrypt(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserName),
                                    ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserNameKey),
                                    ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserNameIv));

                        clientContext.Credentials = new SharePointOnlineCredentials(username, secureString);
                        //check if file with same name already exists
                        if (TryGetFileByServerRelativeUrl(clientContext, uploadDocumentRequest, LoggerId))
                        {
                            Random random = new Random();
                            Log.DebugFormat("In UploadDocument method FileName already exists renaming the file for MessageId - {0}", LoggerId);

                            //if file exists with same name, rename based on current timestamp
                            uploadDocumentRequest.DocumentName = string.Concat(System.IO.Path.GetFileNameWithoutExtension(uploadDocumentRequest.DocumentName), DateTime.Now.ToString("yyyyMMddhhmmss"), random.Next(1000, 9999), System.IO.Path.GetExtension(uploadDocumentRequest.DocumentName));
                            Log.DebugFormat("In UploadDocument method after renaming the file for MessageId - {0} :{1}", LoggerId, uploadDocumentRequest.DocumentName);
                        }
                        //get the documentlibrary to upload
                        Folder folder = clientContext.Web.GetFolderByServerRelativeUrl(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteURL) + "/" + ConfigurationManager.AppSettings.Get(HeaderValueConstants.SiteId));

                        //create file creation information for new upload
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
                            //assign the documentid to response model
                            uploadDocumentResponse.DocumentId = file.UniqueId;
                            //assign the versionnumber to response model
                            uploadDocumentResponse.Version = file.UIVersionLabel;
                            Log.DebugFormat("In UploadDocument method for MessageId - {0} - after uploading file to SPO DocumentId :{1} Version:{2}", LoggerId, uploadDocumentResponse.DocumentId, uploadDocumentResponse.Version);
                            if (ValidateMetadata(uploadDocumentRequest))
                            {
                                Log.DebugFormat("In UploadDocument method - validate metadata success, calling SaveOrUpdateMetaData method for MessageId - {0}", LoggerId);
                                
                                if (!SaveOrUpdateMetaData(clientContext, folder, file, uploadDocumentRequest, uploadDocumentResponse, SpoConstants.OverRideExistingVersion, LoggerId))
                                {
                                    uploadDocumentResponse.DocumentId = Guid.Empty;
                                    uploadDocumentResponse.Version = string.Empty;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Log.DebugFormat("In UploadDocument method for MessageId - {0} - {1}", LoggerId, ErrorMessage.MaxFileSizeContentReached);
                    uploadDocumentResponse.ErrorMessage = ErrorMessage.MaxFileSizeContentReached;
                }
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Log.ErrorFormat("Error in UploadDocument method for MessageId - {0} :{1}", LoggerId, e.Message);
                uploadDocumentResponse.ErrorMessage = ErrorMessage.RemoteName;
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in UploadDocument method for MessageId - {0} :{1}", LoggerId, ex.Message);
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                    uploadDocumentResponse.ErrorMessage = ErrorMessage.FileNotFound;
                else
                    uploadDocumentResponse.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in UploadDocument method for MessageId - {0} :{1}", LoggerId, ex.Message);
                uploadDocumentResponse.ErrorMessage = ex.Message;
            }
            return uploadDocumentResponse;
        }
        /// <summary>
        /// Method to update an existing document
        /// </summary>
        /// <param name="uploadDocumentRequest">Request model for Update document</param>
        /// <param name="LoggerId">MessageId will be used for logging information</param>
        /// <returns></returns>
        private UploadDocumentResponse UpdateDocument(UploadDocumentRequest uploadDocumentRequest, string LoggerId)
        {
            UploadDocumentResponse uploadDocumentResponse = new UploadDocumentResponse();
            SecureString secureString = null;
            try
            {
                Log.DebugFormat("In UpdateDocument method for MessageId - {0} DocumentId :{1}", LoggerId, uploadDocumentRequest.DocumentId.ToString());
                using (ClientContext clientContext = new ClientContext(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteURL)))
                {
                    //Get SPO Credentials for authentication
                    secureString = new NetworkCredential("", CommonHelper.Decrypt(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPassword),
                               ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPasswordKey),
                               ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOPasswordIv))).SecurePassword;

                    String username = CommonHelper.Decrypt(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserName),
                                ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserNameKey),
                                ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOUserNameIv));

                    clientContext.Credentials = new SharePointOnlineCredentials(username, secureString);
                    //Folder folder = clientContext.Web.GetFolderByServerRelativeUrl(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteURL) + "/" + ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOFolder));

                    //retreive the existing document based on DocumentId
                    File file = clientContext.Web.GetFileById(uploadDocumentRequest.DocumentId);
                    clientContext.Load(file);
                    clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);

                    //fetch the folder path of the file which exists in SPO library
                    Folder folder = clientContext.Web.GetFolderByServerRelativeUrl(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPOSiteURL) + "/" + System.IO.Path.GetDirectoryName(file.ServerRelativeUrl));
                    clientContext.Load(folder);
                    clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);

                    if (uploadDocumentRequest.DocumentContent != null && uploadDocumentRequest.DocumentContent.Length > 0 && uploadDocumentRequest.DocumentContent.Length <= Convert.ToInt32(ConfigurationManager.AppSettings.Get(SpoConstants.MaxFileSize)))
                    {
                        //check if the file extension in request is same as the file in SPO library
                        if (System.IO.Path.GetExtension(file.Name).ToUpper() == System.IO.Path.GetExtension(uploadDocumentRequest.DocumentName).ToUpper())
                        {
                            Log.DebugFormat("File Extension validation success for MessageId - {0} - Same file extension provided", LoggerId);
                            if (System.IO.Path.GetFileNameWithoutExtension(file.Name).ToUpper() != System.IO.Path.GetFileNameWithoutExtension(uploadDocumentRequest.DocumentName).ToUpper())
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
                            //update an existing document
                            clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);
                            uploadDocumentResponse.DocumentId = file.UniqueId;
                            uploadDocumentResponse.Version = file.UIVersionLabel;
                            Log.DebugFormat("In UpdateDocument method after updating the document in SPO for MessageId - {0} DocumentId :{1} Version{2}", LoggerId, uploadDocumentResponse.DocumentId, uploadDocumentResponse.Version);
                            if (ValidateMetadata(uploadDocumentRequest))
                            {
                                Log.DebugFormat("In UpdateDocument method - validate metadata success, calling SaveOrUpdateMetaData method for MessageId - {0}", LoggerId);
                                if (!SaveOrUpdateMetaData(clientContext, folder, file, uploadDocumentRequest, uploadDocumentResponse, SpoConstants.OverRideExistingVersion, LoggerId))
                                {
                                    uploadDocumentResponse.DocumentId = Guid.Empty;
                                    uploadDocumentResponse.Version = string.Empty;
                                    uploadDocumentResponse.ErrorMessage = ErrorMessage.UpDateFailed;
                                }
                            }
                        }
                        else
                        {
                            //if file extension in request object is different, throw an error
                            uploadDocumentResponse.DocumentId = Guid.Empty;
                            uploadDocumentResponse.Version = string.Empty;
                            uploadDocumentResponse.ErrorMessage = ErrorMessage.DifferentFileExtension;
                        }
                    }
                    else
                    {
                        if (uploadDocumentRequest.DocumentContent == null || uploadDocumentRequest.DocumentContent.Length == 0)
                        {
                            Log.DebugFormat("In UpdateDocument method - DocumentContent is null or DocumentContent length is zero for MessageId - {0}", LoggerId);
                            Log.DebugFormat("In UpdateDocument method - Update only MetaData for MessageId - {0}", LoggerId);
                            if (ValidateMetadata(uploadDocumentRequest))
                            {
                                Log.DebugFormat("In UpdateDocument method - validate metadata success, calling SaveOrUpdateMetaData method for MessageId - {0}", LoggerId);
                                if (!SaveOrUpdateMetaData(clientContext, folder, file, uploadDocumentRequest, uploadDocumentResponse, !SpoConstants.OverRideExistingVersion, LoggerId))
                                {
                                    Log.DebugFormat("In UpdateDocument method for MessageId - {0} - Update only MetaData failed :{1}", LoggerId, uploadDocumentResponse.ErrorMessage);
                                    uploadDocumentResponse.DocumentId = Guid.Empty;
                                    uploadDocumentResponse.Version = string.Empty;
                                    uploadDocumentResponse.ErrorMessage = ErrorMessage.UpDateFailed;
                                }
                            }
                        }

                        //check if filesize in request object is less than permitted size
                        if (uploadDocumentRequest.DocumentContent != null && uploadDocumentRequest.DocumentContent.Length > 0 && uploadDocumentRequest.DocumentContent.Length > Convert.ToInt32(ConfigurationManager.AppSettings.Get(SpoConstants.MaxFileSize)))
                        {
                            Log.DebugFormat("In UpdateDocument method for MessageId - {0} - {1}", LoggerId, ErrorMessage.MaxFileSizeContentReached);
                            uploadDocumentResponse.ErrorMessage = ErrorMessage.MaxFileSizeContentReached;
                            uploadDocumentResponse.DocumentId = Guid.Empty;
                            uploadDocumentResponse.Version = string.Empty;
                        }
                    }
                }
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Log.ErrorFormat("WebException in UpdateDocument method for MessageId - {0} :{1}", LoggerId, e.Message);
                uploadDocumentResponse.ErrorMessage = ErrorMessage.RemoteName;
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in UpdateDocument method for MessageId - {0} :{1}", LoggerId, ex.Message);
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                    uploadDocumentResponse.ErrorMessage = ErrorMessage.FileNotFound;
                else
                    uploadDocumentResponse.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in UpdateDocument method for MessageId - {0} :{1}", LoggerId, ex.Message);
                uploadDocumentResponse.ErrorMessage = ex.Message;
            }
            return uploadDocumentResponse;
        }
        private static bool TryGetFileByServerRelativeUrl(ClientContext clientContext, UploadDocumentRequest uploadDocumentRequest, string LoggerId)
        {
            bool fileAlreadyExistStatus = false;
            try
            {
                Log.DebugFormat("In TryGetFileByServerRelativeUrl method for MessageId - {0}", LoggerId);
                ResourcePath filePath = ResourcePath.FromDecodedUrl(ConfigurationManager.AppSettings.Get(ConfigurationConstants.SPORelativeURL) + ConfigurationManager.AppSettings.Get(HeaderValueConstants.SiteId) + "/" + uploadDocumentRequest.DocumentName);
                File file = clientContext.Web.GetFileByServerRelativePath(filePath);
                clientContext.Load(file);
                clientContext.ExecuteQueryWithRetry(ExecuteQueryConstants.RetryCount, ExecuteQueryConstants.RetryDelayTime);
                if (file.Exists)
                    fileAlreadyExistStatus = true;
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in TryGetFileByServerRelativeUrl method for MessageId - {0} :{1}", LoggerId, ex.Message);
                if (ex.ServerErrorTypeName == ErrorException.SystemIoFileNotFound)
                {
                    fileAlreadyExistStatus = false;
                }
                else
                    throw;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception in TryGetFileByServerRelativeUrl method for MessageId - {0} :{1}", LoggerId, ex.Message);
                fileAlreadyExistStatus = false;
            }
            Log.DebugFormat("Out TryGetFileByServerRelativeUrl method for MessageId - {0} fileAlreadyExistStatus :{1}", LoggerId, fileAlreadyExistStatus);
            return fileAlreadyExistStatus;
        }
        /// <summary>
        /// Method to update the metadata
        /// </summary>
        /// <param name="clientContext">SPO client context object</param>
        /// <param name="folder">SPO Document library</param>
        /// <param name="file">New file uploaded to document library</param>
        /// <param name="uploadDocumentRequest"> Request model for Upload Document</param>
        /// <param name="uploadDocumentResponse">Response model for Upload Document</param>
        /// <param name="overRideExistingVersion">boolean value to override existing version</param>
        /// <param name="LoggerId">MessageId for logging information</param>
        /// <returns></returns>
        private bool SaveOrUpdateMetaData(ClientContext clientContext, Folder folder, File file, UploadDocumentRequest uploadDocumentRequest, UploadDocumentResponse uploadDocumentResponse, bool overRideExistingVersion, string LoggerId)
        {
            bool bReturnvalue = false;
            try
            {
                Log.DebugFormat("In SaveOrUpdateMetaData method for MessageId - {0}", LoggerId);
                List objList = clientContext.Web.Lists.GetByTitle(ConfigurationManager.AppSettings.Get(HeaderValueConstants.SiteId));
                //fetch the document based on SPO Unique ID
                ListItem objListItem = objList.GetItemByUniqueId(file.UniqueId);

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
                Log.DebugFormat("In SaveOrUpdateMetaData method for MessageId - {0} DocumentId :{1} Version :{2}", LoggerId, uploadDocumentResponse.DocumentId, uploadDocumentResponse.Version);
                bReturnvalue = true;
                Log.DebugFormat("Out of SaveOrUpdateMetaData method for MessageId - {0}", LoggerId);
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Log.ErrorFormat("WebException in SaveOrUpdateMetaData method for MessageId - {0} :{1}", LoggerId, e.Message);
                bReturnvalue = false;
                uploadDocumentResponse.ErrorMessage = ErrorMessage.RemoteName;
            }
            catch (ServerException ex)
            {
                Log.ErrorFormat("ServerException in SaveOrUpdateMetaData method for MessageId - {0} :{1}", LoggerId, ex.Message);
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
                Log.ErrorFormat("Exception in SaveOrUpdateMetaData method for MessageId - {0} :{1}", LoggerId, ex.Message);
                uploadDocumentResponse.ErrorMessage = ex.Message;
            }
            return bReturnvalue;
        }
        /// <summary>
        /// Method to validate if metadata is passed in request object
        /// </summary>
        /// <param name="uploadDocumentRequest">Request model for Upload Document</param>
        /// <returns></returns>
        private bool ValidateMetadata(UploadDocumentRequest uploadDocumentRequest)
        {
            bool validMetadata = false;
            try
            {
                //checks if metadata is passed in request and returns a boolean value
                if (!string.IsNullOrEmpty(uploadDocumentRequest.DealerNumber) || !string.IsNullOrEmpty(uploadDocumentRequest.RequestUser))
                    validMetadata = true;
            }
            catch (Exception e)
            {
                validMetadata = false;
            }
            return validMetadata;
        }
        #endregion
    }
}
