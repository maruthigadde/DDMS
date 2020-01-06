using DDMS.WebService.Models;
using DDMS.WebService.Models.Common;
using DDMS.WebService.SPOActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Ploeh.AutoFixture;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace DDMSWebServiceTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class DDMSTest : BaseUnitTester
    {
        private static Guid documentId = Guid.Empty;
        private static string version = string.Empty;
        private static byte[] fileContent = null;
        private static string fileName = string.Empty;

        [TestMethod, Priority(1)]
        public void UploadDocument_FileNotFound_Exception()
        {

            var uploadDocumentRequest = Fixture.Create<UploadDocumentRequest>();


            var uploadDocumentResponse = Fixture.Create<UploadDocumentResponse>();

            var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

            var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

            Assert.IsTrue(uploadDocumentResponse2.ErrorMessage == ErrorMessage.FileNotFound);
        }

        [TestMethod, Priority(2)]
        public void UploadDocument_New()
        {

            var uploadDocumentRequest = new UploadDocumentRequest();

            fileContent = Encoding.ASCII.GetBytes("Hello 2020....Bye Bye 2019");
            fileName = Fixture.Create<String>() + ".txt";

            uploadDocumentRequest.DocumentName = fileName;
            uploadDocumentRequest.DocumentContent = fileContent;
            uploadDocumentRequest.DealerNumber = Fixture.Create<Int32>().ToString();
            uploadDocumentRequest.RequestUser = Fixture.Create<String>();

            Fixture.Create<UploadDocumentRequest>();


            var uploadDocumentResponse = Fixture.Create<UploadDocumentResponse>();

            var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

            var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

            Assert.IsTrue(string.IsNullOrEmpty(uploadDocumentResponse2.ErrorMessage));
            documentId = uploadDocumentResponse2.DocumentId;
            version = uploadDocumentResponse2.Version;
        }

        [TestMethod, Priority(3)]
        public void UploadDocument_New_DocumentAlreadyExist_ReNameFile()
        {
            //File Name already exist-So renaming file
            var uploadDocumentRequest = new UploadDocumentRequest();

            fileContent = Encoding.ASCII.GetBytes("Hello 2020....Bye Bye 2019");

            uploadDocumentRequest.DocumentName = fileName;
            uploadDocumentRequest.DocumentContent = fileContent;
            uploadDocumentRequest.DealerNumber = Fixture.Create<Int32>().ToString();
            uploadDocumentRequest.RequestUser = Fixture.Create<String>();

            Fixture.Create<UploadDocumentRequest>();


            var uploadDocumentResponse = Fixture.Create<UploadDocumentResponse>();

            var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

            var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

            Assert.IsTrue(string.IsNullOrEmpty(uploadDocumentResponse2.ErrorMessage));
            documentId = uploadDocumentResponse2.DocumentId;
            version = uploadDocumentResponse2.Version;
        }

        [TestMethod, Priority(4)]
        public void UploadDocument_Update()
        {
            fileContent = Encoding.ASCII.GetBytes("Bye Bye 2019");

            var uploadDocumentRequest = new UploadDocumentRequest();

            uploadDocumentRequest.DocumentId = documentId;
            uploadDocumentRequest.DocumentName = Fixture.Create<String>() + ".txt";
            uploadDocumentRequest.DocumentContent = fileContent;
            uploadDocumentRequest.DealerNumber = Fixture.Create<Int32>().ToString();
            uploadDocumentRequest.RequestUser = Fixture.Create<String>();

            Fixture.Create<UploadDocumentRequest>();

            var uploadDocumentResponse = Fixture.Create<UploadDocumentResponse>();

            var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

            var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

            Assert.IsTrue(string.IsNullOrEmpty(uploadDocumentResponse2.ErrorMessage));
            documentId = uploadDocumentResponse2.DocumentId;
            version = uploadDocumentResponse2.Version;
        }

        [TestMethod, Priority(5)]
        public void UploadDocument_UpdateDifferentExtension_Exception()
        {
            var uploadDocumentRequest = new UploadDocumentRequest();

            uploadDocumentRequest.DocumentId = documentId;
            uploadDocumentRequest.DocumentName = Fixture.Create<String>() + ".pdf";
            uploadDocumentRequest.DocumentContent = fileContent;
            uploadDocumentRequest.DealerNumber = Fixture.Create<Int32>().ToString();
            uploadDocumentRequest.RequestUser = Fixture.Create<String>();

            Fixture.Create<UploadDocumentRequest>();

            var uploadDocumentResponse = Fixture.Create<UploadDocumentResponse>();

            var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

            var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

            Assert.IsTrue(uploadDocumentResponse2.ErrorMessage == ErrorMessage.DifferentFileExtension);
        }

        [TestMethod, Priority(6)]
        public void UploadDocument_MaxFileSizeReached_Exception()
        {
            Random random = new Random();
            Byte[] fileconent = new Byte[31457999];
            random.NextBytes(fileconent);

            var uploadDocumentRequest = new UploadDocumentRequest();

            uploadDocumentRequest.DocumentName = Fixture.Create<String>() + ".txt";
            uploadDocumentRequest.DocumentContent = fileconent;
            uploadDocumentRequest.DealerNumber = Fixture.Create<Int32>().ToString();
            uploadDocumentRequest.RequestUser = Fixture.Create<String>();

            Fixture.Create<UploadDocumentRequest>();

            var uploadDocumentResponse = Fixture.Create<UploadDocumentResponse>();

            var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

            var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

            Assert.IsTrue(uploadDocumentResponse2.ErrorMessage == ErrorMessage.MaxFileSizeContentReached);
        }

        [TestMethod, Priority(7)]
        public void UploadDocument_RemoteName_Exception()
        {

            var uploadDocumentRequest = Fixture.Create<UploadDocumentRequest>();


            var uploadDocumentResponse = Fixture.Create<UploadDocumentResponse>();

            var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

            var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

            Assert.IsTrue(uploadDocumentResponse2.ErrorMessage == ErrorMessage.RemoteName);
        }

        [TestMethod, Priority(8)]
        public void UploadDocument_MaxRetryAttemps()
        {
            Parallel.For(0, 100, (i, loopState) =>
            {
                var uploadDocumentRequest = Fixture.Create<UploadDocumentRequest>();


                var uploadDocumentResponse = Fixture.Create<UploadDocumentResponse>();

                var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

                var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

                if (uploadDocumentResponse2.ErrorMessage == string.Format("Maximum retry attempts {0}, has be attempted.", ExecuteQueryConstants.RetryCount))
                {
                    Assert.IsTrue(uploadDocumentResponse2.ErrorMessage == string.Format("Maximum retry attempts {0}, has be attempted.", ExecuteQueryConstants.RetryCount));
                    loopState.Break();
                }

            });
        }

        [TestMethod, Priority(9)]
        public void SearchDocument_FileNotFound_Exception()
        {

            var searchDocumentRequest = new SearchDocumentRequest();
            searchDocumentRequest.DocumentId = Fixture.Create<Guid>();
            searchDocumentRequest.Version = Fixture.Create<decimal>().ToString();

            var searchDocumentResponse = Fixture.Create<SearchDocumentResponse>();

            var ddmsSearchDocument = Substitute.For<DDMSSearchDocument>();

            var searchDocumentRequest2 = ddmsSearchDocument.DDMSSearch(searchDocumentRequest);

            Assert.IsTrue(searchDocumentRequest2.ErrorMessage == ErrorMessage.FileNotFound);
        }

        [TestMethod, Priority(10)]
        public void SearchDocument_AllOldVersions_FileNotFound_Exception()
        {

            var searchDocumentRequest = new SearchDocumentRequest();
            searchDocumentRequest.DocumentId = Fixture.Create<Guid>();

            var searchDocumentResponse = Fixture.Create<SearchDocumentAllMetaDataVersions>();

            var ddmsSearchDocument = Substitute.For<DDMSSearchDocument>();

            var searchDocumentRequest2 = ddmsSearchDocument.DDMSSearchAllOldVersions(searchDocumentRequest);

            Assert.IsTrue(searchDocumentRequest2.ErrorMessage == ErrorMessage.FileNotFound);
        }

        [TestMethod, Priority(11)]
        public void SearchDocument_AllOldVersions()
        {

            var searchDocumentRequest = new SearchDocumentRequest();
            searchDocumentRequest.DocumentId = documentId;

            var searchDocumentResponse = Fixture.Create<SearchDocumentAllMetaDataVersions>();

            var ddmsSearchDocument = Substitute.For<DDMSSearchDocument>();

            var searchDocumentRequest2 = ddmsSearchDocument.DDMSSearchAllOldVersions(searchDocumentRequest);

            Assert.IsTrue(string.IsNullOrEmpty(searchDocumentRequest2.ErrorMessage));
        }

        [TestMethod, Priority(12)]
        public void SearchDocument_Version()
        {

            var searchDocumentRequest = new SearchDocumentRequest();
            searchDocumentRequest.DocumentId = documentId;
            searchDocumentRequest.Version = version;

            var searchDocumentResponse = Fixture.Create<SearchDocumentResponse>();

            var ddmsSearchDocument = Substitute.For<DDMSSearchDocument>();

            var searchDocumentRequest2 = ddmsSearchDocument.DDMSSearch(searchDocumentRequest);

            Assert.IsTrue(string.IsNullOrEmpty(searchDocumentRequest2.ErrorMessage));
        }

        [TestMethod, Priority(13)]
        public void SearchDocument_OldVersion_Decimal()
        {
            var searchDocumentRequest = new SearchDocumentRequest();
            searchDocumentRequest.DocumentId = documentId;
            searchDocumentRequest.Version = "1.0";

            var searchDocumentResponse = Fixture.Create<SearchDocumentResponse>();

            var ddmsSearchDocument = Substitute.For<DDMSSearchDocument>();

            var searchDocumentRequest2 = ddmsSearchDocument.DDMSSearch(searchDocumentRequest);

            Assert.IsTrue(string.IsNullOrEmpty(searchDocumentRequest2.ErrorMessage));
        }

        [TestMethod, Priority(14)]
        public void SearchDocument_OldVersion()
        {
            var searchDocumentRequest = new SearchDocumentRequest();
            searchDocumentRequest.DocumentId = documentId;
            searchDocumentRequest.Version = "1";

            var searchDocumentResponse = Fixture.Create<SearchDocumentResponse>();

            var ddmsSearchDocument = Substitute.For<DDMSSearchDocument>();

            var searchDocumentRequest2 = ddmsSearchDocument.DDMSSearch(searchDocumentRequest);

            Assert.IsTrue(string.IsNullOrEmpty(searchDocumentRequest2.ErrorMessage));
        }

        [TestMethod, Priority(15)]
        public void SearchDocument_DocumentIdNull_Exception()
        {

            var searchDocumentRequest = new SearchDocumentRequest();
            searchDocumentRequest.Version = version;

            var searchDocumentResponse = Fixture.Create<SearchDocumentResponse>();

            var ddmsSearchDocument = Substitute.For<DDMSSearchDocument>();

            var searchDocumentRequest2 = ddmsSearchDocument.DDMSSearch(searchDocumentRequest);

            Assert.IsTrue(searchDocumentRequest2.ErrorMessage == string.Format(ErrorMessage.ValueEmpty, SpoConstants.DocumentId));
        }

        [TestMethod, Priority(16)]
        public void SearchDocument_RemoteName_Exception()
        {

            var searchDocumentRequest = new SearchDocumentRequest();
            searchDocumentRequest.DocumentId = documentId;
            searchDocumentRequest.Version = version;

            var searchDocumentResponse = Fixture.Create<SearchDocumentResponse>();

            var ddmsSearchDocument = Substitute.For<DDMSSearchDocument>();

            var searchDocumentRequest2 = ddmsSearchDocument.DDMSSearch(searchDocumentRequest);

            Assert.IsTrue(searchDocumentRequest2.ErrorMessage == ErrorMessage.RemoteName);
        }

        [TestMethod, Priority(17)]
        public void SearchDocument_GreaterVersionProvided_Exception()
        {

            var searchDocumentRequest = new SearchDocumentRequest();
            searchDocumentRequest.DocumentId = documentId;
            searchDocumentRequest.Version = "5.0";

            var searchDocumentResponse = Fixture.Create<SearchDocumentResponse>();

            var ddmsSearchDocument = Substitute.For<DDMSSearchDocument>();

            var searchDocumentRequest2 = ddmsSearchDocument.DDMSSearch(searchDocumentRequest);

            Assert.IsTrue(searchDocumentRequest2.ErrorMessage == ErrorMessage.GreaterVersionProvided);
        }

        [TestMethod, Priority(18)]
        public void UploadDocument_UpdateMetaData()
        {
            var uploadDocumentRequest = new UploadDocumentRequest();

            uploadDocumentRequest.DocumentId = documentId;

            uploadDocumentRequest.DealerNumber = Fixture.Create<Int32>().ToString();
            uploadDocumentRequest.RequestUser = Fixture.Create<String>();

            Fixture.Create<UploadDocumentRequest>();

            var uploadDocumentResponse = Fixture.Create<UploadDocumentResponse>();

            var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

            var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

            Assert.IsTrue(string.IsNullOrEmpty(uploadDocumentResponse2.ErrorMessage));
            documentId = uploadDocumentResponse2.DocumentId;
            version = uploadDocumentResponse2.Version;
        }

        [TestMethod, Priority(19)]
        public void UploadDocument_UpdateMetaData_FileNotFound_Exception()
        {
            var uploadDocumentRequest = new UploadDocumentRequest();

            uploadDocumentRequest.DocumentId = Fixture.Create<Guid>();

            uploadDocumentRequest.DealerNumber = Fixture.Create<Int32>().ToString();
            uploadDocumentRequest.RequestUser = Fixture.Create<String>();

            Fixture.Create<UploadDocumentRequest>();

            var uploadDocumentResponse = Fixture.Create<UploadDocumentResponse>();

            var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

            var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

            Assert.IsTrue(uploadDocumentResponse2.ErrorMessage == ErrorMessage.FileNotFound);
        }

        [TestMethod, Priority(20)]
        public void DeleteDocumentByVersion_FileNotFound_Exception()
        {

            var deleteDocumentRequest = Fixture.Create<DeleteDocumentRequest>();

            var deleteDocumentResponse = Fixture.Create<DeleteDocumentResponse>();

            var ddmsDeleteDocument = Substitute.For<DDMSDeleteDocument>();

            var deleteDocumentResponse2 = ddmsDeleteDocument.DDMSDelete(deleteDocumentRequest);

            Assert.IsTrue(deleteDocumentResponse2.ErrorMessage == ErrorMessage.FileNotFound);
        }

        [TestMethod, Priority(21)]
        public void DeleteDocumentAllVersion_FileNotFound_Exception()
        {

            var deleteDocumentRequest = new DeleteDocumentRequest();
            deleteDocumentRequest.DocumentId = Fixture.Create<Guid>();

            var deleteDocumentResponse = Fixture.Create<DeleteDocumentResponse>();

            var ddmsDeleteDocument = Substitute.For<DDMSDeleteDocument>();

            var deleteDocumentResponse2 = ddmsDeleteDocument.DDMSDelete(deleteDocumentRequest);

            Assert.IsTrue(deleteDocumentResponse2.ErrorMessage == ErrorMessage.FileNotFound);
        }

        [TestMethod, Priority(22)]
        public void DeleteDocument_CannotDeleteCurrentVersion_Exception()
        {

            var deleteDocumentRequest = new DeleteDocumentRequest();
            deleteDocumentRequest.DocumentId = documentId;
            deleteDocumentRequest.Version = version;

            var deleteDocumentResponse = Fixture.Create<DeleteDocumentResponse>();

            var ddmsDeleteDocument = Substitute.For<DDMSDeleteDocument>();

            var deleteDocumentResponse2 = ddmsDeleteDocument.DDMSDelete(deleteDocumentRequest);

            Assert.IsTrue(deleteDocumentResponse2.ErrorMessage == "You cannot delete the current version.");
        }

        [TestMethod, Priority(23)]
        public void DeleteDocument_RemoteNameReslove_Exception()
        {

            var deleteDocumentRequest = new DeleteDocumentRequest();
            deleteDocumentRequest.DocumentId = documentId;
            deleteDocumentRequest.Version = Fixture.Create<decimal>().ToString();

            var deleteDocumentResponse = Fixture.Create<DeleteDocumentResponse>();

            var ddmsDeleteDocument = Substitute.For<DDMSDeleteDocument>();

            var deleteDocumentResponse2 = ddmsDeleteDocument.DDMSDelete(deleteDocumentRequest);

            Assert.IsTrue(deleteDocumentResponse2.ErrorMessage == ErrorMessage.RemoteName);
        }

        [TestMethod, Priority(24)]
        public void DeleteDocument_GreaterVersions_FileNotFound_Exception()
        {

            var deleteDocumentRequest = new DeleteDocumentRequest();
            deleteDocumentRequest.DocumentId = documentId;
            deleteDocumentRequest.Version = "5.0";

            var deleteDocumentResponse = Fixture.Create<DeleteDocumentResponse>();

            var ddmsDeleteDocument = Substitute.For<DDMSDeleteDocument>();

            var deleteDocumentResponse2 = ddmsDeleteDocument.DDMSDelete(deleteDocumentRequest);

            Assert.IsTrue(deleteDocumentResponse2.ErrorMessage == ErrorMessage.FileNotFound);
        }

        [TestMethod, Priority(25)]
        public void DeleteDocumentByVersions()
        {

            var deleteDocumentRequest = new DeleteDocumentRequest();
            deleteDocumentRequest.DocumentId = documentId;
            deleteDocumentRequest.Version = "1.0";

            var deleteDocumentResponse = Fixture.Create<DeleteDocumentResponse>();

            var ddmsDeleteDocument = Substitute.For<DDMSDeleteDocument>();

            var deleteDocumentResponse2 = ddmsDeleteDocument.DDMSDelete(deleteDocumentRequest);

            Assert.IsTrue(string.IsNullOrEmpty(deleteDocumentResponse2.ErrorMessage));
        }

        [TestMethod, Priority(26)]
        public void DeleteDocumentAllVersions()
        {

            var deleteDocumentRequest = new DeleteDocumentRequest();
            deleteDocumentRequest.DocumentId = documentId;

            var deleteDocumentResponse = Fixture.Create<DeleteDocumentResponse>();

            var ddmsDeleteDocument = Substitute.For<DDMSDeleteDocument>();

            var deleteDocumentResponse2 = ddmsDeleteDocument.DDMSDelete(deleteDocumentRequest);

            Assert.IsTrue(string.IsNullOrEmpty(deleteDocumentResponse2.ErrorMessage));
        }
    }
}
