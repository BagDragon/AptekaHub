using AptekaInternetApp.Models;
using AptekaInternetApp.Models.ModelProgram;
using AptekaInternetApp.Models.TablesDB;
using AptekaInternetApp.View.GeneralFile.UserControls;
using AptekaInternetApp.ViewModel;
using AptekaInternetApp.ViewModel.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CatalogState = AptekaInternetApp.Models.ModelProgram.CatalogState;

namespace AptekaInternetApp.View.MainWindowFile.UsersControls
{
    /// <summary>
    /// Логика взаимодействия для CatalogUserControl.xaml
    /// </summary>
    public partial class CatalogUserControl : UserControl
    {
        private IQueryable<CatalogProduct> _currentQuery;
        private string _currentSearch;
        private List<CatalogProduct> _products;

        #region Конструктор

        public CatalogUserControl(List<CatalogProduct> products)
        {
            InitializeComponent();

            _products = products;
            ListViewProducts.ItemsSource = _products;
            LoadFilters();

            // Инициализируем состояние корзины
            RefreshCartState();

            CartHandler.CartChanged += OnCartChanged;
            this.Unloaded += CatalogUserControl_Unloaded;
        }

        public CatalogUserControl()
        {
            InitializeComponent();
            LoadData();
            CartHandler.CartChanged += OnCartChanged;
            this.Unloaded += CatalogUserControl_Unloaded;
        }

        public CatalogUserControl(IQueryable<CatalogProduct> queryable)
        {
            InitializeComponent();

            _currentQuery = queryable;
            _products = queryable.ToList();
            ListViewProducts.ItemsSource = _products;
            LoadFilters();

            // Инициализируем состояние корзины
            RefreshCartState();

            CartHandler.CartChanged += OnCartChanged;
            this.Unloaded += CatalogUserControl_Unloaded;
        }

        public CatalogUserControl(string search)
        {
            InitializeComponent();
            _currentSearch = search;
            LoadData();
            CartHandler.CartChanged += OnCartChanged;
            this.Unloaded += CatalogUserControl_Unloaded;
        }
        #endregion

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            if (this.Parent != null)
            {
                // Подписываемся когда контрол добавляется в визуальное дерево
                CartHandler.CartChanged += OnCartChanged;

                // Инициализируем состояние корзины
                RefreshCartState();
            }
            else
            {
                // Отписываемся когда контрол удаляется из визуального дерева
                CartHandler.CartChanged -= OnCartChanged;
            }
        }

