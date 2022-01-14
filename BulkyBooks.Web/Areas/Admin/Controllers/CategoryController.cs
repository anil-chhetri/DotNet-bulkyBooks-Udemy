using BulkyBooks.DataAccess;
using BulkyBooks.DataAccess.Repository;
using BulkyBooks.DataAccess.Repository.IRepository;
using BulkyBooks.Models;
using BulkyBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBooks.Web.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var categories = unitOfWork.Category.GetAll();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Category Name cannot be same as Display Order");
            }

            if (ModelState.IsValid)
            {
                unitOfWork.Category.Add(category);
                unitOfWork.Save();
                TempData["success"] = $"{category.Name} is successfully created.";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if(id == 0 || id == null)
            {
                return NotFound();
            }
            Category category = unitOfWork.Category.GetFirstOrDefault(b => b.Id == id.Value);

            if(category == null)
            {
                return NotFound();
            }

            return View(category);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if(category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "Category Name cannot be same as Display Order");
            }

            if(ModelState.IsValid)
            {
                unitOfWork.Category.Update(category);
                unitOfWork.Save();
                TempData["success"] = $"{category.Name} is successfully Modified.";
                return RedirectToAction("index");
            }

            return View(category);
        }

        [HttpGet]
        public IActionResult Delete(int Id)
        {
            if (Id == 0 )
            {
                return NotFound();
            }
            Category category = unitOfWork.Category.GetFirstOrDefault(c => c.Id == Id);

            if(category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int Id)
        {
            if(Id == 0)
            {
                return NotFound();
            }

            var fromDb = unitOfWork.Category.GetFirstOrDefault(c => c.Id == Id);


            if (fromDb == null)
            {
                return NotFound();
            }


            unitOfWork.Category.Remove(fromDb);
            unitOfWork.Save();
            TempData["success"] = $"{fromDb.Name} is successfully Deleted.";
            return RedirectToAction("index");
        }
    }

}
