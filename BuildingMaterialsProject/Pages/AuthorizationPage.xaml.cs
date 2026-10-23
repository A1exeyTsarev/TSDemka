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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BuildingMaterialsProject.AppData;

namespace BuildingMaterialsProject.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationPage.xaml
    /// </summary>
    public partial class AuthorizationPage : Page
    {
        public AuthorizationPage()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            if (login.Length == 0) return;

            var u = AppConnect.model0db.Users.FirstOrDefault(x => x.Login == login);
            if (u == null)
            {
                MessageBox.Show("Пользователь не найден.");
                return;
            }

            AppConnect.user = u;

            if (u.RoleId == 1 || u.RoleId == 2)
                AppFrame.mainFrame.Navigate(new OrderPage());
            else
                AppFrame.mainFrame.Navigate(new ProductsPage());
        }

        private void Guest_Click(object sender, RoutedEventArgs e)
        {
            AppConnect.user = null;
            AppFrame.mainFrame.Navigate(new ProductsPage());
        }
    }
}
