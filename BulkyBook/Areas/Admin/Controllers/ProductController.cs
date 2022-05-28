using BulkyBook.AccessData.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
           return View();
        }

        public IActionResult Upsert(int id)
        {
            ProductVM productVM = new ProductVM();
            productVM.product = new Product();
            productVM.CoverTypesList = _unitOfWork.CoverType.GetAll().Select
                (u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });

            productVM.CategoriesList = _unitOfWork.Category.GetAll().Select
                (u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });

            if (id == 0)
            {
               // Creating product
                return View(productVM);
            }
            
            else
            {

                // Updating product

                // get the product based on the id
                var product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
                // set the productVm and pass it to view
                productVM.product = product;
                return View(productVM);
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            
            if (ModelState.IsValid)
            {

                if (file != null)
                {
                    string webRootPath = _webHostEnvironment.WebRootPath;
                    var upload = Path.Combine(webRootPath, @"images/products");
                    var fileName = Guid.NewGuid().ToString();
                    var extention = Path.GetExtension(file.FileName).ToLower();
                    // check if there is already an image for that product'
                    if (productVM.product.ImageUrl != null)
                    {
                        var oldImage = Path.Combine(webRootPath, productVM.product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImage))
                        {
                            System.IO.File.Delete(oldImage);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extention), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.product.ImageUrl = @"\images\products\" + fileName + extention;
                }
                if (productVM.product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.product);
                    TempData.Add("success", "Product Created Successsfuly");
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.product);
                    TempData.Add("success", "Product Updated Successsfuly");
                }
                _unitOfWork.Save();
           
                return RedirectToAction("Index");
                
                
                
            }
            return View(productVM);

        }
       

        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var productsList = _unitOfWork.Product.GetAll(includedProps: "Category,CoverType");

            return Json(new { data = productsList });

        }

        [HttpDelete]
        public IActionResult Delete(int id)

        {
            var product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
            if (product == null) {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            // delete the image and product.
            if (product.ImageUrl != null)
            {
                var oldImage = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));

                if (System.IO.File.Exists(oldImage))
                {
                    System.IO.File.Delete(oldImage);
                }
            }
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Product Deleted Successfuly" });
        }


        #endregion

    }
}
