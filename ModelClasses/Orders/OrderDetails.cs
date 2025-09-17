using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClasses.Orders
{
    public class OrderDetails
    {
        [Key]
        public int Id { get; set; } 

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public UserOrder? UserOrder { get; set; }

        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [Required]
        public int Count { get; set; }

        [Required]
        public double Price { get; set; }


    }
}
