using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptekaInternetApp.Models.ModelProgram
{
    public class CatalogState
    {
        public List<CatalogProduct> Products { get; set; }
        public string Search { get; set; }
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public int? ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
    }
}
