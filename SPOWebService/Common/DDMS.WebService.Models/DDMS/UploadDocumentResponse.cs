using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDMS.WebService.Models
{
    [ExcludeFromCodeCoverage]
    public class UploadDocumentResponse
    {
        #region Private Fields
        private Guid _documentid;
        private string _version;
        //private string _correlationid;
        private string _errormessage;
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
        //public string CorrelationId
        //{
        //    get
        //    {
        //        return this._correlationid;
        //    }
        //    set
        //    {
        //        this._correlationid = value;
        //    }
        //}
        public string ErrorMessage
        {
            get
            {
                return this._errormessage;
            }
            set
            {
                this._errormessage = value;
            }
        }
        #endregion
    }
}
