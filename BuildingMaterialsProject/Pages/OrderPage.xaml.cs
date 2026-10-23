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
    /// Логика взаимодействия для OrderPage.xaml
    /// </summary>
    public partial class OrderPage : Page
    {
        public OrderPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            listOrders.ItemsSource = AppConnect.model0db.Orders
                .Include("Users")
                .Include("OrderItems")
                .ToList()
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        private void listOrders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => Edit_Click(sender, e);

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.RoleId != 1 && AppConnect.user?.RoleId != 2)
            {
                MessageBox.Show("Доступ ограничен.", "Уведомление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            AppFrame.mainFrame.Navigate(new AddOrderPage(null));
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.RoleId != 1)
            {
                MessageBox.Show("Редактирование доступно только администратору.",
                    "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(listOrders.SelectedItem is Orders o))
            {
                MessageBox.Show("Выберите заказ.", "Уведомление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            AppFrame.mainFrame.Navigate(new AddOrderPage(o));
        }

        private void Del_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.RoleId != 1)
            {
                MessageBox.Show("Удаление доступно только администратору.",
                    "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(listOrders.SelectedItem is Orders o)) return;

            if (MessageBox.Show($"Удалить заказ №{o.OrderNumber}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question)
                != MessageBoxResult.Yes) return;

            try
            {
                foreach (var item in o.OrderItems.ToList())
                {
                    var stock = AppConnect.model0db.StockItems.Find(item.StockItemId);
                    if (stock != null) stock.Quantity += item.Quantity;
                }

                AppConnect.model0db.OrderItems.RemoveRange(o.OrderItems);
                AppConnect.model0db.Orders.Remove(o);
                AppConnect.model0db.SaveChanges();
                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
            => AppFrame.mainFrame.Navigate(new ProductsPage());
    }
}
