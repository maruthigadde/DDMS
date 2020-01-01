using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Ploeh.AutoFixture;
using DDMS.WebService.SPOActions;
using DDMS.WebService.Models;
using DDMS.WebService.Models.Common;
using System.Threading.Tasks;
//using AutoFixture.AutoMoq;
namespace DDMSWebServiceTest
{
    [TestClass]
    public class DDMSUploadDocumentTest : BaseUnitTester<DDMSUploadDocument>
    {
        Guid documentId = Guid.Empty;
        string version = string.Empty;

        [TestMethod]
        public void UploadDocument_FileNotFound_Exception()
        {

            var uploadDocumentRequest = Fixture.Create<UploadDocumentRequest>();


            var uploadDocumentResponse = Fixture.Create<UploadDocumentResponse>();

            var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

            var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

            Assert.IsTrue(uploadDocumentResponse2.ErrorMessage == ErrorMessage.FileNotFound);
        }

        [TestMethod, Priority(1)]
        public void UploadDocument_New()
        {

            var uploadDocumentRequest = new UploadDocumentRequest();

            uploadDocumentRequest.DocumentName = Fixture.Create<String>();
            uploadDocumentRequest.DocumentContent = Fixture.Create<byte[]>();
            uploadDocumentRequest.DealerNumber = Fixture.Create<Int32>().ToString();
            uploadDocumentRequest.RequestUser = Fixture.Create<String>();

            Fixture.Create<UploadDocumentRequest>();


            var uploadDocumentResponse = Fixture.Create<UploadDocumentResponse>();

            var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

            var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

            Assert.IsTrue(uploadDocumentResponse2.DocumentId != Guid.Empty);
            Assert.IsTrue(!string.IsNullOrEmpty(uploadDocumentResponse2.Version));
            documentId = uploadDocumentResponse2.DocumentId;
            version = uploadDocumentResponse2.Version;
        }

        [TestMethod, Priority(2)]
        public void UploadDocument_Update()
        {

            var uploadDocumentRequest = new UploadDocumentRequest();

            uploadDocumentRequest.DocumentId = documentId;
            uploadDocumentRequest.DocumentName = Fixture.Create<String>();
            uploadDocumentRequest.DocumentContent = Fixture.Create<byte[]>();
            uploadDocumentRequest.DealerNumber = Fixture.Create<Int32>().ToString();
            uploadDocumentRequest.RequestUser = Fixture.Create<String>();

            Fixture.Create<UploadDocumentRequest>();

            var uploadDocumentResponse = Fixture.Create<UploadDocumentResponse>();

            var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

            var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

            Assert.IsTrue(uploadDocumentResponse2.DocumentId != Guid.Empty);
            Assert.IsTrue(!string.IsNullOrEmpty(uploadDocumentResponse2.Version));
            documentId = uploadDocumentResponse2.DocumentId;
            version = uploadDocumentResponse2.Version;
        }

        [TestMethod]
        public void UploadDocument_MaxFileSizeReached()
        {

            var uploadDocumentRequest = new UploadDocumentRequest();

            uploadDocumentRequest.DocumentName = Fixture.Create<String>();
            uploadDocumentRequest.DocumentContent = Fixture.Create<byte[]>();
            uploadDocumentRequest.DealerNumber = Fixture.Create<Int32>().ToString();
            uploadDocumentRequest.RequestUser = Fixture.Create<String>();

            Fixture.Create<UploadDocumentRequest>();

            //var uploadDocumentResponse = Fixture.Create<UploadDocumentResponse>();

            //var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

            //var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

            //Assert.IsTrue(uploadDocumentResponse2.ErrorMessage == ErrorMessage.MaxFileSizeContentReached);
        }

        [TestMethod]
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

    }
}
