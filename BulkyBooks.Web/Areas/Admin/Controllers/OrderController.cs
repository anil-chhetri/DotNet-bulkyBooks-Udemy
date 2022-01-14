using BulkyBooks.DataAccess.Repository.IRepository;
using BulkyBooks.Models;
using BulkyBooks.Models.ViewModels;
using BulkyBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBooks.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
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
        public IActionResult GetAll(string status = null)
        {
            IEnumerable<OrderHeader> orderHeader;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
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


        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details_payNow()
        {
            OrderVM.OrderHeader = unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == OrderVM.OrderHeader.Id, "ApplicationUser");
            OrderVM.OrderDetails = unitOfWork.OrderDetail.GetAll(d => d.OrderId == OrderVM.OrderHeader.Id, "Product");


            //logic for checkout using stripe.
            var domain = "https://localhost:44386/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details/{OrderVM.OrderHeader.Id}",
            };



            foreach (var item in OrderVM.OrderDetails)
            {
                var sessionLineItemsOptions = new SessionLineItemOptions
                {
                    
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * item.Count * 100),
                        Currency = "inr",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title,
                        },

                    },
                    Quantity = item.Count,
                };

                options.LineItems.Add(sessionLineItemsOptions);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            //session id and paymentIntentId
            unitOfWork.OrderHeader.UpdateStripSessionId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }


        public IActionResult PaymentConfirmation(int orderId)
        {
            OrderHeader orderHeader = unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == orderId);

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    unitOfWork.OrderHeader.UpdateStatus(orderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    unitOfWork.Save();
                }
            }

            return View(orderId);
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
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


            if (OrderVM.OrderHeader.Carrier != null)
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }

            if (OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            unitOfWork.Save();

            TempData["success"] = "Order Detils Update Successfully.";

            return RedirectToAction(nameof(Details), "order", new { Id = orderHeaderFromDb.Id });
        }



        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            unitOfWork.Save();
            TempData["success"] = "Order Status Updated Successfully.";
            return RedirectToAction("details", "order", new { Id = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var orderHeader = unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);

            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if(orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PayementDueDate = DateTime.Now.AddDays(30);
            }
            unitOfWork.OrderHeader.Update(orderHeader);
            unitOfWork.Save();

            TempData["successs"] = "Order Shipped Successfully";
            return RedirectToAction("details", "order", new { Id = OrderVM.OrderHeader.Id });
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var OrderHeader = unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == OrderVM.OrderHeader.Id);

            if (OrderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = OrderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);
                unitOfWork.OrderHeader.UpdateStatus(OrderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                unitOfWork.OrderHeader.UpdateStatus(OrderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }

            unitOfWork.Save();

            TempData["success"] = "Order Cancelled successfully.";
            return RedirectToAction("details", "order", new { Id = OrderVM.OrderHeader.Id });
        }


    }
}
