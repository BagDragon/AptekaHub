using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptekaInternetApp.Models.ModelProgram
{
    public class SaleHistoryItem
    {
        public int SaleId { get; set; }
        public DateTime SaleDate { get; set; }
        public string ReceiptNumber { get; set; }
        public string CashierName { get; set; }
        public string Customer { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        //public PaymentType PaymentType { get; set; }
        // public string PaymentTypeString => GetPaymentTypeString();
        public ObservableCollection<SaleItemDetail> Items { get; set; } = new ObservableCollection<SaleItemDetail>();


    }

    public class SaleItemDetail
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal PriceWithDiscount { get; set; }
        public decimal AppliedDiscount { get; set; }
        public decimal Total { get; set; }
    }
}
