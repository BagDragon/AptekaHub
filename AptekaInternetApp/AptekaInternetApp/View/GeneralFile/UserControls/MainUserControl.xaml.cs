using AptekaInternetApp.View.MainWindowFile.UsersControls;
using AptekaInternetApp.ViewModel.Handlers;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AptekaInternetApp.View.GeneralFile.UserControls
{
    /// <summary>
    /// Логика взаимодействия для MainUserControl.xaml
    /// </summary>
    public partial class MainUserControl : UserControl
    {
        public MainUserControl()
        {
            InitializeComponent();
        }

        private void BannerProduct1_Click(object sender, RoutedEventArgs e)
        {
            BannersHandler.ManufactureSelection("ЗАО Зелёный Дуб", this);
        }

        private void BannerProduct2_Click(object sender, RoutedEventArgs e)
        {
            BannersHandler.ManufactureSelection("ООО НеКиндер", this);
        }

        private void GoToCatalog_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {             
                mainWindow.ViewContentControl.Content = new CatalogUserControl();
            }
        }

        private void CheckStock_Click(object sender, RoutedEventArgs e)
        {
            BannersHandler.MinStockSelection(this);
        }
    }
}
