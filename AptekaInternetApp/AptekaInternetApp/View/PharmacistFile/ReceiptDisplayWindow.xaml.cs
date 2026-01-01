using AptekaInternetApp.Models;
using AptekaInternetApp.Models.ModelProgram;
using AptekaInternetApp.Models.TablesDB;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AptekaInternetApp.View.PharmacistFile
{
    /// <summary>
    /// Логика взаимодействия для ReceiptDisplayWindow.xaml
    /// </summary>
    public partial class ReceiptDisplayWindow : Window
    {
        private Sale _sale;
        private List<CartProduct> _cartProducts;

        public ReceiptDisplayWindow(Sale sale, List<CartProduct> cartProducts)
        {
            InitializeComponent();
            _sale = sale;
            _cartProducts = cartProducts;
            LoadReceiptData();
        }

        private void LoadReceiptData()
        {
            // Заполняем информацию о чеке
            ReceiptNumberText.Text = $"Чек №: {_sale.ReceiptNumber}";
            SaleDateText.Text = $"Дата: {_sale.Date:dd.MM.yyyy HH:mm}";
            CashierNameText.Text = $"Кассир: {UserSession.FIO ?? "Неизвестно"}";

            // Заполняем итоги
            decimal subtotal = _cartProducts.Sum(p => p.Price * p.Quantity);
            SubtotalText.Text = $"{subtotal:N2} ₽";
            DiscountText.Text = $"-{_sale.Discount:N2} ₽";
            TotalText.Text = $"{_sale.Total:N2} ₽";

            // Заполняем список товаров
            var receiptItems = _cartProducts.Select(p => new ReceiptItem
            {
                ProductName = p.NameProduct,
                Quantity = p.Quantity,
                PriceWithDiscount = p.PriceWithDiscount,
                Total = p.TotalWithDiscount,
                RequiresPrescription = p.RequiresPrescription
            }).ToList();

            ItemsControl.ItemsSource = receiptItems;
        }

        private void SavePdfButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаем папку для PDF чеков
                string pdfFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "АптекаЧеки", "PDF");

                Directory.CreateDirectory(pdfFolder);

                string fileName = $"Чек_{_sale.ReceiptNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string filePath = Path.Combine(pdfFolder, fileName);

                // Создаем PDF
                CreatePdfReceipt(filePath);

                // Показываем сообщение об успехе
                var result = MessageBox.Show($"Чек успешно сохранен в PDF!\n\nФайл: {fileName}\n\nОткрыть PDF для печати?",
                    "PDF сохранен", MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    // Пытаемся открыть PDF для печати
                    TryOpenPdfForPrint(filePath);
                }
                else
                {
                    // Просто открываем папку с файлом
                    Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении PDF:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TryOpenPdfForPrint(string filePath)
        {
            try
            {
                // Сначала пытаемся открыть с командой печати
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true,
                    Verb = "print"
                });

                MessageBox.Show("PDF отправлен на печать!", "Печать",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception printEx)
            {
                // Если не получилось открыть для печати, пробуем просто открыть файл
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });

                    MessageBox.Show("PDF файл открыт", "Файл открыт",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception openEx)
                {
                    // Если и это не получилось, предлагаем открыть папку
                    MessageBox.Show($"Не удалось автоматически открыть PDF файл.\n\n" +
                                  $"Файл сохранен по пути: {filePath}\n\n" +
                                  $"Откройте папку с файлом и откройте его вручную.",
                                  "Не удалось открыть файл",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Открываем папку и выделяем файл
                    Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CreatePdfReceipt(string filePath)
        {
            // Регистрируем провайдер кодировок
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                // Создаем документ с маленьким размером для чека
                Document document = new Document(new Rectangle(288f, 576f), 10, 10, 10, 10);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);

                document.Open();

                // Создаем шрифт с поддержкой кириллицы
                BaseFont baseFont = CreateUnicodeFont();

                Font titleFont = new Font(baseFont, 14, Font.BOLD);
                Font headerFont = new Font(baseFont, 10, Font.BOLD);
                Font normalFont = new Font(baseFont, 9);
                Font smallFont = new Font(baseFont, 8);
                Font boldFont = new Font(baseFont, 9, Font.BOLD);

                // Заголовок
                Paragraph title = new Paragraph("АПТЕКА", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                Paragraph subtitle = new Paragraph("КАССОВЫЙ ЧЕК", headerFont);
                subtitle.Alignment = Element.ALIGN_CENTER;
                document.Add(subtitle);

                document.Add(new Paragraph(" "));

                // Информация о чеке
                document.Add(new Paragraph($"Чек №: {_sale.ReceiptNumber}", normalFont));
                document.Add(new Paragraph($"Дата продажи: {_sale.Date:dd.MM.yyyy HH:mm}", normalFont));
                document.Add(new Paragraph($"Кассир: {UserSession.FIO ?? "Неизвестно"}", normalFont));
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph("----------------------------------------", normalFont));
                document.Add(new Paragraph(" "));

                // Заголовок таблицы товаров
                Paragraph itemsHeader = new Paragraph("ПРОДАННЫЕ ТОВАРЫ:", boldFont);
                document.Add(itemsHeader);
                document.Add(new Paragraph(" "));

                // Товары
                foreach (var item in _cartProducts)
                {
                    // Название товара
                    Paragraph productName = new Paragraph(item.NameProduct, normalFont);
                    document.Add(productName);

                    // Цена и количество
                    string priceLine = $"{item.Quantity} шт. x {item.PriceWithDiscount:N2} ₽ = {item.TotalWithDiscount:N2} ₽";
                    Paragraph priceParagraph = new Paragraph(priceLine, smallFont);
                    priceParagraph.IndentationLeft = 10;
                    document.Add(priceParagraph);

                    // Маркер рецептурного товара
                    if (item.RequiresPrescription)
                    {
                        Paragraph prescription = new Paragraph("★ РЕЦЕПТУРНЫЙ ПРЕПАРАТ", smallFont);
                        prescription.IndentationLeft = 10;
                        prescription.Font.Color = BaseColor.RED;
                        document.Add(prescription);
                    }

                    document.Add(new Paragraph(" "));
                }

                document.Add(new Paragraph("----------------------------------------", normalFont));
                document.Add(new Paragraph(" "));

                // Итоги
                decimal subtotal = _cartProducts.Sum(p => p.Price * p.Quantity);
                document.Add(new Paragraph($"Промежуточный итог: {subtotal:N2} ₽", normalFont));
                document.Add(new Paragraph($"Скидка: {_sale.Discount:N2} ₽", normalFont));

                Paragraph total = new Paragraph($"ИТОГО К ОПЛАТЕ: {_sale.Total:N2} ₽", headerFont);
                total.Alignment = Element.ALIGN_CENTER;
                document.Add(total);

                document.Add(new Paragraph(" "));

                // Благодарность
                Paragraph thanks = new Paragraph("СПАСИБО ЗА ПОКУПКУ!", headerFont);
                thanks.Alignment = Element.ALIGN_CENTER;
                document.Add(thanks);

                document.Add(new Paragraph(" "));

                // Время формирования чека
                Paragraph generated = new Paragraph($"Чек сформирован: {DateTime.Now:dd.MM.yyyy HH:mm}", smallFont);
                generated.Alignment = Element.ALIGN_CENTER;
                document.Add(generated);

                document.Close();
            }
        }

        private BaseFont CreateUnicodeFont()
        {
            // Пытаемся найти системный шрифт с поддержкой кириллицы
            string[] possibleFonts = {
                "arial.ttf",
                "tahoma.ttf",
                "times.ttf",
                "cour.ttf"
            };

            foreach (string fontName in possibleFonts)
            {
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), fontName);
                if (File.Exists(fontPath))
                {
                    try
                    {
                        return BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    }
                    catch
                    {
                        continue; // Пробуем следующий шрифт
                    }
                }
            }

            // Если не нашли подходящий шрифт, используем стандартный с WINANSI
            return BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, BaseFont.NOT_EMBEDDED);
        }
    }

    public class ReceiptItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal PriceWithDiscount { get; set; }
        public decimal Total { get; set; }
        public bool RequiresPrescription { get; set; }
    }
}