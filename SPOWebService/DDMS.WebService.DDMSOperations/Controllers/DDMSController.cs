using DDMS.WebService.Constants;
using DDMS.WebService.ExternalServices.Interfaces;
using DDMS.WebService.Models;
using log4net;
using SPOService.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace DDMS.WebService.DDMSOperations.Controllers
{
    [ExcludeFromCodeCoverage]
    [Authorize(Roles = "DDMS")]
    [HeaderValidate]
    public class DDMSController : ApiController
    {
        private IDDMSSearchDocument ddmsSearchDocument;
        private IDDMSDeleteDocument ddmsDeleteDocument;
        private IDDMSUploadDocument ddmsUploadDocument;
        private static readonly ILog Log = log4net.LogManager.GetLogger(typeof(DDMSController));

        public DDMSController(IDDMSSearchDocument dDMSSearchDocument, IDDMSDeleteDocument dDMSDeleteDocument, IDDMSUploadDocument dDMSUploadDocument)
        {
            ddmsSearchDocument = dDMSSearchDocument;
            ddmsDeleteDocument = dDMSDeleteDocument;
            ddmsUploadDocument = dDMSUploadDocument;
        }

        [HttpGet, Route("~/api/DDMS/{DocumentId:Guid}/"), Route("~/api/DDMS/{DocumentId:Guid}/{Version:decimal}")]
        public IHttpActionResult SearchDocument(Guid documentId, string version = null)
        {
            Guid messageId;
            Dictionary<string, dynamic> keyValuePairs;
            RetrieveHeaders(out keyValuePairs, out messageId);
            SearchDocumentRequest searchDocumentRequest = new SearchDocumentRequest();
            SearchDocumentResponse searchDocumentResponse = new SearchDocumentResponse();
            try
            {
                Logger.LogSetup(messageId.ToString());
                Log.Info("In api/SearchDocument method");
                searchDocumentRequest.DocumentId = documentId;
                searchDocumentRequest.Version = version;

                if (searchDocumentRequest.DocumentId != Guid.Empty)
                {
                    if ((string.IsNullOrEmpty(searchDocumentRequest.Version) || string.IsNullOrWhiteSpace(searchDocumentRequest.Version)))
                    {
                        SearchDocumentAllMetaDataVersions searchDocumentAllMetaDataVersions = new SearchDocumentAllMetaDataVersions();
                        try
                        {
                            Log.Info("In api/SearchDocument, before calling DDMSSearchAllOldVersions method- Document ID :" + searchDocumentRequest.DocumentId.ToString());
                            searchDocumentAllMetaDataVersions = ddmsSearchDocument.DDMSSearchAllOldVersions(searchDocumentRequest);
                            Log.Info("In api/SearchDocument, after calling DDMSSearchAllOldVersions method");
                            if (string.IsNullOrEmpty(searchDocumentAllMetaDataVersions.ErrorMessage))
                            {

                                keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.OK);
                                keyValuePairs.Add(HeaderConstants.Status, HeaderValueConstants.Success);
                                foreach (var field in keyValuePairs)
                                {
                                    if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                                        HttpContext.Current.Response.Headers.Add(field.Key, field.Value.ToString());
                                }
                                return Content(HttpStatusCode.OK, searchDocumentAllMetaDataVersions);
                            }

                            if (searchDocumentAllMetaDataVersions.ErrorMessage == ErrorMessage.FileNotFound)
                            {
                                AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.NotFound, ErrorMessage.FileNotFound);
                                return Content(HttpStatusCode.NotFound, searchDocumentAllMetaDataVersions);
                            }
                            else
                            {
                                AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, searchDocumentAllMetaDataVersions.ErrorMessage);
                                return Content(HttpStatusCode.InternalServerError, searchDocumentAllMetaDataVersions);
                            }
                        }
                        catch (Exception e)
                        {
                            Log.ErrorFormat("Error in api/SearchDocument DDMSSearch method :{0}", e.Message);
                            searchDocumentAllMetaDataVersions.ErrorMessage = e.Message;
                            AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, searchDocumentAllMetaDataVersions.ErrorMessage);
                            return Content(HttpStatusCode.InternalServerError, searchDocumentAllMetaDataVersions);
                        }
                    }

                    try
                    {
                        Log.DebugFormat("In api/SearchDocument, before calling DDMSSearch method- Document ID :{0} Version :{1}", searchDocumentRequest.DocumentId.ToString(), searchDocumentRequest.Version);
                        searchDocumentResponse = ddmsSearchDocument.DDMSSearch(searchDocumentRequest);
                        Log.Info("In api/SearchDocument, after calling DDMSSearch method");

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
                        Log.ErrorFormat("Error in api/SearchDocument DDMSSearch method :{0}", ex.Message);
                        searchDocumentResponse.ErrorMessage = ex.Message;
                        AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, searchDocumentResponse.ErrorMessage);
                        return Content(HttpStatusCode.InternalServerError, searchDocumentResponse);
                    }
                }
                else
                {
                    Log.Info("In api/SearchDocument, DocumentId is empty - Document ID :" + searchDocumentRequest.DocumentId.ToString());
                    AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, string.Format(ErrorMessage.ValueEmpty, "documentId"));
                    return Content(HttpStatusCode.BadRequest, string.Format(ErrorMessage.ValueEmpty, "documentId"));
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error in api/SearchDocument :{0}", ex.Message);

                AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, ex.Message);
                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost, Route("~/api/DDMS/"), ModelStateValidation]
        public IHttpActionResult UploadDocument([FromBody]UploadDocumentRequest uploadDocumentRequest)
        {
            Guid messageId;
            Dictionary<string, dynamic> keyValuePairs;
            RetrieveHeaders(out keyValuePairs, out messageId);
            UploadDocumentResponse uploadDocumentResponse = new UploadDocumentResponse();
            try
            {
                Logger.LogSetup(messageId.ToString());
                Log.Info("In api/UploadDocument method");

                uploadDocumentResponse = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

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
                Log.ErrorFormat("Error in api/UploadDocument DDMSUpload method :{0}", ex.Message);
                uploadDocumentResponse.ErrorMessage = ex.Message;
                AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, uploadDocumentResponse.ErrorMessage);
                return Content(HttpStatusCode.InternalServerError, uploadDocumentResponse);
            }
        }

        [HttpDelete, Route("~/api/DDMS/{DocumentId:Guid}/"), Route("~/api/DDMS/{DocumentId:Guid}/{Version:decimal}")]
        public IHttpActionResult DeleteDocument(Guid documentId, string version = null)
        {
            Guid messageId;
            Dictionary<string, dynamic> keyValuePairs;
            RetrieveHeaders(out keyValuePairs, out messageId);
            DeleteDocumentRequest deleteDocumentRequest = new DeleteDocumentRequest();
            DeleteDocumentResponse deleteDocumentResponse = new DeleteDocumentResponse();
            try
            {
                Logger.LogSetup(messageId.ToString());
                Log.Info("In api/DeleteDocument method");

                deleteDocumentRequest.DocumentId = documentId;
                deleteDocumentRequest.Version = version;
                Log.Info("In api/DeleteDocument before calling DDMSDelete");
                deleteDocumentResponse = ddmsDeleteDocument.DDMSDelete(deleteDocumentRequest);
                Log.Info("In api/DeleteDocument after calling DDMSDelete");

                if (string.IsNullOrEmpty(deleteDocumentResponse.ErrorMessage))
                {
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
                Log.ErrorFormat("Error in api/DeleteDocument DDMSDelete method :{0}", ex.Message);
                deleteDocumentResponse.ErrorMessage = ex.Message;
                AddErrorResponseHeaders(keyValuePairs, (int)HttpStatusCode.InternalServerError, deleteDocumentResponse.ErrorMessage);
                return Content(HttpStatusCode.InternalServerError, deleteDocumentResponse);
            }
        }
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
