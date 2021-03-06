﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDMS.WebService.Models
{
    [ExcludeFromCodeCoverage]
    public class SearchDocumentResponse
    {
        #region Private Fields
        private string _documentname;
        private string _version;
        private byte[] _documentcontent;
        private string _dealernumber;
        private string _requestuser;
        private string _errormessage;
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
