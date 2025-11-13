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
    public partial class AddStock : Form
    {
        string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";
        private InventoryForm _parentform;
        private string imageFilePath = "";
        public AddStock(InventoryForm parentform)
        {
            InitializeComponent();
            _parentform = parentform;
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





        private void AddStock_Load(object sender, EventArgs e)
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

            // Set default date to today
            dtpDateDelivery.Value = DateTime.Now;

           
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
                string imageFileName = "";

                // Copy image to application folder if one was selected
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

                    // STEP 1: Insert product and get the ProductID
                    string insertQuery = @"
                INSERT INTO tbl_Products 
                (ProductName, Category, Stock, Price, SupplierID, ImagePath)
                OUTPUT INSERTED.ProductID
                VALUES
                (@name, @category, @stock, @price, @supplierID, @imagePath)";

                    int newProductId;
                    string generatedBarcode = "";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", txtVegetableName.Text.Trim());
                        cmd.Parameters.AddWithValue("@category", cmbCategory.SelectedItem?.ToString() ?? "");
                        cmd.Parameters.AddWithValue("@stock", int.Parse(txtQuantity.Text));
                        cmd.Parameters.AddWithValue("@price", decimal.Parse(txtPrice.Text));

                        // Supplier ID
                        if (cmbSupplierName.SelectedValue != null)
                        {
                            cmd.Parameters.AddWithValue("@supplierID", cmbSupplierName.SelectedValue);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@supplierID", DBNull.Value);
                        }

                        // Image Path
                        if (!string.IsNullOrEmpty(imageFileName))
                        {
                            cmd.Parameters.AddWithValue("@imagePath", imageFileName);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@imagePath", DBNull.Value);
                        }

                        // Get the new ProductID
                        newProductId = (int)cmd.ExecuteScalar();

                        // STEP 2: Generate barcode using the actual ProductID
                        string timestamp = DateTime.Now.ToString("yyyyMMdd");
                        generatedBarcode = $"VEG{timestamp}{newProductId:D3}";

                        // STEP 3: Update the record with the generated barcode
                        string updateQuery = "UPDATE tbl_Products SET Barcode = @Barcode WHERE ProductID = @ProductID";
                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@Barcode", generatedBarcode);
                            updateCmd.Parameters.AddWithValue("@ProductID", newProductId);
                            updateCmd.ExecuteNonQuery();
                        }
                    }

                    DialogResult result = MessageBox.Show(
     $"Product added successfully!\n\n" +
     $"Product: {txtVegetableName.Text.Trim()}\n" +
     $"Barcode: {generatedBarcode}\n\n" +
     "Do you want to view and print the barcode?",
     "Success",
     MessageBoxButtons.YesNo,
     MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        // Show barcode form
                        BarcodePrintForm barcodeForm = new BarcodePrintForm(generatedBarcode, txtVegetableName.Text.Trim());
                        barcodeForm.ShowDialog();
                    }

                    _parentform?.LoadProducts();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving product: {ex.Message}", "Database Error",
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
            txtVegetableName.Clear();
            txtQuantity.Clear();
            txtPrice.Clear();
            txtTotalCost.Clear();
            cmbCategory.SelectedIndex = -1;
            cmbUnit.SelectedIndex = 0;
            cmbSupplierName.SelectedIndex = -1;
            dtpDateDelivery.Value = DateTime.Now;
            errorProvider1.Clear();

            
            imageFilePath = "";
            if (txtImagePath != null)
                txtImagePath.Text = "";
            if (picProductImage != null)
                picProductImage.Image = null;

            
        }
        private bool IsBarcodeUnique(string barcode)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM tbl_Products WHERE Barcode = @Barcode";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Barcode", barcode);
                        int count = (int)cmd.ExecuteScalar();
                        return count == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking barcode uniqueness: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
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

                        // Display in textbox (if you have one)
                        if (txtImagePath != null)
                        {
                            txtImagePath.Text = System.IO.Path.GetFileName(imageFilePath);
                        }

                        // Optional: Display image preview if you have a PictureBox
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

        private string GenerateBarcode()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Get the next available barcode number
                    string query = "SELECT ISNULL(MAX(CAST(SUBSTRING(Barcode, 4, LEN(Barcode)) AS INT)), 0) FROM tbl_Products WHERE Barcode LIKE 'VEG%'";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        int lastBarcodeNumber = (int)cmd.ExecuteScalar();
                        int nextBarcodeNumber = lastBarcodeNumber + 1;

                        // Format: VEG + YYYYMMDD + sequential number (4 digits)
                        string timestamp = DateTime.Now.ToString("yyyyMMdd");
                        string barcode = $"VEG{timestamp}{nextBarcodeNumber:D4}";

                        // **ADD UNIQUENESS CHECK**
                        if (!IsBarcodeUnique(barcode))
                        {
                            // If not unique, try with incremented number
                            int safetyCounter = 0;
                            do
                            {
                                nextBarcodeNumber++;
                                barcode = $"VEG{timestamp}{nextBarcodeNumber:D4}";
                                safetyCounter++;
                            } while (!IsBarcodeUnique(barcode) && safetyCounter < 100);

                            if (safetyCounter >= 100)
                            {
                                // Fallback to timestamp-based barcode
                                barcode = $"VEG{DateTime.Now:yyyyMMddHHmmssfff}";
                            }
                        }

                        return barcode;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating barcode: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Fallback barcode generation with timestamp
                return $"VEG{DateTime.Now:yyyyMMddHHmmssfff}";
            }
        }

        private void txtTotalCost_TextChanged_1(object sender, EventArgs e)
        {

        }
    }
}