        private void CatalogUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            CartHandler.CartChanged -= OnCartChanged;
            this.Unloaded -= CatalogUserControl_Unloaded;
        }

        private void OnCartChanged(int productId, int newQuantity)
        {
            Dispatcher.Invoke(() =>
            {
                // Обновляем конкретный товар
                var productToUpdate = _products?.FirstOrDefault(p => p.Id == productId);
                if (productToUpdate != null)
                {
                    productToUpdate.OnCartPropertiesChanged();

                    // Находим контейнер элемента и обновляем его
                    var container = ListViewProducts.ItemContainerGenerator.ContainerFromItem(productToUpdate) as ListViewItem;
                    if (container != null)
                    {
                        // Принудительно обновляем DataContext для контейнера
                        var contentPresenter = FindVisualChild<ContentPresenter>(container);
                        if (contentPresenter != null)
                        {
                            contentPresenter.Content = null;
                            contentPresenter.Content = productToUpdate;
                        }
                    }
                }

                // Также обновляем все товары для гарантии
                foreach (var product in _products)
                {
                    product.OnCartPropertiesChanged();
                }
            });
        }

        #region Загрузка данных

        private void LoadData()
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
                                DiscountPercent = discount != null ? discount.Percent : 0
                            };

                if (!string.IsNullOrEmpty(_currentSearch))
                {
                    query = query.Where(p => p.Name.Contains(_currentSearch));
                }

                _currentQuery = query;
                _products = query.ToList();
                ListViewProducts.ItemsSource = _products;
                LoadFilters();
                
                RefreshCartState();
            }
        }

        private void LoadFilters()
        {
            using (var context = new ContextDB())
            {
                subcategoryListView.ItemsSource = context.Subcategories.ToList();
                brandsListView.ItemsSource = context.Manufactures.ToList();
            }
        }

        #endregion

        public void RefreshCartState()
        {
            if (_products != null)
            {
                foreach (var product in _products)
                {
                    product.OnCartPropertiesChanged();
                }

                // Принудительно обновляем привязки
                ListViewProducts.Items.Refresh();
            }
        }

        private void LeftScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void RightScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                if (e.Delta > 0)
                {
                    scrollViewer.LineUp();
                }
                else
                {
                    scrollViewer.LineDown();
                }
                e.Handled = true;
            }
        }

        public object GetState()
        {
            return new CatalogState
            {
                Products = _products,
                Search = _currentSearch
            };
        }

        public void RestoreState(object state)
        {
            if (state is CatalogState catalogState)
            {
                _products = catalogState.Products;
                _currentSearch = catalogState.Search;

                if (_products != null)
                {
                    ListViewProducts.ItemsSource = _products;
                    LoadFilters();
                    RefreshCartState(); // Восстанавливаем состояние корзины
                }
            }
        }

        #region Просмотр товара
        private void ListViewProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var products = ListViewProducts.SelectedItem as CatalogProduct;

            if (products == null)
            {
                MessageBox.Show("Товар не выбран");
                return;
            }

            using (var context = new ContextDB())
            {
                var p = context.Products.FirstOrDefault(b => b.Id == products.Id);
                var s = context.Stocks.FirstOrDefault(b => b.ProductId == p.Id);
                var m = context.Manufactures.FirstOrDefault(b => b.Id == p.ManufacturerId);

                // Загружаем скидку
                var now = DateTime.Now;
                var discount = context.Discounts.FirstOrDefault(d => d.ProductId == p.Id &&
                                                                   d.IsActive &&
                                                                   d.StartDate <= now &&
                                                                   d.EndDate >= now);

                var q = new DetailsProduct
                {
                    Id = p.Id,
                    Name = $"{p.Name} {p.ReleaseForm} {p.Dosage}",
                    Photo = p.ImageUrl,
                    Price = p.Price,
                    DiscountPercent = discount?.Percent ?? 0, // Добавляем скидку
                    Manufacture = m.NameManufacture,
                    formCreate = p.ReleaseForm,
                    typePac = p.Packaging,
                    storageСonditions = p.StorageConditions,
                    RequiresPrescription = p.RequiresPrescription,
                    activeSubst = p.ActiveIngredient,
                    dosage = p.Dosage,
                    ExtDate = p.ShelfLifeMonths,
                    ProdQuantity = s.Quantity,
                    country = m.Country,
                };

                var mainWindow = Window.GetWindow(this) as MainWindow;
                ViewModel.NavigationService.NavigateTo(new ViewProductUserControl(q), GetState());
            }
        }
        #endregion

        #region Добавление в корзину

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.IsAdmin) return; // Админы не могут добавлять в корзину

            var button = sender as Button;
            if (button == null) return;

            var item = button.DataContext as CatalogProduct;
            if (item == null)
            {
                MessageBox.Show("Продукт не выбран!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Сохраняем текущую позицию скролла
            var scrollViewer = FindVisualChild<ScrollViewer>(ListViewProducts);
            double scrollPosition = scrollViewer?.VerticalOffset ?? 0;

            // Добавляем товар в корзину
            CartHandler.HandleCartProductSelection(item.Id, 1);

            // Восстанавливаем позицию скролла
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollPosition);
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void QuantityPlus_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.IsAdmin) return; // Админы не могут управлять корзиной

            var button = sender as Button;
            if (button?.Tag is int productId)
            {
                // Сохраняем текущую позицию скролла
                var scrollViewer = FindVisualChild<ScrollViewer>(ListViewProducts);
                double scrollPosition = scrollViewer?.VerticalOffset ?? 0;

                var currentQuantity = CartHandler.GetProductQuantity(productId);

                // Пытаемся добавить товар
                CartHandler.AddToCart(productId, 1);

                // Восстанавливаем позицию скролла
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (scrollViewer != null)
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollPosition);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void QuantityMinus_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.IsAdmin) return; // Админы не могут управлять корзиной

            var button = sender as Button;
            if (button?.Tag is int productId)
            {
                // Сохраняем текущую позицию скролла
                var scrollViewer = FindVisualChild<ScrollViewer>(ListViewProducts);
                double scrollPosition = scrollViewer?.VerticalOffset ?? 0;

                var currentQuantity = CartHandler.GetProductQuantity(productId);

                if (currentQuantity > 1)
                {
                    CartHandler.UpdateQuantity(productId, currentQuantity - 1);
                }
                else
                {
                    CartHandler.RemoveFromCart(productId);
                }

                // Восстанавливаем позицию скролла
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (scrollViewer != null)
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollPosition);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        // Метод для поиска ScrollViewer
        private static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T)
                    return (T)child;

                T childItem = FindVisualChild<T>(child);
                if (childItem != null) return childItem;
            }
            return null;
        }

        private void ListViewProducts_Loaded(object sender, RoutedEventArgs e)
        {
            // При загрузке инициализируем состояние корзины для всех товаров
            RefreshCartState();
        }

        #endregion

        #region Обновление Товара

        private void UpdateDateProduct_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var product = button.DataContext as CatalogProduct;

            if (product == null)
            {
                MessageBox.Show("Товар не выбран", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Открываем панель управления продуктами в режиме редактирования
            var mainWindow = Window.GetWindow(this) as MainWindow;
            ViewModel.NavigationService.NavigateTo(new ProductMasterUserControl(product.Id), GetState());
        }
        #endregion

        #region Удаление товара
        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var item = button.DataContext as CatalogProduct;
            if (item == null)
            {
                MessageBox.Show("Товар не выбран", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Подтверждение действия
            var result = MessageBox.Show($"Вы уверены, что хотите обнулить запасы товара '{item.Name}'?",
                                       "Подтверждение обнуления запасов",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new ContextDB())
                    {
                        // Находим продукт
                        var product = context.Products.FirstOrDefault(p => p.Id == item.Id);
                        if (product != null)
                        {
                            // Обнуляем запасы товара
                            var stock = context.Stocks.FirstOrDefault(s => s.ProductId == product.Id);
                            if (stock != null)
                            {
                                stock.Quantity = 0;
                                //stock.LastRestock = DateTime.Now;
                            }
                            else
                            {
                                // Если записи о запасах нет, создаем новую с нулевым количеством
                                var newStock = new Stock
                                {
                                    ProductId = product.Id,
                                    Quantity = 0,
                                  //  ProductionDate = DateTime.Now,
                                  //  LastRestock = DateTime.Now
                                };
                                context.Stocks.Add(newStock);
                            }

                            // Деактивируем продукт (опционально)
                            // product.IsActive = false;

                            context.SaveChanges();

                            // Обновляем отображение в списке
                            var productToUpdate = _products.FirstOrDefault(p => p.Id == item.Id);
                            if (productToUpdate != null)
                            {
                                productToUpdate.CountOnWarehouse = 0;
                                // Принудительно обновляем привязки
                                ListViewProducts.Items.Refresh();
                            }

                            MessageBox.Show($"Запасы товара '{item.Name}' обнулены!", "Успех",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обнулении запасов: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            MainUserControl mainUserControl = new MainUserControl();
            mainWindow.ViewContentControl.Content = mainUserControl;
        }

        private void categoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var select = subcategoryListView.SelectedItem as Subcategory;
            SubcategoryHandler.HandleSubcategorySelection(select, this);
        }

        private void brandsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var select = brandsListView.SelectedItem as Manufacturer;
            ManufactureHandler.HandleManufactureSelection(select, this);
        }
    }

    // Конвертер для элементов корзины - скрывает для админа, показывает для пользователей
    public class CartVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isAdmin = UserSession.IsAdmin;

          
            if (parameter?.ToString() == "inverse")
            {
                return isAdmin ? Visibility.Visible : Visibility.Collapsed;
            }
         
            return isAdmin ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
   
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}