using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Data.SqlClient;
using System.IO;
namespace Vegetable_Ordering_System
{
    public partial class OrderForm : Form
    {


        private string _username;
      
        private bool isSettingsMenuVisible = false;
        public OrderForm()
        {
            InitializeComponent();
            _username = "guest";
    
            errorProvider1.BlinkStyle = ErrorBlinkStyle.AlwaysBlink;
            errorProvider1.BlinkRate = 500;
        }

        public OrderForm(string username)
        {
            InitializeComponent();
            _username = username;
            errorProvider1.BlinkStyle = ErrorBlinkStyle.AlwaysBlink;
            errorProvider1.BlinkRate = 500;

         
            if (_username == "merchant")
            {
                btnSuppliers.Visible = false;
                btnSettings.Visible = false;
            }
        }

        private void btnInventory_Click(object sender, EventArgs e)
        {
            InventoryForm inventoryForm = new InventoryForm(_username);
            inventoryForm.Show();
            this.Close();
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            SupplierForm supplierForm = new SupplierForm(_username);
            supplierForm.Show();
            this.Close();
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            MainMenuForm adminForm = new MainMenuForm(_username);
            adminForm.Show();
            this.Close();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            isSettingsMenuVisible = !isSettingsMenuVisible;
            panelSettings.Visible = isSettingsMenuVisible;
            panelSettings.Location = new Point(btnSettings.Left, btnSettings.Bottom);
            panelSettings.BringToFront();
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
        private void HideSettingsMenu()
        {
            panelSettings.Visible = false;  
            isSettingsMenuVisible = false;
        }

        private void OrderForm_Load(object sender, EventArgs e)
        {

     

            RoundCorners(btnDraft, 20);
              RoundCorners(btnBarcode, 20);
            RoundCorners(btnPlaceOrder, 20);
            RoundPanel(panel3, 20);
     
            RoundPanel(flowLayoutPanel2, 20);
            
            timer1.Start();

            if (_username == "admin")
            {
                lblCurrentUser.Text = "Logged in as Admin";
            }
            else if (_username == "merchant")
            {
                lblCurrentUser.Text = "Logged in as Merchant";
                btnSuppliers.Visible = false;
                btnSettings.Visible = false;
            }
            else
            {
                lblCurrentUser.Text = $"Logged in as {_username}";
            }
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt"); 
            lblDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        }

        private void txtCustomer_TextChanged(object sender, EventArgs e)
        {
            // Only set/clear the error here — do not mutate Text (mutation will immediately clear the error)
            Regex regex = new Regex(@"^[a-zA-Z\s]*$");
            if (!regex.IsMatch(txtCustomer.Text))
            {
                errorProvider1.SetError(txtCustomer, "Please enter only letters and spaces.");
            }
            else
            {
                errorProvider1.SetError(txtCustomer, "");
            }
        }

        private void txtCustomer_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow letters, control keys (backspace), and space
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
            {
                e.Handled = true;
                // optional: show a short error while typing (not required)
                errorProvider1.SetError(txtCustomer, "Only letters and spaces allowed.");
            }
            else
            {
                errorProvider1.SetError(txtCustomer, "");
            }
        }

        private void txtCustomer_Validating(object sender, CancelEventArgs e)
        {
            // final validation when leaving the control
            Regex regex = new Regex(@"^[a-zA-Z\s]+$"); // require at least one char if you want
            if (!regex.IsMatch(txtCustomer.Text))
            {
                errorProvider1.SetError(txtCustomer, "Please enter only letters and spaces.");
            }
            else
            {
                errorProvider1.SetError(txtCustomer, "");
            }
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
           
        }
      

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {

        }

        private void lblDate_Click(object sender, EventArgs e)
        {

        }
    }

}
