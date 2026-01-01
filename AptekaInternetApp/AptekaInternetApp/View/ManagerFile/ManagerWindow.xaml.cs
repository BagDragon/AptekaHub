using AptekaInternetApp.View.LoginAndRegWindows;
using AptekaInternetApp.View.ManagerFile.UserControls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace AptekaInternetApp.View.MainWindowFile
{
    /// <summary>
    /// Логика взаимодействия для ManagerWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        private DispatcherTimer _timer;

        public ManagerWindow(string nameManager)
        {
            InitializeComponent();
            NameUser.Text = nameManager;
            PopupUserName.Text = nameManager;
            RoleUser.Text = "Менеджер";
            PopupUserRole.Text = "Менеджер";

            InitializeTimer();
            UpdatePageTitle("Главная панель");
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentTime.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        }

        private void UpdatePageTitle(string title)
        {
            PageTitle.Text = title;
        }

        private void ReportSalesButton_Click(object sender, RoutedEventArgs e)
        {
            ViewUserControls.Content = new SalesReportControl();
            UpdatePageTitle("Отчёты по продажам");
        }

        private void ReportWarehouse_Click(object sender, RoutedEventArgs e)
        {
            ViewUserControls.Content = new WarehouseReportControl();
            UpdatePageTitle("Отчёты по складу");
        }

        private void UserInfo_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ActionUserPanel.Visibility = ActionUserPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void ActionUserPanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Закрываем меню при уходе мыши
            var position = e.GetPosition(ActionUserPanel);
            if (position.X < 0 || position.Y < 0 ||
                position.X > ActionUserPanel.ActualWidth ||
                position.Y > ActionUserPanel.ActualHeight)
            {
                ActionUserPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AuthorizationWindow userLoginWindow = new AuthorizationWindow();
                userLoginWindow.Show();
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            base.OnClosed(e);
        }
    }




}
