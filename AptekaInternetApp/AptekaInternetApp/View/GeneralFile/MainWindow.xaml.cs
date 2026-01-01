using AptekaInternetApp.Models;
using AptekaInternetApp.Models.ModelProgram;
using AptekaInternetApp.Models.Tables;
using AptekaInternetApp.View.GeneralFile.UserControls;
using AptekaInternetApp.View.LoginAndRegWindows;
using AptekaInternetApp.View.MainWindowFile;
using AptekaInternetApp.View.MainWindowFile.UsersControls;
using AptekaInternetApp.View.PharmacistFile.UserControls;
using AptekaInternetApp.ViewModel;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace AptekaInternetApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string loginStr;

        public MainWindow(string login, string role)
        {
            InitializeComponent();

            UserSession.Login = login;
            UserSession.Role = role;

            NavigationService.Initialize(ViewContentControl);

            using (var context = new ContextDB())
            {
                NameUser.Text = login;
                var query = context.Users.FirstOrDefault(b => b.Login == login);

                RoleUser.Text = query.Role;
                PanelUserName.Text = login;
                PanelUserRole.Text = role;

                UserSession.Id = query.Id;
                UserSession.FIO = query.FIO;

                //NavigationService.NavigationChanged += OnNavigationChanged;

                if (!UserSession.IsAdmin)
                {
                    MenuAddCategoriesButton.Visibility = Visibility.Collapsed;
                    MenuAddProductButton.Visibility = Visibility.Collapsed;
                    MenuAddSubcategoriesButton.Visibility = Visibility.Collapsed;

                }
                else 
                {
                    ShoppingCartButton.Visibility = Visibility.Collapsed;
                    CartButton.Visibility = Visibility.Collapsed;
                }
            }

            ViewCategoriesContentControl.Content = new CatalogCategoriesUserControl();
            ViewSubcategoriesContentControl.Content = new CatalogSubcategoriesUserControl();

            // Используем NavigationService для начальной загрузки
            NavigationService.NavigateTo(new MainUserControl());

            SearchTextBox.GotFocus += SearchTextBox_GotFocus;
            SearchTextBox.LostFocus += SearchTextBox_LostFocus;

            UpdateBackButtonVisibility();
        }


        private void OnNavigationChanged()
        {
            UpdateBackButtonVisibility();
        }

        public void UpdateBackButtonVisibility()
        {
            BackButton.GetBindingExpression(Button.VisibilityProperty)?.UpdateTarget();
            //bool canGoBack = NavigationService.CanGoBack();
            //BackButton.Visibility = canGoBack ? Visibility.Visible : Visibility.Collapsed;
            // Debug.WriteLine($"UpdateBackButtonVisibility - CanGoBack: {canGoBack}, BackButton.Visibility: {BackButton.Visibility}, Stack count: {NavigationService.GetStackCount()}");
            Debug.WriteLine($"UpdateBackButtonVisibility - CanGoBack: {NavigationService.CanGoBack()}, Stack count: {NavigationService.GetStackCount()}");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }


        private void ShoppingCartButton_Click(object sender, RoutedEventArgs e)
        {
            ShoppingCartUserControl shoppingCartUserControl = new ShoppingCartUserControl();
            NavigationService.NavigateTo(shoppingCartUserControl);
            UpdateBackButtonVisibility();

        }

        private void MenuAddProductButton_Click(object sender, RoutedEventArgs e)
        {
            ProductMasterUserControl product = new ProductMasterUserControl();
            NavigationService.NavigateTo(product);
            UpdateBackButtonVisibility();
        }

        private void MenuAddCategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            CategoriesMasterUserControl categoriesMasterUserControl = new CategoriesMasterUserControl();
            NavigationService.NavigateTo(categoriesMasterUserControl);
            UpdateBackButtonVisibility();
        }

        private void MenuAddSubcategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            SubcategoriesMasterUserControl subcategoriesMasterUserControl = new SubcategoriesMasterUserControl();
            NavigationService.NavigateTo(subcategoriesMasterUserControl);
            UpdateBackButtonVisibility();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            CatalogUserControl catalogUserControl = new CatalogUserControl(SearchTextBox.Text);
            NavigationService.NavigateTo(catalogUserControl);
            UpdateBackButtonVisibility();
        }

        private void MenuHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            HistoryUserControl historyUserControl = new HistoryUserControl();
            NavigationService.NavigateTo(historyUserControl);
            UpdateBackButtonVisibility();
        }

      
        private void ActionMenuUsers_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ActionUserPanel.Visibility = Visibility.Collapsed;
        }

        private void ActionUsers_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ActionUserPanel.Visibility = ActionUserPanel.Visibility == Visibility.Visible
                                                                    ? Visibility.Collapsed
                                                                    : Visibility.Visible;
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Поиск лекарств и товаров...")
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Поиск лекарств и товаров...";
                SearchTextBox.Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99));
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationWindow  userLoginWindow = new AuthorizationWindow();
            userLoginWindow.Show();
            this.Close();
        }

        private void MenuCategories_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
           // CategoriesListBox.Visibility = Visibility.Collapsed;
        }

        private void OpenMainUserControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigationService.NavigateTo(new MainUserControl());
        }

        private void MenuCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            if (CatalogPanelBorder.Visibility == Visibility.Visible)
            {
                CloseCatalogPanel();
            }
            else
            {
                var catalogPanel = new CatalogPanelUserControl();

                // Обработчик выбора подкатегории
                catalogPanel.SubcategorySelected += (subcategory) =>
                {
                    SubcategoryHandler.HandleSubcategorySelection(subcategory, this);
                    CloseCatalogPanel();
                };

                // Обработчик выбора категории
                catalogPanel.CategorySelected += (category) =>
                {
                    CategoryHandler.HandleCategorySelection(category, this);
                    CloseCatalogPanel();
                };

                catalogPanel.CloseRequested += CloseCatalogPanel;

                // Позиционируем панель
                CatalogPanelContent.Content = catalogPanel;
                CatalogPanelBorder.Visibility = Visibility.Visible;
            }
        }

        private void CloseCatalogPanel()
        {
            CatalogPanelBorder.Visibility = Visibility.Collapsed;
            CatalogPanelContent.Content = null; // Очищаем содержимое
        }

        private void MainGrid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Закрываем панель каталога при клике вне ее области
            if (CatalogPanelBorder.Visibility == Visibility.Visible &&
                !IsMouseOverCatalogPanel(e.GetPosition(this)))
            {
                CloseCatalogPanel();
            }
        }

        private bool IsMouseOverCatalogPanel(Point mousePosition)
        {
            if (CatalogPanelBorder.Visibility != Visibility.Visible)
                return false;

            var catalogPanelBounds = new Rect(
                CatalogPanelBorder.TranslatePoint(new Point(0, 0), this),
                new Size(CatalogPanelBorder.ActualWidth, CatalogPanelBorder.ActualHeight));

            return catalogPanelBounds.Contains(mousePosition);
        }

        private void MenuCatalogButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (CatalogPanelBorder.Visibility == Visibility.Visible &&
                !IsMouseOverCatalogPanel(e.GetPosition(this)))
            {
                CloseCatalogPanel();
            }
        }

        private void ActionUserPanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ActionUserPanel.Visibility = Visibility.Collapsed;
        }

    }



    public class BackButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return NavigationService.CanGoBack() ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
