using AptekaInternetApp.Models;
using AptekaInternetApp.Models.ModelProgram;
using AptekaInternetApp.Models.TablesDB;
using AptekaInternetApp.ViewModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AptekaInternetApp.View.MainWindowFile.UsersControls
{
    /// <summary>
    /// Логика взаимодействия для CatalogCategoriesUserControl.xaml
    /// </summary>
    public partial class CatalogCategoriesUserControl : UserControl
    {

        public CatalogCategoriesUserControl()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            ItemsControlCategories.ItemsSource = CategoryHandler.LoadCategories();
        }

        private void CategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedCategory = button?.Tag as Category;

            if (selectedCategory != null)
            {
                CategoryHandler.HandleCategorySelection(selectedCategory, this);
            }
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.UpdateBackButtonVisibility();
        }

    }
}
