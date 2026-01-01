using AptekaInternetApp.Models;
using AptekaInternetApp.Models.Tables;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;

namespace AptekaInternetApp.View.LoginAndRegWindows
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        public string password;

        public RegistrationWindow()
        {
            InitializeComponent();

            RoleComboBox.Items.Add("Manager");
            RoleComboBox.Items.Add("Pharmacist");
        }


        private async void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {

            string fio = FIOTextBox.Text;
            string login = LoginTextBox.Text;
            string email = EmailTextBox.Text;
            password = PasswordTextBox.Password;

            using (var context = new ContextDB())
            {
               
                var qAdminEmail = context.Users.FirstOrDefault(c => c.Email == email);
                         
                if (qAdminEmail != null)
                {
                    MessageBox.Show("Данная почта уже привязана к другому аккаунту!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
            }

            if (password == RepeatPassword.Password)
                {

                    var commonPasswords = new[] { "password", "123456", "12345678", "123456789", "qwerty", "abc123", "letmein", "welcome", "monkey", "login" };

                    if (commonPasswords.Contains(password.ToLower()))
                    {
                        MessageBox.Show("Пароль не должен быть простым. Выберите другой.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                    using (var context = new ContextDB())
                    {
                        try
                        {                          
                            if (RoleComboBox.SelectedItem != null)
                            {
                                User admin = new User()
                                {
                                    FIO = fio,
                                    Login = login,
                                    Email = email,
                                    Password = hashedPassword,
                                    Role = RoleComboBox.SelectedItem.ToString(),
                                };
                                context.Users.Add(admin);
                            }
                            
                            await context.SaveChangesAsync();

                            MessageBox.Show("Пользователь зарегистрирован", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            AuthorizationWindow userLoginWindow = new AuthorizationWindow();
                            userLoginWindow.Show();
                            this.Close();
                        }
                        catch (DbEntityValidationException ex)
                        {
                            string errorMessages = string.Join("\n",
                                ex.EntityValidationErrors.SelectMany(b => b.ValidationErrors)
                                                          .Select(v => $"{v.PropertyName}: {v.ErrorMessage}"));

                            MessageBox.Show("Ошибка валидации:\n" + errorMessages, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Пароли не сходятся", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationWindow userLoginWindow = new AuthorizationWindow();
            userLoginWindow.Show();
            this.Close();
        }
    }
}
