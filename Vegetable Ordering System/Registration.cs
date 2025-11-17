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

namespace Vegetable_Ordering_System
{
    public partial class Registration : Form
    {

        public Registration()
        {
            InitializeComponent();
        }

        private void Registration_Load(object sender, EventArgs e)
        {

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

        private void txtEmail_KeyPress(object sender, KeyPressEventArgs e)
        {


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

        private void txtContact_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '+')
            {
                e.Handled = true;
                errorProvider1.SetError(txtContact, "Only digits and an optional + are allowed.");
            }
            else
            {
                errorProvider1.SetError(txtContact, "");
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

     

        private void txtSetPassword_TextChanged(object sender, EventArgs e)
        {
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{6,18}$";
            if (!Regex.IsMatch(txtSetPassword.Text, pattern))
            {
                errorProvider1.SetError(
                    txtSetPassword,
                    "Password must be 6-18 chars, include upper, lower, digit, and special char."
                );
            }
            else
            {
                errorProvider1.SetError(txtSetPassword, "");
            }


        }

        private void txtConfirmPassword_TextChanged(object sender, EventArgs e)
        {

            if (txtConfirmPassword.Text != txtSetPassword.Text)
            {
                errorProvider1.SetError(txtConfirmPassword, "Passwords do not match.");
            }
            else
            {
                errorProvider1.SetError(txtConfirmPassword, "");
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
       string.IsNullOrWhiteSpace(txtUsername.Text) ||
       string.IsNullOrWhiteSpace(txtEmail.Text) ||
       string.IsNullOrWhiteSpace(txtContact.Text) ||
       string.IsNullOrWhiteSpace(txtAddress.Text) ||
       string.IsNullOrWhiteSpace(cmbRole.Text) ||
       string.IsNullOrWhiteSpace(txtSetPassword.Text) ||
       string.IsNullOrWhiteSpace(txtConfirmPassword.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Missing Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtSetPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match. Please try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                INSERT INTO tbl_Users (Name, Username, Email, Contact, Address, Role, Password)
                VALUES (@Name, @Username, @Email, @Contact, @Address, @Role, @Password)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", txtName.Text);
                    cmd.Parameters.AddWithValue("@Username", txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@Contact", txtContact.Text);
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                    cmd.Parameters.AddWithValue("@Role", cmbRole.Text);
                    cmd.Parameters.AddWithValue("@Password", txtSetPassword.Text);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Registration successful! You can now log in.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

        
                this.Hide();
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void ClearFields()
        {
            txtName.Clear();
            txtEmail.Clear();
            txtContact.Clear();
            txtAddress.Clear();
            cmbRole.SelectedIndex = -1;
            txtSetPassword.Clear();
            txtConfirmPassword.Clear();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
           
        }

        private void Registration_Load_1(object sender, EventArgs e)
        {

        }
    }
}
