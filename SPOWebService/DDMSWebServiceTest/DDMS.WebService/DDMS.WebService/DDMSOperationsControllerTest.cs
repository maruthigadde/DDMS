using DDMS.WebService.DDMSOperations.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NSubstitute;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web;

namespace SPOServiceUnitTests.DDMS.WebService.DDMS.WebService
{
    [TestClass]
    class DDMSOperationsControllerTest
    {
        //public Moq.Mock<HttpContextBase> HttpContext { get; set; }
        //public Moq.Mock<HttpRequestBase> Request { get; set; }
        //public RouteData RouteData { get; set; }
        //       public void SearchDocument_Do_Success()
        //       {
        //           var paymentResponse = Fixture.Create<Http>();
        //           paymentResponse.Response.ET_RETURN.FirstOrDefault().TYPE = "S";

        //           var bankOfIcelandiaMessage = Fixture.Create<PaymentsEntity>();
        //           var targetConfiguration = Fixture.Create<TargetConfiguration>();
        //           targetConfiguration.ResponseConfigurations.SapConnectorConfig.SuccessResponseType = "S";
        //           targetConfiguration.ResponseConfigurations.SapConnectorConfig.firstlevel_SendSequence = 1;

        //targetConfiguration.ResponseConfigurations.SapConnectorConfig.secondlevel_reject_SendSequence = 2;
        //           targetConfiguration.ResponseConfigurations.SapConnectorConfig.secondlevel_accept_SendSequence = 3;
        //           targetConfiguration.PainField.FirstOrDefault().Name = BuilderConstants.BicOrBeiColumn;
        //           targetConfiguration.PainField.FirstOrDefault().DefaultValue = "BankOfIcelandia";

        //           var outMessage = new OutMessage();
        //           XmlSerializer serializer = new XmlSerializer(typeof(OutMessage));
        //           byte[] data;
        //           using (var ms = new MemoryStream())
        //           {
        //               serializer.Serialize(ms, outMessage);
        //               data = ms.ToArray();
        //           }
        //           XmlData testXmlData = new XmlData { FileName = "test", ByteArray = data };
        //           SutFactory.Dependency<ISapProxy>().PostRequest(Arg.Any<SapPaymentRequest>()).Returns(paymentResponse);
        //           var res = SutFactory.Sut.Send(testXmlData, bankOfIcelandiaMessage, targetConfiguration);
        //           Assert.IsTrue(res.IsSuccess);
        //       }

        [TestMethod]
        public void HomeControllerReturnsIndexViewWhenUserIsAdmin()
        {
           // var ddmsOperationsController = new DDMSOperationsController();
           //// ddmsOperationsController.DeleteDocument

           // var userMock = new Mock<IPrincipal>();
           // userMock.Setup(p => p.IsInRole("DDMS")).Returns(true);

           // var contextMock = new Mock<HttpConte>();
           // contextMock.SetupGet(ctx => ctx.User).Returns(userMock.Object);

           // var controllerContextMock = new Mock<ControllerContext>();
           // controllerContextMock.ExpectGet(con => con.HttpContext)
           //                      .Returns(contextMock.Object);

           // ddmsOperationsController.ControllerContext = controllerContextMock.Object;
           // var result = ddmsOperationsController.Index();
           // userMock.Verify(p => p.IsInRole("admin"));
           // Assert.AreEqual(((ViewResult)result).ViewName, "Index");
        }


    }
}
