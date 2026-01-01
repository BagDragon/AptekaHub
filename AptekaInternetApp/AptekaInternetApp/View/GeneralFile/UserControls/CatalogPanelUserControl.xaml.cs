using AptekaInternetApp.Models;
using AptekaInternetApp.Models.TablesDB;
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
    /// Логика взаимодействия для CatalogPanelUserControl.xaml
    /// </summary>
    public partial class CatalogPanelUserControl : UserControl
    {
        public event Action<Subcategory> SubcategorySelected;
        public event Action<Category> CategorySelected;
        public event Action CloseRequested;

        private List<Subcategory> _allSubcategories;
        private Category _selectedCategory;

        public CatalogPanelUserControl()
        {
            InitializeComponent();
            LoadAllData();
        }

        private void LoadAllData()
        {
            using (var context = new ContextDB())
            {
                // Загружаем категории
                var categories = context.Categories
                    .Where(c => !string.IsNullOrEmpty(c.ImageCategory))
                    .ToList();
                CategoriesItemsControl.ItemsSource = categories;

                // Загружаем ВСЕ подкатегории изначально
                _allSubcategories = context.Subcategories
                    .Where(s => !string.IsNullOrEmpty(s.ImageSubcategory))
                    .ToList();

                // Показываем все подкатегории при загрузке
                ShowAllSubcategories();
            }
        }

        private void ShowAllSubcategories()
        {
            SubcategoriesItemsControl.ItemsSource = _allSubcategories;
            SubcategoriesBorder.Visibility = Visibility.Visible;
            _selectedCategory = null; // Сбрасываем выбранную категорию
        }

        private void CategoryItem_MouseEnter(object sender, MouseEventArgs e)
        {
            var border = sender as Border;
            if (border?.Tag is Category category)
            {
                border.Background = new SolidColorBrush(Color.FromArgb(20, 48, 168, 255));
                ShowSubcategoriesForCategory(category);
            }
        }

        private void CategoryItem_MouseLeave(object sender, MouseEventArgs e)
        {
            var border = sender as Border;
            border.Background = Brushes.Transparent;

            // НЕ возвращаем все подкатегории при уходе мыши
            // Подкатегории остаются от последней выбранной категории
        }

        private void CategoryItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.Tag is Category category)
            {
                // Сохраняем выбранную категорию
                _selectedCategory = category;

                // Показываем подкатегории для выбранной категории
                ShowSubcategoriesForCategory(category);

                // Вызываем событие выбора категории
                CategorySelected?.Invoke(category);

                // Закрываем панель после выбора
                CloseRequested?.Invoke();
            }
        }

        private void ShowSubcategoriesForCategory(Category category)
        {
            // Фильтруем подкатегории по выбранной категории
            var filteredSubcategories = _allSubcategories
                .Where(s => s.CategoryId == category.ID_Category)
                .ToList();

            SubcategoriesItemsControl.ItemsSource = filteredSubcategories;
            SubcategoriesBorder.Visibility = Visibility.Visible;
        }

        private void SubcategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Subcategory subcategory)
            {
                SubcategorySelected?.Invoke(subcategory);
                CloseRequested?.Invoke();
            }
        }

        // Дополнительный метод для сброса к состоянию "все подкатегории"
        public void ResetToAllSubcategories()
        {
            ShowAllSubcategories();
        }
    }
}
