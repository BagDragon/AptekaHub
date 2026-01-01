using AptekaInternetApp.Models;
using AptekaInternetApp.View.MainWindowFile;
using AptekaInternetApp.ViewModel;
using System.Linq;
using System.Net;
using System.Windows;


namespace AptekaInternetApp.View.LoginAndRegWindows
{
    /// <summary>
    /// Логика взаимодействия для UserLoginWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        public string login;
        public string queryLogin;
        public string queryPassword;

        public AuthorizationWindow()
        {
            InitializeComponent();

        }

        public string password;

        private void User_password_PasswordChanged(object sender, RoutedEventArgs e)
        {

            User_passwordTextBox.Text = User_password.Password;
            password = User_passwordTextBox.Text;
        }

        private void User_passwordTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (User_password.Visibility != Visibility.Visible)
            {
                User_password.Password = User_passwordTextBox.Text;
                password = User_password.Password;
            }
        }


        private void VisiblePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (User_password.Visibility == Visibility.Visible)
            {
                User_password.Visibility = Visibility.Collapsed;
                User_passwordTextBox.Visibility = Visibility.Visible;

                User_passwordTextBox.Text = User_password.Password;
            }
            else
            {
                User_passwordTextBox.Visibility = Visibility.Collapsed;
                User_password.Visibility = Visibility.Visible;

                User_password.Password = User_passwordTextBox.Text;
            }
        }


        public void AuthorizationUsers()
        {
            login = UserLogin.Text.Trim();

            queryLogin = "";
            queryPassword = "";


            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин","Ошибка",
    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите пароль", "Ошибка",
    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new ContextDB())
            {
                var queryAdmin = context.Users.FirstOrDefault(b => b.Login == login);

                if (queryAdmin == null)
                {
                    MessageBox.Show("Пользователь не найден", "Ошибка",
    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (queryAdmin.Login != null)
                {
                    queryPassword = queryAdmin.Password;
                    queryLogin = queryAdmin.Login;
                }
                else return;

                if (SecurityAccount.VerifyPasswordHash(password, queryPassword))
                {
                    if (queryAdmin != null)
                    {
                        if (queryAdmin.Role == "Manager")
                        {
                            ManagerWindow managerWindow = new ManagerWindow(queryAdmin.Login);
                            managerWindow.Show();
                            this.Close();

                        }
                        else if (queryAdmin.Role == "Pharmacist")
                        {
                            MainWindow MainWindow = new MainWindow(queryLogin, queryAdmin.Role);
                            MainWindow.Show();
                            this.Close();
                        }
                        else
                        {
                            MainWindow MainWindow = new MainWindow(queryLogin, queryAdmin.Role);
                            MainWindow.Show();
                            this.Close();
                        }
                    }

                }
                else
                {
                    MessageBox.Show("Неверный пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }


        private void AutorizationButton_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationUsers();
        }

        private void InRegistrationWindowButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow mainWindow = new RegistrationWindow();
            mainWindow.Show();
            this.Close();
        }

        private void HelpPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            UpdatePasswordWindow updatePasswordWindow = new UpdatePasswordWindow();
            updatePasswordWindow.Show();
            this.Close();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) 
            {
                AuthorizationUsers();
            }
        }
    }
}
