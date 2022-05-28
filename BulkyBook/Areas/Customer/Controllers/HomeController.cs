using BulkyBook.AccessData.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBook.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includedProps:"Category,CoverType");
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Details(int ProductId)
        {
            if (ProductId == 0)
            {
                return NotFound();
            }

            ShoppingCart shoppingCart = new()
            {
                Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == ProductId, includedProps: "Category,CoverType"),
                Count = 1,
                ProductId = ProductId

            };
            return View(shoppingCart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserID = claim.Value;

            // check if it exist 
            var CartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.ApplicationUserID == claim.Value && u.ProductId == shoppingCart.ProductId);
            if (CartFromDb == null)
            {
                // adding
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            } else
            {
                // updating
                _unitOfWork.ShoppingCart.IncrementCount(CartFromDb, shoppingCart.Count);
            }

            _unitOfWork.Save();
            TempData.Add("success", "Product Added Successsfuly To The Cart");
            return RedirectToAction(nameof(Index), "Cart");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}