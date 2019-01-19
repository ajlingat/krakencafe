using CoffeeShop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShop.Models
{
    public class OrderConfirmationVM
    {
        public OrderHeader OrderHeader { get; set; }
        public List<OrderDetail> OrderDetailList { get; set; }
    }
}
