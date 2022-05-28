using BulkyBook.AccessData.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
           return View();
        }

        public IActionResult Upsert(int id)
        {
            Company company = new Company();

            if (id == 0)
            {
               // Creating Company
                return View(company);
            }
            
            else
            {
                // Updating company

                // get company
                var companyFromDb = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
                
                return View(companyFromDb);
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company company)
        {
            
            if (ModelState.IsValid)
            {

               
                if (company.Id == 0)
                {
                    // Creating
                    _unitOfWork.Company.Add(company);
                    TempData.Add("success", "Company Created Successsfuly");
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                    TempData.Add("success", "Company Updated Successsfuly");
                }
                _unitOfWork.Save();
           
                return RedirectToAction("Index");
                
                
                
            }
            return View(company);

        }
       

        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var companiesList = _unitOfWork.Company.GetAll();

            return Json(new { data = companiesList });

        }

        [HttpDelete]
        public IActionResult Delete(int id)

        {
            var company = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
            if (company == null) {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            
            _unitOfWork.Company.Remove(company);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Company Deleted Successfuly" });
        }


        #endregion

    }
}
