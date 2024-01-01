using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace digioz.Portal.Payment
{
    public class PaymentTypeKey
    {
        public string LoginId { get; set; }
        public string TransactionKey { get; set; }
        public bool TestMode { get; set; }
    }
}
