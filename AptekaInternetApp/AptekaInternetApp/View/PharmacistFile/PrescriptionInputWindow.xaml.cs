using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AptekaInternetApp.View.PharmacistFile
{
    /// <summary>
    /// Логика взаимодействия для PrescriptionInputWindow.xaml
    /// </summary>
    public partial class PrescriptionInputWindow : Window
    {
        public PrescriptionData PrescriptionData { get; private set; }
        public int ProductId { get; }
        public string ProductName { get; }
        public string ProductDescription { get; }

        public PrescriptionInputWindow(int productId, string productName, string productDescription)
        {
            InitializeComponent();

            ProductId = productId;
            ProductName = productName;
            ProductDescription = productDescription;
            PrescriptionData = new PrescriptionData();

            this.DataContext = PrescriptionData;
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(PrescriptionData.PatientName))
            {
                ShowValidationError("Введите ФИО пациента");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PrescriptionData.PrescriptionNumber))
            {
                ShowValidationError("Введите номер рецепта");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PrescriptionData.DoctorName))
            {
                ShowValidationError("Введите ФИО врача");
                return false;
            }

            if (PrescriptionData.IssueDate == default)
            {
                ShowValidationError("Выберите дату выдачи рецепта");
                return false;
            }

            if (PrescriptionData.ExpiryDate == default)
            {
                ShowValidationError("Выберите срок действия рецепта");
                return false;
            }

            if (PrescriptionData.ExpiryDate <= PrescriptionData.IssueDate)
            {
                ShowValidationError("Срок действия должен быть позже даты выдачи");
                return false;
            }

            if (PrescriptionData.ExpiryDate < DateTime.Now)
            {
                ShowValidationError("Срок действия рецепта истек");
                return false;
            }

            HideValidationError();
            return true;
        }

        private void ShowValidationError(string message)
        {
            ValidationTextBlock.Text = message;
            ValidationBorder.Visibility = Visibility.Visible;
        }

        private void HideValidationError()
        {
            ValidationBorder.Visibility = Visibility.Collapsed;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

    public class PrescriptionData
    {
        public string PatientName { get; set; }
        public string PrescriptionNumber { get; set; }
        public string DoctorName { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.Now;
        public DateTime ExpiryDate { get; set; } = DateTime.Now.AddMonths(1);
    }
}
