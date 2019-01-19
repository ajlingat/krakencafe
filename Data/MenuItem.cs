using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShop.Data
{
    public class MenuItem
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public String Name { get; set; }
        [Required]
        public String Description { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Price should be greater than ${1}")]
        [DataType(DataType.Currency)]
        public Decimal Price { get; set; }


        [Display(Name="Category Type")]
        public int CategoryTypeId { get; set; }

        [ForeignKey("CategoryTypeId")]
        public virtual CategoryType CategoryType { get; set; }
    }
}
