using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptekaInternetApp.Models.ModelProgram
{
    public class ReportSales
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Categories { get; set; }
        public string Subcategories { get; set; }
        public decimal ProdPrice { get; set; }
        public decimal ProdQuantity { get; set; }
        public decimal TotalPrice { get; set; }
        
    }
}
