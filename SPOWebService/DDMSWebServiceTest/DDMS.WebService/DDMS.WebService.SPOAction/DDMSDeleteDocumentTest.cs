using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDMS.WebService.SPOActions;
using DDMS.WebService.Models;
using Microsoft.SharePoint.Client;
using DDMS.WebService.Models.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Ploeh.AutoFixture;

namespace DDMSWebServiceTest
{
    [TestClass]
    public class DDMSDeleteDocumentTest : BaseUnitTester<DDMSDeleteDocument>
    {
        Guid documentId = Guid.Empty;
        string version = string.Empty;

        [TestMethod]
        public void DeleteDocumentByVersion_FileNotFound_Exception()
        {

            var deleteDocumentRequest = Fixture.Create<DeleteDocumentRequest>();


            var deleteDocumentResponse = Fixture.Create<DeleteDocumentResponse>();

            var ddmsDeleteDocument = Substitute.For<DDMSDeleteDocument>();

            var deleteDocumentResponse2 = ddmsDeleteDocument.DDMSDelete(deleteDocumentRequest);

            Assert.IsTrue(deleteDocumentResponse2.ErrorMessage == ErrorMessage.FileNotFound);
        }

        [TestMethod]
        public void DeleteDocumentAllVersion_FileNotFound_Exception()
        {

            var deleteDocumentRequest = new DeleteDocumentRequest();
            deleteDocumentRequest.DocumentId = Fixture.Create<Guid>();

            var deleteDocumentResponse = Fixture.Create<DeleteDocumentResponse>();

            var ddmsDeleteDocument = Substitute.For<DDMSDeleteDocument>();

            var deleteDocumentResponse2 = ddmsDeleteDocument.DDMSDelete(deleteDocumentRequest);

            Assert.IsTrue(deleteDocumentResponse2.ErrorMessage == ErrorMessage.FileNotFound);
        }

        [TestMethod]
        public void DeleteDocumentAllVersions()
        {

            var deleteDocumentRequest = new DeleteDocumentRequest();
            deleteDocumentRequest.DocumentId = documentId;

            var deleteDocumentResponse = Fixture.Create<DeleteDocumentResponse>();

            var ddmsDeleteDocument = Substitute.For<DDMSDeleteDocument>();

            var deleteDocumentResponse2 = ddmsDeleteDocument.DDMSDelete(deleteDocumentRequest);

            Assert.IsTrue(!string.IsNullOrEmpty(deleteDocumentResponse2.ErrorMessage));
        }

        [TestMethod]
        public void DeleteDocumentByVersions()
        {

            var deleteDocumentRequest = new DeleteDocumentRequest();
            deleteDocumentRequest.DocumentId = documentId;
            deleteDocumentRequest.Version = Fixture.Create<decimal>().ToString();

            var deleteDocumentResponse = Fixture.Create<DeleteDocumentResponse>();

            var ddmsDeleteDocument = Substitute.For<DDMSDeleteDocument>();

            var deleteDocumentResponse2 = ddmsDeleteDocument.DDMSDelete(deleteDocumentRequest);

            Assert.IsTrue(!string.IsNullOrEmpty(deleteDocumentResponse2.ErrorMessage));
        }
    }
}
