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
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"INSERT INTO tbl_Products 
                     (ProductName, Category, Stock, Price, SupplierID)
                     VALUES
                     (@name, @category, @stock, @price, @supplierID)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", txtVegetableName.Text.Trim());
                        cmd.Parameters.AddWithValue("@category", cmbCategory.SelectedItem?.ToString() ?? "");
                        cmd.Parameters.AddWithValue("@stock", int.Parse(txtQuantity.Text));
                        cmd.Parameters.AddWithValue("@price", decimal.Parse(txtPrice.Text));

                        // SIMPLIFIED: Get SupplierID directly from SelectedValue
                        if (cmbSupplierName.SelectedValue != null)
                        {
                            cmd.Parameters.AddWithValue("@supplierID", cmbSupplierName.SelectedValue);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@supplierID", DBNull.Value);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Product added successfully!", "Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                _parentform?.LoadProducts();
                this.Close();
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
            cmbUnit.SelectedIndex = 0; // Changed to 0 for default selection
            cmbSupplierName.SelectedIndex = -1; // Added this line
            dtpDateDelivery.Value = DateTime.Now;
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
    }
}