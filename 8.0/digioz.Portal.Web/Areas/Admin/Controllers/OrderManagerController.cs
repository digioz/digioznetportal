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
        public async Task<IActionResult> Index()
        {
            var orders = _orderLogic.GetAll().OrderByDescending(x => x.OrderDate);
            return View(orders.ToList());
        }

        [Route("/admin/ordermanager/details/{id}")]
        public async Task<IActionResult> Details(string id)
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

    }
}