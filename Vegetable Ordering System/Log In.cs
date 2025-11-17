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
        private void logIn()
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

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

            string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                   
                    string query = @"
                SELECT Role 
                FROM tbl_Users 
                WHERE Username COLLATE SQL_Latin1_General_CP1_CS_AS = @username 
                AND Password COLLATE SQL_Latin1_General_CP1_CS_AS = @password";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    object roleResult = cmd.ExecuteScalar();

                    if (roleResult != null)
                    {
                        string role = roleResult.ToString();

                        MessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Hide();

                        
                        MainMenuForm mainMenu = new MainMenuForm(username, role);
                        mainMenu.Show();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Invalid Username or Password. (Remember: Case-Sensitive!)",
                            "Login Failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
            
        }

        private void btnLogin_Click_1(object sender, EventArgs e)
        {
            logIn();
        }

        private void checkPassword_CheckedChanged(object sender, EventArgs e)
        {
            if (checkPassword.Checked)
            {
                // Show password - remove password character
                txtPassword.PasswordChar = '\0';
            }
            else
            {
                // Hide password - set password character
                txtPassword.PasswordChar = '●';
            }

        }

        private void Log_In_Load(object sender, EventArgs e)
        {

        }
    }
}
