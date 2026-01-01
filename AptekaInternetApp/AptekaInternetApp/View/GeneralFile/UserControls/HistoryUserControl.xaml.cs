using AptekaInternetApp.Models;
using AptekaInternetApp.Models.ModelProgram;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AptekaInternetApp.View.MainWindowFile.UsersControls
{
    /// <summary>
    /// Логика взаимодействия для HistoryUserControl.xaml
    /// </summary>
    public partial class HistoryUserControl : UserControl
    {
        public ObservableCollection<SaleHistoryItem> Sales { get; set; }

        public HistoryUserControl()
        {
            InitializeComponent();
            Sales = new ObservableCollection<SaleHistoryItem>();

            // Устанавливаем DataContext
            this.DataContext = this;

            LoadSalesHistory();
        }

        private void LoadSalesHistory()
        {
            try
            {
                using (var context = new ContextDB())
                {
                    // Получаем продажи за последний месяц
                    var oneMonthAgo = DateTime.Now.AddMonths(-1);

                    var salesData = (from s in context.Sales
                                     join u in context.Users on s.CashierId equals u.Id
                                     where s.Date >= oneMonthAgo
                                     orderby s.Date descending
                                     select new
                                     {
                                         Sale = s,
                                         Cashier = u
                                     }).ToList();

                    // Очищаем коллекцию перед загрузкой новых данных
                    Sales.Clear();

                    foreach (var data in salesData)
                    {
                        // Получаем товары для этой продажи
                        var saleItems = (from si in context.SalesItems
                                         join p in context.Products on si.ProductId equals p.Id
                                         where si.SaleId == data.Sale.Id
                                         select new SaleItemDetail
                                         {
                                             ProductName = p.Name,
                                             Quantity = si.Quantity,
                                             Price = si.Price,
                                             PriceWithDiscount = si.Price - (si.Price * si.AppliedDiscount / 100),
                                             AppliedDiscount = si.AppliedDiscount,
                                             Total = si.Total
                                         }).ToList();

                        var sale = new SaleHistoryItem
                        {
                            SaleId = data.Sale.Id,
                            SaleDate = data.Sale.Date,
                            ReceiptNumber = data.Sale.ReceiptNumber ?? $"CHK-{data.Sale.Id:D6}",
                            CashierName = data.Cashier.FIO,
                            Customer = data.Sale.Customer ?? "Гость",
                            TotalAmount = data.Sale.Total,
                            DiscountAmount = data.Sale.Discount,
                            FinalAmount = data.Sale.Total - data.Sale.Discount,                           
                            Items = new ObservableCollection<SaleItemDetail>(saleItems)
                        };

                        Sales.Add(sale);
                    }

                    // Обновляем счетчик продаж
                    SalesCountText.Text = $" ({Sales.Count} продаж)";

                    // Показываем/скрываем сообщение об отсутствии продаж
                    if (Sales.Count == 0)
                    {
                        NoSalesMessage.Visibility = Visibility.Visible;
                        SalesItemsControl.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        NoSalesMessage.Visibility = Visibility.Collapsed;
                        SalesItemsControl.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке истории продаж: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                SalesCountText.Text = " (ошибка загрузки)";

                // Показываем сообщение об ошибке
                NoSalesMessage.Visibility = Visibility.Visible;
                SalesItemsControl.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int saleId)
            {
                var sale = Sales.FirstOrDefault(s => s.SaleId == saleId);
                if (sale != null)
                {
                    // Создаем окно с деталями продажи
                    var detailsText = $"Детали продажи #{sale.ReceiptNumber}\n\n" +
                                    $"Дата: {sale.SaleDate:dd.MM.yyyy HH:mm}\n" +
                                    $"Кассир: {sale.CashierName}\n" +
                                    $"Клиент: {sale.Customer}\n" +
                                    // $"Тип оплаты: {sale.PaymentTypeString}\n\n" +
                                    $"Товары:\n";

                    foreach (var item in sale.Items)
                    {
                        detailsText += $"- {item.ProductName}: {item.Quantity} × {item.PriceWithDiscount:N2} руб. = {item.Total:N2} руб.\n";
                    }

                    detailsText += $"\nИтого: {sale.FinalAmount:N2} руб.";

                    MessageBox.Show(detailsText, $"Чек {sale.ReceiptNumber}",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

    }


}
