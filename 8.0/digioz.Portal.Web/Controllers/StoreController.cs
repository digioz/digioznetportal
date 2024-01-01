using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using digioz.Portal.Web.Models.ViewModels;
using System.IO;
using System.Drawing;
using digioz.Portal.Bll;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using digioz.Portal.Bo;
using System.Security.Claims;
using digioz.Portal.Bll.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using digioz.Portal.Web.Areas.Admin.Models.ViewModels;
using System.Threading.Tasks;
using X.PagedList;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using digioz.Portal.Web.Helpers;
using digioz.Portal.Payment;
using digioz.Portal.Web.Models;

namespace digioz.Portal.Web.Controllers
{
    public class StoreController : Controller
    {
        private readonly ILogger<StoreController> _logger;
        private readonly ILogic<Product> _productLogic;
        private readonly ILogic<ProductCategory> _productCategoryLogic;
        private readonly ILogic<ProductOption> _productOptionLogic;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfigLogic _configLogic;
        private readonly ILogic<ShoppingCart> _shoppingCartLogic;
        private readonly ILogic<AspNetUser> _userLogic;
        private readonly ILogic<Profile> _profileLogic;
        private readonly ILogic<Log> _logLogic;
        private readonly ILogic<Order> _orderLogic;
        private readonly ILogic<OrderDetail> _orderDetailLogic;

        public StoreController(
            ILogger<StoreController> logger,
            ILogic<Product> productLogic,
            ILogic<ProductCategory> productCategoryLogic,
            ILogic<ProductOption> productOptionLogic,
            IWebHostEnvironment webHostEnvironment,
            IConfigLogic configLogic,
            ILogic<ShoppingCart> shoppingCartLogic,
            ILogic<AspNetUser> userLogic,
            ILogic<Profile> profileLogic,
            ILogic<Log> logLogic,
            ILogic<Order> orderLogic,
            ILogic<OrderDetail> orderDetailLogic
        )
        {
            _logger = logger;
            _productLogic = productLogic;
            _productCategoryLogic = productCategoryLogic;
            _productOptionLogic = productOptionLogic;
            _webHostEnvironment = webHostEnvironment;
            _configLogic = configLogic;
            _shoppingCartLogic = shoppingCartLogic;
            _userLogic = userLogic;
            _profileLogic = profileLogic;
            _logLogic = logLogic;
            _orderLogic = orderLogic;
            _orderDetailLogic = orderDetailLogic;
        }

        // GET: Store
        public ActionResult Index()
        {
            var products = _productLogic.GetGeneric(x => x.Visible).Take(9).OrderByDescending(x => x.Id);
            return View(products.ToList());
        }

        [Authorize]
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            var product = _productLogic.Get(id);

            var productOptions = _productOptionLogic.GetGeneric(x => x.ProductId == product.Id).ToList();
            ViewBag.Sizes = productOptions.Where(x => x.OptionType == "Size").ToList();
            ViewBag.Colors = productOptions.Where(x => x.OptionType == "Color").ToList();
            ViewBag.MaterialTypes = productOptions.Where(x => x.OptionType == "MaterialType").ToList();

            return View(product);
        }

        [Authorize]
        public ActionResult List()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var config = _configLogic.GetConfig();
            decimal transactionFee = Convert.ToDecimal(config["PaymentTransactionFee"]);

            ViewBag.TransactionFee = transactionFee;

            var shoppingCart = _shoppingCartLogic.GetGeneric(x => x.UserId == userId).ToList();

            ViewBag.ProductLogic = _productLogic;

            return View(shoppingCart);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Add(IFormCollection form)
        {
            // Parse form values
            string productId = form["ID"];
            string size = form["Size"];
            string color = form["Color"];
            string materialType = form["MaterialType"];
            string notesUser = form["UserNotes"];
            Int32 quantity = Convert.ToInt32(form["Quantity"]);

            Guid guid = Guid.NewGuid();
            var product = _productLogic.Get(productId);
            var shoppingCart = new ShoppingCart();

            shoppingCart.Id = guid.ToString();
            shoppingCart.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            shoppingCart.ProductId = product.Id;
            shoppingCart.Quantity = quantity;
            shoppingCart.DateCreated = DateTime.Now;
            shoppingCart.Size = size;
            shoppingCart.Color = color;
            shoppingCart.MaterialType = materialType;
            shoppingCart.Notes = notesUser;

            _shoppingCartLogic.Add(shoppingCart);

            return RedirectToAction("List");
        }

