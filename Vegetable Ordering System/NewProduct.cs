using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vegetable_Ordering_System
{
    public partial class NewProduct : Form
    {
        string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";
        private string imagePath = "";

        public NewProduct()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Select Product Image";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    imagePath = openFileDialog.FileName;
                    txtImagePath.Text = Path.GetFileName(imagePath); // Show only filename

                    // Load image preview
                    try
                    {
                        picProductImage.Image = Image.FromFile(imagePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }




        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void NewProduct_Load(object sender, EventArgs e)
        {
            numericQuantity.Value = 1;
            numericQuantity.Minimum = 0;
            numericQuantity.Maximum = 10000;

            numericPrice.Value = 0;
            numericPrice.Minimum = 0;
            numericPrice.Maximum = 100000;
            numericPrice.DecimalPlaces = 2;
            numericPrice.Increment = 1.00m;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void txtPrice_TextChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(txtProductName.Text))
                {
                    MessageBox.Show("Please enter product name.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtProductName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(cmbCategory.Text))
                {
                    MessageBox.Show("Please enter category.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbCategory.Focus();
                    return;
                }

                // Get values from NumericUpDown controls
                int quantity = (int)numericQuantity.Value;
                decimal price = numericPrice.Value;

                // Additional validation
                if (quantity < 0)
                {
                    MessageBox.Show("Please enter a valid quantity.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    numericQuantity.Focus();
                    return;
                }

                if (price < 0)
                {
                    MessageBox.Show("Please enter a valid price.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    numericPrice.Focus();
                    return;
                }

                string finalImagePath = "";

                // Handle image copying to application directory
                if (!string.IsNullOrEmpty(imagePath))
                {
                    string appDirectory = Application.StartupPath;
                    string imagesFolder = Path.Combine(appDirectory, "ProductImages");

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(imagesFolder))
                        Directory.CreateDirectory(imagesFolder);

                    // Generate unique filename to avoid conflicts
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagePath);
                    finalImagePath = Path.Combine(imagesFolder, fileName);

                    // Copy image to application directory
                    File.Copy(imagePath, finalImagePath, true);

                    // Store only the filename in database
                    finalImagePath = fileName;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"INSERT INTO tbl_Products (ProductName, Category, Stock, Price, ImagePath) 
                                VALUES (@ProductName, @Category, @Stock, @Price, @ImagePath)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ProductName", txtProductName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Category", cmbCategory.Text.Trim());
                        cmd.Parameters.AddWithValue("@Stock", quantity);
                        cmd.Parameters.AddWithValue("@Price", price);

                        // Handle image path - store only filename
                        if (!string.IsNullOrEmpty(finalImagePath))
                        {
                            cmd.Parameters.AddWithValue("@ImagePath", finalImagePath);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@ImagePath", DBNull.Value);
                        }

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Product added successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Failed to add product.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding product: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    }


