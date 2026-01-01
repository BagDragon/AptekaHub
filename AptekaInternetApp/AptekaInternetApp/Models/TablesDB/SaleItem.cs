using AptekaInternetApp.Models.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptekaInternetApp.Models.TablesDB
{
    public class SaleItem
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Sale")]
        public int SaleId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Range(1, 999)]
        public int Quantity { get; set; }

        [Range(0, 999999)]
        public decimal Price { get; set; }

        [Range(0, 100)]
        public decimal AppliedDiscount { get; set; } 

        [Range(0, 999999)]
        public decimal Total { get; set; } 

        public virtual Sale Sale { get; set; }
        public virtual Product Product { get; set; }
    }
}
