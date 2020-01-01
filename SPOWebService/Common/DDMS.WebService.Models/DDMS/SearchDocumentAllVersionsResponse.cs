using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDMS.WebService.Models
{
    [ExcludeFromCodeCoverage]
    public class SearchDocumentAllVersionsResponse
    {
        #region Private Fields
        private string _documentname;
        private string _version;
        private string _dealernumber;
        private string _requestuser;
        private string _documentumid;
        private string _documentumversion;
        //private string _correlationid;
        #endregion

        #region Public Fields
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
        public string DocumentumId
        {
            get
            {
                return this._documentumid;
            }
            set
            {
                this._documentumid = value;
            }
        }
        public string DocumentumVersion
        {
            get
            {
                return this._documentumversion;
            }
            set
            {
                this._documentumversion = value;
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
        #endregion
    }
}
