using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vegetable_Ordering_System
{
    public partial class OrderForm : Form
    {
        private bool isSettingsMenuVisible = false;
        public OrderForm()
        {
            InitializeComponent();
            errorProvider1.BlinkStyle = ErrorBlinkStyle.AlwaysBlink;
            errorProvider1.BlinkRate = 500;
        }

        private void btnInventory_Click(object sender, EventArgs e)
        {
            InventoryForm inventoryForm = new InventoryForm();
            inventoryForm.Show();
            this.Close();
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            SupplierForm supplierForm = new SupplierForm();
            supplierForm.Show();
            this.Close();
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            MainMenuForm adminForm = new MainMenuForm("admin");
            adminForm.Show();
            this.Close();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            isSettingsMenuVisible = !isSettingsMenuVisible;
            panelSettings.Visible = isSettingsMenuVisible;
            panelSettings.Location = new Point(btnSettings.Left, btnSettings.Bottom);
            panelSettings.BringToFront();
        }

        private void btnGeneral_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Open General Settings");
            HideSettingsMenu();
        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Open User Management");
            HideSettingsMenu();
        }

        private void btnSystem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Open System Settings");
            HideSettingsMenu();
        }

        private void btnSecurity_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Open Security Settings");
            HideSettingsMenu();
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Open Backup and Restore");
            HideSettingsMenu();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Vegetable Ordering System\nVersion 1.0\n© 2024 Vegetable Corp");
            HideSettingsMenu();
        }
        private void HideSettingsMenu()
        {
            panelSettings.Visible = false;  
            isSettingsMenuVisible = false;
        }

        private void OrderForm_Load(object sender, EventArgs e)
        {

        }

        private void txtCustomer_TextChanged(object sender, EventArgs e)
        {
            Regex regex = new Regex(@"^[a-zA-Z\s]*$");
            if (!regex.IsMatch(txtCustomer.Text))
            {
                errorProvider1.SetError(txtCustomer, "Please enter only letters and spaces.");
            }   
            else
            {
                errorProvider1.SetError(txtCustomer, "");
            }
        }

        private void txtCustomer_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
            {
                e.Handled = true;
                errorProvider1.SetError(txtCustomer, "Only letters and spaces allowed.");
            }
            else
            {
                errorProvider1.SetError(txtCustomer, "");
            }
        }

        private void txtCustomer_Validating(object sender, CancelEventArgs e)
        {
            Regex regex = new Regex(@"^[a-zA-Z\s]+$");
            if (!regex.IsMatch(txtCustomer.Text))
            {
                errorProvider1.SetError(txtCustomer, "Please enter only letters and spaces.");
            }
            else
            {
                errorProvider1.SetError(txtCustomer, "");
            }
        }

        private void txtContact_TextChanged(object sender, EventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]*$");
            if (!regex.IsMatch(txtContact.Text))
            {   
                errorProvider1.SetError(txtContact, "Please enter only numbers.");
            }
            else
            {
                errorProvider1.SetError(txtContact, "");
            }
        }

        private void txtContact_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                errorProvider1.SetError(txtContact, "Only numbers allowed.");
            }
            else
            {
                errorProvider1.SetError(txtContact, "");
            }
        }

        private void txtContact_Validating(object sender, CancelEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]+$");
            if (!regex.IsMatch(txtContact.Text))
            {
                errorProvider1.SetError(txtContact, "Please enter only numbers.");
            }
            else
            {
                errorProvider1.SetError(txtContact, "");
            }
        }

        private void txtAddress_TextChanged(object sender, EventArgs e)
        {

        }

        private void dateTimeOrder_ValueChanged(object sender, EventArgs e)
        {

        }

        private void cmbDelivery_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnDeliveryFee_TextChanged(object sender, EventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]*\.?[0-9]{0,2}$");
            if (!regex.IsMatch(btnDeliveryFee.Text))
            {
                errorProvider1.SetError(btnDeliveryFee, "Please enter a valid amount (up to 2 decimal places).");
                btnDeliveryFee.Text = Regex.Replace(btnDeliveryFee.Text, @"[^0-9\.]", "");
                btnDeliveryFee.SelectionStart = btnDeliveryFee.Text.Length;
            }
            else
            {
                errorProvider1.SetError(btnDeliveryFee, "");
            }
        }

        private void txtTotal_TextChanged(object sender, EventArgs e)
        {
           if(dataGridView1.Rows.Count > 0)
            {
                decimal total = 0;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["Amount"].Value != null)
                    {
                        total += Convert.ToDecimal(row.Cells["Amount"].Value);
                    }
                }
                txtTotal.Text = total.ToString("F2");
            }

        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            TakeOrder takeOrder = new TakeOrder();
            takeOrder.ShowDialog();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {

        }
    }

}
