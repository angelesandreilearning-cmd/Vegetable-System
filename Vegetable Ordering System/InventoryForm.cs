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
    public partial class InventoryForm : Form
    {
        string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";

        private string currentUser;
        private string currentRole;

        private bool isSettingsMenuVisible = false;

        // Add this constructor to accept a username argument
        public InventoryForm(string username,string role)
        {
            InitializeComponent();
            currentUser = username;
            currentRole = role;
            lblCurrentUser.Text = $"Logged in as {username} ({role})";
            if (currentRole == "Merchant")
            {
                btnSettings.Visible = false;
                btnSuppliers.Visible = false;
            }
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            MainMenuForm adminForm = new MainMenuForm(currentUser, currentRole);
            adminForm.Show();
            this.Close();
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            OrderForm orderForm = new OrderForm(currentUser,currentRole);
            orderForm.Show();
            this.Close();
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            SupplierForm supplierForm = new SupplierForm(currentUser,currentRole);
            supplierForm.Show();
            this.Close();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            isSettingsMenuVisible = !isSettingsMenuVisible;
            panelSettings.Visible = isSettingsMenuVisible;


            panelSettings.Location = new Point(btnSettings.Left, btnSettings.Bottom);
            panelSettings.BringToFront();
        }
        private void HideSettingsMenu()
        {
            panelSettings.Visible = false;
            isSettingsMenuVisible = false;
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

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                LoadProducts();
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // REMOVED IsActive and Unit columns
                SqlCommand command = new SqlCommand(
                    "SELECT ProductID, ProductName, Category, Stock, Price FROM tbl_Products " +
                    "WHERE ProductName LIKE @Search OR Category LIKE @Search",
                    connection);
                command.Parameters.AddWithValue("@Search", "%" + searchText + "%");

                SqlDataReader reader = command.ExecuteReader();

                dgvInventory.Rows.Clear();
                while (reader.Read())
                {
                    dgvInventory.Rows.Add(
                        reader["ProductID"].ToString(),
                        reader["ProductName"].ToString(),
                        reader["Category"].ToString(),
                        reader["Stock"].ToString(),
                        $"₱{Convert.ToDecimal(reader["Price"]):N2}",
                        "kg", // Default unit value
                        "EDIT",
                        "DELETE"
                    );
                }
                connection.Close();
            }
            txtSearch.ForeColor = Color.Black;
        }

        private void panel2_Paint_1(object sender, PaintEventArgs e)
        {

        }
        private void RoundCorners(Button btn, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(new Rectangle(0, 0, radius, radius), 180, 90);
            path.AddLine(radius, 0, btn.Width - radius, 0);
            path.AddArc(new Rectangle(btn.Width - radius, 0, radius, radius), 270, 90);
            path.AddLine(btn.Width, radius, btn.Width, btn.Height - radius);
            path.AddArc(new Rectangle(btn.Width - radius, btn.Height - radius, radius, radius), 0, 90);
            path.AddLine(btn.Width - radius, btn.Height, radius, btn.Height);
            path.AddArc(new Rectangle(0, btn.Height - radius, radius, radius), 90, 90);
            path.CloseFigure();

            btn.Region = new Region(path);
        }

        private void InventoryForm_Load(object sender, EventArgs e)
        {
            timer1.Start();
            RoundCorners(btn_addstk, 20);
            LoadProducts();




        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
            lblDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        }
        public void LoadProducts()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // REMOVED IsActive and Unit columns since they don't exist
                SqlCommand command = new SqlCommand(
                    "SELECT ProductID, ProductName, Category, Stock, Price FROM tbl_Products",
                    connection);
                SqlDataReader reader = command.ExecuteReader();

                dgvInventory.Rows.Clear();
                while (reader.Read())
                {
                    dgvInventory.Rows.Add(
                        reader["ProductID"].ToString(),
                        reader["ProductName"].ToString(),
                        reader["Category"].ToString(),
                        reader["Stock"].ToString(),
                        $"₱{Convert.ToDecimal(reader["Price"]):N2}", // Format price properly
                        "kg", // Default unit value
                        "EDIT",
                        "DELETE"
                    );
                }
                connection.Close();
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {

        }

        private void txtSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
        }

        private void btn_addstk_Click(object sender, EventArgs e)
        {
            AddStock addStockForm = new AddStock(this);
            addStockForm.ShowDialog();
        }

        private void dgvInventory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == 6) // EDIT column
                {
                    // Get product data
                    string productId = dgvInventory.Rows[e.RowIndex].Cells[0].Value.ToString();
                    string productName = dgvInventory.Rows[e.RowIndex].Cells[1].Value.ToString();
                    string category = dgvInventory.Rows[e.RowIndex].Cells[2].Value.ToString();
                    string stock = dgvInventory.Rows[e.RowIndex].Cells[3].Value.ToString();
                    string price = dgvInventory.Rows[e.RowIndex].Cells[4].Value.ToString();
                    // Removed unit since it doesn't exist in database

                    // Open edit form (you'll need to create this)
                    // EditProductForm editForm = new EditProductForm(productId, productName, category, stock, price, this);
                    // editForm.ShowDialog();

                    MessageBox.Show($"Edit product: {productName}\nPrice: {price}\nStock: {stock}", "Edit Product");
                }
                else if (e.ColumnIndex == 7) // DELETE column
                {
                    string productId = dgvInventory.Rows[e.RowIndex].Cells[0].Value.ToString();
                    string productName = dgvInventory.Rows[e.RowIndex].Cells[1].Value.ToString();

                    // Update confirmation message for hard delete
                    if (MessageBox.Show($"Are you sure you want to PERMANENTLY delete {productName}?\n\nThis action cannot be undone.",
                        "Confirm Permanent Delete",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        DeleteProduct(Convert.ToInt32(productId));
                    }
                }
            }
        }
        private void DeleteProduct(int productId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // CHANGED TO HARD DELETE (PERMANENT)
                    SqlCommand command = new SqlCommand(
                        "DELETE FROM tbl_Products WHERE ProductID = @ProductID",
                        connection);
                    command.Parameters.AddWithValue("@ProductID", productId);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Product deleted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadProducts(); // Refresh the list
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting product: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
            txtSearch.Clear();
        }
    }

}
