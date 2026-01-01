using AptekaInternetApp.Models.ModelProgram;
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
    public class Sale
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        [ForeignKey("Cashier")]
        public int CashierId { get; set; }

        [Range(0, 999999)]
        public decimal Total { get; set; }

        [Range(0, 999999)]
        public decimal Discount { get; set; }

        [StringLength(20)]
        public string ReceiptNumber { get; set; }

        [StringLength(100)]
        public string Customer { get; set; }

        public virtual User Cashier { get; set; }
        public virtual ICollection<SaleItem> Items { get; set; }
    }
}
