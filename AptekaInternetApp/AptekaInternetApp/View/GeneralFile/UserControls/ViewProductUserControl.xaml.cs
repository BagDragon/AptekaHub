using AptekaInternetApp.Models.ModelProgram;
using AptekaInternetApp.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using NavigationService = AptekaInternetApp.ViewModel.NavigationService;

namespace AptekaInternetApp.View.MainWindowFile.UsersControls
{
    /// <summary>
    /// Логика взаимодействия для ViewProductUserControl.xaml
    /// </summary>
    public partial class ViewProductUserControl : UserControl
    {
        public int productId;
        private DetailsProduct _detailsProduct;

        public ViewProductUserControl(DetailsProduct detailsProduct)
        {
            InitializeComponent();

            _detailsProduct = detailsProduct;
            this.DataContext = _detailsProduct;

            productId = detailsProduct.Id;

            if (UserSession.IsAdmin)
            {
                AddCartBorder.Visibility = Visibility.Collapsed;
            }

            // Подписываемся на события корзины
            CartHandler.CartChanged += OnCartChanged;

            // Обновляем состояние корзины при загрузке
            UpdateCartState();
        }

        private void UpdateCartState()
        {
            _detailsProduct.OnCartPropertiesChanged();
        }

        private void OnCartChanged(int changedProductId, int newQuantity)
        {
            if (changedProductId == productId)
            {
                // Обновляем состояние только если изменился текущий товар
                UpdateCartState();
            }
        }

        private void AddInCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.IsAdmin) return;

            CartHandler.HandleCartProductSelection(productId, 1);
        }

        private void QuantityPlus_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.IsAdmin) return;

            var currentQuantity = CartHandler.GetProductQuantity(productId);
            if (currentQuantity >= 7)
            {
                MessageBox.Show("Вы превысили лимит покупки товара (7 шт.)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CartHandler.AddToCart(productId, 1);
        }

        private void QuantityMinus_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.IsAdmin) return;

            var currentQuantity = CartHandler.GetProductQuantity(productId);
            if (currentQuantity > 1)
            {
                CartHandler.UpdateQuantity(productId, currentQuantity - 1);
            }
            else
            {
                CartHandler.RemoveFromCart(productId);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Отписываемся от события при выгрузке контрола
            CartHandler.CartChanged -= OnCartChanged;
        }

        public class InverseBooleanToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is bool boolValue)
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;
                return Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

    }


    
}
