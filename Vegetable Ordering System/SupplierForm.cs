using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vegetable_Ordering_System
{
    public partial class SupplierForm : Form
    {
       string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";  
        private string _username;
        private bool isSettingsMenuVisible = false;
     

        public SupplierForm(string username)
        {
            InitializeComponent();
            _username = username;
            LoadSuppliers();
        }

        private void btnInventory_Click(object sender, EventArgs e)
        {
            InventoryForm inventoryForm = new InventoryForm();
            inventoryForm.Show();
            this.Close();
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            OrderForm orderForm = new OrderForm();
            orderForm.Show();
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
        }

        private void panelSettings_Paint(object sender, PaintEventArgs e)
        {

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

            MessageBox.Show("Vegetable Ordering System\nVersion 1.0\nÂ© 2024 Vegetable Corp");
            HideSettingsMenu();
        }
        private void HideSettingsMenu()
        {
            panelSettings.Visible = false;
            isSettingsMenuVisible = false;
        }
        private void RoundCorners(Control ctrl, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(new Rectangle(0, 0, radius, radius), 180, 90);
            path.AddLine(radius, 0, ctrl.Width - radius, 0);
            path.AddArc(new Rectangle(ctrl.Width - radius, 0, radius, radius), 270, 90);
            path.AddLine(ctrl.Width, radius, ctrl.Width, ctrl.Height - radius);
            path.AddArc(new Rectangle(ctrl.Width - radius, ctrl.Height - radius, radius, radius), 0, 90);
            path.AddLine(ctrl.Width - radius, ctrl.Height, radius, ctrl.Height);
            path.AddArc(new Rectangle(0, ctrl.Height - radius, radius, radius), 90, 90);
            path.CloseFigure();

            ctrl.Region = new Region(path);
        }


        private void btnAddSupplier_Click(object sender, EventArgs e)
        {
            AddSupplier supplier = new AddSupplier(this);
            supplier.Show();
            this.Hide();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void SupplierForm_Load(object sender, EventArgs e)
        {
            RoundCorners(btnAddSupplier, 20);
            txtSearch.BorderStyle = BorderStyle.None;
            RoundCorners(txtSearch, 15);

            if (_username == "merchant")
            {
                btnSettings.Visible = false;
                btnSuppliers.Visible = false;
            }
        }

        private void txtSearch_Click(object sender, EventArgs e)
        {
            txtSearch.ForeColor = Color.Black;
            txtSearch.Clear();
        }

        private void SupplierForm_Load_1(object sender, EventArgs e)
        {
            timer1.Start();
            if (_username == "admin")
            {
                lblCurrentUser.Text = "Logged in as Admin";
            }
            else if (_username == "merchant")
            {
                lblCurrentUser.Text = "Logged in as Merchant";
            }
            else
            {
                lblCurrentUser.Text = $"Logged in as {_username}";
            }
        }

        public void LoadSuppliers()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open(); 

            SqlCommand command = new SqlCommand("SELECT SupplierID, SupplierName, Contact, Address, Email,Phone  from tbl_Suppliers", connection); 
            SqlDataReader reader = command.ExecuteReader(); 

            dgv_supplier.Rows.Clear(); 
            while (reader.Read())
            {
                dgv_supplier.Rows.Add(
                    reader["SupplierID"].ToString(),
                    reader["SupplierName"].ToString(),
                    reader["Contact"].ToString(),
                    reader["Address"].ToString(),
                    reader["Email"].ToString(),
                    reader["Phone"].ToString(),
                    "EDIT","DELETE"

                );
            } 

            connection.Close();

        }   
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {

        }

    

        private void btnAddSupplier_Click_1(object sender, EventArgs e)
        {
            AddSupplier addSupplierForm = new AddSupplier(this);
            addSupplierForm.ShowDialog();
        }
        private void deleteSupplier(int supplierID)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            SqlCommand command = new SqlCommand("DELETE FROM tbl_Suppliers WHERE SupplierID = @SupplierID", conn);
            command.Parameters.AddWithValue("@SupplierID", supplierID);
            command.ExecuteNonQuery();

            MessageBox.Show("Supplier successfully deleted!", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            conn.Close();

            LoadSuppliers(); 
        }

        private void dgv_supplier_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 7:
                    if (MessageBox.Show(
                        "Are you sure you want to delete this supplier?",
                        "DELETE RECORD",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    ) == DialogResult.Yes)
                    {
                        int supplierID = Convert.ToInt32(dgv_supplier.Rows[e.RowIndex].Cells[0].Value.ToString());
                        deleteSupplier(supplierID);
                    }
                    break;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
            lblDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        }
    }
}