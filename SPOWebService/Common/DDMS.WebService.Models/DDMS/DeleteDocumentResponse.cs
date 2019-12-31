using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDMS.WebService.Models
{
    public class DeleteDocumentResponse
    {
        #region Private Fields
        //private string _correlationid;
        private string _errormessage;
        #endregion

        #region Public Fields
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
