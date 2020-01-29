using DDMS.WebService.Constants;
using DDMS.WebService.ExternalServices.Interfaces;
using DDMS.WebService.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace DDMS.WebService.DDMSOperations.Controllers
{
    [ExcludeFromCodeCoverage]
    [AuthorizedUser(Application = "DDMS")]
    [HeaderValidate]
    public class DDMSController : ApiController
    {
        private IDDMSSearchDocument ddmsSearchDocument;
        private IDDMSDeleteDocument ddmsDeleteDocument;
        private IDDMSUploadDocument ddmsUploadDocument;
        private static readonly ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DDMSController(IDDMSSearchDocument dDMSSearchDocument, IDDMSDeleteDocument dDMSDeleteDocument, IDDMSUploadDocument dDMSUploadDocument)
        {
            ddmsSearchDocument = dDMSSearchDocument;
            ddmsDeleteDocument = dDMSDeleteDocument;
            ddmsUploadDocument = dDMSUploadDocument;
        }

        /// <summary>
        /// DDMS API GET method to search a document
        /// </summary>
        /// <param name="documentId"> Mandatory attribute for search</param>
        /// <param name="version"> Version number required to get specific version content</param>
        /// <returns></returns>
        [HttpGet, Route("~/api/DDMS/{DocumentId:Guid}/"), Route("~/api/DDMS/{DocumentId:Guid}/{Version:decimal}")]
        public IHttpActionResult SearchDocument(Guid documentId, string version = null)
        {
            Guid messageId;
            Dictionary<string, dynamic> keyValuePairs;
            //Retrieve the header parameters from request object
            RetrieveHeaders(out keyValuePairs, out messageId);
            SearchDocumentRequest searchDocumentRequest = new SearchDocumentRequest();
            SearchDocumentResponse searchDocumentResponse = new SearchDocumentResponse();
            try
            {
                Log.DebugFormat("In api/SearchDocument method for MessageId - {0}", messageId.ToString());
                searchDocumentRequest.DocumentId = documentId;
                searchDocumentRequest.Version = version;
                
                if (searchDocumentRequest.DocumentId != Guid.Empty)
                {
                    //If documentid is not empty and version number is not passed retrieve the document metadata
                    if ((string.IsNullOrEmpty(searchDocumentRequest.Version) || string.IsNullOrWhiteSpace(searchDocumentRequest.Version)))
                    {
                        //Create a new SearchDocumentAllMetadataVersions model object 
                        SearchDocumentAllMetaDataVersions searchDocumentAllMetaDataVersions = new SearchDocumentAllMetaDataVersions();
                        try
                        {
                            Log.DebugFormat("In api/SearchDocument, before calling DDMSSearchAllOldVersions method for MessageId - {0} Document ID :{1}", messageId.ToString(), searchDocumentRequest.DocumentId.ToString());
                            //To fetch document metadata for all versions
                            searchDocumentAllMetaDataVersions = ddmsSearchDocument.DDMSSearchAllOldVersions(searchDocumentRequest, messageId.ToString());
                            Log.DebugFormat("In api/SearchDocument, after calling DDMSSearchAllOldVersions method for MessageId - {0} Document ID :{1}", messageId.ToString(), searchDocumentRequest.DocumentId.ToString());
                            if (string.IsNullOrEmpty(searchDocumentAllMetaDataVersions.ErrorMessage))
                            {
                                //Add headers to response message
                                keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.OK);
                                keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Success);
                                foreach (var field in keyValuePairs)
                                {
                                    if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                                        HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                                }
                                return Content(HttpStatusCode.OK, searchDocumentAllMetaDataVersions);
                            }

                            //Search based on documentid is not successful - file not found
                            if (searchDocumentAllMetaDataVersions.ErrorMessage == ErrorMessage.FileNotFound)
                            {
                                
                                AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.NotFound, ErrorMessage.FileNotFound);
                                return Content(HttpStatusCode.NotFound, searchDocumentAllMetaDataVersions);
                            }
                            else
                            {
                                //Any other failures/exceptions in search functionality
                                AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, searchDocumentAllMetaDataVersions.ErrorMessage);
                                return Content(HttpStatusCode.InternalServerError, searchDocumentAllMetaDataVersions);
                            }
                        }
                        catch (Exception e)
                        {
                            Log.ErrorFormat("Error in api/SearchDocument DDMSSearch method for MessageId - {0} :{ 1}", messageId.ToString(), e.Message);
                            searchDocumentAllMetaDataVersions.ErrorMessage = e.Message;
                            AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, searchDocumentAllMetaDataVersions.ErrorMessage);
                            return Content(HttpStatusCode.InternalServerError, searchDocumentAllMetaDataVersions);
                        }
                    }

                    try
                    {
                        Log.DebugFormat("In api/SearchDocument, before calling DDMSSearch method for MessageId - {0} - Document ID :{1} Version :{2}", messageId.ToString(), searchDocumentRequest.DocumentId.ToString(), searchDocumentRequest.Version);
                        //Fetch the document based on documentid and version number
                        searchDocumentResponse = ddmsSearchDocument.DDMSSearch(searchDocumentRequest, messageId.ToString());
                        Log.DebugFormat("In api/SearchDocument, after calling DDMSSearch method for MessageId - {0} - Document ID :{1} Version :{2}", messageId.ToString(), searchDocumentRequest.DocumentId.ToString(), searchDocumentRequest.Version);

                        if (string.IsNullOrEmpty(searchDocumentResponse.ErrorMessage))
                        {
                            keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.OK);
                            keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Success);
                            foreach (var field in keyValuePairs)
                            {
                                if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                                    HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                            }
                            return Content(HttpStatusCode.OK, searchDocumentResponse);
                        }
                        //Add error response file not found 
                        if (searchDocumentResponse.ErrorMessage == ErrorMessage.FileNotFound)
                        {
                            AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.NotFound, ErrorMessage.FileNotFound);
                            return Content(HttpStatusCode.NotFound, searchDocumentResponse);
                        }
                        else
                        {
                            AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, searchDocumentResponse.ErrorMessage);
                            return Content(HttpStatusCode.InternalServerError, searchDocumentResponse);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorFormat("Error in api/SearchDocument DDMSSearch method for MessageId - {0} :{1}", messageId.ToString(), ex.Message);
                        searchDocumentResponse.ErrorMessage = ex.Message;
                        AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, searchDocumentResponse.ErrorMessage);
                        return Content(HttpStatusCode.InternalServerError, searchDocumentResponse);
                    }
                }
                else
                {
                    Log.DebugFormat("In api/SearchDocument, DocumentId is empty for MessageId - {0} - Document ID :{1}", messageId.ToString(), searchDocumentRequest.DocumentId.ToString());
                    AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, string.Format(ErrorMessage.ValueEmpty, "documentId"));
                    return Content(HttpStatusCode.BadRequest, string.Format(ErrorMessage.ValueEmpty, "documentId"));
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error in api/SearchDocument for MessageId - {0} :{1}", messageId.ToString(), ex.Message);

                AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, ex.Message);
                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        /// <summary>
        /// DDMS API POST method to upload a document
        /// </summary>
        /// <param name="uploadDocumentRequest">byte[] content passed for Upload/Update</param>
        /// <returns>DocumentId & Version number for successful upload</returns>

        [HttpPost, Route("~/api/DDMS/"), ModelStateValidation]
        public IHttpActionResult UploadDocument([FromBody]UploadDocumentRequest uploadDocumentRequest)
        {
            Guid messageId;
            Dictionary<string, dynamic> keyValuePairs;
            
           
            //Retrieve the header parameters from request object
            RetrieveHeaders(out keyValuePairs, out messageId);
            UploadDocumentResponse uploadDocumentResponse = new UploadDocumentResponse();
            try
            {
               

                Log.DebugFormat("In api/UploadDocument, before calling DDMSUpload method for MessageId - {0}", messageId.ToString());
                // Upload or update the document to SPO library
                uploadDocumentResponse = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest, messageId.ToString());
                Log.DebugFormat("In api/UploadDocument, after calling DDMSUpload method for MessageId - {0}", messageId.ToString());
                if (string.IsNullOrEmpty(uploadDocumentResponse.ErrorMessage))
                {
                    keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.OK);
                    keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Success);
                    foreach (var field in keyValuePairs)
                    {
                        if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                            HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                    }
                    return Content(HttpStatusCode.OK, uploadDocumentResponse);
                }
                //If file is not found during update
                if (uploadDocumentResponse.ErrorMessage == ErrorMessage.FileNotFound)
                {
                    AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.NotFound, ErrorMessage.FileNotFound);
                    return Content(HttpStatusCode.NotFound, uploadDocumentResponse);
                }
                else
                {
                    AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, uploadDocumentResponse.ErrorMessage);
                    return Content(HttpStatusCode.InternalServerError, uploadDocumentResponse);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error in api/UploadDocument DDMSUpload method for MessageId - {0} :{1}", messageId.ToString(), ex.Message);
                uploadDocumentResponse.ErrorMessage = ex.Message;
                AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, uploadDocumentResponse.ErrorMessage);
                return Content(HttpStatusCode.InternalServerError, uploadDocumentResponse);
            }
        }

        /// <summary>
        /// DDMS API DELTE Method to Search a document
        /// </summary>
        /// <param name="documentId">Mandatory attribute for Delete</param>
        /// <param name="version">Specific version to delete</param>
        /// <returns></returns>
        [HttpDelete, Route("~/api/DDMS/{DocumentId:Guid}/"), Route("~/api/DDMS/{DocumentId:Guid}/{Version:decimal}")]
        public IHttpActionResult DeleteDocument(Guid documentId, string version = null)
        {
            Guid messageId;
            Dictionary<string, dynamic> keyValuePairs;
            //Retrieve the header parameters from request object
            RetrieveHeaders(out keyValuePairs, out messageId);
            DeleteDocumentRequest deleteDocumentRequest = new DeleteDocumentRequest();
            DeleteDocumentResponse deleteDocumentResponse = new DeleteDocumentResponse();
            try
            {
                Log.DebugFormat("In api/DeleteDocument method for MessageId - {0}", messageId.ToString());
                //Assign the documentid to request model
                deleteDocumentRequest.DocumentId = documentId;
                //Assign the version number to request model
                deleteDocumentRequest.Version = version;
                Log.DebugFormat("In api/DeleteDocument for MessageId - {0} before calling DDMSDelete DocumentId :{1}", messageId.ToString(), deleteDocumentRequest.DocumentId.ToString());
                //Call Delete method
                deleteDocumentResponse = ddmsDeleteDocument.DDMSDelete(deleteDocumentRequest, messageId.ToString());
                Log.DebugFormat("In api/DeleteDocument for MessageId - {0} after calling DDMSDelete DocumentId :{1}", messageId.ToString(), deleteDocumentRequest.DocumentId.ToString());

                if (string.IsNullOrEmpty(deleteDocumentResponse.ErrorMessage))
                {
                    //Add response headers
                    keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.OK);
                    keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Success);
                    foreach (var field in keyValuePairs)
                    {
                        if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                            HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                    }
                    return Content(HttpStatusCode.OK, deleteDocumentResponse);
                }
                //If file is not found based on documentId
                if (deleteDocumentResponse.ErrorMessage == ErrorMessage.FileNotFound)
                {
                    AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.NotFound, ErrorMessage.FileNotFound);
                    return Content(HttpStatusCode.NotFound, deleteDocumentResponse);
                }
                else
                {
                    AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, deleteDocumentResponse.ErrorMessage);
                    return Content(HttpStatusCode.InternalServerError, deleteDocumentResponse);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error in api/DeleteDocument DDMSDelete method for MessageId - {0} :{1}", messageId.ToString(), ex.Message);
                deleteDocumentResponse.ErrorMessage = ex.Message;
                AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, deleteDocumentResponse.ErrorMessage);
                return Content(HttpStatusCode.InternalServerError, deleteDocumentResponse);
            }
        }
        /// <summary>
        /// Method to retrieve the headers from Request object
        /// </summary>
        /// <param name="keyValuePairs">Dictionary to store the headers</param>
        /// <param name="messageId">Unique Id (GUID) passed in each request</param>
        private void RetrieveHeaders(out Dictionary<string, dynamic> keyValuePairs, out Guid messageId)
        {
            messageId = Guid.Parse(Request.Headers.GetValues(HeaderConstants.MessageId).First().ToString());
            keyValuePairs = new Dictionary<string, dynamic>();
            keyValuePairs.Add(HeaderConstants.Code, HeaderErrorConstants.CodeSender);
            keyValuePairs.Add(HeaderConstants.MessageId, messageId.ToString());
            keyValuePairs.Add(HeaderConstants.SiteId, Request.Headers.GetValues(HeaderConstants.SiteId).First().ToString());
            keyValuePairs.Add(HeaderConstants.BusinessId, Request.Headers.GetValues(HeaderConstants.BusinessId).First().ToString());
            keyValuePairs.Add(HeaderConstants.Node, Request.GetRequestContext().VirtualPathRoot);
            keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
        }

        /// <summary>
        /// Method to add error codes and messages to Response Headers
        /// </summary>
        /// <param name="keyValuePairs">Dictionary to store the headers</param>
        /// <param name="statusCode">Error status code</param>
        /// <param name="errorMessage">Error message</param>
        private static void AddErrorResponseHeaders(Dictionary<string, dynamic> keyValuePairs, int statusCode, string errorMessage)
        {
            keyValuePairs.Add(HeaderConstants.ErrorCode, statusCode);
            keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Failed);
            keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
            keyValuePairs.Add(HeaderConstants.ErrorDescription, errorMessage);
            foreach (var field in keyValuePairs)
            {
                if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                    HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
            }
        }
    }
}
