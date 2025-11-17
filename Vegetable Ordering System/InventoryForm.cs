using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;

namespace Vegetable_Ordering_System
{
    public partial class InventoryForm : Form
    {
        string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";

        private string currentUser;
        private string currentRole;
        private bool isSettingsMenuVisible = false;
        private List<Product> productsForPrinting;

        public InventoryForm(string username, string role)
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

        public class Product
        {
            public string ProductID { get; set; }
            public string ProductName { get; set; }
            public decimal Price { get; set; }
            public DateTime DateDelivered { get; set; }
            public string Barcode { get; set; }
        }

        private void btnPrintBarcodes_Click(object sender, EventArgs e)
        {
            PrintAllBarcodes();
        }

        private void PrintAllBarcodes()
        {
            try
            {
                productsForPrinting = new List<Product>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(
                        "SELECT ProductID, ProductName, Price, DateDelivered, Barcode FROM tbl_Products WHERE Stock > 0",
                        connection);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productsForPrinting.Add(new Product
                            {
                                ProductID = reader["ProductID"].ToString(),
                                ProductName = reader["ProductName"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                DateDelivered = reader["DateDelivered"] == DBNull.Value ?
                                              DateTime.Now :
                                              Convert.ToDateTime(reader["DateDelivered"]),
                                Barcode = reader["Barcode"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }

                if (productsForPrinting.Count == 0)
                {
                    MessageBox.Show("No products found to print barcodes.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                PrintDocument pd = new PrintDocument();
                pd.PrintPage += new PrintPageEventHandler(PrintBarcodesPage);

                PrintPreviewDialog preview = new PrintPreviewDialog();
                preview.Document = pd;
                preview.WindowState = FormWindowState.Maximized;
                preview.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing barcodes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void PrintBarcodesPage(object sender, PrintPageEventArgs e)
        {
            if (productsForPrinting == null || productsForPrinting.Count == 0)
                return;

            Graphics g = e.Graphics;
            Font titleFont = new Font("Arial", 16, FontStyle.Bold);
            Font productFont = new Font("Arial", 9, FontStyle.Bold);
            Font priceFont = new Font("Arial", 10, FontStyle.Bold);
            Font barcodeNumberFont = new Font("Arial", 7);
            Font dateFont = new Font("Arial", 7);

            float leftMargin = e.MarginBounds.Left;
            float topMargin = e.MarginBounds.Top;
            float y = topMargin;

            float labelWidth = 180f;
            float labelHeight = 110f;
            int columns = 3;
            float horizontalSpacing = 20f;
            float verticalSpacing = 20f;

            float pageWidth = e.MarginBounds.Width;
            float totalLabelsWidth = (columns * labelWidth) + ((columns - 1) * horizontalSpacing);
            leftMargin = (pageWidth - totalLabelsWidth) / 2;

            string title = "PRODUCT BARCODES";
            SizeF titleSize = g.MeasureString(title, titleFont);
            g.DrawString(title, titleFont, Brushes.Black, (pageWidth - titleSize.Width) / 2, y);
            y += titleSize.Height + 30;

            int currentProduct = 0;
            float startY = y;

            foreach (var product in productsForPrinting)
            {
                int row = currentProduct / columns;
                int col = currentProduct % columns;

                float x = leftMargin + (col * (labelWidth + horizontalSpacing));
                y = startY + (row * (labelHeight + verticalSpacing));

                if (y + labelHeight > e.MarginBounds.Bottom)
                {
                    e.HasMorePages = true;
                    return;
                }

                g.DrawRectangle(Pens.Black, x, y, labelWidth, labelHeight);

                string barcodeText = GenerateBarcode(product);

                Bitmap barcodeImage = GenerateBarcodeImage(barcodeText, 150, 40);

                string displayName = product.ProductName.Length > 20 ?
                    product.ProductName.Substring(0, 20) + "..." : product.ProductName;
                SizeF nameSize = g.MeasureString(displayName, productFont);
                g.DrawString(displayName, productFont, Brushes.Black,
                    x + (labelWidth - nameSize.Width) / 2, y + 5);

                string priceText = $"₱{product.Price:N2}";
                SizeF priceSize = g.MeasureString(priceText, priceFont);
                g.DrawString(priceText, priceFont, Brushes.Black,
                    x + (labelWidth - priceSize.Width) / 2, y + 25);

                string dateText = $"Delivered: {product.DateDelivered:MMM dd, yyyy}";
                SizeF dateSize = g.MeasureString(dateText, dateFont);
                g.DrawString(dateText, dateFont, Brushes.Black,
                    x + (labelWidth - dateSize.Width) / 2, y + 40);

                if (barcodeImage != null)
                {
                    g.DrawImage(barcodeImage, x + (labelWidth - barcodeImage.Width) / 2, y + 55);
                    barcodeImage.Dispose();
                }

                SizeF barcodeSize = g.MeasureString(barcodeText, barcodeNumberFont);
                g.DrawString(barcodeText, barcodeNumberFont, Brushes.Black,
                    x + (labelWidth - barcodeSize.Width) / 2, y + 95);

                currentProduct++;
            }

            e.HasMorePages = false;
        }

        private string GenerateBarcode(Product product)
        {
            // Use the barcode from database if it exists and is valid
            if (!string.IsNullOrEmpty(product.Barcode) && !string.IsNullOrWhiteSpace(product.Barcode))
            {
                string dbBarcode = product.Barcode.Trim();
                if (dbBarcode.Length > 0)
                {
                    return dbBarcode;
                }
            }

            // Fallback: Use ProductID if barcode is missing
            if (!string.IsNullOrEmpty(product.ProductID))
            {
                return $"PROD{product.ProductID.PadLeft(6, '0')}";
            }

            // Last resort: Use date
            return product.DateDelivered.ToString("yyyyMMdd");
        }

        private Bitmap GenerateBarcodeImage(string barcodeText, int width, int height)
        {
            try
            {
                var barcodeWriter = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Height = height,
                        Width = width,
                        Margin = 5,
                        PureBarcode = false
                    }
                };

                Bitmap barcode = barcodeWriter.Write(barcodeText);

                if (barcode == null || barcode.Width == 0 || barcode.Height == 0)
                {
                    throw new Exception("Barcode generation returned empty image");
                }

                return barcode;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Barcode generation error: {ex.Message}\nUsing fallback barcode.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return CreateReliableBarcode(barcodeText, width, height);
            }
        }

        private Bitmap CreateReliableBarcode(string barcodeText, int width, int height)
        {
            try
            {
                var barcodeWriter = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_39,
                    Options = new EncodingOptions
                    {
                        Height = height,
                        Width = width,
                        Margin = 5,
                        PureBarcode = false
                    }
                };

                string code39Text = $"*{barcodeText}*";
                return barcodeWriter.Write(code39Text);
            }
            catch
            {
                return CreateSimpleNumericBarcode(barcodeText, width, height);
            }
        }

        private Bitmap CreateSimpleNumericBarcode(string barcodeText, int width, int height)
        {
            string numericOnly = new string(barcodeText.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(numericOnly))
                numericOnly = "123456";

            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = height,
                    Width = width,
                    Margin = 5,
                    PureBarcode = false
                }
            };

            return barcodeWriter.Write(numericOnly.PadLeft(6, '0'));
        }
                                                                                                                                        
        private void btnDashboard_Click(object sender, EventArgs e)
        {
            MainMenuForm adminForm = new MainMenuForm(currentUser, currentRole);
            adminForm.Show();
            this.Close();
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            OrderForm orderForm = new OrderForm(currentUser, currentRole);
            orderForm.Show();
            this.Close();
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            SupplierForm supplierForm = new SupplierForm(currentUser, currentRole);
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
            MessageBox.Show("Vegetable Ordering System\nVersion 1.0\n© 2024 Vegetable Corp");
            HideSettingsMenu();
        }

        private void panel2_Paint(object sender, PaintEventArgs e) { }

        private void label17_Click(object sender, EventArgs e) { }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (txtSearch.Text == "Search")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
            if (string.IsNullOrEmpty(searchText))
            {
                LoadProducts();
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(
                    "SELECT ProductID, ProductName, Category, Stock, Price, DateDelivered, Supplier FROM tbl_Products " +
                    "WHERE ProductName LIKE @Search OR Category LIKE @Search OR Supplier LIKE @Search",
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
                        "kg",
                        reader["DateDelivered"] == DBNull.Value ? "" : Convert.ToDateTime(reader["DateDelivered"]).ToString("MM/dd/yyyy"),
                        reader["Supplier"]?.ToString() ?? "",
                        "EDIT",
                        "DELETE"
                    );
                }
                connection.Close();
            }
            txtSearch.ForeColor = Color.Black;
        }

