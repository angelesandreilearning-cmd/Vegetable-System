using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Data.SqlClient;
using System.IO;
namespace Vegetable_Ordering_System
{
    public partial class OrderForm : Form
    {


        private string currentUser;
        private string currentRole;
        string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";
        private bool isSettingsMenuVisible = false;
        private DataTable orderItems = new DataTable();


        public OrderForm(string username, string role)
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

            errorProvider1.BlinkStyle = ErrorBlinkStyle.AlwaysBlink;
            errorProvider1.BlinkRate = 500;
        }

      

        private void btnInventory_Click(object sender, EventArgs e)
        {
            InventoryForm inventoryForm = new InventoryForm(currentUser,currentRole);
            inventoryForm.Show();
            this.Close();
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            SupplierForm supplierForm = new SupplierForm(currentUser, currentRole);
            supplierForm.Show();
            this.Close();
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            MainMenuForm adminForm = new MainMenuForm(currentUser, currentRole);
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
        private void LoadProductsToFlowLayout()
        {
            flowLayoutPanel2.Controls.Clear();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT ProductID, ProductName, Price, Stock, ImagePath FROM tbl_Products";

                using (SqlCommand cmd = new SqlCommand(query, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ProductItemControl productControl = new ProductItemControl();
                        productControl.ProductID = Convert.ToInt32(reader["ProductID"]);
                        productControl.ProductName = reader["ProductName"].ToString();
                        productControl.Price = Convert.ToDecimal(reader["Price"]);
                        productControl.Stock = Convert.ToInt32(reader["Stock"]);

                        // Load product image
                        if (!reader.IsDBNull(reader.GetOrdinal("ImagePath")))
                        {
                            string imageFileName = reader["ImagePath"].ToString();
                            productControl.ProductImage = LoadProductImage(imageFileName);
                        }
                        else
                        {
                            productControl.ProductImage = GetDefaultProductImage();
                        }

                        // Set Admin mode
                        productControl.SetAdminMode(currentRole == "Admin");

                        // Handle Edit event
                        productControl.EditPriceClicked += (sender, productId) =>
                        {
                            ShowEditPriceDialog(productId);
                        };

                        // Handle Delete event  
                        productControl.DeleteProductClicked += (sender, productId) =>
                        {
                            DeleteProduct(productId);
                        };

                        // Handle product image click event
                        productControl.ProductImageClicked += (sender, e) =>
                        {
                            AddProductToOrder(e.ProductID, e.Name, e.Price, e.Quantity);
                        };

                        flowLayoutPanel2.Controls.Add(productControl);
                    }
                }
            }
        }
        private void AddProductToOrder(int productId, string productName, decimal price, int quantity)
        {
            // Initialize order items table if not exists
            if (orderItems.Columns.Count == 0)
            {
                orderItems.Columns.Add("Product", typeof(string));
                orderItems.Columns.Add("Qty", typeof(string));
                orderItems.Columns.Add("Unit Price", typeof(string));
                orderItems.Columns.Add("Total", typeof(string));
            }

            // Check if product already exists in order
            DataRow[] existingRows = orderItems.Select($"Product = '{productName.Replace("'", "''")}'");

            if (existingRows.Length > 0)
            {
                // Update existing item quantity
                DataRow row = existingRows[0];
                int currentQty = int.Parse(row["Qty"].ToString().Replace(" kg", ""));
                int newQty = currentQty + quantity;

                row["Qty"] = $"{newQty} kg";
                row["Total"] = $"₱{(newQty * price):N2}";
            }
            else
            {
                // Add new item to order
                DataRow newRow = orderItems.NewRow();
                newRow["Product"] = productName;
                newRow["Qty"] = $"{quantity} kg";
                newRow["Unit Price"] = $"₱{price:N2}/kg";
                newRow["Total"] = $"₱{(price * quantity):N2}";
                orderItems.Rows.Add(newRow);
            }

            // Refresh DataGridView
            dataGridViewOrderSummary.DataSource = null;
            dataGridViewOrderSummary.AutoGenerateColumns = true;
            dataGridViewOrderSummary.DataSource = orderItems;

            // Update total
            UpdateOrderTotal();
        }

        // Method to update order total
        private void UpdateOrderTotal()
        {
            decimal total = 0;
            foreach (DataRow row in orderItems.Rows)
            {
                // Remove ₱ symbol and parse the total
                string totalText = row["Total"].ToString().Replace("₱", "").Trim();
                if (decimal.TryParse(totalText, out decimal itemTotal))
                {
                    total += itemTotal;
                }
            }
            lblTotal.Text = $"Total: ₱{total:N2}";
        }
        private Image LoadProductImage(string imageFileName)
        {
            try
            {
                if (string.IsNullOrEmpty(imageFileName))
                    return GetDefaultProductImage();

                string imagesFolder = Path.Combine(Application.StartupPath, "ProductImages");
                string fullImagePath = Path.Combine(imagesFolder, imageFileName);

                if (File.Exists(fullImagePath))
                {
                    return Image.FromFile(fullImagePath);
                }
                else
                {
                    return GetDefaultProductImage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return GetDefaultProductImage();
            }
        }
            
        private void DeleteProduct(int productId)
        {

            string productName = "";

            // Get product name for confirmation message
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT ProductName FROM tbl_Products WHERE ProductID = @ProductID";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productId);
                    productName = cmd.ExecuteScalar()?.ToString() ?? "Unknown Product";
                }
            }

            // Confirmation dialog
            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete '{productName}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        string query = "DELETE FROM tbl_Products WHERE ProductID = @ProductID";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@ProductID", productId);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Product deleted successfully!", "Success",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Refresh the product list
                                LoadProductsToFlowLayout();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting product: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowEditPriceDialog(int productId)
        {
            string productName = "";
            decimal currentPrice = 0;

            // Get current product details from database
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT ProductName, Price FROM tbl_Products WHERE ProductID = @ProductID";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            productName = reader["ProductName"].ToString();
                            currentPrice = Convert.ToDecimal(reader["Price"]);
                        }
                    }
                }
            }

            // Show your custom form
            EditPriceForm editForm = new EditPriceForm();
            editForm.ProductId = productId;
            editForm.LoadProductData(productName, currentPrice); // Pass data to form

            if (editForm.ShowDialog() == DialogResult.OK)
            {
                // Get the new price from the form and update database
                decimal newPrice = editForm.NewPrice;
                UpdateProductPrice(productId, newPrice, productName);
            }
        }


        private Image GetDefaultProductImage()
        {
            // Create a default placeholder image
            Bitmap bmp = new Bitmap(160, 140);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.LightGray);
                using (Font font = new Font("Arial", 10))
                using (StringFormat sf = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    g.DrawString("No Image Available", font, Brushes.DarkGray,
                        new Rectangle(0, 0, 160, 140), sf);
                }
            }
            return bmp;
        }

        private void UpdateProductPrice(int productId, decimal newPrice, string productName)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "UPDATE tbl_Products SET Price = @Price WHERE ProductID = @ProductID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Price", newPrice);
                        cmd.Parameters.AddWithValue("@ProductID", productId);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show($"Price updated successfully!\n\nProduct: {productName}\nNew Price: ₱{newPrice:N2}",
                                "Success",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            // Refresh the product list
                            LoadProductsToFlowLayout();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating price: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            LoadProductsToFlowLayout();

            flowLayoutPanel2.FlowDirection = FlowDirection.LeftToRight;

            RoundCorners(btnDraft, 20);
              RoundCorners(btnBarcode, 20);
            RoundCorners(btnPlaceOrder, 20);
            RoundPanel(panel3, 20);
     
            RoundPanel(flowLayoutPanel2, 20);
            
            timer1.Start();

           
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
        private void RoundPanel(Panel panel, int cornerRadius)
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(new Rectangle(0, 0, cornerRadius, cornerRadius), 180, 90);
            path.AddArc(new Rectangle(panel.Width - cornerRadius, 0, cornerRadius, cornerRadius), 270, 90);
            path.AddArc(new Rectangle(panel.Width - cornerRadius, panel.Height - cornerRadius, cornerRadius, cornerRadius), 0, 90);
            path.AddArc(new Rectangle(0, panel.Height - cornerRadius, cornerRadius, cornerRadius), 90, 90);
            path.CloseFigure();

            panel.Region = new Region(path);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt"); 
            lblDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        }

        private void txtCustomer_TextChanged(object sender, EventArgs e)
        {
            // Only set/clear the error here — do not mutate Text (mutation will immediately clear the error)
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
            // Allow letters, control keys (backspace), and space
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
            {
                e.Handled = true;
                // optional: show a short error while typing (not required)
                errorProvider1.SetError(txtCustomer, "Only letters and spaces allowed.");
            }
            else
            {
                errorProvider1.SetError(txtCustomer, "");
            }
        }

        private void txtCustomer_Validating(object sender, CancelEventArgs e)
        {
            // final validation when leaving the control
            Regex regex = new Regex(@"^[a-zA-Z\s]+$"); // require at least one char if you want
            if (!regex.IsMatch(txtCustomer.Text))
            {
                errorProvider1.SetError(txtCustomer, "Please enter only letters and spaces.");
            }
            else
            {
                errorProvider1.SetError(txtCustomer, "");
            }
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
           
        }
      

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            NewProduct addForm = new NewProduct();
            addForm.FormClosed += (s, args) => LoadProductsToFlowLayout();
            addForm.ShowDialog();


        }

        private void lblDate_Click(object sender, EventArgs e)
        {

        }

        private void dataGridViewOrderSummary_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
                
        }
    }

}
