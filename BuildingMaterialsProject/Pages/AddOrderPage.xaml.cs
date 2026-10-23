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
using System.Xml.Linq;

namespace BuildingMaterialsProject.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddOrderPage.xaml
    /// </summary>
    public partial class AddOrderPage : Page
    {
        private Orders _order;
        private bool _isNew;

        public AddOrderPage(Orders order)
        {
            InitializeComponent();

            // Сброс флагов режима выбора
            AppConnect.SelectProductMode = false;
            AppConnect.EditingOrder = null;

            _isNew = order == null;
            _order = order ?? new Orders();

            // Клиенты
            cbClient.ItemsSource = AppConnect.model0db.Users.ToList();

            if (_isNew)
            {
                tbHeader.Text = "Новый заказ";
                dpDate.SelectedDate = DateTime.Now;
            }
            else
            {
                tbHeader.Text = $"Заказ №{_order.OrderNumber}";
                dpDate.SelectedDate = _order.OrderDate;
                cbClient.SelectedValue = _order.UserId;
                cbClient.IsEnabled = false; // клиента не меняем
            }

            LoadItems();
        }

        private void LoadItems()
        {
            if (_isNew)
            {
                dgItems.ItemsSource = null;
                tbTotal.Text = "Итого: 0,00 ₽";
                return;
            }

            var items = AppConnect.model0db.OrderItems
                .Where(i => i.OrderId == _order.Id)
                .ToList();

            dgItems.ItemsSource = items;
            tbTotal.Text = $"Итого: {items.Sum(i => i.Sum):N2} ₽";
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.RoleId != 1) return;

            // Сохраняем заказ, если он новый
            SaveOrderFirstIfNew();
            if (_isNew) return; // валидация не прошла

            // Переход в каталог в режиме выбора
            AppConnect.SelectProductMode = true;
            AppConnect.EditingOrder = _order;
            AppFrame.mainFrame.Navigate(new ProductsPage());
        }

        private void SaveOrderFirstIfNew()
        {
            if (!_isNew) return;

            if (cbClient.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? maxNum = AppConnect.model0db.Orders.Any()
                ? AppConnect.model0db.Orders.Max(o => o.OrderNumber)
                : 0;

            _order.OrderNumber = maxNum + 1;
            _order.OrderDate = dpDate.SelectedDate ?? DateTime.Now;
            _order.UserId = (int)cbClient.SelectedValue;

            AppConnect.model0db.Orders.Add(_order);
            AppConnect.model0db.SaveChanges();

            _isNew = false;
            tbHeader.Text = $"Заказ №{_order.OrderNumber}";
            cbClient.IsEnabled = false;
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.RoleId != 1) return;

            if (!(dgItems.SelectedItem is OrderItems item))
            {
                MessageBox.Show("Выберите позицию.", "Уведомление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Удалить «{item.StockItems.Products.Name}»?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question)
                != MessageBoxResult.Yes) return;

            try
            {
                // Возврат остатка
                var stock = AppConnect.model0db.StockItems.Find(item.StockItemId);
                if (stock != null) stock.Quantity += item.Quantity;

                AppConnect.model0db.OrderItems.Remove(item);
                AppConnect.model0db.SaveChanges();
                LoadItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.RoleId != 1) return;

            try
            {
                if (_isNew)
                {
                    SaveOrderFirstIfNew();
                    if (_isNew) return;
                }

                _order.OrderDate = dpDate.SelectedDate ?? DateTime.Now;
                AppConnect.model0db.SaveChanges();

                MessageBox.Show("Сохранено!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                AppFrame.mainFrame.Navigate(new OrderPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
            => AppFrame.mainFrame.Navigate(new OrderPage());
    }

}
