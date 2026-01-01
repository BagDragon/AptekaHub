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
    public static class ManufactureHandler
    {
        public static void HandleManufactureSelection(Manufacturer selectMan, DependencyObject source)
        {
          
            int id = selectMan.Id;


            using (var context = new ContextDB())
            {
                var now = DateTime.Now;

                var query = from p in context.Products
                            join m in context.Manufactures on p.ManufacturerId equals m.Id
                            join s in context.Stocks on p.Id equals s.ProductId
                            join d in context.Discounts on p.Id equals d.ProductId into discounts
                            from discount in discounts.DefaultIfEmpty()
                            where m.Id == id
                            where p.IsActive &&
                                 (discount == null || (discount.IsActive && discount.StartDate <= now && discount.EndDate >= now))
                            select new CatalogProduct
                            {
                                Id = p.Id,
                                ImageCatalogProduct = p.ImageUrl,
                                Name = p.Name,
                                ReleaseForm = p.ReleaseForm,
                                RequiresPrescription = p.RequiresPrescription,
                                Dosage = p.Dosage,
                                CountInPackage = p.PackageContentsQuantity,
                                ActiveSubstance = p.ActiveIngredient,
                                CountOnWarehouse = s.Quantity,
                                CatalogPrice = p.Price,
                                DiscountPercent = (discount != null && discount.IsActive
                                                     && discount.StartDate <= now
                                                     && discount.EndDate >= now)
                                                     ? discount.Percent : 0
                            };


                NavigationService.NavigateTo(new CatalogUserControl(query));
            }

        }

        public static List<Manufacturer> LoadManufactures()
        {
            using (var context = new ContextDB())
            {
                return (from p in context.Products
                        join m in context.Manufactures on p.ManufacturerId equals m.Id
                        select m).Distinct().Take(7).ToList();
            }
        }


    }
}
