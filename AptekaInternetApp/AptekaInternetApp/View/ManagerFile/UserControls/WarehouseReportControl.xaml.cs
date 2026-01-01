using AptekaInternetApp.Models;
using AptekaInternetApp.Models.ModelProgram;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace AptekaInternetApp.View.ManagerFile.UserControls
{
    public partial class WarehouseReportControl : UserControl
    {
        private ContextDB context = new ContextDB();

        public WarehouseReportControl()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            
            UpdateStatistics(null);
        }

        private void GenerateReportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var reportType = ((ComboBoxItem)ReportTypeComboBox.SelectedItem)?.Content.ToString();
                DataTable dataTable = null;

                switch (reportType)
                {
                    case "🔴 Товары с низким запасом":
                        dataTable = GenerateLowStockReport();
                        ReportTitle.Text = "Товары с низким и критическим запасом";
                        break;
                    case "🔵 Товары с избыточным запасом":
                        dataTable = GenerateExcessStockReport();
                        ReportTitle.Text = "Товары с избыточным запасом";
                        break;
                    case "📊 Полный отчет по складу":
                        dataTable = GenerateFullStockReport();
                        ReportTitle.Text = "Полный отчет по складу";
                        break;
                    default:
                        MessageBox.Show("Выберите тип отчета", "Информация",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                }

                ReportDataGrid.ItemsSource = dataTable?.DefaultView;
                UpdateStatistics(dataTable?.DefaultView);

                MessageBox.Show($"Отчет '{reportType}' успешно сформирован!", "Готово",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DataTable GenerateLowStockReport()
        {
            var stocks = context.Stocks
                .Include(s => s.Product)
                .Include(s => s.Product.Manufacturer)
                .Include(s => s.Product.Category)
                .ToList();

            var lowStock = stocks
                .Where(s => (s.Quantity <= s.MinStockLevel || s.Quantity < 20) && s.Quantity > 0)
                .Select(s => new
                {
                    Товар = s.Product.Name,
                    Количество = s.Quantity,
                    МинУровень = s.MinStockLevel,
                    Производитель = s.Product.Manufacturer.NameManufacture,
                    Категория = s.Product.Category.Name,
                    Статус = s.Quantity <= s.MinStockLevel ? "КРИТИЧЕСКИ НИЗКИЙ" : "НИЗКИЙ"
                })
                .OrderBy(s => s.Количество)
                .ToList();

            var dataTable = new DataTable();
            dataTable.Columns.Add("Товар", typeof(string));
            dataTable.Columns.Add("Количество", typeof(int));
            dataTable.Columns.Add("МинУровень", typeof(int));
            dataTable.Columns.Add("Производитель", typeof(string));
            dataTable.Columns.Add("Категория", typeof(string));
            dataTable.Columns.Add("Статус", typeof(string));

            foreach (var item in lowStock)
            {
                dataTable.Rows.Add(item.Товар, item.Количество, item.МинУровень,
                    item.Производитель, item.Категория,
                    item.Статус);
            }

            return dataTable;
        }

        private DataTable GenerateExcessStockReport()
        {
            var stocks = context.Stocks
                .Include(s => s.Product)
                .Include(s => s.Product.Manufacturer)
                .Include(s => s.Product.Category)
                .ToList();

            var excessStock = stocks
                .Where(s => s.Quantity >= s.MaxStockLevel)
                .Select(s => new
                {
                    Товар = s.Product.Name,
                    Количество = s.Quantity,
                    МаксУровень = s.MaxStockLevel,
                    Производитель = s.Product.Manufacturer.NameManufacture,
                    Категория = s.Product.Category.Name,
                    Статус = "Избыток" // Добавляем столбец статуса
                })
                .OrderByDescending(s => s.Количество)
                .ToList();

            var dataTable = new DataTable();
            dataTable.Columns.Add("Товар", typeof(string));
            dataTable.Columns.Add("Количество", typeof(int));
            dataTable.Columns.Add("МаксУровень", typeof(int));
            dataTable.Columns.Add("Производитель", typeof(string));
            dataTable.Columns.Add("Категория", typeof(string));
            dataTable.Columns.Add("Статус", typeof(string)); // Добавляем столбец статуса

            foreach (var item in excessStock)
            {
                dataTable.Rows.Add(item.Товар, item.Количество, item.МаксУровень,
                    item.Производитель, item.Категория, item.Статус);
            }

            return dataTable;
        }

        private DataTable GenerateFullStockReport()
        {
            var stocks = context.Stocks
                .Include(s => s.Product)
                .Include(s => s.Product.Manufacturer)
                .Include(s => s.Product.Category)
                .ToList();

            var fullStock = stocks
                .Select(s => new
                {
                    Товар = s.Product.Name,
                    Количество = s.Quantity,
                    МинУровень = s.MinStockLevel,
                    МаксУровень = s.MaxStockLevel,
                    Производитель = s.Product.Manufacturer.NameManufacture,
                    Категория = s.Product.Category.Name,
                    Статус = GetStockStatus(s.Quantity, s.MinStockLevel, s.MaxStockLevel)
                })
                .OrderBy(s => s.Категория)
                .ThenBy(s => s.Товар)
                .ToList();

            var dataTable = new DataTable();
            dataTable.Columns.Add("Товар", typeof(string));
            dataTable.Columns.Add("Количество", typeof(int));
            dataTable.Columns.Add("МинУровень", typeof(int));
            dataTable.Columns.Add("МаксУровень", typeof(int));
            dataTable.Columns.Add("Производитель", typeof(string));
            dataTable.Columns.Add("Категория", typeof(string));
            dataTable.Columns.Add("Статус", typeof(string));

            foreach (var item in fullStock)
            {
                dataTable.Rows.Add(item.Товар, item.Количество, item.МинУровень, item.МаксУровень,
                    item.Производитель, item.Категория,
                    item.Статус);
            }

            return dataTable;
        }

        private string GetStockStatus(int quantity, int minLevel, int maxLevel)
        {
            if (quantity <= minLevel) return "Низкий запас";
            if (quantity >= maxLevel) return "Избыток";
            return "Норма";
        }

        private void UpdateStatistics(DataView dataView)
        {
            if (dataView == null)
            {
                TotalCountText.Text = "Всего: 0";
                CriticalCountText.Text = "Критично: 0";
                LowCountText.Text = "Низкий: 0";
                ExcessCountText.Text = "Избыток: 0";
                return;
            }

            var table = dataView.Table;
            int totalCount = table.Rows.Count;
            int criticalCount = 0;
            int lowCount = 0;
            int excessCount = 0;

            // Проверяем наличие столбца "Статус" в таблице
            bool hasStatusColumn = table.Columns.Contains("Статус");

            foreach (DataRow row in table.Rows)
            {
                if (hasStatusColumn)
                {
                    var status = row["Статус"]?.ToString();
                    switch (status)
                    {
                        case "КРИТИЧЕСКИ НИЗКИЙ":
                            criticalCount++;
                            break;
                        case "НИЗКИЙ":
                            lowCount++;
                            break;
                        case "Избыток":
                            excessCount++;
                            break;
                    }
                }
                else
                {                  
                    excessCount++;
                }
            }

            TotalCountText.Text = $"Всего: {totalCount}";
            CriticalCountText.Text = $"Критично: {criticalCount}";
            LowCountText.Text = $"Низкий: {lowCount}";
            ExcessCountText.Text = $"Избыток: {excessCount}";
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

                // Заголовок отчёта
                var reportType = ((ComboBoxItem)ReportTypeComboBox.SelectedItem)?.Content.ToString();
                worksheet.Cells[1, 1] = $"Отчёт по складу - {reportType}";
                worksheet.Cells[1, 1].Font.Bold = true;
                worksheet.Cells[1, 1].Font.Size = 14;

                // Дата формирования
                worksheet.Cells[2, 1] = $"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}";
                worksheet.Cells[2, 1].Font.Italic = true;

                // Заголовки таблицы
                for (int i = 0; i < dataView.Table.Columns.Count; i++)
                {
                    worksheet.Cells[5, i + 1] = dataView.Table.Columns[i].ColumnName;
                    worksheet.Cells[5, i + 1].Font.Bold = true;
                    worksheet.Cells[5, i + 1].Interior.Color = Excel.XlRgbColor.rgbLightGray;
                }

                // Данные
                for (int i = 0; i < dataView.Table.Rows.Count; i++)
                {
                    for (int j = 0; j < dataView.Table.Columns.Count; j++)
                    {
                        worksheet.Cells[i + 6, j + 1] = dataView.Table.Rows[i][j];
                    }
                }

                // Форматирование
                worksheet.Columns.AutoFit();

                // Подсветка статусов цветом (только если есть столбец "Статус")
                if (dataView.Table.Columns.Contains("Статус"))
                {
                    for (int i = 0; i < dataView.Table.Rows.Count; i++)
                    {
                        var status = dataView.Table.Rows[i]["Статус"]?.ToString();
                        var statusColumn = GetColumnIndex(dataView.Table, "Статус");

                        if (statusColumn >= 0)
                        {
                            var statusCell = worksheet.Cells[i + 6, statusColumn + 1];

                            switch (status)
                            {
                                case "КРИТИЧЕСКИ НИЗКИЙ":
                                case "Низкий запас":
                                    statusCell.Interior.Color = Excel.XlRgbColor.rgbRed;
                                    statusCell.Font.Color = Excel.XlRgbColor.rgbWhite;
                                    break;
                                case "НИЗКИЙ":
                                    statusCell.Interior.Color = Excel.XlRgbColor.rgbYellow;
                                    break;
                                case "Избыток":
                                    statusCell.Interior.Color = Excel.XlRgbColor.rgbLightBlue;
                                    break;
                                case "Норма":
                                    statusCell.Interior.Color = Excel.XlRgbColor.rgbLightGreen;
                                    break;
                            }
                        }
                    }
                }

                MessageBox.Show("Отчет экспортирован в Excel", "Готово",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetColumnIndex(DataTable table, string columnName)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].ColumnName == columnName)
                    return i;
            }
            return -1;
        }
    }
}