using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Ploeh.AutoFixture;
using DDMS.WebService.SPOActions;
using DDMS.WebService.Models;
using Microsoft.SharePoint.Client;
using DDMS.WebService.Models.Common;
//using AutoFixture.AutoMoq;
namespace DDMSWebServiceTest
{
    [TestClass]
    public class DDMSUploadDocumentTest:BaseUnitTester<DDMSUploadDocument>
    {
        [TestMethod]
        public void UploadDocument_FileNotFound_Exception()
        {

            var uploadDocumentRequest = Fixture.Create<UploadDocumentRequest>();


            var uploadDocumentResponse = Fixture.Create<UploadDocumentResponse>();

            var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

            var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

            Assert.IsTrue(uploadDocumentResponse2.ErrorMessage == ErrorMessage.FileNotFound);
        }

        [TestMethod]
        public void UploadDocument_New()
        {

            var uploadDocumentRequest = new UploadDocumentRequest();

            uploadDocumentRequest.DocumentName = Fixture.Create<String>();

            Fixture.Create<UploadDocumentRequest>();


            //var uploadDocumentResponse = Fixture.Create<uploadDocumentResponse>();

            //var ddmsUploadDocument = Substitute.For<DDMSUploadDocument>();

            //var uploadDocumentResponse2 = ddmsUploadDocument.DDMSUpload(uploadDocumentRequest);

            //Assert.IsTrue(Guid.Empty(uploadDocumentResponse2.DocumentId));

        }

    }
}
