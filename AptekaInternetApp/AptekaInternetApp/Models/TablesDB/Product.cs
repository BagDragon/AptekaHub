using AptekaInternetApp.Models.TablesDB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptekaInternetApp.Models.Tables
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; }

        [Required, Range(0, 999999)]
        public decimal Price { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [ForeignKey("Subcategory")]
        public int SubcategoryId { get; set; }

        [ForeignKey("Manufacturer")]
        public int ManufacturerId { get; set; }

        [StringLength(50)]
        public string ReleaseForm { get; set;}

        [StringLength(50)]
        public string Packaging { get; set; }
       
        public string StorageConditions { get; set; }

        [StringLength(200)]
        public string ActiveIngredient { get; set; }

        [StringLength(50)]
        public string Dosage { get; set; }
        
        public string PackageContentsQuantity { get; set; }

        public int ShelfLifeMonths { get; set; }
        public string ImageUrl { get; set; }
        public bool RequiresPrescription { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual Category Category { get; set; }
        public virtual Subcategory Subcategory { get; set; }
        public virtual Manufacturer Manufacturer { get; set; }
        public virtual ICollection<Stock> Stocks { get; set; }
        public virtual ICollection<Discount> Discounts { get; set; }
        public virtual ICollection<Prescription> Prescriptions { get; set; }
    }
}
