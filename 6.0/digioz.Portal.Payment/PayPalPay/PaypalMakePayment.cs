using System.Collections.Generic;
using System.Text;
//using PayPal.Api;
using System;
using digioz.Portal.Bll.Interfaces;

namespace digioz.Portal.Payment.PayPalPay
{
    public class PaypalMakePayment : IMakePayment
    {
        IConfigLogic _configLogic;
        public PaypalMakePayment(IConfigLogic configLogic) 
        {
            _configLogic = configLogic;
        }
        public PayResponse ProcessPayment(Pay pay, PaymentType payType, PaymentTypeKey key, List<PayLineItem> payLineItems) 
        {
            throw new NotImplementedException();
        }

        // American Express Test Card:  370000000000002
        // Discover Test Card:          6011000000000012
        // Visa Test Card:              4007000000027
        // Second Visa Test Card:       4012888818888
        // Third Visa Test Card:        4111111111111111
        // JCB:                         3088000000000017
        // Diners Club/ Carte Blanche:  38000000000006
        //test : 4032039225956558, 2022, 323
        public PayResponse ProcessPayment(Pay pay, PaymentTypeKey key, List<PayLineItem> payLineItems) {
            /* Assumptions: Tax=0, Shipping=0 */
            //PayResponse payResponse = new PayResponse();
            //var config = _configLogic.GetConfig();
            //Dictionary<string, string> paypalConfigs = new Dictionary<string, string>();
            //paypalConfigs.Add("clientId", config["PaypalClientId"]);
            //paypalConfigs.Add("clientSecret", config["PaypalClientSecret"]);
            //paypalConfigs.Add("mode", config["PaypalMode"]);
            //paypalConfigs.Add("connectionTimeout", config["PaypalConnectionTimeout"]);
            //string accessToken = new PayPal.OAuthTokenCredential(paypalConfigs["clientId"], paypalConfigs["clientSecret"], paypalConfigs).GetAccessToken();
            //PayPal.Api.APIContext apiContext = new PayPal.Api.APIContext(accessToken);
            //apiContext.Config = paypalConfigs;
            //Item item;
            //List<Item> itms = new List<Item>();

            //foreach (PayLineItem payLineItem in payLineItems) {
            //    item = new Item();
            //    item.name = payLineItem.Name;
            //    item.currency = "USD";
            //    item.price = payLineItem.Price.ToString();
            //    item.quantity = payLineItem.Quantity.ToString();
            //    item.sku = payLineItem.ID;
            //    itms.Add(item);
            //}

            //ItemList itemList = new ItemList();
            //itemList.items = itms;
            //ShippingAddress ship_address = new ShippingAddress();
            //ship_address.city = pay.ShippingCity;
            //ship_address.country_code = pay.ShippingCountryCode;
            //ship_address.line1 = pay.ShippingAddress;
            //ship_address.postal_code = pay.ShippingZip;
            //ship_address.state = pay.ShippingState;
            //ship_address.recipient_name = pay.FirstName + " " + pay.LastName;
            //itemList.shipping_address = ship_address;

            ////Address for the payment
            //Address billingAddress = new Address();
            //billingAddress.city = pay.BillingCity;
            //billingAddress.country_code = pay.BillingCountryCode;
            //billingAddress.line1 = pay.BillingAddress;
            //billingAddress.postal_code = pay.BillingZip;
            //billingAddress.state = pay.BillingState;

            ////Now Create an object of credit card and add above details to it
            ////Please replace your credit card details over here which you got from paypal
            //CreditCard crdtCard = new CreditCard();
            //crdtCard.billing_address = billingAddress;
            //crdtCard.cvv2 = pay.CCCardCode;
            //crdtCard.expire_month = int.Parse(pay.CCExpMonth);
            //crdtCard.expire_year = int.Parse(pay.CCExpYear);
            //crdtCard.first_name = pay.FirstName;
            //crdtCard.last_name = pay.LastName;
            //crdtCard.number = pay.CCNumber;
            //crdtCard.type = pay.CCType;
            //// Specify details of your payment amount.
            //Details details = new Details();
            //details.shipping = "0";
            //details.subtotal = pay.Total.ToString();
            //details.tax = "0";

            //Amount amnt = new Amount();
            //amnt.currency = "USD";
            //amnt.total = pay.Total.ToString();
            //amnt.details = details;

            //Transaction tran = new Transaction();
            //tran.amount = amnt;
            //tran.description = pay.Description;
            //tran.item_list = itemList;
            //tran.invoice_number = pay.ID;

            //List<Transaction> transactions = new List<Transaction>();
            //transactions.Add(tran);

            //FundingInstrument fundInstrument = new FundingInstrument();
            //fundInstrument.credit_card = crdtCard;

            //List<FundingInstrument> fundingInstrumentList = new List<FundingInstrument>();
            //fundingInstrumentList.Add(fundInstrument);

            //Payer payr = new Payer();
            //payr.funding_instruments = fundingInstrumentList;
            //payr.payment_method = "credit_card";
            //payr.payer_info = new PayerInfo {
            //    email = pay.Email
            //};
            //// finally create the payment object and assign the payer object & transaction list to it
            //PayPal.Api.Payment pymnt = new PayPal.Api.Payment();
            //pymnt.intent = "sale";
            //pymnt.payer = payr;
            //pymnt.transactions = transactions;

            //try {
            //    // Create a payment using a valid APIContext
            //    var createdPayment = pymnt.Create(apiContext);
            //    System.Diagnostics.Debug.WriteLine("created...");
            //    payResponse.TrxApproved = createdPayment.state.ToLower() == "approved";
            //    payResponse.TrxMessage = createdPayment.failure_reason;
            //} catch (PayPal.PaymentsException e) {
            //    StringBuilder sb = new StringBuilder();
            //    sb.AppendLine("Error:    " + e.Details.name);
            //    sb.AppendLine("Message:  " + e.Details.message);

            //    foreach (var errorDetails in e.Details.details) {
            //        sb.AppendLine("Details:  " + errorDetails.field + " -> " + errorDetails.issue);
            //    }
            //    //System.Diagnostics.Debug.WriteLine(sb.ToString());
            //    payResponse.TrxApproved = false;
            //    payResponse.TrxMessage = sb.ToString();
            //}

            //return payResponse;

            return null;
        }
    }

}
