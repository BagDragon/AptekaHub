using AptekaInternetApp.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AptekaInternetApp.Models.ModelProgram
{
    public class DetailsProduct : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public decimal Price { get; set; }
        public bool RequiresPrescription { get; set; }
        // Свойства для скидок
        private decimal _discountPercent;
        public decimal DiscountPercent
        {
            get => _discountPercent;
            set
            {
                _discountPercent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PriceWithDiscount));
                OnPropertyChanged(nameof(HasDiscount));
            }
        }

        public decimal PriceWithDiscount => Price - (Price * (DiscountPercent / 100m));
        public bool HasDiscount => DiscountPercent > 0;

        public string Manufacture { get; set; }
        public string formCreate { get; set; }
        public string typePac { get; set; }
        public string storageСonditions { get; set; }
        public string activeSubst { get; set; }
        public string dosage { get; set; }
        public string country { get; set; }
        public int ExtDate { get; set; }
        public int ProdQuantity { get; set; }

        // Свойства для корзины
        private bool _isInCart;
        public bool IsInCart
        {
            get => _isInCart;
            set
            {
                _isInCart = value;
                OnPropertyChanged();
            }
        }

        private int _cartQuantity;
        public int CartQuantity
        {
            get => _cartQuantity;
            set
            {
                _cartQuantity = value;
                OnPropertyChanged();
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
