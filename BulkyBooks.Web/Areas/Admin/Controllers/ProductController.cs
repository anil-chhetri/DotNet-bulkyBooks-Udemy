using BulkyBooks.DataAccess.Repository.IRepository;
using BulkyBooks.Models;
using BulkyBooks.Models.ViewModels;
using BulkyBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBooks.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment hostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            this.hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            ProductViewModel productViewModel = new ProductViewModel()
            {
                Product = new(),
                CategoryList = unitOfWork.Category
                                .GetAll()
                                .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() }),
                CoverTypeList = unitOfWork.CoverType
                                .GetAll()
                                .Select(ct => new SelectListItem { Text = ct.Name, Value = ct.Id.ToString() })
            };

            //ViewBag.CategoryList = CategoryList;
            //ViewBag.CoverTypeList = CoverTypeList;

            if (id == null || id == 0)
            {
                //insert
                return View(productViewModel);
            }
            else
            {
                //update
                productViewModel.Product = unitOfWork.Product.GetFirstOrDefault(p => p.Id == id);
            }

            return View(productViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel productVM, IFormFile formFile)
        {

            if (ModelState.IsValid)
            {
                if (formFile != null)
                {

                    var wwwPath = hostEnvironment.WebRootPath;
                    var fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwPath, @"images\products");
                    var extension = Path.GetExtension(formFile.FileName);

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        var oldPath = wwwPath + productVM.Product.ImageUrl;
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    using (var file = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        formFile.CopyTo(file);
                    }

                    productVM.Product.ImageUrl = Path.Combine(@"\images\products", fileName + extension);
                }

                if (productVM.Product.Id == 0)
                {
                    unitOfWork.Product.Add(productVM.Product);
                    TempData["success"] = "Product Created successfully";
                }
                else
                {
                    unitOfWork.Product.Update(productVM.Product);
                    TempData["success"] = "Product updated successfully";

                }
                unitOfWork.Save();
                return RedirectToAction("index");



            }
            return View(productVM);
        }



        #region API calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = unitOfWork.Product.GetAll(IncludeProperties: "Category, CoverType");
            return Json(new { data = productList });
        }


        [HttpDelete]
        public IActionResult DeletePOST(int? id)
        {
            if(id == null || id == 0)
            {
                return Json(new { success = false, message = "Error while deleting." });
            }

            var obj = unitOfWork.Product.GetFirstOrDefault(p => p.Id == id);

            if(obj == null)
            {
                return Json(new { success = false, message = "Error while deleting." });
            }

            var oldPath = hostEnvironment.WebRootPath + obj.ImageUrl;
            if(System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            unitOfWork.Product.Remove(obj);
            unitOfWork.Save();

            return Json(new { success = true, message = "Product Successfully deleted." });

        }
        #endregion

    }
}
