using AptekaInternetApp.Models;
using AptekaInternetApp.Models.ModelProgram;
using AptekaInternetApp.Models.TablesDB;
using AptekaInternetApp.View.MainWindowFile.UsersControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace AptekaInternetApp.ViewModel
{
    public static class SubcategoryHandler
    {
        public static void HandleSubcategorySelection(Subcategory selectSub, DependencyObject source)
        {
            if (selectSub == null)
            {
                MessageBox.Show("Выберите подкатегорию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int id = selectSub.ID_Subcategory;
            int idCat = selectSub.CategoryId;

            using (var context = new ContextDB())
            {
                var now = DateTime.Now;

                var products = from p in context.Products
                               join m in context.Manufactures on p.ManufacturerId equals m.Id
                               join s in context.Stocks on p.Id equals s.ProductId
                               join d in context.Discounts on p.Id equals d.ProductId into discounts
                               from discount in discounts.DefaultIfEmpty()
                               where p.SubcategoryId == id
                               where p.IsActive &&
                               (discount == null || (discount.IsActive && discount.StartDate <= now && discount.EndDate >= now))
                               select new CatalogProduct
                               {
                                   Id = p.Id,
                                   ImageCatalogProduct = p.ImageUrl,
                                   Name = p.Name,
                                   ReleaseForm = p.ReleaseForm,
                                   Dosage = p.Dosage,
                                   CountInPackage = p.PackageContentsQuantity,
                                   ActiveSubstance = p.ActiveIngredient,
                                   RequiresPrescription = p.RequiresPrescription,
                                   CountOnWarehouse = s.Quantity,
                                   CatalogPrice = p.Price,
                                   DiscountPercent = (discount != null && discount.IsActive
                                                     && discount.StartDate <= now
                                                     && discount.EndDate >= now)
                                                     ? discount.Percent : 0
                               };

                var productList = products.ToList();

                CatalogUserControl catalogUserControl = new CatalogUserControl(products);

                var subcategories = from s in context.Subcategories
                                    where s.CategoryId == idCat
                                    select s;

                var manufacturers = (from p in context.Products
                                     join m in context.Manufactures on p.ManufacturerId equals m.Id
                                     where p.CategoryId == idCat
                                     select m).Distinct();

                catalogUserControl.brandsListView.ItemsSource = manufacturers.ToList();
                catalogUserControl.subcategoryListView.ItemsSource = subcategories.ToList();

                NavigationService.NavigateTo(catalogUserControl, new CatalogState
                {
                    Products = productList,
                    SubcategoryId = id,
                    CategoryId = idCat
                });
            }
        }

        public static List<Subcategory> LoadSubcategories()
        {
            using (var context = new ContextDB())
            {
                var targetIds = new List<int> { 4, 15, 10, 9, 6 };
                return context.Subcategories
                             .Where(s => targetIds.Contains(s.ID_Subcategory))
                             .ToList();
            }
        }
    }
}
