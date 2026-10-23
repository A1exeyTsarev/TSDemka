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
    /// Логика взаимодействия для ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        private List<Products> _all = new List<Products>();
        public ProductsPage()
        {
            InitializeComponent();
            var mw = Application.Current.MainWindow as MainWindow;
            mw.fio.Text = AppConnect.user?.FIO ?? "Гость";

            // Гость и авторизованный клиент — только просмотр
            if (AppConnect.user == null || AppConnect.user.RoleId == 3)
                filterPanel.Visibility = Visibility.Collapsed;

            Load();
        }

        private void Load()
        {
            _all = AppConnect.model0db.Products.ToList();

            cmbSort.Items.Clear();
            cmbSort.Items.Add("Без сортировки");
            cmbSort.Items.Add("Цена ↑");
            cmbSort.Items.Add("Цена ↓");
            cmbSort.SelectedIndex = 0;

            cmbFilter.Items.Clear();
            cmbFilter.Items.Add("Все категории");
            foreach (var c in AppConnect.model0db.Categories.ToList())
                cmbFilter.Items.Add(c.CategoryName);
            cmbFilter.SelectedIndex = 0;

            Apply();
        }

        private void Apply()
        {
            var list = _all.AsEnumerable();

            string q = txtSearch.Text.Trim().ToLower();
            if (q.Length > 0)
                list = list.Where(p =>
                    p.Name.ToLower().Contains(q) ||
                    (p.Description ?? "").ToLower().Contains(q));

            if (cmbFilter.SelectedIndex > 0)
            {
                string cat = cmbFilter.SelectedItem.ToString();
                list = list.Where(p =>
                    p.Subcategories.Categories.CategoryName == cat);
            }

            if (cmbSort.SelectedIndex == 1)
                list = list.OrderBy(p => p.FinalPrice);
            if (cmbSort.SelectedIndex == 2)
                list = list.OrderByDescending(p => p.FinalPrice);

            listProduct.ItemsSource = list.ToList();
        }

        private void Filters_Changed(object sender, EventArgs e) => Apply();

        private void listProduct_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(listProduct.SelectedItem is Products p)) return;

            if (AppConnect.SelectProductMode)
            {
                AppFrame.mainFrame.Navigate(new ProductDetailsPage(p, AppConnect.EditingOrder));
                return;
            }

            if (AppConnect.user == null)
            {
                MessageBox.Show("Авторизуйтесь.");
                return;
            }

            AppFrame.mainFrame.Navigate(new ProductDetailsPage(p));
        }

    

        private void Del_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.RoleId != 1)
            {
                MessageBox.Show("Удаление доступно только администратору.",
                    "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(listProduct.SelectedItem is Products p)) return;

            if (MessageBox.Show($"Удалить «{p.Name}»?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question)
                != MessageBoxResult.Yes) return;

            try
            {
                AppConnect.model0db.Products.Remove(p);
                AppConnect.model0db.SaveChanges();
                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cart_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user == null)
            {
                MessageBox.Show("Авторизуйтесь.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            AppFrame.mainFrame.Navigate(new CartPage());
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user == null || AppConnect.user.RoleId == 3)
            {
                MessageBox.Show("Раздел доступен менеджеру и администратору.",
                    "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            AppFrame.mainFrame.Navigate(new OrderPage());
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            AppConnect.user = null;
            AppFrame.mainFrame.Navigate(new AuthorizationPage());
        }
    }
}

