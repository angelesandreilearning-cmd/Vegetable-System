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
        private string currentUser;
        private string currentRole;
        private bool isSettingsMenuVisible = false;


        public SupplierForm(string username, string role)
        {
            InitializeComponent();
            currentUser = username;
            currentRole = role;
            LoadSuppliers();
            lblCurrentUser.Text = $"Logged in as {username} ({role})";
            if (currentRole == "Merchant")
            {
                btnSettings.Visible = false;
                btnSuppliers.Visible = false;
            }
        }

        private void btnInventory_Click(object sender, EventArgs e)
        {
            InventoryForm inventoryForm = new InventoryForm(currentUser,currentRole);
            inventoryForm.Show();
            this.Close();
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            OrderForm orderForm = new OrderForm(currentUser, currentRole);
            orderForm.Show();
            this.Close();
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            MainMenuForm mainMenu = new MainMenuForm(currentUser, currentRole);
            mainMenu.Show();
            this.Close();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {

            isSettingsMenuVisible = !isSettingsMenuVisible;
            panelSettings.Visible = isSettingsMenuVisible;
            panelSettings.Location = new Point(btnSettings.Left, btnSettings.Bottom);
            panelSettings.BringToFront();
        }

        private void panelSettings_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnGeneral_Click(object sender, EventArgs e)
        {

            Registration registrationForm = new Registration();
            registrationForm.ShowDialog();
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

         
        }

        private void txtSearch_Click(object sender, EventArgs e)
        {
            txtSearch.ForeColor = Color.Black;
            txtSearch.Clear();
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
            if (txtSearch.Text == "Search")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
            string searchText = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                LoadSuppliers();
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT SupplierID, SupplierName, Contact, Address, Email, Phone 
                        FROM tbl_Suppliers 
                        WHERE SupplierName LIKE @Search OR Contact LIKE @Search OR Email LIKE @Search";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Search", "%" + searchText + "%");

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
                        "EDIT", "DELETE"
                    );
                }
                connection.Close();
            }
        }

    

        private void btnAddSupplier_Click_1(object sender, EventArgs e)
        {
            AddSupplier addSupplierForm = new AddSupplier(this);
            addSupplierForm.ShowDialog();
        }
        private void deleteSupplier(int supplierID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand command = new SqlCommand("DELETE FROM tbl_Suppliers WHERE SupplierID = @SupplierID", conn);
                    command.Parameters.AddWithValue("@SupplierID", supplierID);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Supplier successfully deleted!", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadSuppliers();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting supplier: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgv_supplier_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            switch (e.ColumnIndex)
            {
                case 6:
                    EditSupplier(e.RowIndex);
                    break;

                case 7:
                    if (MessageBox.Show("Are you sure you want to delete this supplier?",
                        "DELETE RECORD", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        int supplierID = Convert.ToInt32(dgv_supplier.Rows[e.RowIndex].Cells[0].Value.ToString());
                        deleteSupplier(supplierID);
                    }
                    break;
            }
        }
        private void EditSupplier(int rowIndex)
        {
            int supplierID = Convert.ToInt32(dgv_supplier.Rows[rowIndex].Cells[0].Value);
            string name = dgv_supplier.Rows[rowIndex].Cells[1].Value.ToString();
            MessageBox.Show($"Edit supplier: {name}", "Edit Supplier");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
            lblDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        }

        private void SupplierForm_Load_1(object sender, EventArgs e)
        {
            RoundCorners(btnAddSupplier, 20);
            txtSearch.BorderStyle = BorderStyle.None;
            RoundCorners(txtSearch, 15);
            timer1.Start();

        }

        private void label8_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.Hide();
                Log_In loginForm = new Log_In();
                loginForm.Show();
                this.Close();
            }
        }

        private void lblDate_Click(object sender, EventArgs e)
        {

        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
         
        }

        private void txtSearch_Click_1(object sender, EventArgs e)
        {
            txtSearch.ForeColor = Color.Black;
            txtSearch.Clear();
        }
    }
}