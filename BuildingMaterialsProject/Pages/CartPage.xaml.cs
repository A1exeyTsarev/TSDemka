using BuildingMaterialsProject.AppData;
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

namespace BuildingMaterialsProject.Pages
{
    /// <summary>
    /// Логика взаимодействия для CartPage.xaml
    /// </summary>
    public partial class CartPage : Page
    {
        public CartPage()
        {
            InitializeComponent();
            listCart.ItemsSource = Cart.Items;
            UpdateTotal();
        }

        private void UpdateTotal()
            => tbTotal.Text = $"Итого: {Cart.TotalSum():N2} ₽";

        private void Qty_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is CartItem item)
            {
                if (!int.TryParse(tb.Text, out int q) || q <= 0)
                {
                    MessageBox.Show("Количество должно быть целым числом > 0.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tb.Text = item.Quantity.ToString();
                    return;
                }

                var stock = AppConnect.model0db.StockItems.Find(item.StockItemId);
                if (stock != null && q > stock.Quantity)
                {
                    MessageBox.Show($"Доступно только {stock.Quantity} шт.",
                        "Недостаточно товара",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    tb.Text = item.Quantity.ToString();
                    return;
                }

                item.Quantity = q;
                listCart.Items.Refresh();
                UpdateTotal();
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is CartItem item)
            {
                Cart.Items.Remove(item);
                listCart.Items.Refresh();
                UpdateTotal();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (Cart.Items.Count == 0) return;

            if (MessageBox.Show("Очистить корзину?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question)
                != MessageBoxResult.Yes) return;

            Cart.Clear();
            listCart.Items.Refresh();
            UpdateTotal();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
            => AppFrame.mainFrame.Navigate(new ProductsPage());

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (Cart.Items.Count == 0)
            {
                MessageBox.Show("Корзина пуста.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int? maxNum = AppConnect.model0db.Orders.Any()
                    ? AppConnect.model0db.Orders.Max(o => o.OrderNumber)
                    : 0;

                var order = new Orders
                {
                    OrderNumber = maxNum + 1,
                    OrderDate = DateTime.Now,
                    UserId = AppConnect.user.Id
                };
                AppConnect.model0db.Orders.Add(order);
                AppConnect.model0db.SaveChanges();

                foreach (var ci in Cart.Items)
                {
                    var stock = AppConnect.model0db.StockItems.Find(ci.StockItemId);
                    if (stock == null || stock.Quantity < ci.Quantity)
                    {
                        MessageBox.Show(
                            $"Недостаточно «{ci.ProductName}» размера {ci.SizeValue}.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    AppConnect.model0db.OrderItems.Add(new OrderItems
                    {
                        OrderId = order.Id,
                        StockItemId = ci.StockItemId,
                        Quantity = ci.Quantity,
                        PricePerUnit = ci.PricePerUnit
                    });

                    stock.Quantity -= ci.Quantity;
                }

                AppConnect.model0db.SaveChanges();
                Cart.Clear();

                MessageBox.Show($"Заказ №{order.OrderNumber} оформлен!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                AppFrame.mainFrame.Navigate(new ProductsPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка оформления заказа",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
