using BulkyBooks.DataAccess.Repository.IRepository;
using BulkyBooks.Models;
using BulkyBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBooks.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var data = unitOfWork.CoverType.GetAll();
            return View(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType coverType)
        {
            if(ModelState.IsValid)
            {
                unitOfWork.CoverType.Add(coverType);
                unitOfWork.Save();
                TempData["success"] = $"{coverType.Name} successfully created.";
                return RedirectToAction("Index");
            }
            return View();
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            if(id == 0)
            {
                return NotFound();
            }
            var coverType = unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);

            if(coverType == null)
            {
                return NotFound();
            }

            return View(coverType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType coverType)
        {
            if(ModelState.IsValid)
            {
                unitOfWork.CoverType.Update(coverType);
                unitOfWork.Save();
                TempData["success"] = $"{coverType.Name} Modeified successfully.";
                return RedirectToAction("index");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if(id == 0)
            {
                return NotFound();
            }
            var coverType = unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);

            if(coverType == null)
            {
                return NotFound();
            }

            return View(coverType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(CoverType coverType)
        {
            var fromDb = unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == coverType.Id);
            if(fromDb == null)
            {
                return NotFound();
            }

            unitOfWork.CoverType.Remove(fromDb);
            unitOfWork.Save();
            TempData["success"] = $"{fromDb.Name} successfully Deleted.";
            return RedirectToAction("index");
        }
    }
}
