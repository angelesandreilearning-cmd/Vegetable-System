using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Vegetable_Ordering_System
{
    public partial class EditSupplier : Form
    {
        string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";
        private SupplierForm _parentForm;
        private int _supplierID;

        public EditSupplier(SupplierForm parentForm, int supplierID)
        {
            InitializeComponent();
            _parentForm = parentForm;
            _supplierID = supplierID;
            LoadSupplierData();
        }

        private void LoadSupplierData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Remove "Phone" from the SELECT query since you're not using it
                    string query = "SELECT SupplierName, Contact, Address, Email FROM tbl_Suppliers WHERE SupplierID = @SupplierID";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SupplierID", _supplierID);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        txtName.Text = reader["SupplierName"].ToString();
                        txtContact.Text = reader["Contact"].ToString();
                        txtAddress.Text = reader["Address"].ToString();
                        txtEmail.Text = reader["Email"].ToString();
                        // Phone field removed since it's not in your database table
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading supplier data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                errorProvider1.SetError(txtName, "Name cannot be empty or spaces only.");
            }
            else
            {
                Regex regex = new Regex(@"^[a-zA-Z\s]+$");
                if (!regex.IsMatch(txtName.Text))
                {
                    errorProvider1.SetError(txtName, "Please enter only letters and spaces.");
                }
                else
                {
                    errorProvider1.SetError(txtName, "");
                }
            }
        }

        private void txtName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
            {
                e.Handled = true;
                errorProvider1.SetError(txtName, "Only letters and spaces are allowed.");
            }
            else
            {
                errorProvider1.SetError(txtName, "");
            }
        }

        private void txtContact_TextChanged(object sender, EventArgs e)
        {
            Regex regex = new Regex(@"^09\d{9}$");
            if (!regex.IsMatch(txtContact.Text))
            {
                errorProvider1.SetError(txtContact, "Contact must start with 09 and have 11 digits.");
            }
            else
            {
                errorProvider1.SetError(txtContact, "");
            }
        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {
            Regex regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!regex.IsMatch(txtEmail.Text))
            {
                errorProvider1.SetError(txtEmail, "Please enter a valid email address.");
            }
            else
            {
                errorProvider1.SetError(txtEmail, "");
            }
        }

        private void txtAddress_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                errorProvider1.SetError(txtAddress, "Address cannot be empty.");
            }
            else
            {
                errorProvider1.SetError(txtAddress, "");
            }
        }

        private void txtContact_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                errorProvider1.SetError(txtContact, "Only numbers are allowed.");
            }
            else
            {
                errorProvider1.SetError(txtContact, "");
            }
        }

        private void btnUpdateSupplier_Click(object sender, EventArgs e)
        {
          if (!ValidateAllFields())
        return;

    try
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            
            // Remove Phone from the UPDATE query since your table doesn't have it
            SqlCommand command = new SqlCommand(
                "UPDATE tbl_Suppliers SET SupplierName = @name, Contact = @contact, " +
                "Address = @address, Email = @email WHERE SupplierID = @supplierID", conn);

            command.Parameters.AddWithValue("@name", txtName.Text.Trim());
            command.Parameters.AddWithValue("@contact", txtContact.Text.Trim());
            command.Parameters.AddWithValue("@address", txtAddress.Text.Trim());
            command.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
            command.Parameters.AddWithValue("@supplierID", _supplierID);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                MessageBox.Show("Supplier updated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                _parentForm.LoadSuppliers();
                this.Close();
            }
            else
            {
                MessageBox.Show("No changes were made to the supplier.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error updating supplier: {ex.Message}", "Database Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
        }

        private bool ValidateAllFields()
        {
            bool isValid = true;
            errorProvider1.Clear();

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                errorProvider1.SetError(txtName, "Supplier name is required.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtContact.Text))
            {
                errorProvider1.SetError(txtContact, "Contact number is required.");
                isValid = false;
            }
            else if (!Regex.IsMatch(txtContact.Text, @"^09\d{9}$"))
            {
                errorProvider1.SetError(txtContact, "Contact must start with 09 and have 11 digits.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                errorProvider1.SetError(txtEmail, "Email is required.");
                isValid = false;
            }
            else if (!Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                errorProvider1.SetError(txtEmail, "Please enter a valid email address.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                errorProvider1.SetError(txtAddress, "Address is required.");
                isValid = false;
            }

            return isValid;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void EditSupplier_Load(object sender, EventArgs e)
        {
            // Form load logic if needed
        }

        private void EditSupplier_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Key press logic if needed
        }
    }
}