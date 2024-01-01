using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace digioz.Portal.Payment
{
    public class PayResponse
    {
        public string TrxDescription { get; set; }
        public bool TrxApproved { get; set; }
        public string TrxAuthorizationCode { get; set; }
        public string TrxMessage { get; set; }
        public string TrxResponseCode { get; set; }
        public string TrxID { get; set; }
    }
}
