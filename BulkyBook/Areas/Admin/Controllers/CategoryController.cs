using BulkyBook.AccessData;
using BulkyBook.Models;
using BulkyBook.AccessData.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> categories = _unitOfWork.Category.GetAll();
            return View(categories);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {

            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name cannot be exactly the display order.");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();
                TempData.Add("success", "Category Added Successsfuly");
                return RedirectToAction("Index");
            }

            return View(category);

        }


        public IActionResult Edit(int? id)
        {
            if (id == 0 || id == null) return NotFound();

            else
            {
                var category = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);

                if (category == null) return NotFound();
                else
                {
                    return View(category);
                }
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {

            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name cannot be exactly the display order.");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(category);
                _unitOfWork.Save();
                TempData.Add("success", "Category Updated Successsfuly");
                return RedirectToAction("Index");
            }
            return View(category);

        }

        public IActionResult Delete(int? id)
        {
            if (id == 0 || id == null) return NotFound();

            else
            {
                var category = _unitOfWork.Category.GetFirstOrDefault(o => o.Id == id);

                if (category == null) return NotFound();
                else
                {
                    return View(category);
                }
            }

        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int id)
        {

            var category = _unitOfWork.Category.GetFirstOrDefault(o=> o.Id==id);

            if (category == null) return NotFound();

            _unitOfWork.Category.Remove(category);
            _unitOfWork.Save();
            TempData.Add("success", "Category Deleted Successsfuly");
            return RedirectToAction("Index");


        }

    }
}
