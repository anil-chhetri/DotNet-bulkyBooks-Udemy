using BulkyBooks.DataAccess.Repository.IRepository;
using BulkyBooks.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBooks.Web.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var productList = unitOfWork.Product.GetAll(IncludeProperties: "Category,CoverType");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart obj = new()
            {
                Count = 1,
                ProductId = productId,
                Product = unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, IncludeProperties: "Category,CoverType")
            };

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCart.ApplicationUserId = claim.Value;

            if(!ModelState.IsValid)
            {
                ShoppingCart obj = new()
                {
                    Count = shoppingCart.Count,
                    ProductId = shoppingCart.ProductId,
                    Product = unitOfWork.Product.GetFirstOrDefault(u => u.Id == shoppingCart.ProductId, IncludeProperties: "Category,CoverType")
                };
                return View(obj);
            }

            var fromDb = unitOfWork.ShoppingCart
                         .GetFirstOrDefault(c => c.ApplicationUserId == shoppingCart.ApplicationUserId && c.ProductId == shoppingCart.ProductId);

            if (fromDb == null)
            {
                unitOfWork.ShoppingCart.Add(shoppingCart);
            }
            else
            {
                unitOfWork.ShoppingCart.IncreaseOrder(shoppingCart, shoppingCart.Count);
            }

            unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
