using BulkyBook.AccessData;
using BulkyBook.Models;
using BulkyBook.AccessData.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Controllers
{
    [Area("Admin")]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<CoverType> coverTypes = _unitOfWork.CoverType.GetAll();
            return View(coverTypes);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType coverType)
        {

           
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Add(coverType);
                _unitOfWork.Save();
                TempData.Add("success", "Cover Type Added Successsfuly");
                return RedirectToAction("Index");
            }

            return View(coverType);

        }


        public IActionResult Edit(int? id)
        {
            if (id == 0 || id == null) return NotFound();

            else
            {
                var coverType = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Id == id);

                if (coverType == null) return NotFound();
                else
                {
                    return View(coverType);
                }
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType coverType)
        {

         
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Update(coverType);
                _unitOfWork.Save();
                TempData.Add("success", "Cover Type Updated Successsfuly");
                return RedirectToAction("Index");
            }
            return View(coverType);

        }

        public IActionResult Delete(int? id)
        {
            if (id == 0 || id == null) return NotFound();

            else
            {
                var coverType = _unitOfWork.CoverType.GetFirstOrDefault(o => o.Id == id);

                if (coverType == null) return NotFound();
                else
                {
                    return View(coverType);
                }
            }

        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int id)
        {

            var product = _unitOfWork.Product.GetFirstOrDefault(o=> o.Id==id);

            if (product == null) return NotFound();

            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();
            TempData.Add("success", "Product Deleted Successsfuly");
            return RedirectToAction("Index");


        }

    }
}
