using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vegetable_Ordering_System
{
    public partial class Log_In : Form
    {
        public  Log_In()
        {
            InitializeComponent();
        }

        private void logIn_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnLogIn_Click(object sender, EventArgs e)
        {
           logIn();
        }
        private void logIn() {

            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{6,18}$";

         
            errorProvider1.SetError(txtPassword, "");

            if (!Regex.IsMatch(password, pattern))
            {
                errorProvider1.SetError(
                    txtPassword,
                    "Password must be 6-18 chars, include upper, lower, digit, and special char."
                );
                return;
            }

            if (username == "admin" && password == "Password@123")
            {
                MessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide();
               MainMenuForm adminForm = new MainMenuForm(username);
                adminForm.ShowDialog();
            }
            else if (username == "merchant" && password == "Password@123")
            {
                MessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide();
             MainMenuForm merchantForm = new MainMenuForm(username);
                merchantForm.ShowDialog();

            }

            else
            {
                MessageBox.Show("Invalid username or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtUsername_Click(object sender, EventArgs e)
        {
            txtUsername.ForeColor = Color.Black;
            txtUsername.Clear();    
        }

       

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            txtUsername.ForeColor = Color.Black;
        }
      

        private void txtPassword_TextChanged_1(object sender, EventArgs e)
        {
            txtPassword.ForeColor = Color.Black;
            txtPassword.PasswordChar = '●';
        }

        private void txtPassword_Click_1(object sender, EventArgs e)
        {
            txtPassword.ForeColor = Color.Black;
            txtPassword.Clear();
        }

        private void label1_Click_1(object sender, EventArgs e)
        {
            Registration regForm = new Registration();
            regForm.Show();
            this.Hide();
        }

        private void btnLogin_Click_1(object sender, EventArgs e)
        {
            logIn();
        }
    }
}
