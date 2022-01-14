using BulkyBooks.DataAccess.Repository.IRepository;
using BulkyBooks.Models;
using BulkyBooks.Models.ViewModels;
using BulkyBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBooks.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IEmailSender emailSender;

        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            this.unitOfWork = unitOfWork;
            this.emailSender = emailSender;
        }
        public IActionResult Index()
        {
            var claims = (ClaimsIdentity)User.Identity;
            var identity = claims.FindFirst(ClaimTypes.NameIdentifier);

            var userId = identity.Value;

            var shoppingCarts = new ShoppingCartViewModel()
            {
                ShoppingCarts = unitOfWork.ShoppingCart.GetAll(filter: c => c.ApplicationUserId == userId, IncludeProperties: "Product"),
                OrderHeader = new Models.OrderHeader()

            };

            foreach (var item in shoppingCarts.ShoppingCarts)
            {
                item.Product.ProductPrice = price(item.Count
                                            , item.Product.Price, item.Product.Price50, item.Product.Price100);

                shoppingCarts.OrderHeader.TotalAmount += (item.Count * item.Product.Price);
            }


            return View(shoppingCarts);
        }

        [NonAction]
        public double price(int count, double price, double price50, double price100)
        {
            if (count <= 50)
            {
                return price;
            }
            else
            {
                if (count <= 100)
                {
                    return price50;
                }
                else
                {
                    return price100;
                }

            }

        }



        public IActionResult Add(int cartid)
        {
            var fromdb = unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartid);
            if (fromdb != null)
            {
                unitOfWork.ShoppingCart.IncreaseOrder(fromdb, 1);
                unitOfWork.Save();
            }
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Minus(int cartid)
        {
            var fromdb = unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartid);
            if (fromdb != null)
            {
                unitOfWork.ShoppingCart.DecreseOrder(fromdb, 1);
                unitOfWork.Save();
            }
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Delete(int cartId)
        {
            var fromdb = unitOfWork.ShoppingCart.GetFirstOrDefault(s => s.Id == cartId);
            if (fromdb != null)
            {
                unitOfWork.ShoppingCart.Remove(fromdb);
                unitOfWork.Save();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claims = (ClaimsIdentity)User.Identity;
            var identity = claims.FindFirst(ClaimTypes.NameIdentifier);
            var userId = identity.Value;
            var applicationUser = unitOfWork.ApplicationUser.GetFirstOrDefault(filter: a => a.Id == userId);

            var shoppingCarts = new ShoppingCartViewModel()
            {
                ShoppingCarts = unitOfWork.ShoppingCart.GetAll(filter: c => c.ApplicationUserId == userId, IncludeProperties: "Product"),
                OrderHeader = new Models.OrderHeader()

            };

            shoppingCarts.OrderHeader.Name = applicationUser.Name;
            shoppingCarts.OrderHeader.StreetAddress = applicationUser.StreetAddress;
            shoppingCarts.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            shoppingCarts.OrderHeader.City = applicationUser.City;
            shoppingCarts.OrderHeader.State = applicationUser.State;
            shoppingCarts.OrderHeader.PostalCode = applicationUser.PostalCode;

            foreach (var item in shoppingCarts.ShoppingCarts)
            {
                item.Product.ProductPrice = price(item.Count
                                            , item.Product.Price, item.Product.Price50, item.Product.Price100);

                shoppingCarts.OrderHeader.TotalAmount += (item.Count * item.Product.Price);
            }


            return View(shoppingCarts);
        }


        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(ShoppingCartViewModel shoppingCartVM)
         {
            var claims = (ClaimsIdentity)User.Identity;
            var identity = claims.FindFirst(ClaimTypes.NameIdentifier);
            var userId = identity.Value;
            var applicationUser = unitOfWork.ApplicationUser.GetFirstOrDefault(filter: a => a.Id == userId);


            shoppingCartVM.ShoppingCarts = unitOfWork.ShoppingCart
                                            .GetAll(filter: c => c.ApplicationUserId == userId, IncludeProperties: "Product");

            shoppingCartVM.OrderHeader.ApplicationUserId = userId;
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            }
            else
            {
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            }

            unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            unitOfWork.Save();



            foreach (var item in shoppingCartVM.ShoppingCarts)
            {
                item.Product.ProductPrice = price(item.Count
                                            , item.Product.Price, item.Product.Price50, item.Product.Price100);

                shoppingCartVM.OrderHeader.TotalAmount += (item.Count * item.Product.Price);
            }


            foreach (var item in shoppingCartVM.ShoppingCarts)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = item.ProductId,
                    OrderId = shoppingCartVM.OrderHeader.Id,
                    Count = item.Count,
                    Price = item.Product.ProductPrice,
                };

                unitOfWork.OrderDetail.Add(orderDetail);
                unitOfWork.Save();
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {

                //logic for checkout using stripe.
                var domain = "https://localhost:44386/";
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + $"customer/cart/index",
                };



                foreach (var item in shoppingCartVM.ShoppingCarts)
                {
                    var sessionLineItemsOptions = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Product.ProductPrice * 100),
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
                unitOfWork.OrderHeader.UpdateStripSessionId(shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);

            }
            else
            {
                return RedirectToAction("OrderConfirmation", "cart", new { Id = shoppingCartVM.OrderHeader.Id });
            }

        }

        public IActionResult OrderConfirmation(int Id)
        {
            OrderHeader orderHeader = unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == Id, "ApplicationUser");

            if(orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    unitOfWork.OrderHeader.UpdateStatus(Id, SD.StatusApproved, SD.PaymentStatusApproved);
                    unitOfWork.Save();
                }
            }

            emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New order Bulky Books.", "<p> order completed </p>");


            List<ShoppingCart> shoppingCarts = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            unitOfWork.Save();

            return View(Id);
        }


    }
}
