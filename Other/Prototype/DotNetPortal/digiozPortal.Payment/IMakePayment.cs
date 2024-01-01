using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace digiozPortal.Payment
{
    interface IMakePayment
    {
        PayResponse ProcessPayment(Pay pay, PaymentTypeKey key, List<PayLineItem> payLineItems);
    }
}
