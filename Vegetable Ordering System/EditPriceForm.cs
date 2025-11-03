using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vegetable_Ordering_System
{
    public partial class EditPriceForm : Form
    {
        public int ProductId { get; set; }
        public decimal NewPrice => nudNewPrice.Value;

        public EditPriceForm()
        {
            InitializeComponent();
        }

        private void EditPriceForm_Load(object sender, EventArgs e)
        {
            nudNewPrice.Focus();
            nudNewPrice.Select(0, nudNewPrice.Text.Length);
        }
        public void LoadProductData(string productName, decimal currentPrice)
        {
            lblProductName.Text = productName;
            lblPrice.Text = $"₱{currentPrice:N2}";
            nudNewPrice.Value = currentPrice;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
