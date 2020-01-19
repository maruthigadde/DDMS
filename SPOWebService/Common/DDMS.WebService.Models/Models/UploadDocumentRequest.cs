using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DDMS.WebService.Models
{
    [ExcludeFromCodeCoverage]
    public class UploadDocumentRequest
    {
        #region Private Fields
        private Guid _documentid;
        private string _documentname;
        private byte[] _documentcontent;
        private string _dealernumber;
        private string _requestuser;

        #endregion

        #region Public Fields
        public Guid DocumentId
        {
            get
            {
                return this._documentid;
            }
            set
            {
                this._documentid = value;
            }
        }
        [Required(AllowEmptyStrings = false, ErrorMessage = "DocumentName is Missing or DocumentName is invalid")]
        [RegularExpression(@"^[a-zA-Z0-9_\~\!\@\#\$\%\^\&\(\)\-\`\+\=\{\[\}\]\;\'\.\ \,]{1,400}\.(doc|ppt|xls|docx|pptx|xlsx|pdf|zip|jpg|tif|gif|DOC|PPT|XLS|PDF|ZIP|JPG|TIF|GIF|DOCX|PPTX|XLSX)$", ErrorMessage = @"Document name should be of max 400 characters and special-characters like '/*\<|>?""' are not allowed")]
        public string DocumentName
        {
            get
            {
                return this._documentname;
            }
            set
            {
                this._documentname = value;
            }
        }
        [Required(ErrorMessage = "DocumentContent is missing")]
        public byte[] DocumentContent
        {
            get
            {
                return this._documentcontent;
            }
            set
            {
                this._documentcontent = value;
            }
        }
        public string DealerNumber
        {
            get
            {
                return this._dealernumber;
            }
            set
            {
                this._dealernumber = value;
            }
        }
        public string RequestUser
        {
            get
            {
                return this._requestuser;
            }
            set
            {
                this._requestuser = value;
            }
        }

       

        #endregion
    }
}
