using AptekaInternetApp.Models;
using AptekaInternetApp.Models.ModelProgram;
using AptekaInternetApp.Models.TablesDB;
using AptekaInternetApp.View.MainWindowFile.UsersControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AptekaInternetApp.ViewModel.Handlers
{
    public static class BannersHandler
    {
        public static void ManufactureSelection(string selectP, DependencyObject source)
        {
            if (string.IsNullOrEmpty(selectP))
            {
                MessageBox.Show("Выберите производителя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new ContextDB())
            {
                var now = DateTime.Now;

                var products = from p in context.Products
                               join m in context.Manufactures on p.ManufacturerId equals m.Id
                               join s in context.Stocks on p.Id equals s.ProductId
                               join d in context.Discounts on p.Id equals d.ProductId into discounts
                               from discount in discounts.DefaultIfEmpty()
                               where p.Manufacturer.NameManufacture == selectP
                                     && s.Quantity > 0
                                     && p.IsActive
                               select new CatalogProduct
                               {
                                   Id = p.Id,
                                   ImageCatalogProduct = p.ImageUrl,
                                   Name = p.Name,
                                   ReleaseForm = p.ReleaseForm,
                                   Dosage = p.Dosage,
                                   ActiveSubstance = p.ActiveIngredient,
                                   CountOnWarehouse = s.Quantity,
                                   CatalogPrice = p.Price,
                                   CountInPackage = p.PackageContentsQuantity,
                                   RequiresPrescription = p.RequiresPrescription,
                                   DiscountPercent = (discount != null && discount.IsActive
                                                     && discount.StartDate <= now
                                                     && discount.EndDate >= now)
                                                     ? discount.Percent : 0
                               };

                var productList = products.ToList();
                NavigationService.NavigateTo(new CatalogUserControl(products), new CatalogState
                {
                    Products = productList,
                    ManufacturerName = selectP
                });
            }
        }

        public static void MinStockSelection(DependencyObject source)
        {
            using (var context = new ContextDB())
            {
                var now = DateTime.Now;

                var query = from p in context.Products
                            join m in context.Manufactures on p.ManufacturerId equals m.Id
                            join s in context.Stocks on p.Id equals s.ProductId
                            join d in context.Discounts on p.Id equals d.ProductId into discounts
                            from discount in discounts.DefaultIfEmpty()
                            where p.IsActive &&
                                 (discount == null || (discount.IsActive && discount.StartDate <= now && discount.EndDate >= now))
                            orderby s.Quantity ascending
                            select new CatalogProduct
                            {
                                Id = p.Id,
                                ImageCatalogProduct = p.ImageUrl,
                                Name = p.Name,
                                ReleaseForm = p.ReleaseForm,
                                Dosage = p.Dosage,
                                ActiveSubstance = p.ActiveIngredient,
                                CountOnWarehouse = s.Quantity,
                                CatalogPrice = p.Price,
                                DiscountPercent = (discount != null && discount.IsActive
                                                    && discount.StartDate <= now
                                                    && discount.EndDate >= now)
                                                    ? discount.Percent : 0
                            };

                var products = query.ToList();
                NavigationService.NavigateTo(new CatalogUserControl(products), new CatalogState
                {
                    Products = products
                });
            }
        }
    }
}
