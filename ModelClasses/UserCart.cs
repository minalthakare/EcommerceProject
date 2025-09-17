using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClasses
{
    public class UserCart
    {
        [Key]
        public int Id { get; set; }

        public int? ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product product { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser applicationUser { get; set; }

        public int Quantity { get; set; }
        [NotMapped]
        public double Price { get; set; }


    }
}

