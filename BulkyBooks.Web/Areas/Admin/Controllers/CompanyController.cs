using BulkyBooks.DataAccess.Repository.IRepository;
using BulkyBooks.Models;
using BulkyBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBooks.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upsert(int? Id)
        {
            Company company;
            if (Id == null || Id == 0)
            {
                ViewData["Action"] = "Create Company";
                company = new Company();
            }
            else
            {

                ViewData["Action"] = "Edit Company";
                company = unitOfWork.Company.GetFirstOrDefault(c => c.Id == Id);
                if (company == null)
                {
                    return NotFound();
                }
            }

            return View(company);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company company)
        {
            if (!UInt64.TryParse(company.PhoneNumber, out UInt64 PhoneOut))
            {
                ModelState.AddModelError("PhoneNumber", "Phone Number Cannot contain strings");
            }


            if (!int.TryParse(company.PostalCode, out int PostalOut))
            {
                ModelState.AddModelError("PostalCode", "Postal Code Cannot contain strings");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Action"] = company.Id == 0 ? "Create Company" : "Edit Company";
                return View(company);
            }


            if (company.Id == 0)
            {
                unitOfWork.Company.Add(company);
                unitOfWork.Save();
                TempData["success"] = "Company Added Successfully.";
            }
            else
            {
                unitOfWork.Company.Update(company);
                unitOfWork.Save();
                TempData["success"] = "Company Modified Successfully.";

            }


            return RedirectToAction("Index");
        }

        public IActionResult GetCompany()
        {
            var companyList = unitOfWork.Company.GetAll();
            var success = true;
            if(companyList == null)
            {
                success = false;
            }
            return Json(new { data = companyList  , success = success });
        }

        [HttpDelete]
        public IActionResult Delete(int Id)
        {
            var success = true;
            var message = "Deleted Successfully."; 
            if(Id == 0)
            {
                success = false;
                message = "Invalid Id, Nothing deleted.";
                TempData["error"] = "Invalid Id, Nothing deleted.";
            }
            else
            {
                var fromDb = unitOfWork.Company.GetFirstOrDefault(c => c.Id == Id);

                if (fromDb == null)
                {
                    success = false;
                    message = "Invalid Id, Nothing deleted.";
                    TempData["error"] = "Invalid Id, Nothing deleted.";

                }
                else
                {
                    unitOfWork.Company.Remove(fromDb);
                    unitOfWork.Save();
                    Debug.WriteLine("Deleted Successfully.");
                    TempData["success"] = "Data deleted Successfully.";
                }
            }
            

            return Json(new { message=message, success=success });
        }
    }
}
