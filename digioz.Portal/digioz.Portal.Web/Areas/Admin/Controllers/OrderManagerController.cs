using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using digioz.Portal.Bll;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Utilities;
using digioz.Portal.Web.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class OrderManagerController : Controller
    {
        private readonly ILogger<OrderManagerController> _logger;
        private readonly IConfigLogic _configLogic;
        private readonly ILogic<Order> _orderLogic;
        private readonly ILogic<OrderDetail> _orderDetailLogic;
        private readonly ILogic<AspNetUser> _userLogic;
        private readonly ILogic<Product> _productLogic;

        public OrderManagerController(
            ILogger<OrderManagerController> logger,
            IConfigLogic configLogic,
            ILogic<Order> orderLogic,
            ILogic<OrderDetail> orderDetailLogic,
            ILogic<AspNetUser> userLogic,
            ILogic<Product> productLogic
        )
        {
            _logger = logger;
            _configLogic = configLogic;
            _orderLogic = orderLogic;
            _orderDetailLogic = orderDetailLogic;
            _userLogic = userLogic;
            _productLogic = productLogic;
        }

        // GET: OrderManager
        public ActionResult Index()
        {
            var orders = _orderLogic.GetAll().OrderByDescending(x => x.OrderDate);
            return View(orders.ToList());
        }

        // GET: OrderManager/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            OrderManagerViewModel OMVM = new OrderManagerViewModel();
            Order order = _orderLogic.Get(id);
            var orderDetails = _orderDetailLogic.GetGeneric(x => x.OrderId == id).ToList();

            var config = _configLogic.GetConfig();
            string encryptionKey = config["SiteEncryptionKey"];
            var encryptString = new EncryptString();

            string creditCardNumber;

            try
            {
                creditCardNumber = encryptString.Decrypt(encryptionKey, order.Ccnumber);
            }
            catch
            {
                creditCardNumber = string.Empty;
            }

            order.Ccnumber = creditCardNumber;

            OMVM.Order = order;
            OMVM.OrderDetails = orderDetails;

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.ProductLogic = _productLogic;

            return View(OMVM);
        }

        // GET: OrderManager/Create
        public ActionResult Create()
        {
            ViewBag.UserID = new SelectList(_userLogic.GetAll(), "Id", "UserName");
            return View();
        }

        // POST: OrderManager/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("UserID,OrderDate,FirstName,LastName,ShippingAddress,ShippingAddress2,ShippingCity,ShippingState,ShippingZip,ShippingCountry,BillingAddress,BillingAddress2,BillingCity,BillingState,BillingZip,BillingCountry,Phone,Email,Total,CCNumber,CCExp,CCCardCode,CCAmount,TrxDescription,TrxApproved,TrxAuthorizationCode,TrxMessage,TrxResponseCode,TrxID")] Order order)
        {
            Guid ID = Guid.NewGuid();
            order.Id = ID.ToString();

            if (ModelState.IsValid)
            {
                _orderLogic.Add(order);

                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(_userLogic.GetAll(), "Id", "UserName", order.UserId);
            return View(order);
        }

        // GET: OrderManager/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Order order = _orderLogic.Get(id);
            if (order == null)
            {
                return NotFound();
            }

            var config = _configLogic.GetConfig();
            string encryptionKey = config["SiteEncryptionKey"];
            var encryptString = new EncryptString();

            string creditCardNumber;

            try
            {
                creditCardNumber = encryptString.Decrypt(encryptionKey, order.Ccnumber);
            }
            catch
            {
                creditCardNumber = string.Empty;
            }

            order.Ccnumber = creditCardNumber;

            ViewBag.UserID = new SelectList(_userLogic.GetAll(), "Id", "UserName", order.UserId);

            return View(order);
        }

        // POST: OrderManager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("ID,UserID,OrderDate,FirstName,LastName,ShippingAddress,ShippingAddress2,ShippingCity,ShippingState,ShippingZip,ShippingCountry,BillingAddress,BillingAddress2,BillingCity,BillingState,BillingZip,BillingCountry,Phone,Email,Total,CCNumber,CCExp,CCCardCode,CCAmount,TrxDescription,TrxApproved,TrxAuthorizationCode,TrxMessage,TrxResponseCode,TrxID,InvoiceNumber")] Order order)
        {
            var config = _configLogic.GetConfig();
            string encryptionKey = config["SiteEncryptionKey"];
            var encryptString = new EncryptString();

            string creditCardNumber = encryptString.Encrypt(encryptionKey, order.Ccnumber);
            order.Ccnumber = creditCardNumber;

            if (ModelState.IsValid)
            {
                _orderLogic.Edit(order);

                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(_userLogic.GetAll(), "Id", "UserName", order.UserId);

            return View(order);
        }

        // GET: OrderManager/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            OrderManagerViewModel OMVM = new OrderManagerViewModel();
            Order order = _orderLogic.Get(id); 
            var orderDetails = _orderDetailLogic.GetGeneric(x => x.OrderId == id).ToList();

            var config = _configLogic.GetConfig();
            string encryptionKey = config["SiteEncryptionKey"];
            var encryptString = new EncryptString();
            string creditCardNumber;

            try
            {
                creditCardNumber = encryptString.Decrypt(encryptionKey, order.Ccnumber); 
            }
            catch
            {
                creditCardNumber = string.Empty;
            }
            
            order.Ccnumber = creditCardNumber;

            OMVM.Order = order;
            OMVM.OrderDetails = orderDetails;

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.ProductLogic = _productLogic;

            return View(OMVM);
        }

        // POST: OrderManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var order = _orderLogic.Get(id);

            // Remove Order Details first
            var orderDetails = _orderDetailLogic.GetGeneric(x => x.OrderId == id).ToList();

            foreach(var orderDetail in orderDetails)
            {
                _orderDetailLogic.Delete(orderDetail);
            }

            // Now remove the Order 
            _orderLogic.Delete(order);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Search(string searchString = "")
        {
            searchString = searchString.Trim();
            var usersViewModel = new List<OrderManagerViewModel>();

            if (searchString.IsNullEmpty())
            {
                return RedirectToAction("Index", "OrderManager");
            }

            // Search Records
            var models = _orderLogic.GetGeneric(x => x.Email.Contains(searchString) || x.InvoiceNumber.Contains(searchString)).ToList();

            return View(models);
        }
    }
}