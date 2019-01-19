using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CoffeeShop.Data;
using CoffeeShop.Models.AccountViewModels;
using CoffeeShop.Repositories;
using CoffeeShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<CategoryType> categoryTypes = _context.CategoryType.OrderBy(u => u.DisplayOrder).ToList();
            ViewBag.Category = categoryTypes;

            var applicationDbContext = _context.MenuItem.Include(m => m.CategoryType);
            return View(await applicationDbContext.ToListAsync());
        }
        [Authorize]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = _context.MenuItem
                .Include(m => m.CategoryType).Where(x => x.Id == id)
                .FirstOrDefault();
            if (menuItem == null)
            {
                return NotFound();
            }

            ShoppingCart cartOjb = new ShoppingCart()
            {
                MenuItemId = menuItem.Id,
                menuItem = menuItem
            };

            return View(cartOjb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details(int? id, ShoppingCart shoppingCart)
        {
            //if (id == null)
            //{
            //    return NotFound();
            //}
            if (ModelState.IsValid)
            {


                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                shoppingCart.ApplicationUserId = claim.Value;

                ShoppingCart cart = new ShoppingCart();
                cart = _context.ShoppingCart.Where(c => c.ApplicationUserId == shoppingCart.ApplicationUserId
                                     && c.MenuItemId == shoppingCart.MenuItemId).FirstOrDefault();

                if (cart == null)
                {

                    _context.ShoppingCart.Add(shoppingCart);
                }
                else
                {
                    cart.Count = cart.Count + shoppingCart.Count;
                }
                 _context.SaveChanges();

                //Add Session and increment count
                var count = _context.ShoppingCart.Where(c => c.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32("CartCount", count);

                //ModelState.AddModelError("", "There have been errors.");
                
                return RedirectToAction("Index");
            }
            else
            {
                var menuItem = _context.MenuItem
                .Include(m => m.CategoryType)
                .FirstOrDefault(m => m.Id == id);

                ShoppingCart cartOjb = new ShoppingCart()
                {
                    MenuItemId = menuItem.Id,
                    menuItem = menuItem
                };
                return View(shoppingCart);
            }
        }
    }
}

      