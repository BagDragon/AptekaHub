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
using System.Windows.Shapes;
using AptekaInternetApp.View.LoginAndRegWindows.UserControls;

namespace AptekaInternetApp.View.LoginAndRegWindows
{
    /// <summary>
    /// Логика взаимодействия для UpdatePasswordWindow.xaml
    /// </summary>
    

    public partial class UpdatePasswordWindow : Window
    {
        public UpdatePasswordWindow()
        {
            InitializeComponent();
            UpdatePasswordContentControl.Content = new RequestEmailUserControl();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationWindow userLoginWindow = new AuthorizationWindow();    
            userLoginWindow.Show();
            this.Close();
        }


    }
}
