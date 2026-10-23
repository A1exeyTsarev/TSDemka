using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BuildingMaterialsProject.AppData
{
    class AppConnect
    {
        public static ChudoObuvEntities model0db;

        public static Users user;
        
        public static bool SelectProductMode;
        public static Orders EditingOrder;
    }
    // ============================================================
    //  РАСШИРЕНИЯ СУЩНОСТЕЙ EF (partial-классы)
    // ============================================================

    public partial class Users
    {
        public string FIO => $"{Surname} {FirstName} {Patronymic}".Trim();
    }

    public partial class OrderItems
    {
        public decimal Sum => (decimal)(Quantity * PricePerUnit);
    }

    public partial class Orders
    {
        public string ClientFIO => Users?.FIO ?? "";
        public decimal TotalSum => OrderItems.Sum(i => i.Sum);
    }

    public partial class Products
    {
        /// <summary>Суммарное количество по всем размерам.</summary>
        public int TotalQuantity => (int)StockItems.Sum(s => s.Quantity);

        /// <summary>Строка «много» / «мало».</summary>
        public string QuantityText => TotalQuantity > 5 ? "много" : "мало";

        /// <summary>Есть ли скидка 25% (нет заказов за прошлый календарный месяц).</summary>
        public bool HasDiscount
        {
            get
            {
                DateTime from = DateTime.Now.AddMonths(-1);
                return !AppConnect.model0db.OrderItems.Any(oi =>
                    oi.StockItems.ProductId == Id &&
                    oi.Orders.OrderDate >= from &&
                    oi.Orders.OrderDate < DateTime.Now);
            }
        }

        /// <summary>Цена со скидкой.</summary>
        public decimal FinalPrice => HasDiscount
            ? (decimal)(Math.Round(Convert.ToDouble(Price * 0.75m), 2))
            : (decimal)Price;

        /// <summary>Цвет подсветки строки.</summary>
        public Brush RowColor => TotalQuantity <= 3
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff8080"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D2F6E7"));

        /// <summary>Путь к картинке (или заглушка).</summary>
        public string ImagePath => string.IsNullOrEmpty(Image)
            ? "/Images/picture.png"
            : $"/Images/{Image}";

        /// <summary>Размеры с положительным остатком.</summary>
        public List<StockItems> AvailableSizes =>
            StockItems.Where(s => s.Quantity > 0).ToList();
    }

    // ============================================================
    //  КОРЗИНА
    // ============================================================

    public class CartItem
    {
        public int StockItemId { get; set; }
        public string ProductName { get; set; }
        public string ManufacturerName { get; set; }
        public decimal SizeValue { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal Sum => Quantity * PricePerUnit;
    }

    public static class Cart
    {
        public static List<CartItem> Items { get; } = new List<CartItem>();

        public static decimal TotalSum()
        {
            decimal s = 0;
            foreach (var i in Items) s += i.Sum;
            return s;
        }

        public static void Clear() => Items.Clear();
    }
}


  