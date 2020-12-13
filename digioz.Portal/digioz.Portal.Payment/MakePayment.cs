using System;
using System.Collections.Generic;
using digioz.Portal.Payment.AuthNet;

namespace digioz.Portal.Payment
{
    public enum PaymentType
    {
        AuthorizeNet,
        PayPal,
        GooglePay,
        WireTransfer,
        ElectronicCheck
    }

    public class MakePayment : IMakePayment
    {
        IMakePayment _makePayment;
        public MakePayment(IMakePayment makePayment) 
        {
            _makePayment = makePayment;
        }

        public PayResponse ProcessPayment(Pay pay, PaymentTypeKey key, List<PayLineItem> payLineItems) 
        {
            throw new NotImplementedException();
        }
        public PayResponse ProcessPayment(Pay pay, PaymentType payType, PaymentTypeKey key, List<PayLineItem> payLineItems )
        {
            IMakePayment makePayment;
            PayResponse payResponse = null;

            if (payType == PaymentType.AuthorizeNet)
            {
                makePayment = new AuthNetMakePayment();

                payResponse = makePayment.ProcessPayment(pay, key, payLineItems);
            }
            if (payType == PaymentType.PayPal)
            {
                payResponse = _makePayment.ProcessPayment(pay, key, payLineItems);
            }

            return payResponse;
        }
    }
}
