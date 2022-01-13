using BulkyBooks.DataAccess.Repository.IRepository;
using BulkyBooks.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBooks.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        #region API
        public IActionResult GetAll()
        {
            var orderHeader = unitOfWork.OrderHeader.GetAll(IncludeProperties: "ApplicationUser");
            return Json(new { data = orderHeader });    
        }
        #endregion


        public IActionResult Index()
        {
            return View();
        }
    }
}
