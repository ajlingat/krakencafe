using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Data;
using CoffeeShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Controllers
{
    [Authorize(Roles = SD.AdminUser)]
    public class CategoryTypeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryTypeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.CategoryType.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryType categoryType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categoryType);
                _context.SaveChanges();

                TempData["message"] = "Category type created!";

                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "There have been errors.");

            return View(categoryType);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CategoryType categoryType = _context.CategoryType.SingleOrDefault(x => x.Id == id);
            if (categoryType == null)
            {
                return NotFound();
            }

            return View(categoryType);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int? id, CategoryType categoryType)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(categoryType);
                _context.SaveChanges();

                TempData["message"] = "Category type edited!";

                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "There have been errors.");

            return View(categoryType);
        }
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CategoryType categoryType = _context.CategoryType.SingleOrDefault(x => x.Id == id);
            if (categoryType == null)
            {
                return NotFound();
            }
            return View(categoryType);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CategoryType categoryType = _context.CategoryType.SingleOrDefault(x => x.Id == id);
            if (categoryType == null)
            {
                return NotFound();
            }

            return View(categoryType);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id, CategoryType categoryType)
        {
            if (id == null)
            {
                return NotFound();
            }

            categoryType = _context.CategoryType.SingleOrDefault(x => x.Id == id);
            if (categoryType == null)
            {
                return NotFound();
            }

            _context.Remove(categoryType);
            _context.SaveChanges();

            TempData["message"] = "Person deleted!";

            return RedirectToAction("Index");
        }
    }
}