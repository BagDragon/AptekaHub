using AptekaInternetApp.Models;
using AptekaInternetApp.Models.TablesDB;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AptekaInternetApp.View.MainWindowFile.UsersControls
{
    public partial class SubcategoriesMasterUserControl : UserControl
    {
        private ContextDB _context;
        private Subcategory _selectedSubcategory;
        private bool _isEditMode = false;

        public SubcategoriesMasterUserControl()
        {
            InitializeComponent();
            _context = new ContextDB();
            LoadCategories();
            LoadSubcategories();
        }

        private void LoadCategories()
        {
            try
            {
                CategoriesComboBox.ItemsSource = _context.Categories.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void LoadSubcategories()
        {
            try
            {
                SubcategoriesDataGrid.ItemsSource = _context.Subcategories
                    .Include("Category")
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки подкатегорий: {ex.Message}");
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
                PathPreview.Data = null;
            }
        }

        private void CategoriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateCategory();
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateName();
        }

        private void ValidateCategory()
        {
            if (CategoriesComboBox.SelectedValue == null)
            {
                CategoryValidationText.Text = "Выберите категорию";
                CategoryValidationText.Visibility = Visibility.Visible;
            }
            else
            {
                CategoryValidationText.Visibility = Visibility.Collapsed;
            }
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
            ValidateCategory();
            ValidateName();

            if (CategoryValidationText.Visibility == Visibility.Visible ||
                NameValidationText.Visibility == Visibility.Visible)
                return false;

            if (string.IsNullOrWhiteSpace(PathTextBox.Text))
            {
                MessageBox.Show("Введите SVG Path код для иконки подкатегории");
                return false;
            }

            return true;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                var subcategory = new Subcategory
                {
                    Name = NameTextBox.Text.Trim(),
                    ImageSubcategory = PathTextBox.Text.Trim(),
                    CategoryId = (int)CategoriesComboBox.SelectedValue
                };

                _context.Subcategories.Add(subcategory);
                _context.SaveChanges();

                ClearForm();
                LoadSubcategories();
                MessageBox.Show("Подкатегория успешно добавлена");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении подкатегории: {ex.Message}");
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int subcategoryId)
            {
                _selectedSubcategory = _context.Subcategories.Find(subcategoryId);
                if (_selectedSubcategory != null)
                {
                    _isEditMode = true;
                    NameTextBox.Text = _selectedSubcategory.Name;
                    PathTextBox.Text = _selectedSubcategory.ImageSubcategory;
                    CategoriesComboBox.SelectedValue = _selectedSubcategory.CategoryId;

                    AddButton.Visibility = Visibility.Collapsed;
                    UpdateButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm() || _selectedSubcategory == null) return;

            try
            {
                _selectedSubcategory.Name = NameTextBox.Text.Trim();
                _selectedSubcategory.ImageSubcategory = PathTextBox.Text.Trim();
                _selectedSubcategory.CategoryId = (int)CategoriesComboBox.SelectedValue;

                _context.SaveChanges();

                CancelEditMode();
                LoadSubcategories();
                MessageBox.Show("Подкатегория успешно обновлена");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении подкатегории: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelEditMode();
        }

        private void CancelEditMode()
        {
            _isEditMode = false;
            _selectedSubcategory = null;
            ClearForm();

            AddButton.Visibility = Visibility.Visible;
            UpdateButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int subcategoryId)
            {
                var subcategory = _context.Subcategories.Find(subcategoryId);
                if (subcategory != null)
                {
                    var result = MessageBox.Show(
                        $"Вы уверены, что хотите удалить подкатегорию '{subcategory.Name}'?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Проверяем, есть ли связанные продукты
                            if (subcategory.Products?.Any() == true)
                            {
                                MessageBox.Show("Невозможно удалить подкатегорию, так как с ней связаны продукты");
                                return;
                            }

                            _context.Subcategories.Remove(subcategory);
                            _context.SaveChanges();

                            LoadSubcategories();
                            MessageBox.Show("Подкатегория успешно удалена");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при удалении подкатегории: {ex.Message}");
                        }
                    }
                }
            }
        }

        private void SubcategoriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Опционально: можно добавить функциональность при выборе строки
        }

        private void ClearForm()
        {
            NameTextBox.Text = "";
            PathTextBox.Text = "";
            CategoriesComboBox.SelectedIndex = -1;
            PathPreview.Data = null;
            NameValidationText.Visibility = Visibility.Collapsed;
            CategoryValidationText.Visibility = Visibility.Collapsed;
        }
    }
}