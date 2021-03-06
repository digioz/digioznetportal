﻿using System.Collections.Generic;

namespace digiozPortal.Payment
{
    public interface IMakePayment
    {
        PayResponse ProcessPayment(Pay pay, PaymentTypeKey key, List<PayLineItem> payLineItems);
        PayResponse ProcessPayment(Pay pay, PaymentType payType, PaymentTypeKey key, List<PayLineItem> payLineItems);
    }
}
