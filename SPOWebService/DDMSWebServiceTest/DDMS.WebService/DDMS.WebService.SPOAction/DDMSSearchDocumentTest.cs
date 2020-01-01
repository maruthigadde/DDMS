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
    public class DDMSSearchDocumentTest : BaseUnitTester<DDMSSearchDocument>
    {
        [TestMethod]
        public void SearchDocument_FileNotFound_Exception()
        {

            var searchDocumentRequest = Fixture.Create<SearchDocumentRequest>();


            var searchDocumentResponse = Fixture.Create<SearchDocumentResponse>();

            var ddmsSearchDocument = Substitute.For<DDMSSearchDocument>();

            var searchDocumentRequest2 = ddmsSearchDocument.DDMSSearch(searchDocumentRequest);

            Assert.IsTrue(searchDocumentRequest2.ErrorMessage == ErrorMessage.FileNotFound);
        }
    }
}
