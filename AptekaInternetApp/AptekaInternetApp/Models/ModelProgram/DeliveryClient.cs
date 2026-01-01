using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptekaInternetApp.Models.ModelProgram
{
    public class DeliveryClient : INotifyPropertyChanged
    {
        public int IdDelivery { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public string ClientName { get; set; }
        public string StatusDelivery { get; set; }
        public DateTime DateOrderClient  { get; set; }

        private string _clientAddress;
        public string ClientAddress
        {
            get => _clientAddress;
            set
            {
                if (_clientAddress != value)
                {
                    _clientAddress = value;
                    OnPropertyChanged(nameof(ClientAddress));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
