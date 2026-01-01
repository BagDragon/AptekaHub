using AptekaInternetApp.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace AptekaInternetApp.Models.ModelProgram
{
    public class CatalogProduct : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string ImageCatalogProduct { get; set; }
        public string Name { get; set; }
        public string ReleaseForm { get; set; }
        public string Dosage { get; set; }
        public string CountInPackage { get; set; }

        public string NameProduct => $"{Name} {ReleaseForm} {CountInPackage}";

        public bool RequiresPrescription { get; set; }

        public string ActiveSubstance { get; set; }
        public int CountOnWarehouse { get; set; }
        public decimal CatalogPrice { get; set; }

        public decimal DiscountPercent { get; set; }
        public decimal PriceWithDiscount
        {
            get
            {
                if (DiscountPercent > 0)
                {
                    return CatalogPrice - (CatalogPrice * DiscountPercent / 100m);
                }
                return CatalogPrice;
            }
        }        
        public bool HasDiscount => DiscountPercent > 0;

        private bool _isInCart;
        public bool IsInCart
        {
            get => _isInCart;
            set
            {
                if (_isInCart != value)
                {
                    _isInCart = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _cartQuantity;
        public int CartQuantity
        {
            get => _cartQuantity;
            set
            {
                if (_cartQuantity != value)
                {
                    _cartQuantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnCartPropertiesChanged()
        {
            IsInCart = CartHandler.IsProductInCart(Id);
            CartQuantity = CartHandler.GetProductQuantity(Id);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
