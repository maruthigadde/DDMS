using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DDMS.WebService.Models.Honda
{
    public class DeleteDocumentRequest
    {
        #region Private Fields
        private Guid _documentid;
        private string _version;
        #endregion

        #region Public Fields
        [Required(AllowEmptyStrings = false, ErrorMessage = "DocumentId is mandatory")]
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

        public string Version
        {
            get
            {
                return this._version;
            }
            set
            {
                this._version = value;
            }
        }
        #endregion
    }
}
