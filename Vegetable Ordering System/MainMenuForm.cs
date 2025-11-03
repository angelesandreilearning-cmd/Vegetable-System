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
using System.Windows.Forms.DataVisualization.Charting;
using System.Data.SqlClient;

namespace Vegetable_Ordering_System
{
    public partial class MainMenuForm : Form
    {
        private string currentUser;
        private string currentRole;
        private string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";
        private Timer refreshTimer;


        public MainMenuForm(string username, string role)
        {
            InitializeComponent();
            currentUser = username;
            currentRole = role;

            lblCurrentUser.Text = $"Logged in as {username} ({role})";
            ApplyRoleRestrictions();

            // Initialize refresh timer (every 60 seconds)
            refreshTimer = new Timer();
            refreshTimer.Interval = 60000;
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();

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

            // Load dashboard data and initialize chart
            LoadDashboardData();
            InitializeChart();
            LoadDailyOrdersChart();


        }
        private void LoadDashboardData()
        {
            try
            {
                var dashboardData = GetDashboardData();

                // Update dashboard labels with real data
                lblOrdersToday.Text = dashboardData.OrdersToday.ToString();
                lblTotalSales.Text = $"₱{dashboardData.TotalSalesToday:N2}";
                lblTopSelling.Text = dashboardData.TopSellingVegetable;
                lblLowStock.Text = dashboardData.LowStockCount.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeChart()
        {
            // Configure chart appearance
            chartDailyOrders.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chartDailyOrders.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            chartDailyOrders.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            chartDailyOrders.ChartAreas[0].AxisX.Interval = 1;

            // Set chart title
            chartDailyOrders.Titles.Clear();
            chartDailyOrders.Titles.Add("Daily Orders Trend");
            chartDailyOrders.Titles[0].Font = new Font("Arial", 10, FontStyle.Bold);
            chartDailyOrders.Titles[0].ForeColor = Color.FromArgb(0, 100, 0);
        }

        private void LoadDailyOrdersChart()
        {
            try
            {
                var dailyData = GetDailyOrdersData();

                        if (chartDailyOrders != null && !chartDailyOrders.IsDisposed)
                {
                    // Clear existing series
                    chartDailyOrders.Series.Clear();

                    // Create and configure series
                    Series series = new Series("Orders");
                    series.ChartType = SeriesChartType.Column;
                    series.Color = Color.FromArgb(0, 150, 0);
                    series.IsValueShownAsLabel = true;
                    series.LabelForeColor = Color.FromArgb(0, 100, 0);
                    series.Font = new Font("Arial", 8, FontStyle.Bold);

                    // Add data points
                    foreach (var data in dailyData)
                    {
                        series.Points.AddXY(data.Hour, data.OrderCount);
                    }

                    // Add series to chart
                    chartDailyOrders.Series.Add(series);

                    // Customize appearance
                    chartDailyOrders.ChartAreas[0].AxisX.Title = "Hour of Day";
                    chartDailyOrders.ChartAreas[0].AxisY.Title = "Number of Orders";
                    chartDailyOrders.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 9, FontStyle.Bold);
                    chartDailyOrders.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 9, FontStyle.Bold);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading chart data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public class DashboardData
        {
            public int OrdersToday { get; set; }
            public decimal TotalSalesToday { get; set; }
            public string TopSellingVegetable { get; set; }
            public string LowStockCount { get; set; }
        }

        public class DailyOrderData
        {
            public int Hour { get; set; }
            public string HourLabel { get; set; }
            public int OrderCount { get; set; }
        }

        // Database methods
        private DashboardData GetDashboardData()
        {
            var data = new DashboardData();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Orders Today
                string ordersQuery = "SELECT COUNT(*) FROM tbl_Sales WHERE CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)";
                using (SqlCommand cmd = new SqlCommand(ordersQuery, con))
                {
                    data.OrdersToday = (int)cmd.ExecuteScalar();
                }

                // Total Sales Today
                string salesQuery = "SELECT ISNULL(SUM(TotalAmount), 0) FROM tbl_Sales WHERE CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)";
                using (SqlCommand cmd = new SqlCommand(salesQuery, con))
                {
                    data.TotalSalesToday = (decimal)cmd.ExecuteScalar();
                }

                // Top Selling Vegetable
                string topSellingQuery = @"
                    SELECT TOP 1 ProductName 
                    FROM tbl_Sales_Items si
                    JOIN tbl_Sales s ON si.SaleID = s.SaleID
                    WHERE CAST(s.OrderDate AS DATE) = CAST(GETDATE() AS DATE)
                    GROUP BY ProductName
                    ORDER BY SUM(Quantity) DESC";

                using (SqlCommand cmd = new SqlCommand(topSellingQuery, con))
                {
                    object result = cmd.ExecuteScalar();
                    data.TopSellingVegetable = result?.ToString() ?? "No sales today";
                }

                // Low Stock Count
                string lowStockQuery = @"
    SELECT TOP 1 ProductName, Stock 
    FROM tbl_Products 
    WHERE Stock <= 10
    ORDER BY Stock ASC";  // ← Orders by lowest stock first
                using (SqlCommand cmd = new SqlCommand(lowStockQuery, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string productName = reader["ProductName"].ToString();
                            int remainingKg = Convert.ToInt32(reader["Stock"]);
                            data.LowStockCount = $"" +
                                $" {productName} ({remainingKg}kg)";
                        }
                        else
                        {
                            data.LowStockCount = "No low stock";
                        }
                    }
                }
            }

                return data;
        }
        private List<DailyOrderData> GetDailyOrdersData()
        {
            var dailyData = new List<DailyOrderData>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
                    SELECT 
                        DATEPART(HOUR, OrderDate) AS Hour,
                        COUNT(*) AS OrderCount
                    FROM tbl_Sales 
                    WHERE CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)
                    GROUP BY DATEPART(HOUR, OrderDate)
                    ORDER BY Hour";

                using (SqlCommand cmd = new SqlCommand(query, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int hour = Convert.ToInt32(reader["Hour"]);
                        dailyData.Add(new DailyOrderData
                        {
                            Hour = hour,
                            HourLabel = $"{hour}:00",
                            OrderCount = Convert.ToInt32(reader["OrderCount"])
                        });
                    }
                }

                // Fill in missing hours with zero values
                for (int hour = 0; hour < 24; hour++)
                {
                    if (!dailyData.Any(d => d.Hour == hour))
                    {
                        dailyData.Add(new DailyOrderData
                        {
                            Hour = hour,
                            HourLabel = $"{hour}:00",
                            OrderCount = 0
                        });
                    }
                }

                // Sort by hour
                dailyData = dailyData.OrderBy(d => d.Hour).ToList();
            }

            return dailyData;
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            LoadDashboardData();
            LoadDailyOrdersChart();
        }

        // Add this to clean up resources
        private void MainMenuForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
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
            panelSettings.Visible = !panelSettings.Visible;
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadDashboardData();
            LoadDailyOrdersChart();

        }
    }
}
