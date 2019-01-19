using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Data;
using CoffeeShop.Models;
using CoffeeShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Controllers
{
    [Authorize(Roles = SD.AdminUser)]
    public class OrderPickupController : Controller
    {
        private ApplicationDbContext _context;

        public OrderPickupController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OrderPickup(string option = null, string search = null)
        {
            List<OrderConfirmationVM> OrderConfirmationVMList = new List<OrderConfirmationVM>();
            if (search != null)
            {
                var user = new ApplicationUser();
                List<OrderHeader> orderHeaderList = new List<OrderHeader>();
                if (option == "order")
                {
                    orderHeaderList = _context.OrderHeader.Where(o => o.OrderHeaderId == Convert.ToInt32(search)).ToList();
                }
                else
                {
                    if (option == "email")
                    {
                        user = _context.Users.Where(u => u.Email.ToLower().Contains(search.ToLower())).FirstOrDefault();
                    }
                }
                if (user != null || orderHeaderList.Count > 0)
                {
                    if (orderHeaderList.Count == 0)
                    {
                        orderHeaderList = _context.OrderHeader.Where(o => o.UserId == user.Id).OrderByDescending(o => o.OrderDate).ToList();
                    }

                    foreach (OrderHeader item in orderHeaderList)
                    {
                        OrderConfirmationVM individual = new OrderConfirmationVM();
                        individual.OrderDetailList = _context.OrderDetail.Where(o => o.OrderId == item.OrderHeaderId).ToList();
                        individual.OrderHeader = item;

                        OrderConfirmationVMList.Add(individual);
                    }
                }
            }
            else
            {
                //if there is no search criteria
                List<OrderHeader> OrderHeaderList = _context.OrderHeader.Where(u => u.Status == SD.StatusReady)
                                                                        .OrderByDescending(u => u.OrderDate).ToList();

                foreach (OrderHeader item in OrderHeaderList)
                {
                    OrderConfirmationVM individual = new OrderConfirmationVM();
                    individual.OrderDetailList = _context.OrderDetail.Where(o => o.OrderId == item.OrderHeaderId).ToList();
                    individual.OrderHeader = item;

                    OrderConfirmationVMList.Add(individual);
                }
            }
            return View(OrderConfirmationVMList);
        }

        public IActionResult OrderPickupDetails(int orderId)
        {
            OrderConfirmationVM OrderConfirmationVM = new OrderConfirmationVM();

            OrderConfirmationVM.OrderHeader = _context.OrderHeader.Where(o => o.OrderHeaderId == orderId).FirstOrDefault();
            OrderConfirmationVM.OrderHeader.ApplicationUser = _context.Users.Where(u => u.Id == OrderConfirmationVM.OrderHeader.UserId).FirstOrDefault();
            OrderConfirmationVM.OrderDetailList = _context.OrderDetail.Where(o => o.OrderId == OrderConfirmationVM.OrderHeader.OrderHeaderId).ToList();

            return View(OrderConfirmationVM);
        }

        [HttpPost]
        public IActionResult OrderPickupDetails(int orderId, OrderConfirmationVM orderConfirmationVM)
        {
            OrderConfirmationVM OrderConfirmationVM = new OrderConfirmationVM();
            OrderConfirmationVM = orderConfirmationVM;

            OrderHeader orderHeader = _context.OrderHeader.Find(orderId);
            orderHeader.Status = SD.StatusCompleted;

            _context.SaveChanges();

            return RedirectToAction("ManageOrder", "ManageOrder");
        }
    }
}