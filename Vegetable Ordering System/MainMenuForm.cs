using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Vegetable_Ordering_System
{
    public partial class MainMenuForm : Form
    {
        private string currentUser;
        private string currentRole;
        string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";
        private Timer refreshTimer;

        public MainMenuForm(string username, string role)
        {
            InitializeComponent();
            currentUser = username;
            currentRole = role;

            lblCurrentUser.Text = $"Logged in as {username} ({role})";
            ApplyRoleRestrictions();
            timer1.Start();
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
            InitializeWeeklySalesChart(); // ADD THIS LINE
            LoadDailyOrdersChart();
            LoadTopVegetablesChart();
            LoadWeeklySalesChart(); // ADD THIS LINE

            // Load weekly summary
            LoadWeeklySummary();

        }
        private string GetSeasonalMessage()
        {
            int month = DateTime.Now.Month;

            if (month >= 3 && month <= 5)
            {
                return "Hot season: Expect higher demand for cooling vegetables (e.g., cucumbers, tomatoes).";
            }
            else if (month >= 6 && month <= 11)
            {
                return "Rainy season: Root crops and hardy vegetables may see increased sales.";
            }
            else if (month == 12)
            {
                return "Holiday season: Overall vegetable sales may increase due to festivities.";
            }
            else // January and February
            {
                return "Cool season: Leafy vegetables may be in higher demand.";
            }
        }
        public class MonthlySalesData
        {
            public int Month { get; set; }
            public string MonthName { get; set; }
            public decimal TotalSales { get; set; }
        }
        private List<MonthlySalesData> GetMonthlySalesData()
        {
            var monthlyData = new List<MonthlySalesData>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
            SELECT 
                MONTH(OrderDate) AS Month,
                DATENAME(MONTH, OrderDate) AS MonthName,
                ISNULL(SUM(TotalAmount), 0) AS TotalSales
            FROM tbl_Sales
            WHERE YEAR(OrderDate) = YEAR(GETDATE())
            GROUP BY MONTH(OrderDate), DATENAME(MONTH, OrderDate)
            ORDER BY Month";

                using (SqlCommand cmd = new SqlCommand(query, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        monthlyData.Add(new MonthlySalesData
                        {
                            Month = Convert.ToInt32(reader["Month"]),
                            MonthName = reader["MonthName"].ToString(),
                            TotalSales = Convert.ToDecimal(reader["TotalSales"])
                        });
                    }
                }

                // Fill in missing months with zero sales
                for (int month = 1; month <= 12; month++)
                {
                    if (!monthlyData.Any(m => m.Month == month))
                    {
                        monthlyData.Add(new MonthlySalesData
                        {
                            Month = month,
                            MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                            TotalSales = 0
                        });
                    }
                }

                // Sort by month
                monthlyData = monthlyData.OrderBy(m => m.Month).ToList();
            }

            return monthlyData;
        }

        private void LoadDashboardData()
        {
            try
            {
                var dashboardData = GetDashboardData();

                // MAIN VALUES
                lblOrdersToday.Text = dashboardData.OrdersToday.ToString();
                lblTotalSales.Text = $"₱{dashboardData.TotalSalesToday:N2}";
                lblTopSelling.Text = dashboardData.TopSellingVegetable;
                lblLowStock.Text = dashboardData.LowStockCount.ToString();

                // ---- TREND INDICATORS ----
                lblOrdersTrend.Text = GetTrend(
                    dashboardData.OrdersToday,
                    dashboardData.OrdersYesterday
                );

                lblSalesTrend.Text = GetTrend(
                    dashboardData.TotalSalesToday,
                    dashboardData.TotalSalesYesterday
                );

                // ----- SET COLORS -----
                lblOrdersTrend.ForeColor =
                    lblOrdersTrend.Text.Contains("▲") ? Color.Green :
                    lblOrdersTrend.Text.Contains("▼") ? Color.Red :
                    Color.Gray;

                lblSalesTrend.ForeColor =
                    lblSalesTrend.Text.Contains("▲") ? Color.Green :
                    lblSalesTrend.Text.Contains("▼") ? Color.Red :
                    Color.Gray;

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading dashboard data: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        public class SeasonalImpact
        {
            public string Season { get; set; }
            public string WeatherCondition { get; set; }
            public string ImpactOnSales { get; set; }
            public string RecommendedActions { get; set; }
            public decimal SalesMultiplier { get; set; }
            public string PriceTrend { get; set; }
            public string TopVegetables { get; set; }
            public string Risks { get; set; }
            public string Opportunities { get; set; }
        }
      
     
       
        public class TopSellingProduct
        {
            public string ProductName { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalSales { get; set; }
        }

        // Add this method to get top selling products data
        private List<TopSellingProduct> GetTopSellingProducts(int topCount = 3)
        {
            var topProducts = new List<TopSellingProduct>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // This query gets top products from ALL sales (remove the date filter)
                string query = @"
            SELECT TOP (@TopCount) 
                ProductName,
                SUM(Quantity) as TotalQuantity,
                SUM(Quantity * UnitPrice) as TotalSales
            FROM tbl_Sales_Items si
            JOIN tbl_Sales s ON si.SaleID = s.SaleID
            GROUP BY ProductName
            ORDER BY SUM(Quantity) DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@TopCount", topCount);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            topProducts.Add(new TopSellingProduct
                            {
                                ProductName = reader["ProductName"].ToString(),
                                TotalQuantity = Convert.ToInt32(reader["TotalQuantity"]),
                                TotalSales = Convert.ToDecimal(reader["TotalSales"])
                            });
                        }
                    }
                }
            }

            return topProducts;
        }
        private void LoadTopVegetablesChart()
        {
            try
            {
                // Comprehensive null and disposed checking - ADD THIS
                if (chartTopVegetables == null || chartTopVegetables.IsDisposed)
                {
                    System.Diagnostics.Debug.WriteLine("Chart control is not available or disposed");
                    return;
                }

                // Ensure we're on the UI thread - ADD THIS
                if (chartTopVegetables.InvokeRequired)
                {
                    chartTopVegetables.Invoke(new Action(LoadTopVegetablesChart));
                    return;
                }

                var topProducts = GetTopSellingProducts(3);

                // Check if we have data - ADD THIS
                if (topProducts == null || !topProducts.Any())
                {
                    // Show "No Data" message on chart
                    chartTopVegetables.Series.Clear();
                    chartTopVegetables.Titles.Clear();
                    chartTopVegetables.Titles.Add("No Data Available");
                    return;
                }

                // Reset chart (your existing code)
                chartTopVegetables.Series.Clear();
                chartTopVegetables.Titles.Clear();
                chartTopVegetables.ChartAreas.Clear();
                chartTopVegetables.Legends.Clear();

                // Basic chart area (your existing code)
                ChartArea area = new ChartArea();
                area.Area3DStyle.Enable3D = true;
                area.Area3DStyle.Inclination = 30;
                chartTopVegetables.ChartAreas.Add(area);

                Title title = new Title("Top 3 Products");
                title.Font = new Font("Arial", 11, FontStyle.Bold);
                chartTopVegetables.Titles.Add(title);

                // Basic series (your existing code)
                Series series = new Series();
                series.ChartType = SeriesChartType.Pie;
                series.IsValueShownAsLabel = true;
                series.Label = "#PERCENT{P0}";
                series.Font = new Font("Arial", 9, FontStyle.Bold);
                series.LabelForeColor = Color.White;

                // Basic colors (your existing code)
                Color[] basicColors = { Color.SteelBlue, Color.SeaGreen, Color.Orange, Color.Purple, Color.Tomato };

                // Add data with validation - ENHANCED THIS
                foreach (var product in topProducts)
                {
                    if (product != null && !string.IsNullOrEmpty(product.ProductName))
                    {
                        DataPoint point = new DataPoint();
                        point.SetValueXY(product.ProductName, product.TotalQuantity);
                        point.Color = basicColors[series.Points.Count % basicColors.Length];
                        point.LegendText = $"{product.ProductName} ({product.TotalQuantity}kg)";
                        series.Points.Add(point);
                    }
                }

                // Only add series if we have points - ADD THIS
                if (series.Points.Count > 0)
                {
                    chartTopVegetables.Series.Add(series);

                    // Basic legend on right (your existing code)
                    Legend leg = new Legend();
                    leg.Docking = Docking.Right;
                    chartTopVegetables.Legends.Add(leg);
                }
                else
                {
                    // Show no data message if no valid points
                    chartTopVegetables.Titles.Clear();
                    chartTopVegetables.Titles.Add("No Valid Data");
                }

            }
            catch (Exception ex)
            {
                // Enhanced error handling
                System.Diagnostics.Debug.WriteLine("Top vegetables chart load error: " + ex.Message);
                // Show error on chart if possible - ADD THIS
                if (chartTopVegetables != null && !chartTopVegetables.IsDisposed)
                {
                    try
                    {
                        chartTopVegetables.Titles.Clear();
                        chartTopVegetables.Titles.Add("Chart Unavailable");
                    }
                    catch { /* Ignore secondary errors */ }
                }
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
            Title dailyTitle = new Title("Daily Orders Trend");
            dailyTitle.Font = new Font("Arial", 10, FontStyle.Bold);
            dailyTitle.ForeColor = Color.FromArgb(0, 100, 0);
            chartDailyOrders.Titles.Add(dailyTitle);
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

        // Data Models
        public class DashboardData
        {

            public int OrdersToday { get; set; }
            public int OrdersYesterday { get; set; }

            public decimal TotalSalesToday { get; set; }
            public decimal TotalSalesYesterday { get; set; }

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
                string ordersYesterdayQuery =
    "SELECT COUNT(*) FROM tbl_Sales WHERE CAST(OrderDate AS DATE) = CAST(DATEADD(day, -1, GETDATE()) AS DATE)";

                using (SqlCommand cmd = new SqlCommand(ordersYesterdayQuery, con))
                {
                    data.OrdersYesterday = (int)cmd.ExecuteScalar();
                }

                string salesYesterdayQuery =
    "SELECT ISNULL(SUM(TotalAmount), 0) FROM tbl_Sales WHERE CAST(OrderDate AS DATE) = CAST(DATEADD(day, -1, GETDATE()) AS DATE)";

                using (SqlCommand cmd = new SqlCommand(salesYesterdayQuery, con))
                {
                    data.TotalSalesYesterday = (decimal)cmd.ExecuteScalar();
                }


                // Low Stock Count
                string lowStockQuery = @"
                    SELECT TOP 1 ProductName, Stock 
                    FROM tbl_Products 
                    WHERE Stock <= 10
                    ORDER BY Stock ASC";
                using (SqlCommand cmd = new SqlCommand(lowStockQuery, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string productName = reader["ProductName"].ToString();
                            int remainingKg = Convert.ToInt32(reader["Stock"]);
                            data.LowStockCount = $"{productName} ({remainingKg}kg)";
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
            LoadTopVegetablesChart();
            
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

        private void textBox1_TextChanged(object sender, EventArgs e) { }

        private void labelOrder_Click(object sender, EventArgs e) { }

        private void label4_Click(object sender, EventArgs e) { }

        private void labelUser_Click(object sender, EventArgs e) { }

        private void panel1_Paint(object sender, PaintEventArgs e) { }

        private void pictureBox2_Click(object sender, EventArgs e) { }

        private void btnLogin_Click(object sender, EventArgs e) { }

        // Add this method to fix CS1061 (designer expects it)
        private void panelNav_Paint(object sender, PaintEventArgs e)
        {
            // Leave empty unless custom painting is required
        }

        private void panel1_Paint_1(object sender, PaintEventArgs e) { }

        private void btnHome_Click(object sender, EventArgs e)
        {
            btnDashboard.BackColor = Color.FromArgb(0, 150, 0);
        }

        private void btnDOrder_Click(object sender, EventArgs e) { }

        private void label7_Click(object sender, EventArgs e) { }

        private void panel7_Paint(object sender, PaintEventArgs e) { }

        private void cartesianChart1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e) { }

        private void label2_Click(object sender, EventArgs e) { }

        private void pictureBox5_Click(object sender, EventArgs e) { }

        private void label13_Click(object sender, EventArgs e) { }

        private void label15_Click(object sender, EventArgs e) { }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
           
            SupplierForm supplierForm = new SupplierForm(currentUser, currentRole);
            supplierForm.Show();
            this.Close();
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

        private void panelDailyOrders_Paint(object sender, PaintEventArgs e) { }

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

        private void HideSettingsMenu()
        {
            panelSettings.Visible = false;
        }

         private void btnGeneral_Click(object sender, EventArgs e)
        {
            Registration registrationForm = new Registration();
            registrationForm.ShowDialog();


           
        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
          
        }

        private void btnSystem_Click(object sender, EventArgs e)
        {
           
        }

        private void btnSecurity_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Open Security Settings", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            HideSettingsMenu();
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Open Backup & Logs", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            HideSettingsMenu();
        }

        // Designer-required handlers added below
        private void panelSettings_Paint(object sender, PaintEventArgs e)
        {
            // No custom painting required
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Vegetable Ordering System\nVersion1.0\n©2024 Vegetable Corp", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
            HideSettingsMenu();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            // Placeholder
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Update clock labels
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
            lblDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
        }

        private void Sales_Click(object sender, EventArgs e)
        {
            Logs logs = new Logs(currentRole, currentUser);
            logs.Show();
        
         
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
            lblDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        }

        private void label3_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.Hide();
                Log_In loginForm = new Log_In();
                loginForm.Show();
                this.Close();
            }
        }
        private string GetTrend(decimal today, decimal yesterday)
        {
            if (yesterday == 0 && today > 0)
                return "▲ 100%";

            if (yesterday == 0 && today == 0)
                return "-";

            decimal change = ((today - yesterday) / yesterday) * 100;

            if (change > 0)
                return $"▲ {change:N0}%";
            else if (change < 0)
                return $"▼ {Math.Abs(change):N0}%";
            else
                return "No change";
        }

        public class WeeklySalesData
        {
            public string DayOfWeek { get; set; }
            public string DateRange { get; set; }
            public decimal TotalSales { get; set; }
            public int OrderCount { get; set; }
        }

        private List<WeeklySalesData> GetWeeklySalesData()
        {
            var weeklyData = new List<WeeklySalesData>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
            SELECT 
                DATEPART(WEEKDAY, OrderDate) AS DayNumber,
                DATENAME(WEEKDAY, OrderDate) AS DayName,
                CAST(MIN(OrderDate) AS DATE) AS StartDate,
                CAST(MAX(OrderDate) AS DATE) AS EndDate,
                ISNULL(SUM(TotalAmount), 0) AS TotalSales,
                COUNT(*) AS OrderCount
            FROM tbl_Sales 
            WHERE OrderDate >= DATEADD(DAY, -7, GETDATE())
            GROUP BY DATEPART(WEEKDAY, OrderDate), DATENAME(WEEKDAY, OrderDate)
            ORDER BY DATEPART(WEEKDAY, OrderDate)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        weeklyData.Add(new WeeklySalesData
                        {
                            DayOfWeek = reader["DayName"].ToString(),
                            DateRange = $"{Convert.ToDateTime(reader["StartDate"]):MM/dd} - {Convert.ToDateTime(reader["EndDate"]):MM/dd}",
                            TotalSales = Convert.ToDecimal(reader["TotalSales"]),
                            OrderCount = Convert.ToInt32(reader["OrderCount"])
                        });
                    }
                }

                // Fill in missing days with zero values
                string[] allDays = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

                foreach (string day in allDays)
                {
                    if (!weeklyData.Any(w => w.DayOfWeek.Equals(day, StringComparison.OrdinalIgnoreCase)))
                    {
                        weeklyData.Add(new WeeklySalesData
                        {
                            DayOfWeek = day,
                            DateRange = "No data",
                            TotalSales = 0,
                            OrderCount = 0
                        });
                    }
                }

                // Sort by day of week (Sunday = 1, Monday = 2, etc.)
                weeklyData = weeklyData.OrderBy(w => Array.IndexOf(allDays, w.DayOfWeek)).ToList();
            }

            return weeklyData;
        }

        private void InitializeWeeklySalesChart()
        {
            try
            {
                if (chartWeeklySales == null) return;

                // Clear existing series and titles
                chartWeeklySales.Series.Clear();
                chartWeeklySales.Titles.Clear();
                chartWeeklySales.ChartAreas.Clear();
                chartWeeklySales.Legends.Clear();

                // Create chart area
                ChartArea chartArea = new ChartArea();
                chartArea.AxisX.MajorGrid.Enabled = false;
                chartArea.AxisY.MajorGrid.Enabled = false;
                chartArea.AxisX.LabelStyle.Angle = -45;
                chartArea.AxisX.Interval = 1;
                chartArea.AxisY.Title = "Sales (₱)";
                chartArea.AxisX.Title = "Day of Week";
                chartArea.AxisY.TitleFont = new Font("Arial", 9, FontStyle.Bold);
                chartArea.AxisX.TitleFont = new Font("Arial", 9, FontStyle.Bold);
                chartWeeklySales.ChartAreas.Add(chartArea);

                // Add title
                Title title = new Title("Weekly Sales Trend");
                title.Font = new Font("Arial", 10, FontStyle.Bold);
                title.ForeColor = Color.FromArgb(0, 100, 0);
                chartWeeklySales.Titles.Add(title);

                // Create series for sales
                Series salesSeries = new Series("Sales");
                salesSeries.ChartType = SeriesChartType.Column;
                salesSeries.Color = Color.FromArgb(76, 175, 80); 
                salesSeries.IsValueShownAsLabel = true;
                salesSeries.LabelFormat = "₱#,##0";
                salesSeries.LabelForeColor = Color.White;
                salesSeries.Font = new Font("Arial", 8, FontStyle.Bold);
                salesSeries.YAxisType = AxisType.Primary;

                // Create series for order count (line)
                Series ordersSeries = new Series("Orders");
                ordersSeries.ChartType = SeriesChartType.Line;
                ordersSeries.Color = Color.FromArgb(255, 152, 0); 
                ordersSeries.IsValueShownAsLabel = true;
                ordersSeries.LabelForeColor = Color.DarkRed;
                ordersSeries.Font = new Font("Arial", 8, FontStyle.Bold);
                ordersSeries.YAxisType = AxisType.Secondary;
                ordersSeries.BorderWidth = 3;

                // Add series to chart
                chartWeeklySales.Series.Add(salesSeries);
                chartWeeklySales.Series.Add(ordersSeries);

                // Configure secondary Y axis for order count
                chartArea.AxisY2.Enabled = AxisEnabled.True;
                chartArea.AxisY2.MajorGrid.Enabled = false;
                chartArea.AxisY2.Title = "No. Orders";
                chartArea.AxisY2.TitleFont = new Font("Arial", 9, FontStyle.Bold);

                // Add legend
                Legend legend = new Legend();
                legend.Docking = Docking.Top;
                legend.Alignment = StringAlignment.Center;
                legend.Font = new Font("Arial", 9, FontStyle.Bold);
                chartWeeklySales.Legends.Add(legend);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing weekly sales chart: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadWeeklySalesChart()
        {
            try
            {
                if (chartWeeklySales == null || chartWeeklySales.IsDisposed) return;

                var weeklyData = GetWeeklySalesData();

                // Clear existing data
                chartWeeklySales.Series["Sales"].Points.Clear();
                chartWeeklySales.Series["Orders"].Points.Clear();

                // Add data points
                foreach (var data in weeklyData)
                {
                    // Add sales data (primary Y-axis) - CHANGED TO GREEN
                    DataPoint salesPoint = new DataPoint();
                    salesPoint.SetValueXY(data.DayOfWeek, (double)data.TotalSales);
                    salesPoint.Color = Color.FromArgb(76, 175, 80); // Fresh green color
                    salesPoint.LabelToolTip = $"₱{data.TotalSales:N2}\n{data.DateRange}";
                    chartWeeklySales.Series["Sales"].Points.Add(salesPoint);

                    // Add order count data (secondary Y-axis)
                    DataPoint ordersPoint = new DataPoint();
                    ordersPoint.SetValueXY(data.DayOfWeek, data.OrderCount);
                    ordersPoint.Color = Color.FromArgb(255, 152, 0); // Keep orange for orders line
                    ordersPoint.LabelToolTip = $"{data.OrderCount} orders\n{data.DateRange}";
                    chartWeeklySales.Series["Orders"].Points.Add(ordersPoint);
                }

                // Customize data point labels
                foreach (DataPoint point in chartWeeklySales.Series["Sales"].Points)
                {
                    point.Label = $"₱{point.YValues[0]:#,##0}";
                }

                foreach (DataPoint point in chartWeeklySales.Series["Orders"].Points)
                {
                    point.Label = $"{point.YValues[0]}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading weekly sales chart: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private (decimal TotalWeeklySales, int TotalWeeklyOrders, decimal AverageDailySales) GetWeeklySummary()
        {
            decimal totalSales = 0;
            int totalOrders = 0;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
            SELECT 
                ISNULL(SUM(TotalAmount), 0) AS TotalSales,
                COUNT(*) AS TotalOrders
            FROM tbl_Sales 
            WHERE OrderDate >= DATEADD(DAY, -7, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        totalSales = Convert.ToDecimal(reader["TotalSales"]);
                        totalOrders = Convert.ToInt32(reader["TotalOrders"]);
                    }
                }
            }

            decimal averageDailySales = totalOrders > 0 ? totalSales / 7 : 0;

            return (totalSales, totalOrders, averageDailySales);
        }

        private void LoadWeeklySummary()
        {
            try
            {
                var (totalSales, totalOrders, averageDailySales) = GetWeeklySummary();

                

                Console.WriteLine($"Weekly Summary - Sales: ₱{totalSales:N2}, Orders: {totalOrders}, Avg Daily: ₱{averageDailySales:N2}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading weekly summary: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lblDate_Click(object sender, EventArgs e)
        {

        }

        private void chartTopVegetables_Click(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {

        }

        private void chartWeeklySales_Click(object sender, EventArgs e)
        {

        }

        private void lblTotalSales_Click(object sender, EventArgs e)
        {

        }
    }
}