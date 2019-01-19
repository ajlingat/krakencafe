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
    public class ManageOrderController : Controller
    {
        private ApplicationDbContext _context;

        public ManageOrderController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public IActionResult ManageOrder()
        {
            List<OrderConfirmationVM> OrderConfirmationVMList = new List<OrderConfirmationVM>();

            List<OrderHeader> OrderHeaderList = _context.OrderHeader.Where(u => u.Status != SD.StatusCompleted &&
                                                                                u.Status != SD.StatusReady &&
                                                                                u.Status != SD.StatusCancelled)
                                                                    .OrderByDescending(u => u.OrderDate).ToList();

            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderConfirmationVM individual = new OrderConfirmationVM();
                individual.OrderDetailList = _context.OrderDetail.Where(o => o.OrderId == item.OrderHeaderId).ToList();
                individual.OrderHeader = item;

                OrderConfirmationVMList.Add(individual);
            }
            return View(OrderConfirmationVMList);
        }

        [HttpPost]
        public IActionResult OrderPrepare(int orderId)
        {
            OrderHeader orderHeader = _context.OrderHeader.Find(orderId);
            orderHeader.Status = SD.StatusInProcess;
            _context.SaveChanges();

            return RedirectToAction("ManageOrder");
        }

        [HttpPost]
        public IActionResult OrderReady(int orderId)
        {
            OrderHeader orderHeader = _context.OrderHeader.Find(orderId);
            orderHeader.Status = SD.StatusReady;
            _context.SaveChanges();

            return RedirectToAction("ManageOrder");
        }

        [HttpPost]
        public IActionResult OrderCancel(int orderId)
        {
            OrderHeader orderHeader = _context.OrderHeader.Find(orderId);
            orderHeader.Status = SD.StatusCancelled;
            _context.SaveChanges();

            return RedirectToAction("ManageOrder");
        }
    }
}