using System;
using System.Drawing;
using System.Windows.Forms;

namespace Vegetable_Ordering_System
{
    public partial class ProductItemControl : UserControl
    {
        public int ProductID { get; set; }
        private decimal _price;

        public string ProductName
        {
            get => lblName.Text;
            set => lblName.Text = value;
        }

        public decimal Price
        {
            get => _price; // Return stored price
            set
            {
                _price = value;
                lblPrice.Text = $"₱{value:N2} / kg";
            }
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

        public Image ProductImage
        {
            get => pictureBox1.Image;
            set => pictureBox1.Image = value ?? GetDefaultImage();
        }

        // EVENTS
        public event EventHandler<ProductEventArgs> AddToOrderClicked;
        public event EventHandler<int> EditPriceClicked;
        public event EventHandler<int> DeleteProductClicked;
        public event EventHandler<ProductEventArgs> ProductImageClicked;
        public ProductItemControl()
        {
            InitializeComponent();
         
        }

        private Image GetDefaultImage()
        {
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.LightGray);
                using (Font font = new Font("Arial", 8))
                using (StringFormat sf = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    g.DrawString("No Image", font, Brushes.DarkGray,
                        new Rectangle(0, 0, bmp.Width, bmp.Height), sf);
                }
            }
            return bmp;
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            AddToOrderClicked?.Invoke(this, new ProductEventArgs(ProductID, ProductName, Price, Quantity));
        }

        // NEW METHODS
        private void OnEditPriceClicked()
        {
            EditPriceClicked?.Invoke(this, ProductID);
        }

        private void OnDeleteProductClicked()
        {
            DeleteProductClicked?.Invoke(this, ProductID);
        }

        public void SetAdminMode(bool isAdmin)
        {
            lblEditPrice.Visible = isAdmin;
            label1.Visible = isAdmin;
        }

        public void LoadProduct(string name, decimal price, int stock, Image image)
        {
            lblName.Text = name;
            Price = price;
            lblStock.Text = $"Stock: {stock} kg";
            ProductImage = image;
        }

        private void lblEditPrice_Click(object sender, EventArgs e)
        {
            OnEditPriceClicked();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            OnDeleteProductClicked();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            ProductImageClicked?.Invoke(this, new ProductEventArgs(ProductID, ProductName, Price, Quantity));

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
