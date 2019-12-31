using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DDMS.WebService.Models;
using DDMS.WebService.Models.Common;
using DDMS.WebService.ExternalServices.Interfaces;
using System.Web;
using log4net;
using SPOService.LogManager;

namespace DDMS.WebService.DDMSOperations.Controllers
{
    //[Authorize(Roles="DDMS")]
    [HeaderValidate]
    public class DDMSOperationsController : ApiController
    {
        private IDDMSSearchDocument ddmsSearchDocument;
        private IDDMSDeleteDocument ddmsDeleteDocument;
        private IDDMSUploadDocument ddmsUploadDocument;
        private static readonly ILog Log = log4net.LogManager.GetLogger(typeof(DDMSOperationsController));

        public DDMSOperationsController(IDDMSSearchDocument dDMSSearchDocument, IDDMSDeleteDocument dDMSDeleteDocument, IDDMSUploadDocument dDMSUploadDocument)
        {
            ddmsSearchDocument = dDMSSearchDocument;
            ddmsDeleteDocument = dDMSDeleteDocument;
            ddmsUploadDocument = dDMSUploadDocument;
        }

        [HttpGet, Route("~/api/DDMS/{DocumentId:Guid}/"), Route("~/api/DDMS/{DocumentId:Guid}/{Version:decimal}")]
        public IHttpActionResult SearchDocument(Guid documentId, string version = null)
        {
            Guid messageId = Guid.Empty;
            Dictionary<string, dynamic> keyValuePairs = new Dictionary<string, dynamic>();
            SearchDocumentRequest searchDocumentRequest = new SearchDocumentRequest();
            SearchDocumentResponse searchDocumentResponse = new SearchDocumentResponse();
            try
            {
                messageId = Guid.Parse(Request.Headers.GetValues(HeaderConstants.MessageId).First().ToString());
                keyValuePairs.Add(HeaderConstants.Code, HeaderErrorConstants.CodeSender);
                keyValuePairs.Add(HeaderConstants.MessageId, messageId.ToString());
                keyValuePairs.Add(HeaderConstants.SiteId, Request.Headers.GetValues(HeaderConstants.SiteId).First().ToString());
                keyValuePairs.Add(HeaderConstants.BusinessId, Request.Headers.GetValues(HeaderConstants.BusinessId).First().ToString());
                keyValuePairs.Add(HeaderConstants.Node, Request.GetRequestContext().VirtualPathRoot);

                Logger.LogSetup(messageId.ToString());
                Log.Info("In api/SearchDocument method");
                searchDocumentRequest.DocumentId = documentId;
                searchDocumentRequest.Version = version;

                if (searchDocumentRequest.DocumentId != Guid.Empty)
                {
                    if ((string.IsNullOrEmpty(searchDocumentRequest.Version) || string.IsNullOrWhiteSpace(searchDocumentRequest.Version)))
                    {
                        List<SearchDocumentAllVersionsResponse> searchDocumentAllVersionsResponse = new List<SearchDocumentAllVersionsResponse>();
                        try
                        {
                            Log.Info("In api/SearchDocument, before calling DDMSSearchAllOldVersions method- Document ID :" + searchDocumentRequest.DocumentId.ToString());
                            searchDocumentAllVersionsResponse = ddmsSearchDocument.DDMSSearchAllOldVersions(searchDocumentRequest);
                            Log.Info("In api/SearchDocument, after calling DDMSSearchAllOldVersions method");
                            keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                            keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.OK);
                            keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Success);
                            foreach (var field in keyValuePairs)
                            {
                                if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                                    HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                            }
                            return Content(HttpStatusCode.OK, searchDocumentAllVersionsResponse);
                        }
                        catch (Exception e)
                        {
                            Log.ErrorFormat("Error in api/SearchDocument :{0}", e.Message);
                            if (e.Message == ErrorMessage.FileNotFound)
                            {
                                keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                                keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.NotFound);
                                keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Failed);
                                keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                                keyValuePairs.Add(HeaderConstants.ErrorDescription, ErrorMessage.FileNotFound);
                                foreach (var field in keyValuePairs)
                                {
                                    if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                                        HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                                }
                                return Content(HttpStatusCode.NotFound, ErrorMessage.FileNotFound);
                            }
                            else
                            {
                                keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                                keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.InternalServerError);
                                keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Failed);
                                keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                                keyValuePairs.Add(HeaderConstants.ErrorDescription, e.Message);
                                foreach (var field in keyValuePairs)
                                {
                                    if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                                        HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                                }
                                return Content(HttpStatusCode.InternalServerError, e.Message);
                            }
                        }
                    }

                    try
                    {
                        Log.DebugFormat("In api/SearchDocument, before calling DDMSSearch method- Document ID :{0} Version :{1}", searchDocumentRequest.DocumentId.ToString(), searchDocumentRequest.Version);
                        searchDocumentResponse = ddmsSearchDocument.DDMSSearch(searchDocumentRequest);
                        Log.Info("In api/SearchDocument, after calling DDMSSearch method");
                        
                        if (string.IsNullOrEmpty(searchDocumentResponse.ErrorMessage))
                        {

                            keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                            keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.OK);
                            keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Success);
                            foreach (var field in keyValuePairs)
                            {
                                if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                                    HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                            }
                            return Content(HttpStatusCode.OK, searchDocumentResponse);
                        }

                        if (searchDocumentResponse.ErrorMessage == ErrorMessage.FileNotFound)
                        {
                            keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                            keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.NotFound);
                            keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Failed);
                            keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                            keyValuePairs.Add(HeaderConstants.ErrorDescription, ErrorMessage.FileNotFound);
                            foreach (var field in keyValuePairs)
                            {
                                if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                                    HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                            }
                            return Content(HttpStatusCode.NotFound, searchDocumentResponse);
                        }
                        else
                        {
                            keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                            keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.InternalServerError);
                            keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Failed);
                            keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                            keyValuePairs.Add(HeaderConstants.ErrorDescription, searchDocumentResponse.ErrorMessage);
                            foreach (var field in keyValuePairs)
                            {
                                if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                                    HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                            }
                            return Content(HttpStatusCode.InternalServerError, searchDocumentResponse);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorFormat("Error in api/SearchDocument DDMSSearch method :{0}", ex.Message);
                        searchDocumentResponse.ErrorMessage = ex.Message;
                        
                        keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                        keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.InternalServerError);
                        keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Failed);
                        keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                        keyValuePairs.Add(HeaderConstants.ErrorDescription, searchDocumentResponse.ErrorMessage);
                        foreach (var field in keyValuePairs)
                        {
                            if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                                HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                        }
                        return Content(HttpStatusCode.InternalServerError, searchDocumentResponse);
                    }
                }
                else
                {
                    Log.Info("In api/SearchDocument, DocumentId is empty - Document ID :" + searchDocumentRequest.DocumentId.ToString());
                    keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                    keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.InternalServerError);
                    keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Failed);
                    keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                    keyValuePairs.Add(HeaderConstants.ErrorDescription, string.Format(ErrorMessage.ValueEmpty, "documentId"));
                    foreach (var field in keyValuePairs)
                    {
                        if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                            HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                    }
                    return Content(HttpStatusCode.BadRequest, string.Format(ErrorMessage.ValueEmpty, "documentId"));
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error in api/SearchDocument :{0}", ex.Message);

                keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.InternalServerError);
                keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Failed);
                keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                keyValuePairs.Add(HeaderConstants.ErrorDescription, ex.Message);
                foreach (var field in keyValuePairs)
                {
                    if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                        HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                }
                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost, Route("~/api/DDMS/"), ModelStateValidation]
        public IHttpActionResult UploadDocument([FromBody]UploadDocumentRequest uploadDocumentRequest)
        {
            Guid messageId = Guid.Empty;
            Dictionary<string, dynamic> keyValuePairs = new Dictionary<string, dynamic>();
            UploadDocumentResponse uploadDocumentResponse = new UploadDocumentResponse();
            try
            {
                messageId = Guid.Parse(Request.Headers.GetValues(HeaderConstants.MessageId).First().ToString());
                keyValuePairs.Add(HeaderConstants.Code, HeaderErrorConstants.CodeSender);
                keyValuePairs.Add(HeaderConstants.MessageId, messageId.ToString());
                keyValuePairs.Add(HeaderConstants.SiteId, Request.Headers.GetValues(HeaderConstants.SiteId).First().ToString());
                keyValuePairs.Add(HeaderConstants.BusinessId, Request.Headers.GetValues(HeaderConstants.BusinessId).First().ToString());
                keyValuePairs.Add(HeaderConstants.Node, Request.GetRequestContext().VirtualPathRoot);

                Logger.LogSetup(messageId.ToString());
                Log.Info("In api/UploadDocument method");

                uploadDocumentResponse = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);
                
                if (string.IsNullOrEmpty(uploadDocumentResponse.ErrorMessage))
                {
                    keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                    keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.OK);
                    keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Success);
                    foreach (var field in keyValuePairs)
                    {
                        if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                            HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                    }
                    return Content(HttpStatusCode.OK, uploadDocumentResponse);
                }

                if (uploadDocumentResponse.ErrorMessage == ErrorMessage.FileNotFound)
                {
                    keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                    keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.NotFound);
                    keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Failed);
                    keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                    keyValuePairs.Add(HeaderConstants.ErrorDescription, ErrorMessage.FileNotFound);
                    foreach (var field in keyValuePairs)
                    {
                        if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                            HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                    }
                    return Content(HttpStatusCode.NotFound, uploadDocumentResponse);
                }
                else
                {
                    keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                    keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.InternalServerError);
                    keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Failed);
                    keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                    keyValuePairs.Add(HeaderConstants.ErrorDescription, uploadDocumentResponse.ErrorMessage);
                    foreach (var field in keyValuePairs)
                    {
                        if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                            HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                    }
                    return Content(HttpStatusCode.InternalServerError, uploadDocumentResponse);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error in api/UploadDocument DDMSUpload method :{0}", ex.Message);
                uploadDocumentResponse.ErrorMessage = ex.Message;
                
                keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.InternalServerError);
                keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Failed);
                keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                keyValuePairs.Add(HeaderConstants.ErrorDescription, uploadDocumentResponse.ErrorMessage);
                foreach (var field in keyValuePairs)
                {
                    if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                        HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                }
                return Content(HttpStatusCode.InternalServerError, uploadDocumentResponse);
            }
        }

        [HttpDelete, Route("~/api/DDMS/{DocumentId:Guid}/"), Route("~/api/DDMS/{DocumentId:Guid}/{Version:decimal}")]
        public IHttpActionResult DeleteDocument(Guid documentId, string version = null)
        {
            Guid messageId = Guid.Empty;
            Dictionary<string, dynamic> keyValuePairs = new Dictionary<string, dynamic>();
            DeleteDocumentRequest deleteDocumentRequest = new DeleteDocumentRequest();
            DeleteDocumentResponse deleteDocumentResponse = new DeleteDocumentResponse();
            try
            {
                messageId = Guid.Parse(Request.Headers.GetValues(HeaderConstants.MessageId).First().ToString());
                keyValuePairs.Add(HeaderConstants.Code, HeaderErrorConstants.CodeSender);
                keyValuePairs.Add(HeaderConstants.MessageId, messageId.ToString());
                keyValuePairs.Add(HeaderConstants.SiteId, Request.Headers.GetValues(HeaderConstants.SiteId).First().ToString());
                keyValuePairs.Add(HeaderConstants.BusinessId, Request.Headers.GetValues(HeaderConstants.BusinessId).First().ToString());
                keyValuePairs.Add(HeaderConstants.Node, Request.GetRequestContext().VirtualPathRoot);

                Logger.LogSetup(messageId.ToString());
                Log.Info("In api/DeleteDocument method");

                deleteDocumentRequest.DocumentId = documentId;
                deleteDocumentRequest.Version = version;
                Log.Info("In api/DeleteDocument before calling DDMSDelete");
                deleteDocumentResponse = ddmsDeleteDocument.DDMSDelete(deleteDocumentRequest);
                Log.Info("In api/DeleteDocument after calling DDMSDelete");
                
                if (string.IsNullOrEmpty(deleteDocumentResponse.ErrorMessage))
                {
                    keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                    keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.OK);
                    keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Success);
                    foreach (var field in keyValuePairs)
                    {
                        if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                            HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                    }
                    return Content(HttpStatusCode.OK, deleteDocumentResponse);
                }

                if (deleteDocumentResponse.ErrorMessage == ErrorMessage.FileNotFound)
                {
                    keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                    keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.NotFound);
                    keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Failed);
                    keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                    keyValuePairs.Add(HeaderConstants.ErrorDescription, deleteDocumentResponse.ErrorMessage);
                    foreach (var field in keyValuePairs)
                    {
                        if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                            HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                    }
                    return Content(HttpStatusCode.NotFound, deleteDocumentResponse);
                }
                else
                {
                    keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                    keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.InternalServerError);
                    keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Failed);
                    keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                    keyValuePairs.Add(HeaderConstants.ErrorDescription, deleteDocumentResponse.ErrorMessage);
                    foreach (var field in keyValuePairs)
                    {
                        if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                            HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                    }
                    return Content(HttpStatusCode.InternalServerError, deleteDocumentResponse);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error in api/DeleteDocument DDMSDelete method :{0}", ex.Message);
                deleteDocumentResponse.ErrorMessage = ex.Message;
                
                keyValuePairs.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.InternalServerError);
                keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Failed);
                keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                keyValuePairs.Add(HeaderConstants.ErrorDescription, deleteDocumentResponse.ErrorMessage);
                foreach (var field in keyValuePairs)
                {
                    if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                        HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                }
                return Content(HttpStatusCode.InternalServerError, deleteDocumentResponse);
            }
        }
    }
}
