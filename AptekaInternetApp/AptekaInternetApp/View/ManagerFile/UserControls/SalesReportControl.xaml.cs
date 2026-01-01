using AptekaInternetApp.Models;
using AptekaInternetApp.Models.ModelProgram;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace AptekaInternetApp.View.ManagerFile.UserControls
{
    public partial class SalesReportControl : UserControl
    {
        private ContextDB context = new ContextDB();

        public SalesReportControl()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            StartDatePicker.SelectedDate = DateTime.Today.AddDays(-7);
            EndDatePicker.SelectedDate = DateTime.Today;
           
        }

        private void PeriodTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPeriod = ((ComboBoxItem)PeriodTypeComboBox.SelectedItem)?.Content.ToString();

            switch (selectedPeriod)
            {
                case "Сегодня":
                    StartDatePicker.SelectedDate = DateTime.Today;
                    EndDatePicker.SelectedDate = DateTime.Today;
                    break;
                case "Вчера":
                    StartDatePicker.SelectedDate = DateTime.Today.AddDays(-1);
                    EndDatePicker.SelectedDate = DateTime.Today.AddDays(-1);
                    break;
                case "Текущая неделя":
                    var today = DateTime.Today;
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);
                    StartDatePicker.SelectedDate = startOfWeek;
                    EndDatePicker.SelectedDate = today;
                    break;
                case "Текущий месяц":
                    StartDatePicker.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    EndDatePicker.SelectedDate = DateTime.Today;
                    break;
            }
        }

        private void GenerateReportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var startDate = StartDatePicker.SelectedDate ?? DateTime.Today.AddDays(-7);
                var endDate = EndDatePicker.SelectedDate ?? DateTime.Today;
                var reportType = ((ComboBoxItem)ReportTypeComboBox.SelectedItem)?.Content.ToString();

                if (startDate > endDate)
                {
                    MessageBox.Show("Дата начала не может быть больше даты окончания", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                
                DateTime startDateWithTime = startDate.Date;
                DateTime endDateWithTime = endDate.Date.AddDays(1).AddTicks(-1);

                DataTable dataTable = null;

                switch (reportType)
                {
                    case "💰 Ежедневная выручка и ключевые показатели":
                        dataTable = GenerateDailyRevenueReport(startDateWithTime, endDateWithTime);
                        ReportTitle.Text = $"Ежедневная выручка за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
                        break;
                    case "🏆 Топ продаж по количеству":
                        dataTable = GenerateTopProductsReport(startDateWithTime, endDateWithTime);
                        ReportTitle.Text = $"Топ продаж по количеству за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
                        break;
                    case "📦 Анализ продаж по категориям":
                        dataTable = GenerateCategoryAnalysisReport(startDateWithTime, endDateWithTime);
                        ReportTitle.Text = $"Анализ продаж по категориям за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
                        break;
                    default:
                        MessageBox.Show("Выберите тип отчета", "Информация",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                }

                ReportDataGrid.ItemsSource = dataTable?.DefaultView;
                

                MessageBox.Show($"Отчет успешно сформирован!\nПериод: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}",
                              "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DataTable GenerateDailyRevenueReport(DateTime startDate, DateTime endDate)
        {
            
            var salesData = context.Sales
                .Where(s => s.Date >= startDate && s.Date <= endDate)
                .Select(s => new
                {
                    Date = s.Date,
                    Total = s.Total,
                    Items = s.Items.Sum(i => i.Quantity)
                })
                .ToList(); // Выполняем запрос

            // Теперь группируем в памяти
            var salesByDay = salesData
                .GroupBy(s => s.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(s => s.Total),
                    Operations = g.Count(),
                    Products = g.Sum(s => s.Items)
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Создаем таблицу
            var table = new DataTable();
            table.Columns.Add("Дата", typeof(string));
            table.Columns.Add("Выручка", typeof(decimal));
            table.Columns.Add("Операции", typeof(int));
            table.Columns.Add("Средний", typeof(decimal));
            table.Columns.Add("Товары", typeof(int));
            table.Columns.Add("День", typeof(string));

            foreach (var day in salesByDay)
            {
                var average = day.Operations > 0 ? day.Revenue / day.Operations : 0;
                table.Rows.Add(
                    day.Date.ToString("dd.MM.yyyy"),
                    day.Revenue,
                    day.Operations,
                    Math.Round(average, 2),
                    day.Products,
                    GetRussianDayOfWeek(day.Date.DayOfWeek)
                );
            }

            return table;
        }

        private DataTable GenerateTopProductsReport(DateTime startDate, DateTime endDate)
        {
            // LINQ запрос в SQL-подобном стиле
            var topProducts = (from si in context.SalesItems
                               join s in context.Sales on si.SaleId equals s.Id
                               join p in context.Products on si.ProductId equals p.Id
                               where s.Date >= startDate && s.Date <= endDate
                               group si by new { p.Id, p.Name } into g
                               select new
                               {
                                   Product = g.Key.Name,
                                   Quantity = g.Sum(si => si.Quantity),
                                   Revenue = g.Sum(si => si.Total),
                                   AveragePrice = g.Sum(si => si.Total) / g.Sum(si => si.Quantity)
                               } into result
                               where result.Quantity > 0
                               orderby result.Quantity descending
                               select result).Take(20).ToList();

            var table = new DataTable();
            table.Columns.Add("№", typeof(int));
            table.Columns.Add("Товар", typeof(string));
            table.Columns.Add("Количество", typeof(int));
            table.Columns.Add("Выручка", typeof(decimal));
            table.Columns.Add("Средняя цена", typeof(decimal));
            table.Columns.Add("Доля в количестве", typeof(string));

            // Считаем общее количество для расчета долей
            var totalQuantity = topProducts.Sum(x => x.Quantity);

            for (int i = 0; i < topProducts.Count; i++)
            {
                var product = topProducts[i];
                var share = totalQuantity > 0 ? (product.Quantity * 100.0 / totalQuantity) : 0;

                table.Rows.Add(
                    i + 1,
                    product.Product,
                    product.Quantity,
                    Math.Round(product.Revenue, 2),
                    Math.Round(product.AveragePrice, 2),
                    $"{Math.Round(share, 1)}%"
                );
            }

            return table;
        }

        private DataTable GenerateCategoryAnalysisReport(DateTime startDate, DateTime endDate)
        {
            // LINQ запрос в SQL-подобном стиле
            var categories = (from si in context.SalesItems
                              join s in context.Sales on si.SaleId equals s.Id
                              join p in context.Products on si.ProductId equals p.Id
                              join c in context.Categories on p.CategoryId equals c.ID_Category
                              where s.Date >= startDate && s.Date <= endDate
                              group si by new { c.ID_Category, c.Name } into g
                              select new
                              {
                                  Category = g.Key.Name,
                                  Revenue = g.Sum(si => si.Total),
                                  Products = g.Sum(si => si.Quantity),
                                  UniqueProducts = (from si2 in g select si2.ProductId).Distinct().Count(),
                                  AverageCheck = g.Average(si => si.Total)
                              } into result
                              orderby result.Revenue descending
                              select result).ToList();

            var totalRevenue = categories.Sum(x => x.Revenue);
            var totalProducts = categories.Sum(x => x.Products);

            var table = new DataTable();
            table.Columns.Add("Категория", typeof(string));
            table.Columns.Add("Выручка", typeof(decimal));
            table.Columns.Add("Доля в выручке %", typeof(decimal));
            table.Columns.Add("Количество товаров", typeof(int));
            table.Columns.Add("Доля в количестве %", typeof(decimal));
            table.Columns.Add("Уникальных товаров", typeof(int));
            table.Columns.Add("Средний чек", typeof(decimal));

            foreach (var category in categories)
            {
                var revenueShare = totalRevenue > 0 ? (category.Revenue / totalRevenue * 100) : 0;
                var quantityShare = totalProducts > 0 ? (category.Products * 100.0 / totalProducts) : 0;

                table.Rows.Add(
                    category.Category,
                    Math.Round(category.Revenue, 2),
                    Math.Round(revenueShare, 1),
                    category.Products,
                    Math.Round(quantityShare, 1),
                    category.UniqueProducts,
                    Math.Round(category.AverageCheck, 2)
                );
            }

            return table;
        }

        private string GetRussianDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday: return "Пн";
                case DayOfWeek.Tuesday: return "Вт";
                case DayOfWeek.Wednesday: return "Ср";
                case DayOfWeek.Thursday: return "Чт";
                case DayOfWeek.Friday: return "Пт";
                case DayOfWeek.Saturday: return "Сб";
                case DayOfWeek.Sunday: return "Вс";
                default: return "";
            }
        }

        private void ExportExcelBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataView = ReportDataGrid.ItemsSource as DataView;
                if (dataView == null || dataView.Table.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта", "Информация",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var excelApp = new Excel.Application();
                excelApp.Visible = true;
                var workbook = excelApp.Workbooks.Add();
                var worksheet = workbook.ActiveSheet;

                // Заголовок
                worksheet.Cells[1, 1] = ReportTitle.Text;
                worksheet.Cells[1, 1].Font.Bold = true;

                // Период
                worksheet.Cells[2, 1] = $"Период: {StartDatePicker.SelectedDate:dd.MM.yyyy} - {EndDatePicker.SelectedDate:dd.MM.yyyy}";
                worksheet.Cells[3, 1] = $"Сформировано: {DateTime.Now}";

                // Заголовки таблицы
                for (int i = 0; i < dataView.Table.Columns.Count; i++)
                {
                    worksheet.Cells[5, i + 1] = dataView.Table.Columns[i].ColumnName;
                    worksheet.Cells[5, i + 1].Font.Bold = true;
                }

                // Данные
                for (int i = 0; i < dataView.Table.Rows.Count; i++)
                {
                    for (int j = 0; j < dataView.Table.Columns.Count; j++)
                    {
                        worksheet.Cells[i + 6, j + 1] = dataView.Table.Rows[i][j];
                    }
                }

                // Авто-ширина
                worksheet.Columns.AutoFit();

                MessageBox.Show("Отчет экспортирован в Excel", "Готово",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}