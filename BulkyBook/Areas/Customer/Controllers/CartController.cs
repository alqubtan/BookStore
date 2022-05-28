using BulkyBook.AccessData.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM shoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartVM = new ShoppingCartVM
            {
                cartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserID == claim.Value, includedProps: "Product,Product.CoverType"),
                OrderHeader = new()
            };

            foreach (var cart in shoppingCartVM.cartList)
            {
                cart.Price = GetPriceBasedOnCount(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                shoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            return View(shoppingCartVM);
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            // Get all shopping carts of the user
            shoppingCartVM = new ShoppingCartVM
            {
                cartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserID == claim.Value, includedProps: "Product,Product.CoverType"),
                OrderHeader = new()
            };

            // Get all the user information and shipping details of him
            shoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.state;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;

            // Set the orderHeader.total by increment each cart price
            foreach (var cart in shoppingCartVM.cartList)
            {
                cart.Price = GetPriceBasedOnCount(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                shoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            return View(shoppingCartVM);


        }
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST(ShoppingCartVM shoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // get all carts in order
            shoppingCartVM.cartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserID == claim.Value, includedProps: "Product");

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            // set all needed information in orderHeader
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // {User Flow}
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                // {Company Flow}
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelyedPayment;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;



            // calc orderHeader.OrderTotal
            foreach (var cart in shoppingCartVM.cartList)
            {
                cart.Price = GetPriceBasedOnCount(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                shoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Save();


            // Set up Order Detail
            foreach (var cart in shoppingCartVM.cartList)
            {
                OrderDetail OrderDetail = new OrderDetail
                {
                    ProductId = cart.ProductId,
                    Count = cart.Count,
                    Price = cart.Price,
                    OrderId = shoppingCartVM.OrderHeader.Id

                };
                _unitOfWork.OrderDetail.Add(OrderDetail);
                _unitOfWork.Save();

            }


            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // {User Flow}
                var domain = "https://localhost:44359";
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                    LineItems = new List<SessionLineItemOptions>(),

                    Mode = "payment",
                    SuccessUrl = domain + $"/customer/cart/orderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + $"/customer/cart/index",
                };

                foreach (var cart in shoppingCartVM.cartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(cart.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = cart.Product.Title
                            },
                        },
                        Quantity = cart.Count
                    };

                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);

                _unitOfWork.OrderHeader.UpdatePaymentSession(shoppingCartVM.OrderHeader.Id, session.Id, session.Id);

                _unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);

            }
            else
            {
                return RedirectToAction("orderConfirmation", "Cart", new
                {
                    id = shoppingCartVM.OrderHeader.Id
                });
            }

            // Stripe Settings 



            //_unitOfWork.ShoppingCart.RemoveRange(shoppingCartVM.cartList);
            //_unitOfWork.Save();
            //return RedirectToAction(nameof(Index));


        }

        public IActionResult orderConfirmation(int id)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);

            if (orderHeader.PaymentStatus != SD.PaymentStatusDelyedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll
            (u => u.ApplicationUserID == orderHeader.ApplicationUserId).ToList();

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);


        }

        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Delete(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }


        private double GetPriceBasedOnCount(int quantity, double price, double price50, double price100)
        {
            if (quantity < 50)
            {
                return price;
            }
            else
            {
                if (price < 100)
                {
                    return price50;
                }
                return price100;
            }

        }



    }
}
