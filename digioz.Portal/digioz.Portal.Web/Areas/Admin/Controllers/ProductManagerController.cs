using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using digioz.Portal.Bll;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Utilities;
using digioz.Portal.Web.Areas.Admin.Models.ViewModels;
using digioz.Portal.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class ProductManagerController : Controller
    {
        private readonly ILogger<ProductManagerController> _logger;
        private readonly ILogic<Product> _productLogic;
        private readonly ILogic<ProductCategory> _productCategoryLogic;
        private readonly ILogic<ProductOption> _productOptionLogic;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductManagerController(
            ILogger<ProductManagerController> logger,
            ILogic<Product> productLogic,
            ILogic<ProductCategory> productCategoryLogic,
            ILogic<ProductOption> productOptionLogic,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _logger = logger;
            _productLogic = productLogic;
            _productCategoryLogic = productCategoryLogic;
            _productOptionLogic = productOptionLogic;
            _webHostEnvironment = webHostEnvironment;
        }

        private string GetImageFolderPath()
        {
            var webRootPath = _webHostEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, "img");

            return path;
        }

        private async Task CropImageAndSave(IFormFile file, string path, int width, int height)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            using var img = Image.FromStream(memoryStream);
            Helpers.ImageHelper.SaveImageWithCrop(img, width, height, path);
        }

        // GET: ProductManager
        public async Task<IActionResult> Index()
        {
            var products = _productLogic.GetAll(); 

            return View(products);
        }

        // GET: ProductManager/Details/5
        [Route("/admin/productmanager/details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var product = _productLogic.Get(id);

            if (product == null)
            {
                return NotFound();
            }

            var productOptions = _productOptionLogic.GetGeneric(x => x.ProductId == product.Id).ToList();
            ViewBag.Sizes = productOptions.Where(x => x.OptionType == "Size").ToList();
            ViewBag.Colors = productOptions.Where(x => x.OptionType == "Color").ToList();
            ViewBag.MaterialTypes = productOptions.Where(x => x.OptionType == "MaterialType").ToList();

            return View(product);
        }

        // GET: ProductManager/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.ProductCategoryID = new SelectList(_productCategoryLogic.GetAll(), "Id", "Name");

            return View();
        }

        // POST: ProductManager/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind("ProductCategoryId,Name,Make,Model,Sku,Image,Price,Cost,QuantityPerUnit,Weight,Dimensions,Sizes,Colors,MaterialType,PartNumber,ShortDescription,Description,ManufacturerURL,UnitsInStock,OutOfStock,Notes,SizeList,ColorList,MaterialTypeList,Visible")] Product product, IFormFile file, IFormCollection form)
        {
            if (file != null && Utility.IsFileAnImage(file.FileName))
            {
                var imgFolder = GetImageFolderPath();
                Guid guidName = Guid.NewGuid();
                var fileName = guidName.ToString() + Path.GetExtension(file.FileName);
                var pathFull = Path.Combine(imgFolder, "Products", "Full", fileName);
                var pathThumb = Path.Combine(imgFolder, "Products", "Thumb", fileName);

                // Save Original Image
                using (Stream fileStream = new FileStream(pathFull, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Save Thumbnail Image
                await CropImageAndSave(file, pathThumb, 120, 120);

                product.Image = fileName;
            }

            Guid ID = Guid.NewGuid();
            product.Id = ID.ToString();

            if (ModelState.IsValid)
            {
                // Save the Product Record
                _productLogic.Add(product);

                // Check if new Product Options have been added 
                var sizesArray = form["SizeList"].ToArray();
                var colorsArray = form["ColorList"].ToArray();
                var materialTypesArray = form["MaterialTypeList"].ToArray();

                // Add Sizes
                if (sizesArray != null)
                {
                    List<ProductOption> sizeOptions = new List<ProductOption>();
                    foreach (var size in sizesArray)
                    {
                        Guid sizeId = Guid.NewGuid();
                        ProductOption sizeOption = new ProductOption();
                        sizeOption.Id = sizeId.ToString();
                        sizeOption.ProductId = product.Id.ToString();
                        sizeOption.OptionType = "Size";
                        sizeOption.OptionValue = size.ToString();

                        sizeOptions.Add(sizeOption);
                        _productOptionLogic.Add(sizeOption);
                    }
                }

                // Add Colors
                if (colorsArray != null)
                {
                    List<ProductOption> colorOptions = new List<ProductOption>();
                    foreach (var color in colorsArray)
                    {
                        Guid colorId = Guid.NewGuid();
                        ProductOption colorOption = new ProductOption();
                        colorOption.Id = colorId.ToString();
                        colorOption.ProductId = product.Id.ToString();
                        colorOption.OptionType = "Color";
                        colorOption.OptionValue = color.ToString();

                        colorOptions.Add(colorOption);
                        _productOptionLogic.Add(colorOption);
                    }
                }

                // Add Material
                if (materialTypesArray != null)
                {
                    List<ProductOption> materialTypeOptions = new List<ProductOption>();
                    foreach (var materialType in materialTypesArray)
                    {
                        Guid materialTypeId = Guid.NewGuid();
                        ProductOption materialTypeOption = new ProductOption();
                        materialTypeOption.Id = materialTypeId.ToString();
                        materialTypeOption.ProductId = product.Id.ToString();
                        materialTypeOption.OptionType = "MaterialType";
                        materialTypeOption.OptionValue = materialType.ToString();

                        materialTypeOptions.Add(materialTypeOption);
                        _productOptionLogic.Add(materialTypeOption);
                    }
                }

                return RedirectToAction("Index");
            }

            ViewBag.ProductCategoryID = new SelectList(_productCategoryLogic.GetAll(), "Id", "Name", product.ProductCategoryId);
            return View(product);
        }

        // GET: ProductManager/Edit/5
        [Route("/admin/productmanager/edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var product = _productLogic.Get(id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.ProductCategoryID = new SelectList(_productCategoryLogic.GetAll(), "Id", "Name", product.ProductCategoryId);

            var productOptions = _productOptionLogic.GetGeneric(x => x.ProductId == product.Id).ToList();
            ViewBag.Sizes = productOptions.Where(x => x.OptionType == "Size").ToList();
            ViewBag.Colors = productOptions.Where(x => x.OptionType == "Color").ToList();
            ViewBag.MaterialTypes = productOptions.Where(x => x.OptionType == "MaterialType").ToList();

            return View(product);
        }

        // POST: ProductManager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("EditAsync")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind("Id,ProductCategoryId,Name,Make,Model,Sku,Image,Price,Cost,QuantityPerUnit,Weight,Dimensions,Sizes,Colors,MaterialType,PartNumber,ShortDescription,Description,ManufacturerURL,UnitsInStock,OutOfStock,Notes,SizeList,ColorList,MaterialTypeList,Visible")] Product product, IFormFile file, IFormCollection form)
        {
            if (file != null && Utility.IsFileAnImage(file.FileName))
            {
                Guid guidName = Guid.NewGuid();
                var fileName = guidName.ToString() + Path.GetExtension(file.FileName);
                var imgFolder = GetImageFolderPath();
                var pathFull = Path.Combine(imgFolder, "Products", "Full", fileName);
                var pathThumb = Path.Combine(imgFolder, "Products", "Thumb", fileName);

                // Save Original Image
                using (Stream fileStream = new FileStream(pathFull, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Save Thumbnail Image
                await CropImageAndSave(file, pathThumb, 120, 120);

                product.Image = fileName;
            }

            if (ModelState.IsValid)
            {
                // Save Product Record
                _productLogic.Edit(product);

                // Check if new Product Options have been added or modified
                var sizesArray = form["SizeList"].ToArray();
                var colorsArray = form["ColorList"].ToArray();
                var materialTypesArray = form["MaterialTypeList"].ToArray();

                // Get existing Product Options
                var currentSizes = _productOptionLogic.GetGeneric(x => x.ProductId == product.Id && x.OptionType == "Size").ToList();
                var currentColors = _productOptionLogic.GetGeneric(x => x.ProductId == product.Id && x.OptionType == "Color").ToList();
                var currentMaterialTypes = _productOptionLogic.GetGeneric(x => x.ProductId == product.Id && x.OptionType == "MaterialType").ToList();

                // Update Sizes
                if (currentSizes != null)
                {
                    foreach (var size in currentSizes)
                    {
                        // Remove the ones that have been removed
                        if (!sizesArray.Contains(size.OptionValue))
                        {
                            var deletedSize = _productOptionLogic.GetGeneric(x => x.ProductId == product.Id && x.OptionType == "Size" && x.OptionValue == size.OptionValue).SingleOrDefault();
                            _productOptionLogic.Delete(deletedSize);
                        }
                    }
                }

                if (sizesArray != null)
                {
                    foreach (var size in sizesArray)
                    {
                        // Add the ones that do not exist
                        var sizeNew = _productOptionLogic.GetGeneric(x => x.ProductId == product.Id && x.OptionValue == size.ToString() && x.OptionType == "Size").SingleOrDefault();

                        if (sizeNew == null)
                        {
                            Guid sizeId = Guid.NewGuid();
                            ProductOption sizeOption = new ProductOption();
                            sizeOption.Id = sizeId.ToString();
                            sizeOption.ProductId = product.Id.ToString();
                            sizeOption.OptionType = "Size";
                            sizeOption.OptionValue = size.ToString();

                            _productOptionLogic.Add(sizeOption);
                        }
                    }
                }

                // Update Colors
                if (currentColors != null)
                {
                    foreach (var color in currentColors)
                    {
                        // Remove the ones that have been removed
                        if (!colorsArray.Contains(color.OptionValue))
                        {
                            var deletedColor = _productOptionLogic.GetGeneric(x => x.ProductId == product.Id && x.OptionType == "Color" && x.OptionValue == color.OptionValue).SingleOrDefault();
                            _productOptionLogic.Delete(deletedColor);
                        }
                    }
                }

                if (colorsArray != null)
                {
                    foreach (var color in colorsArray)
                    {
                        // Add the ones that do not exist
                        var colorNew = _productOptionLogic.GetGeneric(x => x.ProductId == product.Id && x.OptionValue == color.ToString() && x.OptionType == "Color").ToString();

                        if (colorNew == null)
                        {
                            Guid colorId = Guid.NewGuid();
                            ProductOption colorOption = new ProductOption();
                            colorOption.Id = colorId.ToString();
                            colorOption.ProductId = product.Id.ToString();
                            colorOption.OptionType = "Color";
                            colorOption.OptionValue = color.ToString();

                            _productOptionLogic.Add(colorOption);
                        }
                    }
                }

                // Update Material
                if (currentMaterialTypes != null)
                {
                    foreach (var materialType in currentMaterialTypes)
                    {
                        // Remove the ones that have been removed
                        if (!materialTypesArray.Contains(materialType.OptionValue))
                        {
                            var deletedMaterialType = _productOptionLogic.GetGeneric(x => x.ProductId == product.Id && x.OptionType == "MaterialType" && x.OptionValue == materialType.OptionValue).SingleOrDefault();
                            _productOptionLogic.Delete(deletedMaterialType);
                        }
                    }
                }

                if (materialTypesArray != null)
                {
                    foreach (var materialType in materialTypesArray)
                    {
                        // Add the ones that do not exist
                        var materialTypeNew = _productOptionLogic.GetGeneric(x => x.ProductId == product.Id && x.OptionValue == materialType.ToString() && x.OptionType == "MaterialType").SingleOrDefault();

                        if (materialTypeNew == null)
                        {
                            Guid materialTypeId = Guid.NewGuid();
                            ProductOption materialTypeOption = new ProductOption();
                            materialTypeOption.Id = materialTypeId.ToString();
                            materialTypeOption.ProductId = product.Id.ToString();
                            materialTypeOption.OptionType = "MaterialType";
                            materialTypeOption.OptionValue = materialType.ToString();

                            _productOptionLogic.Add(materialTypeOption);
                        }
                    }
                }

                return RedirectToAction("Index");
            }
            ViewBag.ProductCategoryID = new SelectList(_productCategoryLogic.GetAll(), "Id", "Name", product.ProductCategoryId);

            return View(product);
        }

        // GET: ProductManager/Delete/5
        [Route("/admin/productmanager/delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Product product = _productLogic.Get(id); 

            if (product == null)
            {
                return NotFound();
            }

            var productOptions = _productOptionLogic.GetGeneric(x => x.ProductId == product.Id).ToList();
            ViewBag.Sizes = productOptions.Where(x => x.OptionType == "Size").ToList();
            ViewBag.Colors = productOptions.Where(x => x.OptionType == "Color").ToList();
            ViewBag.MaterialTypes = productOptions.Where(x => x.OptionType == "MaterialType").ToList();

            return View(product);
        }

        // POST: ProductManager/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            Product product = _productLogic.Get(id);

            // First Remove Product Options
            var productOptions = _productOptionLogic.GetGeneric(x => x.ProductId == product.Id).ToList();

            foreach (var productOption in productOptions)
            {
                _productOptionLogic.Delete(productOption);
            }

            // Now Remove the Product
            _productLogic.Delete(product);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Search(string searchString = "")
        {
            searchString = searchString.Trim();

            if (searchString.IsNullEmpty())
            {
                return RedirectToAction("Index", "ProductManager");
            }

            // Search Records
            var models = _productLogic.GetGeneric(x => x.Name.Contains(searchString) 
                                                || x.Make.Contains(searchString)
                                                || x.Model.Contains(searchString)
                                                || x.PartNumber.Contains(searchString)
                                                || x.Sku.Contains(searchString)).ToList();

            return View("Index", models);
        }
    }
}