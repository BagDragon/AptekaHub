using AptekaInternetApp.Models;
using AptekaInternetApp.Models.ModelProgram;
using AptekaInternetApp.Models.Tables;
using AptekaInternetApp.Models.TablesDB;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AptekaInternetApp.View.MainWindowFile.UsersControls
{
    public partial class ProductMasterUserControl : UserControl
    {
        private ObservableCollection<Category> _categories;
        private ObservableCollection<Subcategory> _subcategories;
        private ObservableCollection<Manufacturer> _manufacturers;
        private ObservableCollection<Product> _products;

        private Product _currentProduct;
        private bool _isEditMode = false;
        private string _currentImagePath;
        private byte[] _imageData;

        public ProductMasterUserControl()
        {
            InitializeComponent();
            LoadData();
            ClearForm();
        }

        public ProductMasterUserControl(int productId)
        {
            InitializeComponent();
            LoadData();
            LoadProductForEditing(productId);
        }

        private void LoadData()
        {
            try
            {
                using (var context = new ContextDB())
                {
                    _categories = new ObservableCollection<Category>(
                        context.Categories.OrderBy(c => c.Name).ToList());
                    CategoryComboBox.ItemsSource = _categories;

                    _manufacturers = new ObservableCollection<Manufacturer>(
                        context.Manufactures.OrderBy(m => m.NameManufacture).ToList());
                    ManufacturerComboBox.ItemsSource = _manufacturers;

                    _products = new ObservableCollection<Product>(
                        context.Products
                            .Include(p => p.Category)
                            .Include(p => p.Subcategory)
                            .Include(p => p.Manufacturer)
                            .Where(p => p.IsActive)
                            .OrderBy(p => p.Name)
                            .ToList());

                    QuickProductComboBox.ItemsSource = _products;
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxImage.Error);
            }
        }

        private void LoadProductForEditing(int productId)
        {
            try
            {
                using (var context = new ContextDB())
                {
                    var product = context.Products
                        .Include(p => p.Category)
                        .Include(p => p.Subcategory)
                        .Include(p => p.Manufacturer)
                        .Include(p => p.Stocks)
                        .FirstOrDefault(p => p.Id == productId);

                    if (product != null)
                    {
                        LoadProductForEditing(product);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка загрузки продукта: {ex.Message}", "Ошибка", MessageBoxImage.Error);
            }
        }

        private void LoadProductForEditing(Product product)
        {
            if (product == null) return;

            _currentProduct = product;
            _isEditMode = true;
            // Основные данные
            NameTextBox.Text = product.Name;
            PriceTextBox.Text = product.Price.ToString("F2");
            ReleaseFormTextBox.Text = product.ReleaseForm ?? "";
            DosageTextBox.Text = product.Dosage ?? "";
            PackagingTextBox.Text = product.Packaging ?? "";
            PackageContentsTextBox.Text = product.PackageContentsQuantity ?? "";
            ActiveIngredientTextBox.Text = product.ActiveIngredient ?? ""; 
            StorageConditionsTextBox.Text = product.StorageConditions ?? "";
            ShelfLifeTextBox.Text = product.ShelfLifeMonths.ToString();
            RequiresPrescriptionCheckBox.IsChecked = product.RequiresPrescription;
            IsActiveCheckBox.IsChecked = product.IsActive;

            // Изображение
            LoadProductImage(product.ImageUrl);

            // Категория и подкатегория
            if (product.Category != null)
            {
                CategoryComboBox.SelectedItem = _categories?.FirstOrDefault(c => c.ID_Category == product.CategoryId);
                if (product.CategoryId > 0)
                {
                    LoadSubcategoriesForCategory(product.CategoryId);
                    if (product.Subcategory != null)
                    {
                        SubcategoryComboBox.SelectedItem = _subcategories?.FirstOrDefault(s => s.ID_Subcategory == product.SubcategoryId);
                    }
                }
            }

            // Производитель
            if (product.Manufacturer != null)
            {
                ManufacturerComboBox.SelectedItem = _manufacturers?.FirstOrDefault(m => m.Id == product.ManufacturerId);
            }

            // Запасы
            LoadStockInfo(product.Id);

            StatusText.Text = $" - Редактирование: {product.Name}";
        }

        private void LoadProductImage(string imageUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    if (File.Exists(imageUrl))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imageUrl, UriKind.RelativeOrAbsolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        ProductImage.Source = bitmap;
                        _currentImagePath = imageUrl;
                        ImageInfoText.Text = Path.GetFileName(imageUrl);
                    }
                    else if (imageUrl.StartsWith("/Resources/Images"))
                    {
                        ProductImage.Source = new BitmapImage(new Uri(imageUrl, UriKind.Relative));
                        _currentImagePath = imageUrl;
                        ImageInfoText.Text = "Ресурс приложения";
                    }
                    else
                    {
                        SetDefaultImage();
                    }
                }
                else
                {
                    SetDefaultImage();
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка загрузки изображения: {ex.Message}", "Предупреждение", MessageBoxImage.Warning);
                SetDefaultImage();
            }
        }

        private void SetDefaultImage()
        {
            ProductImage.Source = null;
            _currentImagePath = null;
            _imageData = null;
            ImageInfoText.Text = "Изображение не выбрано";
        }

        private void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Image files (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
                    Title = "Выберите изображение продукта"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    _currentImagePath = openFileDialog.FileName;

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_currentImagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    ProductImage.Source = bitmap;

                    _imageData = File.ReadAllBytes(_currentImagePath);
                    ImageInfoText.Text = Path.GetFileName(_currentImagePath);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка", MessageBoxImage.Error);
            }
        }

        private void RemoveImageButton_Click(object sender, RoutedEventArgs e)
        {
            SetDefaultImage();
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is Category selectedCategory)
            {
                LoadSubcategoriesForCategory(selectedCategory.ID_Category);
            }
            else
            {
                SubcategoryComboBox.ItemsSource = null;
                SubcategoryComboBox.SelectedIndex = -1;
            }
        }

        private void LoadSubcategoriesForCategory(int categoryId)
        {
            try
            {
                using (var context = new ContextDB())
                {
                    _subcategories = new ObservableCollection<Subcategory>(
                        context.Subcategories
                            .Where(s => s.CategoryId == categoryId)
                            .OrderBy(s => s.Name)
                            .ToList());

                    SubcategoryComboBox.ItemsSource = _subcategories;
                    SubcategoryComboBox.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка загрузки подкатегорий: {ex.Message}", "Ошибка", MessageBoxImage.Error);
            }
        }

        private void QuickProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (QuickProductComboBox.SelectedItem is Product selectedProduct)
            {
                LoadProductForEditing(selectedProduct);
            }
        }

        private void LoadStockInfo(int productId)
        {
            try
            {
                using (var context = new ContextDB())
                {
                    var stock = context.Stocks.FirstOrDefault(s => s.ProductId == productId);
                    if (stock != null)
                    {
                        CurrentStockText.Text = $"{stock.Quantity} шт.";
                        StockQuantityTextBox.Text = stock.Quantity.ToString();
                        MinStockTextBox.Text = stock.MinStockLevel.ToString();
                        MaxStockTextBox.Text = stock.MaxStockLevel.ToString();

                        // Определяем статус запасов
                        string status = "Норма";
                        string statusColor = "#27AE60";

                        if (stock.Quantity <= stock.MinStockLevel)
                        {
                            status = "Низкий запас!";
                            statusColor = "#E74C3C";
                        }
                        else if (stock.Quantity >= stock.MaxStockLevel * 0.9)
                        {
                            status = "Высокий запас";
                            statusColor = "#F39C12";
                        }

                        StockStatusText.Text = $"Статус: {status}";
                    }
                    else
                    {
                        CurrentStockText.Text = "0 шт.";
                        StockQuantityTextBox.Text = "0";
                        MinStockTextBox.Text = "5";
                        MaxStockTextBox.Text = "500";
                        StockStatusText.Text = "Статус: Нет данных";
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка загрузки запасов: {ex.Message}", "Ошибка", MessageBoxImage.Error);
            }
        }

        private void SaveProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                using (var context = new ContextDB())
                {
                    if (_isEditMode && _currentProduct != null)
                    {
                        // Редактирование
                        UpdateProduct();
                    }
                    else
                    {
                        // Добавление нового
                        AddProduct();
                    }

                    LoadData();
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxImage.Error);
            }
        }

        public void UpdateProduct()
        {
            using (var context = new ContextDB())
            {
                var product = context.Products.FirstOrDefault(p => p.Id == _currentProduct.Id);
                if (product != null)
                {
                    UpdateProductData(product);
                    context.SaveChanges();
                    ShowMessage("Продукт успешно обновлен!", "Успех", MessageBoxImage.Information);
                }
            }
        }


        public void AddProduct()
        {
            using (var context = new ContextDB())
            {
                var product = new Product();
                UpdateProductData(product);
                context.Products.Add(product);
                context.SaveChanges();

                // Создание записи о запасах
                var stock = new Stock
                {
                    ProductId = product.Id,
                    Quantity = 0,
                    MinStockLevel = int.Parse(MinStockTextBox.Text),
                    MaxStockLevel = int.Parse(MaxStockTextBox.Text),
                    LastRestock = DateTime.Now
                };
                context.Stocks.Add(stock);
                context.SaveChanges();

                ShowMessage("Продукт успешно добавлен!", "Успех", MessageBoxImage.Information);
                ClearForm();
            }
        }

        private void UpdateProductData(Product product)
        {
            product.Name = NameTextBox.Text.Trim();
            product.Price = decimal.Parse(PriceTextBox.Text);
            product.ReleaseForm = ReleaseFormTextBox.Text.Trim();
            product.Dosage = DosageTextBox.Text.Trim();
            product.Packaging = PackagingTextBox.Text.Trim();
            product.PackageContentsQuantity = PackageContentsTextBox.Text.Trim();
            product.ActiveIngredient = ActiveIngredientTextBox.Text.Trim();
            product.StorageConditions = StorageConditionsTextBox.Text.Trim();
            product.ShelfLifeMonths = int.Parse(ShelfLifeTextBox.Text);
            product.RequiresPrescription = RequiresPrescriptionCheckBox.IsChecked ?? false;
            product.IsActive = IsActiveCheckBox.IsChecked ?? true;

            // Обработка изображения
            if (!string.IsNullOrEmpty(_currentImagePath) && _imageData != null)
            {
                string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Images", "ProductImage");
                if (!Directory.Exists(resourcesPath))
                    Directory.CreateDirectory(resourcesPath);

                string fileName = $"{Path.GetFileName(_currentImagePath)}";
                string fullPath = Path.Combine(resourcesPath, fileName);

                File.WriteAllBytes(fullPath, _imageData);
                product.ImageUrl = $"/Resources/Images/ProductImage/{fileName}";
            }
            else if (string.IsNullOrEmpty(_currentImagePath))
            {
                product.ImageUrl = null;
            }

            if (CategoryComboBox.SelectedItem is Category selectedCategory)
                product.CategoryId = selectedCategory.ID_Category;

            if (SubcategoryComboBox.SelectedItem is Subcategory selectedSubcategory)
                product.SubcategoryId = selectedSubcategory.ID_Subcategory;

            if (ManufacturerComboBox.SelectedItem is Manufacturer selectedManufacturer)
                product.ManufacturerId = selectedManufacturer.Id;
        }

        private void UpdateStockButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProduct == null)
            {
                ShowMessage("Сначала выберите продукт для редактирования запасов", "Внимание", MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(StockQuantityTextBox.Text, out int quantity) || quantity < 0)
            {
                ShowMessage("Введите корректное количество", "Ошибка", MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(MinStockTextBox.Text, out int minStock) || minStock < 0)
            {
                ShowMessage("Введите корректное минимальное количество", "Ошибка", MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(MaxStockTextBox.Text, out int maxStock) || maxStock <= minStock)
            {
                ShowMessage("Максимальное количество должно быть больше минимального", "Ошибка", MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new ContextDB())
                {
                    var stock = context.Stocks.FirstOrDefault(s => s.ProductId == _currentProduct.Id);
                    if (stock == null)
                    {
                        stock = new Stock
                        {
                            ProductId = _currentProduct.Id,
                            Quantity = quantity,
                            MinStockLevel = minStock,
                            MaxStockLevel = maxStock,
                            LastRestock = DateTime.Now
                        };
                        context.Stocks.Add(stock);
                    }
                    else
                    {
                        stock.Quantity = quantity; // Устанавливаем точное количество
                        stock.MinStockLevel = minStock;
                        stock.MaxStockLevel = maxStock;
                        stock.LastRestock = DateTime.Now;
                    }

                    context.SaveChanges();

                    // Обновляем отображение
                    CurrentStockText.Text = $"{stock.Quantity} шт.";

                    ShowMessage("Запасы успешно обновлены!", "Успех", MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка обновления запасов: {ex.Message}", "Ошибка", MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _currentProduct = null;
            _isEditMode = false;

            NameTextBox.Clear();
            PriceTextBox.Clear();
            ReleaseFormTextBox.Clear();
            DosageTextBox.Clear();
            PackagingTextBox.Clear();
            PackageContentsTextBox.Clear();
            ActiveIngredientTextBox.Clear();
            StorageConditionsTextBox.Clear();
            ShelfLifeTextBox.Clear();
            RequiresPrescriptionCheckBox.IsChecked = false;
            IsActiveCheckBox.IsChecked = true;

            SetDefaultImage();

            CategoryComboBox.SelectedIndex = -1;
            SubcategoryComboBox.ItemsSource = null;
            SubcategoryComboBox.SelectedIndex = -1;
            ManufacturerComboBox.SelectedIndex = -1;
            QuickProductComboBox.SelectedIndex = -1;

            StockQuantityTextBox.Text = "0";
            MinStockTextBox.Text = "5";
            MaxStockTextBox.Text = "500";
            CurrentStockText.Text = "Не выбрано";
            StockStatusText.Text = "Статус: Неизвестно";

            StatusText.Text = " - Добавление нового продукта";
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ShowMessage("Введите название продукта", "Ошибка", MessageBoxImage.Error);
                return false;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price < 0)
            {
                ShowMessage("Введите корректную цену", "Ошибка", MessageBoxImage.Error);
                return false;
            }

            if (CategoryComboBox.SelectedItem == null)
            {
                ShowMessage("Выберите категорию", "Ошибка", MessageBoxImage.Error);
                return false;
            }

            if (SubcategoryComboBox.SelectedItem == null)
            {
                ShowMessage("Выберите подкатегорию", "Ошибка", MessageBoxImage.Error);
                return false;
            }

            if (ManufacturerComboBox.SelectedItem == null)
            {
                ShowMessage("Выберите производителя", "Ошибка", MessageBoxImage.Error);
                return false;
            }

            if (!int.TryParse(ShelfLifeTextBox.Text, out int shelfLife) || shelfLife <= 0)
            {
                ShowMessage("Введите корректный срок годности", "Ошибка", MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void ShowMessage(string message, string title, MessageBoxImage icon)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }
    }
}