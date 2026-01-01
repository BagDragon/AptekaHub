using AptekaInternetApp.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AptekaInternetApp.View.LoginAndRegWindows.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ChangePasswordUserControl.xaml
    /// </summary>
    public partial class ChangePasswordUserControl : UserControl
    {
        public string loginUser;
        public ChangePasswordUserControl(string login)
        {
            InitializeComponent();
            loginUser = login;
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as UpdatePasswordWindow;

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(NewPassword.Text);

            using (var context = new ContextDB())
            {
                if (NewPassword.Text == ConfirmedNewPassword.Text)
                {
                    var query = context.Users.FirstOrDefault(c => c.Login == loginUser);
                    query.Password = hashedPassword;
                    context.SaveChanges();
                    MessageBox.Show("Пароль успешно изменён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);


                    AuthorizationWindow userLoginWindow = new AuthorizationWindow();
                    userLoginWindow.Show();
                    mainWindow.Close();

                    

                }
                else
                {
                    MessageBox.Show("Пароли не сходятся", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
    }
}
