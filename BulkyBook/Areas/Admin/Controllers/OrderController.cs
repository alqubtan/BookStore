using BulkyBook.AccessData.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private IUnitOfWork _unitOfWork { get; set; }

        [BindProperty]
        public orderVm orderVm { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderID)
        {
            orderVm = new orderVm()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderID, includedProps: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == orderID, includedProps: "Product")

            };

            return View(orderVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVm.OrderHeader.Id, tracked: false);
            orderHeaderFromDb.Name = orderVm.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderVm.OrderHeader.PhoneNumber;
            orderHeaderFromDb.State = orderVm.OrderHeader.State;
            orderHeaderFromDb.City = orderVm.OrderHeader.City;
            orderHeaderFromDb.PostalCode = orderVm.OrderHeader.PostalCode;
            orderHeaderFromDb.StreetAddress = orderVm.OrderHeader.StreetAddress;

            if (orderVm.OrderHeader.Carrier != null)
            {
                orderHeaderFromDb.Carrier = orderVm.OrderHeader.Carrier;
            }
            if (orderVm.OrderHeader.TackingNumber != null)
            {
                orderHeaderFromDb.TackingNumber = orderVm.OrderHeader.TackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Order Details Updated Successfuly";
            return RedirectToAction("Details", "Order", new { orderId = orderHeaderFromDb.Id });
        }
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderVm.OrderHeader.Id, SD.StatusInProccess);
            _unitOfWork.Save();
            TempData["success"] = "Order Status Updated Successfuly";
            return RedirectToAction("Details", "Order", new { orderId = orderVm.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVm.OrderHeader.Id, tracked: false);
            orderHeaderFromDb.Carrier = orderVm.OrderHeader.Carrier;
            orderHeaderFromDb.TackingNumber = orderVm.OrderHeader.TackingNumber;
            orderHeaderFromDb.OrderStatus = SD.StatusShipped;
            orderHeaderFromDb.ShippingDate = DateTime.Now;
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Order Shiped Successfuly";
            return RedirectToAction("Details", "Order", new { orderId = orderVm.OrderHeader.Id });
        }


        #region API Calls
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                // get all orders for admin or employee
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includedProps: "ApplicationUser");
            }
            else
            {
                // get just orders by logged in user
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

                orderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == claim.Value, includedProps: "ApplicationUser");

            }


            switch (status)
            {
                case "Pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelyedPayment);
                    break;
                case "InProcess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProccess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "Approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:

                    break;

            }

            return Json(new { data = orderHeaders });
        }
        #endregion


    }
}
