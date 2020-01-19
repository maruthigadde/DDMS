using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDMS.WebService.Models
{
    [ExcludeFromCodeCoverage]
    public class DeleteDocumentResponse
    {
        #region Private Fields

        private string _errormessage;
        #endregion

        #region Public Fields
        
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
