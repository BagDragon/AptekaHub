using AptekaInternetApp.Models.ModelProgram;
using AptekaInternetApp.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace AptekaInternetApp.View.LoginAndRegWindows.UserControls
{
    /// <summary>
    /// Логика взаимодействия для CodeEnterUserControl.xaml
    /// </summary>
    public partial class CodeEnterUserControl : UserControl
    {
        public int code;
        public string login;
        public string email;
        public CodeEnterUserControl(Users user)
        {
            InitializeComponent();
            this.DataContext = user;
            
            login = user.Login;
            email = user.Email;

            code = SendMessageEmail.SendMessage(email);
        }

        private void ConfirmEmailButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as UpdatePasswordWindow;

            if (InputCodeTextBox.Text == code.ToString())
            {              
                mainWindow.UpdatePasswordContentControl.Content = new ChangePasswordUserControl(login);
            }
            else
            {
                MessageBox.Show("Введённый код неверный", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
        }

        private void SendAgainPasswordHyperLink_Click(object sender, RoutedEventArgs e)
        {
          code= SendMessageEmail.SendMessage(email);
        }
    }
}