        [Authorize]
        public ActionResult Remove(string id)
        {
            var shoppingCartItem = _shoppingCartLogic.Get(id);
            _shoppingCartLogic.Delete(shoppingCartItem);

            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult Update(string id, IFormCollection form)
        {
            var cartItem = _shoppingCartLogic.Get(id); 
            var quantity = form["Quantity"].ToString();

            if (quantity != null && cartItem != null)
            {
                Int32 quantityNumber = Convert.ToInt32(quantity);
                cartItem.Quantity = quantityNumber;

                _shoppingCartLogic.Edit(cartItem);
            }
            
            return RedirectToAction("List");
        }

        [Authorize]
        public ActionResult Empty()
        {
            string userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = _shoppingCartLogic.GetGeneric(x => x.UserId == userID);

            foreach (var cartItem in cartItems)
			{
                _shoppingCartLogic.Delete(cartItem);
			}

            return RedirectToAction("List");
        }

        [Authorize]
        public ActionResult Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userLogic.Get(userId);

            var cart = _shoppingCartLogic.GetGeneric(x => x.UserId == user.Id).ToList();

            if (cart.Count < 1)
            {
                return RedirectToAction("Index");
            }

            var profile = _profileLogic.GetGeneric(x => x.UserId == user.Id).SingleOrDefault();
            var cvm = new CheckOutViewModel();

            if (profile != null)
			{
                cvm.FirstName = profile.FirstName;
                cvm.LastName = profile.LastName;
                cvm.ShippingAddress = profile.Address;
                cvm.ShippingAddress2 = profile.Address2;
                cvm.ShippingCity = profile.City;
                cvm.ShippingState = profile.State;
                cvm.ShippingZip = profile.Zip;
                cvm.ShippingCountry = profile.Country;
                cvm.BillingAddress = profile.Address;
                cvm.BillingAddress2 = profile.Address2;
                cvm.BillingCity = profile.City;
                cvm.BillingState = profile.State;
                cvm.BillingZip = profile.Zip;
                cvm.BillingCountry = profile.Country;
                cvm.Email = profile.Email;
			}

            return View(cvm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout([Bind("ID,FirstName,LastName,ShippingAddress,ShippingAddress2,ShippingCity,ShippingState,ShippingZip,ShippingCountry,BillingAddress,BillingAddress2,BillingCity,BillingState,BillingZip,BillingCountry,Phone,Email,CCNumber,CCExpMonth, CCExpYear,CCCardCode,PaymentGateway")] CheckOutViewModel checkOutViewModel)
        {
            string userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<OrderDetail> orderDetails = new List<OrderDetail>();
            decimal total = 0;
            Guid orderID = Guid.NewGuid();
            string invoiceNumber = Utility.GetUniqueKey(15);
            var config = _configLogic.GetConfig();
            List<PayLineItem> payLineItems = new List<PayLineItem>();
            decimal transactionFee = Convert.ToDecimal(config["PaymentTransactionFee"]);
            checkOutViewModel.PaymentGateway = "AuthorizeNet";

            try
            {
                // Get Total
                var cart = _shoppingCartLogic.GetGeneric(x => x.UserId == userID).ToList();
                int i = 1;

                foreach (var item in cart)
                {
                    var product = _productLogic.Get(item.ProductId);
                    PayLineItem payLineItem = new PayLineItem();
                    payLineItem.ID = i.ToString();
                    payLineItem.Name = product.Name;
                    payLineItem.Description = product.ShortDescription;
                    payLineItem.Quantity = item.Quantity;
                    payLineItem.Price = product.Price; 
                    payLineItem.Taxable = false;

                    total += product.Price * item.Quantity;

                    payLineItems.Add(payLineItem);

                    i++;
                }

                // Add a transaction fee line item if needed
                if (transactionFee > 0)
                {
                    PayLineItem payLineItem = new PayLineItem();
                    payLineItem.ID = i.ToString();
                    payLineItem.Name = "Transaction Fee";
                    payLineItem.Description = "Transaction fee for processing this order";
                    payLineItem.Quantity = 1;
                    payLineItem.Price = transactionFee;
                    payLineItem.Taxable = false;

                    payLineItems.Add(payLineItem);

                    total += transactionFee;
                }
                
                // Make Payment
                MakePayment makePayment = new MakePayment();
                var payResponse = PayResponse(checkOutViewModel, orderID, total, config, makePayment, invoiceNumber, payLineItems);

                if (payResponse != null && payResponse.TrxApproved == true)
                {
                    // Create Order
                    Order order = new Order();
                    CreateOrder(checkOutViewModel, order, orderID, userID, total, payResponse, invoiceNumber);

                    // Add Order Detail
                    CreateOrderDetails(cart, order, orderDetails);

                    // Email Confirmation
                    SendEmail(config, order, _logLogic);

                    // Clear Shopping Cart
                    foreach (var item in cart)
					{
                        _shoppingCartLogic.Delete(item);
					}

                    // Redirect to Confirmation Page
                    TempData["OrderConfirmationID"] = order.InvoiceNumber;
                    return RedirectToAction("Confirmation");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "We were unable to process your Order. Please contact the site Administrator.");
                    string failureMessage = "An error occurred while processing an order request." + Environment.NewLine;

                    if (payResponse != null)
                    {
                        failureMessage += "User: " + checkOutViewModel.FirstName + " " + checkOutViewModel.LastName + Environment.NewLine;
                        failureMessage += "Message: " + payResponse.TrxMessage + " " + payResponse.TrxDescription + Environment.NewLine;
                        failureMessage += "Response Code:" + payResponse.TrxResponseCode;
                    }

                    Utility.AddLogEntry(failureMessage, _logLogic);
                }
                
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "We were unable to process your Order. Please contact the site Administrator.");
                string error = "Error Placing Order for User " + User.Identity.Name + " - Exception: " + ex.Message + ex.StackTrace.ToString();
                Utility.AddLogEntry(error, _logLogic);
            }

            return View(checkOutViewModel);
        }

