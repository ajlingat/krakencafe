using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CoffeeShop.Data;
using CoffeeShop.Models;
using CoffeeShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private ApplicationDbContext _context;

        public ShoppingCartController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Details()
        {
            ShoppingCartVM shoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new OrderHeader()
            };
            shoppingCartVM.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            var cart = _context.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value);
            if(cart != null)
            {
                shoppingCartVM.listCart = cart.ToList();
            }
            foreach (var list in shoppingCartVM.listCart)
            {
                list.menuItem = _context.MenuItem.FirstOrDefault(m => m.Id == list.MenuItemId);
                shoppingCartVM.OrderHeader.OrderTotal = shoppingCartVM.OrderHeader.OrderTotal + (list.menuItem.Price * list.Count);
                if (list.menuItem.Description.Length > 100)
                {
                    list.menuItem.Description = list.menuItem.Description.Substring(0, 99) + "...";
                }
            }

            return View(shoppingCartVM);
        }

        [HttpPost]
        public IActionResult Plus (int cartId)
        {
            var cart = _context.ShoppingCart.Where(c => c.ShoppingCartId == cartId).FirstOrDefault();
            cart.Count += 1;
            _context.SaveChanges();

            return RedirectToAction("Details");
        }

        [HttpPost]
        public IActionResult Minus (int cartId)
        {
            var cart = _context.ShoppingCart.Where(c => c.ShoppingCartId == cartId).FirstOrDefault();
            if (cart.Count == 1)
            {
                _context.ShoppingCart.Remove(cart);
                _context.SaveChanges();
                HttpContext.Session.SetInt32("CartCount", _context.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count);
            }
            else
            {
                cart.Count -= 1;
                _context.SaveChanges();
            }

            return RedirectToAction("Details");
        }

        [HttpPost]
        public IActionResult PlaceOrder(ShoppingCartVM shoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            shoppingCartVM.listCart = _context.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value).ToList();

            OrderHeader orderHeader = shoppingCartVM.OrderHeader;
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.UserId = claim.Value;
            shoppingCartVM.OrderHeader.Status = SD.StatusSubmitted;
            _context.OrderHeader.Add(orderHeader);
            _context.SaveChanges();

            foreach (var item in shoppingCartVM.listCart)
            {
                item.menuItem = _context.MenuItem.FirstOrDefault(m => m.Id == item.MenuItemId);
                OrderDetail orderDetails = new OrderDetail
                {
                    MenuItemId = item.MenuItemId,
                    OrderId = orderHeader.OrderHeaderId,
                    Name = item.menuItem.Name,
                    Description = item.menuItem.Description,
                    Price = item.menuItem.Price,
                    Count = item.Count
                };
                _context.OrderDetail.Add(orderDetails);
            }
            _context.ShoppingCart.RemoveRange(shoppingCartVM.listCart);

            HttpContext.Session.SetInt32("CartCount", 0);
            _context.SaveChanges();
            return RedirectToAction("OrderConfirmation",new { id = orderHeader.OrderHeaderId });
        }

        public IActionResult OrderConfirmation(int id)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderConfirmationVM orderConfirmationVM = new OrderConfirmationVM()
            {
                OrderHeader = _context.OrderHeader.Where(o => o.OrderHeaderId == id && o.UserId == claim.Value).FirstOrDefault(),
                OrderDetailList = _context.OrderDetail.Where(o => o.OrderId == id).ToList()
            };

            return View(orderConfirmationVM);
        }
        public IActionResult OrderHistory(int id = 0)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            List<OrderConfirmationVM> orderList = new List<OrderConfirmationVM>();

            List<OrderHeader> orderHeaderList = _context.OrderHeader.Where(u => u.UserId == claim.Value).OrderByDescending(u => u.OrderDate).ToList();

            if (id == 0 && orderHeaderList.Count > 4)
            {
                orderHeaderList = orderHeaderList.Take(5).ToList();
            }

            foreach (OrderHeader item in orderHeaderList)
            {
                OrderConfirmationVM individual = new OrderConfirmationVM();
                individual.OrderHeader = item;
                individual.OrderDetailList = _context.OrderDetail.Where(o => o.OrderDetailId == item.OrderHeaderId).ToList();

                orderList.Add(individual);
            }
            return View(orderList);
        }
    }
}