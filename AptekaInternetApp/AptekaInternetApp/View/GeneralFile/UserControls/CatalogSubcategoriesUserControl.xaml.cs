using AptekaInternetApp.Models;
using AptekaInternetApp.Models.ModelProgram;
using AptekaInternetApp.Models.TablesDB;
using AptekaInternetApp.ViewModel;
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

namespace AptekaInternetApp.View.MainWindowFile.UsersControls
{
    /// <summary>
    /// Логика взаимодействия для CatalogSubcategoriesUserControl.xaml
    /// </summary>
    public partial class CatalogSubcategoriesUserControl : UserControl
    {
        public CatalogSubcategoriesUserControl()
        {
            InitializeComponent();
            LoadSubcategories();
        }

        private void LoadSubcategories()
        {
            ItemsControlSubcategories.ItemsSource = SubcategoryHandler.LoadSubcategories();
        }

        private void SubcategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedSubcategory = button?.Tag as Subcategory;

            if (selectedSubcategory != null)
            {
                SubcategoryHandler.HandleSubcategorySelection(selectedSubcategory, this);
            }

            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.UpdateBackButtonVisibility();
        }

    }
}
