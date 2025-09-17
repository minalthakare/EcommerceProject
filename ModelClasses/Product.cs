using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ModelClasses
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Range(1, 5000, ErrorMessage = "Range 1 to 4999.99 only")]
        
        public double Price { get; set; }

        [Required]
        [MaxLength(2000, ErrorMessage = "Length can not exist more than 30 characters")]
        public string Description { get; set; }


        public ICollection<PImage>? ImgUrls { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }


        public string? HomeImgUrl { get; set; }
    }
}
