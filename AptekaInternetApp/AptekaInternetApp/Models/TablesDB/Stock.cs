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
    public class Stock
    {
        [Key]
        public int Id { get; set; }
       
        public int ProductId { get; set; }

        [Range(0, 99999)]
        public int Quantity { get; set; }

        public int MinStockLevel { get; set; } = 5;
        public int MaxStockLevel { get; set; } = 500;
                   
        public DateTime LastRestock { get; set; } = DateTime.Now;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
