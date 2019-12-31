using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDMS.WebService.Models;
using Newtonsoft.Json.Linq;

namespace DDMS.WebService.ExternalServices.Interfaces
{
    public interface IDDMSUploadDocument
    {
        UploadDocumentResponse DDMSUpload(UploadDocumentRequest uploadDocumentRequest);
        //UploadDocumentResponse DDMSUploadUsingRestApi(UploadDocumentRequest uploadDocumentRequest);
    }
}
