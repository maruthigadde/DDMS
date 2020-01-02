using System;


namespace WebServiceClient.Models
{
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