        private void CreateOrderDetails(List<ShoppingCart> cart, Order order, List<OrderDetail> orderDetails)
        {
            var config = _configLogic.GetConfig();
            decimal transactionFee = Convert.ToDecimal(config["PaymentTransactionFee"]);

            foreach (var item in cart)
            {
                Guid orderDetailId = Guid.NewGuid();
                var orderDetail = new OrderDetail();
                var product = _productLogic.Get(item.ProductId);

                orderDetail.Id = orderDetailId.ToString();
                orderDetail.OrderId = order.Id;
                orderDetail.ProductId = item.ProductId;
                orderDetail.Quantity = item.Quantity;
                orderDetail.UnitPrice = product.Price;
                orderDetail.Description = product.Name;
                orderDetail.Size = item.Size;
                orderDetail.Color = item.Color;
                orderDetail.MaterialType = item.MaterialType;
                orderDetail.Notes = item.Notes;

                orderDetails.Add(orderDetail);
            }

            if (transactionFee > 0)
            {
                string transactionCategoryString = string.Empty;
                string transactionProductString = string.Empty;

                var transactionCategory = _productCategoryLogic.GetGeneric(x => x.Name == "Transaction").SingleOrDefault();
                var transactionProduct = _productCategoryLogic.GetGeneric(x => x.Name == "Transaction Fee").SingleOrDefault();

                if (transactionCategory == null)
                {
                    Guid transactionCategoryId = Guid.NewGuid();
                    ProductCategory transactionCategoryNew = new ProductCategory();
                    transactionCategoryNew.Id = transactionCategoryId.ToString();
                    transactionCategoryNew.Name = "Transaction";
                    transactionCategoryNew.Visible = false;
                    transactionCategoryNew.Description = "This category is for holding transaction related products. DO NOT REMOVE.";

                    _productCategoryLogic.Add(transactionCategoryNew);
                    transactionCategoryString = transactionCategoryId.ToString();
                }
                else
                {
                    transactionCategoryString = transactionCategory.Id.ToString();
                }

                // create the transaction fee product if it does not exist
                if (transactionProduct == null)
                {
                    Guid transactionProductId = Guid.NewGuid();
                    Product transactionProductNew = new Product();
                    transactionProductNew.Id = transactionProductId.ToString();
                    transactionProductNew.ProductCategoryId = transactionCategoryString;
                    transactionProductNew.Name = "Transaction Fee";
                    transactionProductNew.Price = transactionFee;
                    transactionProductNew.ShortDescription = "Transaction Fee";
                    transactionProductNew.Description = "Transaction Fee";
                    transactionProductNew.UnitsInStock = 100000;
                    transactionProductNew.OutOfStock = false;
                    transactionProductNew.Visible = false;

                    _productLogic.Add(transactionProductNew);
                    transactionProductString = transactionProductNew.Id.ToString();
                }
                else
                {
                    transactionProductString = transactionProduct.Id.ToString();
                }

                Guid orderDetailId = Guid.NewGuid();
                var orderDetail = new OrderDetail();

                orderDetail.Id = orderDetailId.ToString();
                orderDetail.OrderId = order.Id;
                orderDetail.ProductId = transactionProductString;
                orderDetail.Quantity = 1;
                orderDetail.UnitPrice = transactionFee;
                orderDetail.Description = "Transaction Fee";

                orderDetails.Add(orderDetail);
            }

            // Save Order Details
            foreach (var orderDetail in orderDetails)
			{
                _orderDetailLogic.Add(orderDetail);
			}
        }

