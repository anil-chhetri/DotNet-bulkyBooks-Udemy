using BulkyBooks.DataAccess.Repository.IRepository;
using BulkyBooks.Models;
using BulkyBooks.Models.ViewModels;
using BulkyBooks.Utility;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBooks.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        #region API
        public IActionResult GetAll(string status=null)
        {
            IEnumerable<OrderHeader> orderHeader;

            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeader = unitOfWork.OrderHeader.GetAll(IncludeProperties: "ApplicationUser");
            }
            else
            {
                var cliams = (ClaimsIdentity)User.Identity;
                var identity = cliams.FindFirst(ClaimTypes.NameIdentifier);

                orderHeader = unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == identity.Value, IncludeProperties: "ApplicationUser"); ;
            }

            switch (status)
            {
                case "pending":
                    orderHeader = orderHeader.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;

                case "inprocess":
                    orderHeader = orderHeader.Where(o => o.OrderStatus == SD.StatusInProcess);
                    break;

                case "completed":
                    orderHeader = orderHeader.Where(o => o.OrderStatus == SD.StatusShipped);
                    break;

                case "approved":
                    orderHeader = orderHeader.Where(o => o.OrderStatus == SD.StatusApproved);
                    break;

                default:
                    break;
            }


            return Json(new { data = orderHeader });    
        }
        #endregion


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int Id)
        {
            OrderVM = new OrderVM
            {
                OrderHeader = unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == Id, IncludeProperties: "ApplicationUser"),
                OrderDetails = unitOfWork.OrderDetail.GetAll(d => d.OrderId == Id, IncludeProperties: "Product")

            };
            return View(OrderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateDetails()
        {
            var orderHeaderFromDb = unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);

            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;


            if(OrderVM.OrderHeader.Carrier != null)
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }

            if(OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            unitOfWork.Save();

            TempData["success"] = "Order Detils Update Successfully.";

            return RedirectToAction(nameof(Details), "order", new { Id = orderHeaderFromDb.Id } );
        }


    }
}
