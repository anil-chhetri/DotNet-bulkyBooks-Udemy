using BulkyBooks.DataAccess.Repository.IRepository;
using BulkyBooks.Models;
using BulkyBooks.Models.ViewModels;
using BulkyBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public CartController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
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
            shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;

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

            unitOfWork.ShoppingCart.RemoveRange(shoppingCartVM.ShoppingCarts);
            unitOfWork.Save();

            return RedirectToAction("Index", "Home");
        }
    }
}
