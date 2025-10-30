using System;
using System.Drawing;
using System.Windows.Forms;

namespace Vegetable_Ordering_System
{
    public partial class ProductItemControl : UserControl
    {
        public int ProductID { get; set; }

        public string ProductName
        {
            get => lblName.Text;
            set => lblName.Text = value;
        }

        public decimal Price
        {
            get
            {
                string text = lblPrice.Text.Replace("₱", "").Replace("/kg", "").Trim();
                return decimal.TryParse(text, out var result) ? result : 0;
            }
            set => lblPrice.Text = $"₱{value:N2} / kg";
        }

        public int Stock
        {
            get
            {
                string text = lblStock.Text.Replace("Stock:", "").Replace("kg", "").Trim();
                return int.TryParse(text, out var result) ? result : 0;
            }
            set => lblStock.Text = $"Stock: {value} kg";
        }

        public int Quantity
        {
            get => (int)numericUpDown1.Value;
            set => numericUpDown1.Value = value;
        }

        public event EventHandler<ProductEventArgs> AddToOrderClicked;

        public ProductItemControl()
        {
            InitializeComponent();
            button1.Click += Button1_Click;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            AddToOrderClicked?.Invoke(this, new ProductEventArgs(ProductID, ProductName, Price, Quantity));
        }

        public void LoadProduct(string name, decimal price, int stock, Image image)
        {
            lblName.Text = name;
            lblPrice.Text = $"₱{price:N2} / kg";
            lblStock.Text = $"Stock: {stock} kg";
        
        }
    }

    public class ProductEventArgs : EventArgs
    {
        public int ProductID { get; }
        public string Name { get; }
        public decimal Price { get; }
        public int Quantity { get; }

        public ProductEventArgs(int id, string name, decimal price, int qty)
        {
            ProductID = id;
            Name = name;
            Price = price;
            Quantity = qty;
        }
    }
}
