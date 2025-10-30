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
    public partial class AddSupplier : Form
    {
        string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";
        private SupplierForm _parentform;   

        public AddSupplier(SupplierForm parentform)
        {
            InitializeComponent();
            _parentform = parentform;
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



        private void AddSupplier_KeyPress(object sender, KeyPressEventArgs e)
        {

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

        private void AddSupplier_Load(object sender, EventArgs e)
        {

        }

        private void btnAddSupplier_Click(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand command = new SqlCommand(
    "INSERT INTO tbl_Suppliers (SupplierName, Contact, Address, Email) " +
    "VALUES (@name, @contact, @address, @email)", conn);

            // 🎯 Add values from your input fields (TextBoxes)
            command.Parameters.AddWithValue("@name", txtName.Text);
            command.Parameters.AddWithValue("@contact", txtContact.Text);
            command.Parameters.AddWithValue("@address", txtAddress.Text);
            command.Parameters.AddWithValue("@email", txtEmail.Text);
          
            command.ExecuteNonQuery();  
            _parentform.LoadSuppliers();    
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
