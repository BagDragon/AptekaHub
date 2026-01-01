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
    public static class CategoryHandler
    {
        public static void HandleCategorySelection(Category select, DependencyObject source)
        {
            if (select == null)
            {
                MessageBox.Show("Выберите категорию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int id = select.ID_Category;
            var now = DateTime.Now;

            using (var context = new ContextDB())
            {
                var query = from p in context.Products
                            join m in context.Manufactures on p.ManufacturerId equals m.Id
                            join s in context.Stocks on p.Id equals s.ProductId
                            join d in context.Discounts on p.Id equals d.ProductId into discounts
                            from discount in discounts.DefaultIfEmpty()
                            where p.CategoryId == id
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

                // Materialize the query immediately
                var products = query.ToList();

                CatalogUserControl catalogUserControl = new CatalogUserControl(products);

                var subcategories = from s in context.Subcategories
                                    where s.CategoryId == id
                                    select s;

                var manufacturers = (from p in context.Products
                                     join m in context.Manufactures on p.ManufacturerId equals m.Id
                                     join s in context.Stocks on p.Id equals s.ProductId
                                     where p.CategoryId == id
                                     select m).Distinct();

                catalogUserControl.brandsListView.ItemsSource = manufacturers.ToList();
                catalogUserControl.subcategoryListView.ItemsSource = subcategories.ToList();

                NavigationService.NavigateTo(catalogUserControl, new CatalogState
                {
                    Products = products,
                    CategoryId = id
                });
            }
        }

        public static List<Category> LoadCategories()
        {
            using (var context = new ContextDB())
            {
                return context.Categories.ToList();
            }
        }
    }
}

