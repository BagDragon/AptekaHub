using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
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
using AptekaInternetApp.Models;
using AptekaInternetApp.Models.ModelProgram;
using AptekaInternetApp.ViewModel;

namespace AptekaInternetApp.View.LoginAndRegWindows.UserControls
{
    /// <summary>
    /// Логика взаимодействия для RequestEmailUserControl.xaml
    /// </summary>
    public partial class RequestEmailUserControl : UserControl
    {
        public RequestEmailUserControl()
        {
            InitializeComponent();
        }
        
        private void SendEmailButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as UpdatePasswordWindow;
           
            if (!EmailTextBox.Text.Contains("@"))
            {
                using(var context = new ContextDB())
                {
                    var query = context.Users.FirstOrDefault(b=> b.Login == EmailTextBox.Text);

                    if (query == null)
                    {
                        MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                  
                    
                    Users user = new Users()
                    {
                        Login = query.Login,
                        Email = query.Email,                      
                    };
                    mainWindow.UpdatePasswordContentControl.Content = new CodeEnterUserControl(user);
                }
            }
            if (EmailTextBox.Text.Contains("@"))
            {
                using (var context = new ContextDB())
                {
                    var query = context.Users.FirstOrDefault(b => b.Email == EmailTextBox.Text);

                    if (query == null)
                    {
                        MessageBox.Show("Почта не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                   
                    MessageBox.Show("Сообщение отправлено на вашу почту", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    Users user = new Users()
                    {
                        Login = query.Login,
                        Email = query.Email,                    
                    };
                    mainWindow.UpdatePasswordContentControl.Content = new CodeEnterUserControl(user);
                }
            }
        }


        

    }
}
