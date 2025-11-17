using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Vegetable_Ordering_System
{
    public partial class EditStock : Form
    {
        string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";
        private InventoryForm _parentform;
        private string imageFilePath = "";
        private int _productID;

        public EditStock(InventoryForm parentform, int productID)
        {
            InitializeComponent();
            _parentform = parentform;
            _productID = productID;
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

        private void EditStock_Load(object sender, EventArgs e)
        {
            RoundCorners(btnSave, 20);
            RoundCorners(btnClear, 20);
            RoundCorners(btnCancel, 20);

            cmbUnit.Items.AddRange(new string[] { "kg", "bundle" });
            cmbUnit.SelectedIndex = 0;

            cmbCategory.Items.AddRange(new string[]
            {
        "Leafy Vegetables (Madahong Gulay)",
        "Root Vegetables (Ugat na Gulay)",
        "Fruit Vegetables (Bunga ng Gulay)"
            });
            cmbCategory.SelectedIndex = -1;

            // Load suppliers into dropdown
            LoadSuppliers();

            // Load existing product data
            LoadProductData();
        }

        private void LoadProductData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT ProductName, Category, Stock, Price, SupplierID, 
                            ImagePath, DateDelivered, Supplier, Barcode, Unit 
                            FROM tbl_Products WHERE ProductID = @ProductID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductID", _productID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtVegetableName.Text = reader["ProductName"].ToString();
                                txtQuantity.Text = reader["Stock"].ToString();
                                txtPrice.Text = reader["Price"].ToString();

                                // Set category
                                string category = reader["Category"].ToString();
                                if (cmbCategory.Items.Contains(category))
                                    cmbCategory.SelectedItem = category;

                                // Set unit
                                string unit = reader["Unit"].ToString();
                                if (cmbUnit.Items.Contains(unit))
                                    cmbUnit.SelectedItem = unit;

                                // Set supplier
                                if (reader["SupplierID"] != DBNull.Value)
                                {
                                    int supplierID = Convert.ToInt32(reader["SupplierID"]);
                                    cmbSupplierName.SelectedValue = supplierID;
                                }

                                // Set date delivered
                                if (reader["DateDelivered"] != DBNull.Value)
                                    dtpDateDelivery.Value = Convert.ToDateTime(reader["DateDelivered"]);

                                // Load image if exists
                                if (reader["ImagePath"] != DBNull.Value)
                                {
                                    string imagePath = reader["ImagePath"].ToString();
                                    string fullImagePath = System.IO.Path.Combine(Application.StartupPath, "ProductImages", imagePath);

                                    if (System.IO.File.Exists(fullImagePath))
                                    {
                                        picProductImage.Image = Image.FromFile(fullImagePath);
                                        txtImagePath.Text = imagePath;
                                    }
                                }

                                CalculateTotalCost();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading product data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT SupplierID, SupplierName FROM tbl_Suppliers ORDER BY SupplierName";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        DataTable dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());

                        cmbSupplierName.DataSource = dt;
                        cmbSupplierName.DisplayMember = "SupplierName";
                        cmbSupplierName.ValueMember = "SupplierID";
                    }
                }
                cmbSupplierName.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading suppliers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateTotalCost()
        {
            if (decimal.TryParse(txtPrice.Text, out decimal price) &&
                int.TryParse(txtQuantity.Text, out int quantity))
            {
                decimal total = 0;

                if (cmbUnit.SelectedItem != null)
                {
                    string unit = cmbUnit.SelectedItem.ToString();
                    if (unit == "bundle")
                    {
                        total = price * (quantity * 10);
                    }
                    else if (unit == "kg")
                    {
                        total = price * quantity;
                    }
                }

                txtTotalCost.Text = total.ToString("0.00");
            }
            else
            {
                txtTotalCost.Text = "";
            }
        }

        private void txtVegetableName_TextChanged(object sender, EventArgs e)
        {
            ValidateVegetableName();
        }

        private void ValidateVegetableName()
        {
            if (string.IsNullOrWhiteSpace(txtVegetableName.Text))
            {
                errorProvider1.SetError(txtVegetableName, "Product name cannot be empty.");
            }
            else if (!Regex.IsMatch(txtVegetableName.Text, @"^[a-zA-Z\s]+$"))
            {
                errorProvider1.SetError(txtVegetableName, "Please enter only letters and spaces.");
            }
            else
            {
                errorProvider1.SetError(txtVegetableName, "");
            }
        }

        private void txtQuantity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                errorProvider1.SetError(txtQuantity, "Quantity must contain digits only.");
            }
            else
            {
                errorProvider1.SetError(txtQuantity, "");
            }
        }

        private void txtQuantity_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                errorProvider1.SetError(txtQuantity, "Quantity is required.");
            }
            else if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                errorProvider1.SetError(txtQuantity, "Quantity must be a positive number.");
            }
            else
            {
                errorProvider1.SetError(txtQuantity, "");
            }

            CalculateTotalCost();
        }

        private void txtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
                errorProvider1.SetError(txtPrice, "Only numbers and a decimal point allowed.");
            }
            else
            {
                errorProvider1.SetError(txtPrice, "");
            }

            if (e.KeyChar == '.' && (sender as TextBox).Text.Contains("."))
            {
                e.Handled = true;
                errorProvider1.SetError(txtPrice, "Only one decimal point allowed.");
            }
        }

        private void txtPrice_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                errorProvider1.SetError(txtPrice, "Price is required.");
            }
            else if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                errorProvider1.SetError(txtPrice, "Enter a valid positive number.");
            }
            else
            {
                errorProvider1.SetError(txtPrice, "");
            }

            CalculateTotalCost();
        }

        private void cmbUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalculateTotalCost();
        }

        private void txtVegetableName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
            {
                e.Handled = true;
                errorProvider1.SetError(txtVegetableName, "Only letters and spaces are allowed.");
            }
            else
            {
                errorProvider1.SetError(txtVegetableName, "");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateAllFields())
                return;

            try
            {
                string imageFileName = txtImagePath.Text; // Keep existing image by default

                // Copy new image to application folder if one was selected
                if (!string.IsNullOrEmpty(imageFilePath))
                {
                    string imagesFolder = System.IO.Path.Combine(Application.StartupPath, "ProductImages");

                    // Create directory if it doesn't exist
                    if (!System.IO.Directory.Exists(imagesFolder))
                    {
                        System.IO.Directory.CreateDirectory(imagesFolder);
                    }

                    // Generate unique filename
                    string extension = System.IO.Path.GetExtension(imageFilePath);
                    imageFileName = $"{Guid.NewGuid()}{extension}";
                    string destinationPath = System.IO.Path.Combine(imagesFolder, imageFileName);

                    // Copy the file
                    System.IO.File.Copy(imageFilePath, destinationPath, true);
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string updateQuery = @"
    UPDATE tbl_Products 
    SET ProductName = @name, 
        Category = @category, 
        Stock = @stock, 
        Price = @price, 
        Unit = @unit,
        SupplierID = @supplierID, 
        ImagePath = @imagePath, 
        DateDelivered = @dateDelivered, 
        Supplier = @supplier
    WHERE ProductID = @productID";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", txtVegetableName.Text.Trim());
                        cmd.Parameters.AddWithValue("@category", cmbCategory.SelectedItem?.ToString() ?? "");
                        cmd.Parameters.AddWithValue("@stock", int.Parse(txtQuantity.Text));
                        cmd.Parameters.AddWithValue("@price", decimal.Parse(txtPrice.Text));
                        cmd.Parameters.AddWithValue("@unit", cmbUnit.SelectedItem?.ToString() ?? "");

                        // Supplier ID
                        if (cmbSupplierName.SelectedValue != null)
                        {
                            cmd.Parameters.AddWithValue("@supplierID", cmbSupplierName.SelectedValue);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@supplierID", DBNull.Value);
                        }

                        // Date Delivered
                        cmd.Parameters.AddWithValue("@dateDelivered", dtpDateDelivery.Value);

                        // Supplier Name (from dropdown text)
                        cmd.Parameters.AddWithValue("@supplier", cmbSupplierName.Text);

                        // Image Path - use existing if no new image selected
                        if (!string.IsNullOrEmpty(imageFileName))
                        {
                            cmd.Parameters.AddWithValue("@imagePath", imageFileName);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@imagePath", DBNull.Value);
                        }

                        cmd.Parameters.AddWithValue("@productID", _productID);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Product updated successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            _parentform?.LoadProducts();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("No changes were made to the product.", "Information",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating product: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateAllFields()
        {
            bool isValid = true;
            errorProvider1.Clear();

            // Validate Product Name
            if (string.IsNullOrWhiteSpace(txtVegetableName.Text))
            {
                errorProvider1.SetError(txtVegetableName, "Product name is required.");
                isValid = false;
            }

            // Validate Category
            if (cmbCategory.SelectedIndex == -1)
            {
                errorProvider1.SetError(cmbCategory, "Please select a category.");
                isValid = false;
            }

            // Validate Quantity
            if (string.IsNullOrWhiteSpace(txtQuantity.Text) || !int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                errorProvider1.SetError(txtQuantity, "Valid quantity is required.");
                isValid = false;
            }

            // Validate Price
            if (string.IsNullOrWhiteSpace(txtPrice.Text) || !decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                errorProvider1.SetError(txtPrice, "Valid price is required.");
                isValid = false;
            }

            // Validate Unit
            if (cmbUnit.SelectedIndex == -1)
            {
                errorProvider1.SetError(cmbUnit, "Please select a unit.");
                isValid = false;
            }

            // Validate Supplier
            if (cmbSupplierName.SelectedIndex == -1)
            {
                errorProvider1.SetError(cmbSupplierName, "Please select a supplier.");
                isValid = false;
            }

            return isValid;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            // For edit form, clearing might not be appropriate
            // Instead, reload the original data
            LoadProductData();
            errorProvider1.Clear();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtTotalCost_TextChanged(object sender, EventArgs e)
        {
            // Keep this empty - needed for the event
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files (*.bmp;*.jpg;*.jpeg;*.png;*.gif)|*.bmp;*.jpg;*.jpeg;*.png;*.gif";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Get the selected file
                        imageFilePath = openFileDialog.FileName;

                        // Display in textbox
                        if (txtImagePath != null)
                        {
                            txtImagePath.Text = System.IO.Path.GetFileName(imageFilePath);
                        }

                        // Display image preview
                        if (picProductImage != null)
                        {
                            Image originalImage = Image.FromFile(imageFilePath);
                            picProductImage.Image = originalImage;
                        }

                        MessageBox.Show("Image selected successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void txtTotalCost_TextChanged_1(object sender, EventArgs e)
        {

        }
    }
}