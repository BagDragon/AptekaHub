using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptekaInternetApp.Models.ModelProgram
{
    public class CartProduct
    {
        public int Id { get; set; }
        public string NameProduct { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string CountInPackage { get; set; }

        public string ImageCatalogProduct { get; set; }
        public int StockQuantity { get; set; }
        public bool RequiresPrescription { get; set; }

        public decimal DiscountPercent { get; set; }
        public decimal PriceWithDiscount
        {
            get
            {
                if (DiscountPercent > 0)
                {
                    return Price - (Price * DiscountPercent / 100m);
                }
                return Price;
            }
        }

        public decimal TotalWithDiscount => PriceWithDiscount * Quantity;
        public bool HasDiscount => DiscountPercent > 0;
    }
}