        private void CreateOrder(CheckOutViewModel checkOutViewModel, Order order, Guid orderID, string userID, decimal total, PayResponse payResponse, string invoiceNumber)
        {
            var config = _configLogic.GetConfig();
            string encryptionKey = config["SiteEncryptionKey"];
            var encryptString = new EncryptString();

            order.Id = orderID.ToString();
            order.UserId = userID;
            order.InvoiceNumber = invoiceNumber;
            order.OrderDate = DateTime.Now;
            order.FirstName = checkOutViewModel.FirstName;
            order.LastName = checkOutViewModel.LastName;

            order.ShippingAddress = checkOutViewModel.ShippingAddress;
            order.ShippingAddress2 = checkOutViewModel.ShippingAddress2;
            order.ShippingCity = checkOutViewModel.ShippingCity;
            order.ShippingState = checkOutViewModel.ShippingState;
            order.ShippingZip = checkOutViewModel.ShippingZip;
            order.ShippingCountry = checkOutViewModel.ShippingCountry;

            order.BillingAddress = checkOutViewModel.BillingAddress;
            order.BillingAddress2 = checkOutViewModel.BillingAddress2;
            order.BillingCity = checkOutViewModel.BillingCity;
            order.BillingState = checkOutViewModel.BillingState;
            order.BillingZip = checkOutViewModel.BillingZip;
            order.BillingCountry = checkOutViewModel.BillingCountry;

            order.Phone = checkOutViewModel.Phone;
            order.Email = checkOutViewModel.Email;
            order.Total = total;

            order.Ccnumber = encryptString.Encrypt(encryptionKey, checkOutViewModel.CCNumber);
            order.Ccexp = checkOutViewModel.CCExpMonth + checkOutViewModel.CCExpYear.Substring(2,checkOutViewModel.CCExpYear.Length - 2); 
            order.CccardCode = checkOutViewModel.CCCardCode;
            order.Ccamount = total;

            // ToTo - Fill in Payment Transaction values
            order.TrxDescription = payResponse.TrxDescription;
            order.TrxApproved = payResponse.TrxApproved;
            order.TrxAuthorizationCode = payResponse.TrxAuthorizationCode;
            order.TrxMessage = payResponse.TrxMessage;
            order.TrxResponseCode = payResponse.TrxResponseCode;
            order.TrxId = payResponse.TrxID;

            _orderLogic.Add(order);
        }

        private static void SendEmail(Dictionary<string, string> config, Order order, ILogic<Log> logLogic)
        {
            try
            {
                EmailModel email = new EmailModel();

                email.SMTPServer = config["SMTPServer"];
                email.SMTPUsername = config["SMTPUsername"];
                email.SMTPPassword = config["SMTPPassword"];
                email.FromEmail = config["WebmasterEmail"];

                email.ToEmail = order.Email;
                email.Subject = " Your Order has been placed on " + config["SiteName"];
                email.Message = "Dear User,<br /><br />"
                                +
                                "We have received an Order from you and are in the process of processing your order. <br /><br />"
                                + "Your Order ID: " + order.InvoiceNumber + " <br /><br />"
                                + "If you have any questions feel free to contact us at " + email.FromEmail + " or visit " +
                                config["SiteURL"] + " for more information.<br /><br />"
                                + "Thanks,<br />"
                                + "The " + config["SiteName"] + " Management Team";

                bool resultEmailSubmit = Utility.SubmitMail(email, logLogic);
            }
            catch (Exception)
            {
                // Exception handled by SubmitMail
            }
        }

