using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vegetable_Ordering_System
{
    public partial class MainMenuForm : Form
    {
        private string currentUser;
        private string currentRole;


        public MainMenuForm(string username, string role)
        {
            InitializeComponent();
            currentUser = username;
            currentRole = role;

            lblCurrentUser.Text = $"Logged in as {username} ({role})";
            ApplyRoleRestrictions();

        }
        private void ApplyRoleRestrictions()
        {
            if (currentRole == "Merchant")
            {
                btnSettings.Visible = false;
                btnSuppliers.Visible = false;
               
            }
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {
            timer1.Start();
          
            RoundPanel(panelOrderToday, 20);
            RoundPanel(panelSales, 20);
            RoundPanel(panelStocks, 20);
            RoundPanel(panelTopSelling, 20);
            RoundPanel(panelDailyOrders, 20);

          
        }
        
         
       
        private void RoundPanel(Panel panel, int cornerRadius)
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(new Rectangle(0, 0, cornerRadius, cornerRadius), 180, 90);
            path.AddArc(new Rectangle(panel.Width - cornerRadius, 0, cornerRadius, cornerRadius), 270, 90);
            path.AddArc(new Rectangle(panel.Width - cornerRadius, panel.Height - cornerRadius, cornerRadius, cornerRadius), 0, 90);
            path.AddArc(new Rectangle(0, panel.Height - cornerRadius, cornerRadius, cornerRadius), 90, 90);
            path.CloseFigure();

            panel.Region = new Region(path);
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void labelOrder_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void labelUser_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            
        }
        // Add this method to fix CS1061
        private void panelNav_Paint(object sender, PaintEventArgs e)
        {
            // You can leave this empty if you don't need custom painting logic
        }

        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            btnDashboard.BackColor = Color.FromArgb(0, 150, 0);
        }

        private void btnDOrder_Click(object sender, EventArgs e)
        {
      
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void panel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cartesianChart1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            this.Hide();
            SupplierForm supplierForm = new SupplierForm(currentUser, currentRole);
            supplierForm.ShowDialog();
            this.Show();

        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            OrderForm orderForm = new OrderForm(currentUser, currentRole);
            orderForm.Show();
            this.Close();
        }

        private void btnInventory_Click(object sender, EventArgs e)
        {
            InventoryForm inventoryForm = new InventoryForm(currentUser, currentRole);
            inventoryForm.Show();
            this.Close();

        }

        private void panelDailyOrders_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Hide();
                Log_In loginForm = new Log_In();
                loginForm.Show();
                this.Close();
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
           
         

  
            panelSettings.Location = new Point(btnSettings.Left, btnSettings.Bottom);
            panelSettings.BringToFront();
        }

      

        private void btnGeneral_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Open General Settings");
            HideSettingsMenu();
        }

        
       
        private void HideSettingsMenu()
        {
            panelSettings.Visible = false;
         
        }

        private void btnSystem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Open System Configuration");
            HideSettingsMenu();
        }

        private void btnSecurity_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Open Security Settings");
            HideSettingsMenu();
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Open Backup & Logs");
            HideSettingsMenu();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Vegetable Ordering System\nVersion 1.0\n© 2024 Vegetable Corp");
            HideSettingsMenu();
        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Open User Management");
            HideSettingsMenu();
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (panelSettings.Visible &&
                !panelSettings.Bounds.Contains(e.Location) &&
                !btnSettings.Bounds.Contains(e.Location))
            {
                HideSettingsMenu();
            }
        }

        private void panelSettings_Paint(object sender, PaintEventArgs e)
        {

        }

        private void labelCurrentUser_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
            lblDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        }

        private void lblCurrentUser_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
