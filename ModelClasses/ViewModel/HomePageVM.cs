using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ModelClasses.ViewModel
{
    public class HomePageVM
    {
        public IEnumerable<Product> ProductList { get; set; }

        public IEnumerable<Category> CategoryList { get; set; }

        public string searchByName { get; set; }

        public string searchByCategory { get; set; }
    }
}
