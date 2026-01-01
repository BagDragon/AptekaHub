using AptekaInternetApp.Models.Tables;
using AptekaInternetApp.Models.TablesDB;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptekaInternetApp.Models
{
    public class ContextDB : DbContext
    {
        public ContextDB() : base(
            "Data Source=HOME-PC; Initial Catalog=AptekaHubDB; Integrated Security=True;"
            //"Data Source=HOME-PC; Initial Catalog=AptekaHubDB; Integrated Security=True;"
            )
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Manufacturer> Manufactures { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SalesItems { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }  
        public DbSet<User> Users { get; set; }
    }
}
