using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vegetable_Ordering_System
{
    public partial class InventoryForm : Form
    {
        string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";

        private string currentUser;
        private string currentRole;

        private bool isSettingsMenuVisible = false;
       
        // Add this constructor to accept a username argument
        public InventoryForm(string username,string role)
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

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            MainMenuForm adminForm = new MainMenuForm(currentUser, currentRole);
            adminForm.Show();
            this.Close();
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            OrderForm orderForm = new OrderForm(currentUser,currentRole);
            orderForm.Show();
            this.Close();
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            SupplierForm supplierForm = new SupplierForm(currentUser,currentRole);
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
            MessageBox.Show("Open General Settings");
            HideSettingsMenu();
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

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
         
            txtSearch.ForeColor = Color.Black;
        }

        private void panel2_Paint_1(object sender, PaintEventArgs e)
        {

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

        private void InventoryForm_Load(object sender, EventArgs e)
        {
            timer1.Start();
            RoundCorners(btn_addstk, 20);
         

           
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
            lblDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        }
        public void LoadVegetables()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            SqlCommand command = new SqlCommand("SELECT VegetableID, VegetableName, Quantity, Price FROM tbl_Vegetables", connection);
            SqlDataReader reader = command.ExecuteReader();

            dataGridView1.Rows.Clear();
            while (reader.Read())
            {
                dataGridView1.Rows.Add(
                    reader["VegetableID"].ToString(),
                    reader["VegetableName"].ToString(),
                    reader["Quantity"].ToString(),
                    reader["Price"].ToString(),
                    "EDIT",
                    "DELETE"
                );
            }

            connection.Close();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {

        }

        private void txtSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
        }

        private void btn_addstk_Click(object sender, EventArgs e)
        {
            AddStock addStockForm = new AddStock(this);
            addStockForm.ShowDialog();
        }
    }

}
