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
    public class ProductCategoryManagerController : Controller
    {
        private readonly ILogger<ProductCategoryManagerController> _logger;
        private readonly ILogic<ProductCategory> _productCategoryLogic;

        public ProductCategoryManagerController(
            ILogger<ProductCategoryManagerController> logger,
            ILogic<ProductCategory> productCategoryLogic
        )
        {
            _logger = logger;
            _productCategoryLogic = productCategoryLogic;
        }

        // GET: ProductCategoryManager
        public async Task<IActionResult> Index()
        {
            var models = _productCategoryLogic.GetAll();
            return View(models);
        }

        // GET: ProductCategoryManager/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var productCategory = _productCategoryLogic.Get(id);

            if (productCategory == null)
            {
                return NotFound();
            }

            return View(productCategory);
        }

        // GET: ProductCategoryManager/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: ProductCategoryManager/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Name,Description,Visible")] ProductCategory productCategory)
        {
            Guid ID = Guid.NewGuid();

            productCategory.Id = ID.ToString();

            if (ModelState.IsValid)
            {
                _productCategoryLogic.Add(productCategory);

                return RedirectToAction("Index");
            }

            return View(productCategory);
        }

        // GET: ProductCategoryManager/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var productCategory = _productCategoryLogic.Get(id); 

            if (productCategory == null)
            {
                return NotFound();
            }

            return View(productCategory);
        }

        // POST: ProductCategoryManager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Name,Description,Visible")] ProductCategory productCategory)
        {
            if (ModelState.IsValid)
            {
                _productCategoryLogic.Edit(productCategory);

                return RedirectToAction("Index");
            }

            return View(productCategory);
        }

        // GET: ProductCategoryManager/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var productCategory = _productCategoryLogic.Get(id);

            if (productCategory == null)
            {
                return NotFound();
            }

            return View(productCategory);
        }

        // POST: ProductCategoryManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var productCategory = _productCategoryLogic.Get(id);
            _productCategoryLogic.Delete(productCategory);

            return RedirectToAction("Index");
        }
    }
}