        private void panel2_Paint_1(object sender, PaintEventArgs e) { }

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
                SqlCommand command = new SqlCommand(
                    "SELECT ProductID, ProductName, Category, Stock, Price, DateDelivered, Supplier FROM tbl_Products",
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
                        $"₱{Convert.ToDecimal(reader["Price"]):N2}",
                        "kg",
                        reader["DateDelivered"] == DBNull.Value ? "" : Convert.ToDateTime(reader["DateDelivered"]).ToString("MM/dd/yyyy"),
                        reader["Supplier"]?.ToString() ?? "",
                        "EDIT",
                        "DELETE"
                    );
                }
                connection.Close();
            }
        }

        private void btnExport_Click(object sender, EventArgs e) { }

        private void txtSearch_Click(object sender, EventArgs e)
        {
            txtSearch.ForeColor = Color.Black;
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
                if (e.ColumnIndex == 8)
                {
                    string productId = dgvInventory.Rows[e.RowIndex].Cells[0].Value.ToString();
                    string productName = dgvInventory.Rows[e.RowIndex].Cells[1].Value.ToString();
                    string category = dgvInventory.Rows[e.RowIndex].Cells[2].Value.ToString();
                    string stock = dgvInventory.Rows[e.RowIndex].Cells[3].Value.ToString();
                    string price = dgvInventory.Rows[e.RowIndex].Cells[4].Value.ToString();
                    string dateDelivered = dgvInventory.Rows[e.RowIndex].Cells[6].Value?.ToString();
                    string supplier = dgvInventory.Rows[e.RowIndex].Cells[7].Value?.ToString();

                    MessageBox.Show($"Edit product: {productName}\nPrice: {price}\nStock: {stock}\nDate Delivered: {dateDelivered}\nSupplier: {supplier}", "Edit Product");
                }
                else if (e.ColumnIndex == 9)
                {
                    string productId = dgvInventory.Rows[e.RowIndex].Cells[0].Value.ToString();
                    string productName = dgvInventory.Rows[e.RowIndex].Cells[1].Value.ToString();

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
                    SqlCommand command = new SqlCommand(
                        "DELETE FROM tbl_Products WHERE ProductID = @ProductID",
                        connection);
                    command.Parameters.AddWithValue("@ProductID", productId);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Product deleted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadProducts();
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

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e) { }
    }
}