        private static PayResponse PayResponse(CheckOutViewModel checkOutViewModel, Guid orderID, decimal total,
            Dictionary<string, string> config, MakePayment makePayment, string invoiceNumber, List<PayLineItem> payLineItems)
        {
            Pay pay = new Pay();
            pay.ID = invoiceNumber; // orderID.ToString();
            pay.OrderDate = DateTime.Now;
            pay.FirstName = checkOutViewModel.FirstName;
            pay.LastName = checkOutViewModel.LastName;

            pay.ShippingAddress = checkOutViewModel.ShippingAddress;
            pay.ShippingAddress2 = checkOutViewModel.ShippingAddress2;
            pay.ShippingCity = checkOutViewModel.ShippingCity;
            pay.ShippingState = checkOutViewModel.ShippingState;
            pay.ShippingZip = checkOutViewModel.ShippingZip;
            pay.ShippingCountry = checkOutViewModel.ShippingCountry;
            pay.ShippingCountry = checkOutViewModel.ShippingCountry;
            pay.ShippingCountryCode = Utility.GetCountryCode(checkOutViewModel.ShippingCountry);

            pay.BillingAddress = checkOutViewModel.BillingAddress;
            pay.BillingAddress2 = checkOutViewModel.BillingAddress2;
            pay.BillingCity = checkOutViewModel.BillingCity;
            pay.BillingState = checkOutViewModel.BillingState;
            pay.BillingZip = checkOutViewModel.BillingZip;
            pay.BillingCountry = checkOutViewModel.BillingCountry;
            pay.BillingCountryCode = Utility.GetCountryCode(checkOutViewModel.BillingCountry);

            pay.Phone = checkOutViewModel.Phone;
            pay.Email = checkOutViewModel.Email;
            pay.Total = total;

            pay.CCNumber = checkOutViewModel.CCNumber;
            pay.CCExp = checkOutViewModel.CCExpMonth + checkOutViewModel.CCExpYear.Substring(2, checkOutViewModel.CCExpYear.Length - 2);
            pay.CCExpYear = checkOutViewModel.CCExpYear;
            pay.CCExpMonth = checkOutViewModel.CCExpMonth ;
            pay.CCCardCode = checkOutViewModel.CCCardCode;
            pay.CCAmount = total;
            pay.CCType = Utility.CreditCardType(pay.CCNumber);
            pay.Description = "Order on " + DateTime.Now + " - " + config["SiteName"];

            PaymentTypeKey key = new PaymentTypeKey();
            key.LoginId = config["PaymentLoginID"];
            key.TransactionKey = config["PaymentTransactionKey"];
            key.TestMode = Convert.ToBoolean(config["PaymentTestMode"]);
            var paymentType = (PaymentType)Enum.Parse(typeof(PaymentType), checkOutViewModel.PaymentGateway);
            PayResponse payResponse = makePayment.ProcessPayment(pay, paymentType, key, payLineItems);
            return payResponse;
        }

        public ActionResult Confirmation()
        {
            if (TempData["OrderConfirmationID"] != null)
            {
                @ViewBag.OrderConfirmationID = TempData["OrderConfirmationID"].ToString();
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult ByCategory(string id, int? page)
        {
            var products = _productLogic.GetGeneric(x => x.ProductCategoryId == id && x.Visible == true).ToList();

            int pageSize = 9;
            int pageNumber = (page ?? 1);

            ViewBag.CategoryId = id;

            return View(products.ToPagedList(pageNumber, pageSize));
        }

        [Authorize]
        public ActionResult History()
        {
            List<OrderManagerViewModel> OMVMs = new List<OrderManagerViewModel>();

            string userID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = _orderLogic.GetGeneric(x => x.UserId == userID).ToList();
            var config = _configLogic.GetConfig();
            string encryptionKey = config["SiteEncryptionKey"];
            var encryptString = new EncryptString();

            foreach (var order in orders)
            {
                var orderDetails = _orderDetailLogic.GetGeneric(x => x.OrderId == order.Id).ToList();
                OrderManagerViewModel OMVM = new OrderManagerViewModel();

                string creditCardNumber;

                try
                {
                    creditCardNumber = encryptString.Decrypt(encryptionKey, order.Ccnumber);
                    creditCardNumber = StringUtils.CreditCardNumberShowLast4Chars(creditCardNumber);
                }
                catch
                {
                    creditCardNumber = string.Empty;
                }

                order.Ccnumber = creditCardNumber;

                OMVM.Order = order;
                OMVM.OrderDetails = orderDetails;

                OMVMs.Add(OMVM);
            }

            ViewBag.ProductLogic = _productLogic;

            return View(OMVMs);
        }
    }
}