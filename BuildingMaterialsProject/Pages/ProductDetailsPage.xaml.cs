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
    /// Логика взаимодействия для ProductDetailsPage.xaml
    /// </summary>
    public partial class ProductDetailsPage : Page
    {
        private Products _p;
        private Orders _editingOrder;

        public ProductDetailsPage(Products p) : this(p, null) { }

        public ProductDetailsPage(Products p, Orders editingOrder)
        {
            InitializeComponent();
            _p = p;
            _editingOrder = editingOrder;
            DataContext = p;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (!(cbSize.SelectedItem is StockItems stock))
            {
                MessageBox.Show("Выберите размер.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(tbQty.Text, out int q) || q <= 0)
            {
                MessageBox.Show("Некорректное количество.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (q > stock.Quantity)
            {
                MessageBox.Show($"Доступно только {stock.Quantity} шт.",
                    "Недостаточно товара",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // === Режим добавления в существующий заказ ===
            if (_editingOrder != null)
            {
                try
                {
                    AppConnect.model0db.OrderItems.Add(new OrderItems
                    {
                        OrderId = _editingOrder.Id,
                        StockItemId = stock.Id,
                        Quantity = q,
                        PricePerUnit = _p.FinalPrice
                    });

                    stock.Quantity -= q;
                    AppConnect.model0db.SaveChanges();

                    var order = AppConnect.EditingOrder;
                    AppConnect.SelectProductMode = false;
                    AppConnect.EditingOrder = null;

                    AppFrame.mainFrame.Navigate(new AddOrderPage(order));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }

            // === Обычный режим — в корзину ===
            var existing = Cart.Items.FirstOrDefault(c => c.StockItemId == stock.Id);
            if (existing != null)
            {
                existing.Quantity += q;
            }
            else
            {
                Cart.Items.Add(new CartItem
                {
                    StockItemId = stock.Id,
                    ProductName = _p.Name,
                    ManufacturerName = _p.Manufacturers.ManufacturerName,
                    SizeValue = (decimal)stock.Sizes.SizeValue,
                    Quantity = q,
                    PricePerUnit = _p.FinalPrice
                });
            }

            MessageBox.Show("Добавлено в корзину.", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
            => AppFrame.mainFrame.Navigate(new ProductsPage());
    }
}

