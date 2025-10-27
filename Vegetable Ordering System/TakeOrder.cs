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
    public partial class TakeOrder : Form
    {
        public TakeOrder()
        {
            InitializeComponent();
        }

        private void TakeOrder_Load(object sender, EventArgs e)
        {

        }

        private void btnCarrots_Click(object sender, EventArgs e)
        {
            listBoxOrder.Items.Add("Carrots");
        }

        private void btnRepolio_Click(object sender, EventArgs e)
        {

            listBoxOrder.Items.Add("Repolio");
        }

        private void btnPatatas_Click(object sender, EventArgs e)
        {

            listBoxOrder.Items.Add("Patatas");
        }

        private void btnSayote_Click(object sender, EventArgs e)
        {

            listBoxOrder.Items.Add("Sayote");
        }

        private void btnSilingPula_Click(object sender, EventArgs e)
        {

            listBoxOrder.Items.Add("Siling Pula");
        }

        private void btnSibuyas_Click(object sender, EventArgs e)
        {

            listBoxOrder.Items.Add("Sibuyas");
        }

        private void btnKamatis_Click(object sender, EventArgs e)
        {

            listBoxOrder.Items.Add("Kamatis");
        }

        private void btnBawang_Click(object sender, EventArgs e)
        {

            listBoxOrder.Items.Add("Bawang");
        }

        private void btnLuya_Click(object sender, EventArgs e)
        {

            listBoxOrder.Items.Add("Luya");
        }

        private void btnTalong_Click(object sender, EventArgs e)
        {

            listBoxOrder.Items.Add("Talong");
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listBoxOrder.SelectedItem != null)
            {
                listBoxOrder.Items.Remove(listBoxOrder.SelectedItem);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            OrderForm orderForm = new OrderForm();
            orderForm.Show();
            this.Close();
        }

        private void btnQty1_Click(object sender, EventArgs e)
        {

        }
    }  
}
