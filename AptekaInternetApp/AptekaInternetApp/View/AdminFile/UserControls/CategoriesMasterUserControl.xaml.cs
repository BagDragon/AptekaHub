using AptekaInternetApp.Models;
using AptekaInternetApp.Models.TablesDB;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AptekaInternetApp.View.MainWindowFile.UsersControls
{
    public partial class CategoriesMasterUserControl : UserControl
    {
        private ContextDB _context;
        private Category _selectedCategory;
        private bool _isEditMode = false;

        public CategoriesMasterUserControl()
        {
            InitializeComponent();
            _context = new ContextDB();
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                CategoriesDataGrid.ItemsSource = _context.Categories.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void PathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePathPreview();
        }

        private void UpdatePathPreview()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(PathTextBox.Text))
                {
                    var geometry = Geometry.Parse(PathTextBox.Text);
                    PathPreview.Data = geometry;
                }
                else
                {
                    PathPreview.Data = null;
                }
            }
            catch (Exception)
            {
                // Невалидный Path - очищаем preview
                PathPreview.Data = null;
            }
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateName();
        }

        private void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                NameValidationText.Text = "Название обязательно";
                NameValidationText.Visibility = Visibility.Visible;
                return;
            }

            if (NameTextBox.Text.Length < 3 || NameTextBox.Text.Length > 50)
            {
                NameValidationText.Text = "Название должно быть от 3 до 50 символов";
                NameValidationText.Visibility = Visibility.Visible;
                return;
            }

            NameValidationText.Visibility = Visibility.Collapsed;
        }

        private bool ValidateForm()
        {
            ValidateName();

            if (NameValidationText.Visibility == Visibility.Visible)
                return false;

            if (string.IsNullOrWhiteSpace(PathTextBox.Text))
            {
                MessageBox.Show("Введите SVG Path код для иконки категории");
                return false;
            }

            return true;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                var category = new Category
                {
                    Name = NameTextBox.Text.Trim(),
                    ImageCategory = PathTextBox.Text.Trim()
                };

                _context.Categories.Add(category);
                _context.SaveChanges();

                ClearForm();
                LoadCategories();
                MessageBox.Show("Категория успешно добавлена");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении категории: {ex.Message}");
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int categoryId)
            {
                _selectedCategory = _context.Categories.Find(categoryId);
                if (_selectedCategory != null)
                {
                    _isEditMode = true;
                    NameTextBox.Text = _selectedCategory.Name;
                    PathTextBox.Text = _selectedCategory.ImageCategory;

                    AddButton.Visibility = Visibility.Collapsed;
                    UpdateButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm() || _selectedCategory == null) return;

            try
            {
                _selectedCategory.Name = NameTextBox.Text.Trim();
                _selectedCategory.ImageCategory = PathTextBox.Text.Trim();

                _context.SaveChanges();

                CancelEditMode();
                LoadCategories();
                MessageBox.Show("Категория успешно обновлена");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении категории: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelEditMode();
        }

        private void CancelEditMode()
        {
            _isEditMode = false;
            _selectedCategory = null;
            ClearForm();

            AddButton.Visibility = Visibility.Visible;
            UpdateButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int categoryId)
            {
                var category = _context.Categories.Find(categoryId);
                if (category != null)
                {
                    var result = MessageBox.Show(
                        $"Вы уверены, что хотите удалить категорию '{category.Name}'?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Проверяем, есть ли связанные подкатегории
                            if (category.Subcategories?.Any() == true)
                            {
                                MessageBox.Show("Невозможно удалить категорию, так как с ней связаны подкатегории");
                                return;
                            }

                            _context.Categories.Remove(category);
                            _context.SaveChanges();

                            LoadCategories();
                            MessageBox.Show("Категория успешно удалена");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при удалении категории: {ex.Message}");
                        }
                    }
                }
            }
        }

        private void CategoriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Опционально: можно добавить функциональность при выборе строки
        }

        private void ClearForm()
        {
            NameTextBox.Text = "";
            PathTextBox.Text = "";
            PathPreview.Data = null;
            NameValidationText.Visibility = Visibility.Collapsed;
        }
    }
}