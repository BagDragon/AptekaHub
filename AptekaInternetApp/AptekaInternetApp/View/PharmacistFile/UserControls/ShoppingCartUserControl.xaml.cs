using AptekaInternetApp.Models;
using AptekaInternetApp.Models.ModelProgram;
using AptekaInternetApp.Models.TablesDB;
using AptekaInternetApp.View.GeneralFile.UserControls;
using AptekaInternetApp.View.MainWindowFile.UsersControls;
using AptekaInternetApp.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Path = System.IO.Path;

namespace AptekaInternetApp.View.PharmacistFile.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ShoppingCartUserControl.xaml
    /// </summary>
    public partial class ShoppingCartUserControl : UserControl
    {
        private List<CartProduct> _cartProducts;

        public ShoppingCartUserControl()
        {
            InitializeComponent();
            LoadCartData();
        }

        private void LoadCartData()
        {
            _cartProducts = CartHandler.LoadCart();
            ListViewProducts.ItemsSource = _cartProducts;

            UpdateCartSummary();
            UpdateItemsCount();
        }

        private void UpdateCartSummary()
        {
            decimal subtotal = CartHandler.GetTotalAmount();
            decimal totalWithDiscount = CartHandler.GetTotalAmountWithDiscount();
            decimal totalDiscount = CartHandler.GetTotalDiscount();

            SubtotalTextBlock.Text = subtotal.ToString("N2") + " ₽";
            DiscountTextBlock.Text = totalDiscount.ToString("N2") + " ₽";
            TotalTextBlock.Text = totalWithDiscount.ToString("N2") + " ₽";
        }

        private decimal CalculateDiscount()
        {
            decimal subtotal = CartHandler.GetTotalAmount();
            return subtotal > 1000 ? subtotal * 0.05m : 0;
        }

        private void UpdateItemsCount()
        {
            int totalItems = CartHandler.GetTotalItems();
            ItemsCountTextBlock.Text = $"Состав: {totalItems} товар(ов)";
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartProduct product)
            {
                if (product.Quantity >= 7)
                {
                    MessageBox.Show("Вы превысили лимит покупки товара (7 шт.)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CartHandler.AddToCart(product.Id, 1);
                RefreshCart();
            }
        }

        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartProduct product)
            {
                if (product.Quantity > 1)
                {
                    CartHandler.UpdateQuantity(product.Id, product.Quantity - 1);
                }
                else
                {
                    CartHandler.RemoveFromCart(product.Id);
                }
                RefreshCart();
            }
        }

        private void RefreshCart()
        {
            _cartProducts = CartHandler.LoadCart();
            ListViewProducts.ItemsSource = null;
            ListViewProducts.ItemsSource = _cartProducts;

            UpdateCartSummary();
            UpdateItemsCount();
        }

        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_cartProducts.Any())
            {
                MessageBox.Show("Корзина пуста", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверяем рецептурные товары
            var prescriptionItems = _cartProducts.Where(p => p.RequiresPrescription).ToList();
            if (prescriptionItems.Any())
            {
                // Для каждого рецептурного товара проверяем наличие действующего рецепта
                foreach (var prescriptionItem in prescriptionItems)
                {
                    if (!HasValidPrescription(prescriptionItem.Id))
                    {
                        // Запрашиваем рецепт
                        var result = RequestPrescription(prescriptionItem);
                        if (!result)
                        {
                            MessageBox.Show($"Не удалось оформить заказ. Требуется рецепт для товара: {prescriptionItem.NameProduct}",
                                          "Рецепт требуется", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                }
            }

            // Продолжаем оформление продажи
            ProcessOrder();
        }

        private bool HasValidPrescription(int productId)
        {
            using (var context = new ContextDB())
            {
                var now = DateTime.Now;
                return context.Prescriptions.Any(p =>
                    p.ProductId == productId &&
                    p.IsUsed == false &&
                    p.ExpiryDate >= now &&
                    p.IssueDate <= now);
            }
        }

        private bool RequestPrescription(CartProduct product)
        {
            var prescriptionWindow = new PrescriptionInputWindow(
                product.Id,
                product.NameProduct,
                $"Рецептурный препарат - {product.NameProduct}");

            if (prescriptionWindow.ShowDialog() == true)
            {
                return SavePrescription(product.Id, prescriptionWindow.PrescriptionData);
            }

            return false;
        }

        private bool SavePrescription(int productId, PrescriptionData data)
        {
            try
            {
                using (var context = new ContextDB())
                {
                    var prescription = new Prescription
                    {
                        ProductId = productId,
                        PatientName = data.PatientName,
                        Number = data.PrescriptionNumber,
                        Doctor = data.DoctorName,
                        IssueDate = data.IssueDate,
                        ExpiryDate = data.ExpiryDate,
                        IsUsed = false,
                        UsedDate = null
                    };

                    context.Prescriptions.Add(prescription);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении рецепта: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void ProcessOrder()
        {
            try
            {
                using (var context = new ContextDB())
                {
                    decimal subtotal = _cartProducts.Sum(p => p.Price * p.Quantity);
                    decimal totalDiscount = _cartProducts.Sum(p => (p.Price - p.PriceWithDiscount) * p.Quantity);
                    decimal totalWithDiscount = subtotal - totalDiscount;

                    // Создаем продажу
                    var sale = new Sale
                    {
                        Date = DateTime.Now,
                        CashierId = UserSession.Id,
                        Total = totalWithDiscount,
                        Discount = totalDiscount,
                        //PaymentType = PaymentType.Cash,
                        ReceiptNumber = GenerateReceiptNumber(),
                        Customer = "Клиент аптеки"
                    };

                    context.Sales.Add(sale);
                    context.SaveChanges();

                    // Добавляем товары продажи
                    foreach (var cartItem in _cartProducts)
                    {
                        var itemDiscount = (cartItem.Price - cartItem.PriceWithDiscount) * cartItem.Quantity;
                        var itemDiscountPercent = cartItem.HasDiscount ? cartItem.DiscountPercent : 0;

                        var saleItem = new SaleItem
                        {
                            SaleId = sale.Id,
                            ProductId = cartItem.Id,
                            Quantity = cartItem.Quantity,
                            Price = cartItem.Price,
                            AppliedDiscount = itemDiscountPercent,
                            Total = cartItem.TotalWithDiscount
                        };

                        context.SalesItems.Add(saleItem);

                        // Обновляем остатки на складе
                        var stock = context.Stocks.FirstOrDefault(s => s.ProductId == cartItem.Id);
                        if (stock != null)
                        {
                            stock.Quantity -= cartItem.Quantity;
                            if (stock.Quantity < 0) stock.Quantity = 0;
                        }
                    }

                    // Отмечаем рецепты как использованные
                    var prescriptionItems = _cartProducts.Where(p => p.RequiresPrescription).ToList();
                    foreach (var prescriptionItem in prescriptionItems)
                    {
                        var prescription = context.Prescriptions
                            .FirstOrDefault(p => p.ProductId == prescriptionItem.Id &&
                                               p.IsUsed == false &&
                                               p.ExpiryDate >= DateTime.Now);

                        if (prescription != null)
                        {
                            prescription.IsUsed = true;
                            prescription.UsedDate = DateTime.Now;
                        }
                    }

                    context.SaveChanges();

                    // Очищаем корзину
                    CartHandler.ClearCart();

                    var receiptWindow = new ReceiptDisplayWindow(sale, _cartProducts);
                    receiptWindow.ShowDialog();
                    
                    MessageBox.Show("Покупка оформлена успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Возвращаемся в главное меню
                    ContinueShopping_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateReceiptNumber()
        {
            return $"R{DateTime.Now:yyyyMMddHHmmss}";
        }

       

        private void ContinueShopping_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ViewContentControl.Content = new CatalogUserControl();
            }
        }
    }
}