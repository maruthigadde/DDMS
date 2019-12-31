using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDMS.WebService.Models;

namespace DDMS.WebService.ExternalServices.Interfaces
{
    public interface IDDMSSearchDocument
    {
        SearchDocumentResponse DDMSSearch(SearchDocumentRequest hondaSearchDocumentRequest);
        List<SearchDocumentAllVersionsResponse> DDMSSearchAllOldVersions(SearchDocumentRequest hondaSearchDocumentRequest);
        //SearchDocumentResponse SearchDocumentByVersion(SearchDocumentRequest hondaSearchDocumentRequest);
    }
}
