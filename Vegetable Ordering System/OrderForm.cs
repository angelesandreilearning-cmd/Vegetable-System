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
using ZXing;    
using ZXing.Common;

using System.Drawing.Printing;

namespace Vegetable_Ordering_System
{
    public partial class OrderForm : Form
    {
        private string currentUser;
        private string currentRole;
        string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";
        private bool isSettingsMenuVisible = false;
        private DataTable orderItems = new DataTable();
        private int currentPage = 1;
        private int pageSize = 9; // 3 columns x 3 rows
        private int totalProducts = 0;
        private int totalPages = 0;
        private string currentSearchTerm = "";

        public OrderForm(string username, string role)
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

            errorProvider1.BlinkStyle = ErrorBlinkStyle.AlwaysBlink;
            errorProvider1.BlinkRate = 500;

            InitializePrintDocument();

        }
        private System.Drawing.Printing.PrintDocument printDocument;
        private Order currentOrder;

        private void InitializePrintDocument()
        {
            printDocument = new System.Drawing.Printing.PrintDocument();
            printDocument.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(PrintDocument_PrintPage);

            printDocument.DefaultPageSettings.PaperSize = new PaperSize("Short Bond", 850, 1100);
            printDocument.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);
        }
        private void PrintDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            if (currentOrder == null) return;

            Graphics graphics = e.Graphics;

            // Use monospaced font for receipt-like appearance
            Font companyFont = new Font("Courier New", 12, FontStyle.Bold); // Reduced from 14
            Font headerFont = new Font("Courier New", 9, FontStyle.Bold);   // Reduced from 10
            Font normalFont = new Font("Courier New", 8, FontStyle.Regular); // Reduced from 9
            Font smallFont = new Font("Courier New", 7, FontStyle.Regular);  // Reduced from 8
            Font totalFont = new Font("Courier New", 9, FontStyle.Bold);     // Reduced from 10

            // Receipt width - make it smaller to fit thermal paper
            int receiptWidth = 260; // Reduced from 280
            float leftMargin = 5;   // Reduced from 10
            float rightMargin = receiptWidth - 5;
            float yPos = 10;        // Start a bit higher

            // Center the receipt on the page
            float pageCenter = e.PageBounds.Width / 2;
            float receiptCenter = receiptWidth / 2;
            float xOffset = pageCenter - receiptCenter;

            // Company Header - Centered
            string companyName = "VEGETABLE ORDERING SYSTEM";
            SizeF companySize = graphics.MeasureString(companyName, companyFont);
            graphics.DrawString(companyName, companyFont, Brushes.Black,
                xOffset + (receiptWidth - companySize.Width) / 2, yPos);
            yPos += companySize.Height + 5;

            // Address/Contact info
            string address = "Fresh Vegetables Market";
            string contact = "Contact: 0912-345-6789";
            SizeF addressSize = graphics.MeasureString(address, smallFont);
            SizeF contactSize = graphics.MeasureString(contact, smallFont);

            graphics.DrawString(address, smallFont, Brushes.Black,
                xOffset + (receiptWidth - addressSize.Width) / 2, yPos);
            yPos += addressSize.Height + 3;
            graphics.DrawString(contact, smallFont, Brushes.Black,
                xOffset + (receiptWidth - contactSize.Width) / 2, yPos);
            yPos += contactSize.Height + 8;

            // Separator line
            graphics.DrawLine(new Pen(Color.Black, 1), xOffset + leftMargin, yPos, xOffset + rightMargin, yPos);
            yPos += 8;

            // Order Information in two columns
            float col1 = xOffset + leftMargin;
            float col2 = xOffset + receiptWidth / 2;

            graphics.DrawString("ORDER RECEIPT", headerFont, Brushes.Black, col1, yPos);
            yPos += headerFont.Height + 5;

            // Order ID
            graphics.DrawString($"Order ID:", normalFont, Brushes.Black, col1, yPos);
            graphics.DrawString($"{currentOrder.OrderId}", headerFont, Brushes.Black, col2, yPos);
            yPos += normalFont.Height + 3;

            // Date - FIXED: Use shorter format to prevent overflow
            string shortDate = FormatDateForReceipt(currentOrder.DateDisplay);
            graphics.DrawString($"Date:", normalFont, Brushes.Black, col1, yPos);
            graphics.DrawString($"{shortDate}", normalFont, Brushes.Black, col2, yPos);
            yPos += normalFont.Height + 3;

            // Time
            graphics.DrawString($"Time:", normalFont, Brushes.Black, col1, yPos);
            graphics.DrawString($"{currentOrder.TimeDisplay}", normalFont, Brushes.Black, col2, yPos);
            yPos += normalFont.Height + 3;

            // Customer
            graphics.DrawString($"Customer:", normalFont, Brushes.Black, col1, yPos);
            string shortCustomer = currentOrder.CustomerName.Length > 15 ?
                currentOrder.CustomerName.Substring(0, 12) + "..." : currentOrder.CustomerName;
            graphics.DrawString($"{shortCustomer}", normalFont, Brushes.Black, col2, yPos);
            yPos += normalFont.Height + 3;

            // Payment
            graphics.DrawString($"Payment:", normalFont, Brushes.Black, col1, yPos);
            graphics.DrawString($"{currentOrder.PaymentType}", normalFont, Brushes.Black, col2, yPos);
            yPos += normalFont.Height + 3;

            // GCash reference if applicable - FIXED LAYOUT
            if (currentOrder.PaymentType == "GCash")
            {
                string gcashRef = GetGCashReference(currentOrder.OrderId);
                if (!string.IsNullOrEmpty(gcashRef))
                {
                    // Truncate GCash reference if too long
                    string displayRef = gcashRef.Length > 16 ? gcashRef.Substring(0, 13) + "..." : gcashRef;

                    graphics.DrawString($"GCash Ref:", normalFont, Brushes.Black, col1, yPos);
                    graphics.DrawString($"{displayRef}", smallFont, Brushes.Black, col2, yPos); // Use smaller font
                    yPos += normalFont.Height + 3;
                }
            }

            yPos += 3;

            // Items separator
            graphics.DrawLine(new Pen(Color.Black, 1), xOffset + leftMargin, yPos, xOffset + rightMargin, yPos);
            yPos += 8;

            // Column headers for items - ADJUSTED POSITIONS
            graphics.DrawString("ITEM", headerFont, Brushes.Black, xOffset + leftMargin, yPos);
            graphics.DrawString("QTY", headerFont, Brushes.Black, xOffset + leftMargin + 110, yPos); // Adjusted
            graphics.DrawString("AMOUNT", headerFont, Brushes.Black, xOffset + leftMargin + 160, yPos); // Adjusted
            yPos += headerFont.Height + 3;

            // Line under headers
            graphics.DrawLine(new Pen(Color.Black, 1), xOffset + leftMargin, yPos, xOffset + rightMargin, yPos);
            yPos += 5;

            // Order Items
            foreach (var item in currentOrder.OrderItems)
            {
                // Check if we need a new page
                if (yPos > e.PageBounds.Height - 120) // Increased buffer
                {
                    e.HasMorePages = true;
                    return;
                }

                // Product name (truncate if too long)
                string productName = item.ProductName;
                if (productName.Length > 18) // Reduced from 20
                {
                    productName = productName.Substring(0, 15) + "...";
                }

                graphics.DrawString(productName, normalFont, Brushes.Black, xOffset + leftMargin, yPos);
                graphics.DrawString($"{item.Quantity}kg", normalFont, Brushes.Black, xOffset + leftMargin + 110, yPos);
                graphics.DrawString($"₱{item.TotalPrice:N2}", normalFont, Brushes.Black, xOffset + leftMargin + 160, yPos);
                yPos += normalFont.Height + 2;

                // Show unit price below product name
                graphics.DrawString($"@₱{item.UnitPrice:N2}/kg", smallFont, Brushes.Black, xOffset + leftMargin + 3, yPos);
                yPos += smallFont.Height + 4; // Reduced spacing
            }

            yPos += 3;

            // Total separator (double line)
            graphics.DrawLine(new Pen(Color.Black, 1), xOffset + leftMargin, yPos, xOffset + rightMargin, yPos);
            yPos += 2;
            graphics.DrawLine(new Pen(Color.Black, 1), xOffset + leftMargin, yPos, xOffset + rightMargin, yPos);
            yPos += 6;

            // Total Amount - ADJUSTED POSITIONS
            graphics.DrawString("TOTAL:", totalFont, Brushes.Black, xOffset + leftMargin + 110, yPos);
            graphics.DrawString($"₱{currentOrder.TotalAmount:N2}", totalFont, Brushes.Black, xOffset + leftMargin + 160, yPos);
            yPos += totalFont.Height + 8;

            // Payment details section
            graphics.DrawString("PAYMENT DETAILS", headerFont, Brushes.Black, xOffset + leftMargin, yPos);
            yPos += headerFont.Height + 5;

            if (currentOrder.PaymentType == "Cash")
            {
                // Get payment amount from textbox
                if (decimal.TryParse(txtTotalPayment.Text, out decimal payment))
                {
                    graphics.DrawString("Amount Paid:", normalFont, Brushes.Black, xOffset + leftMargin, yPos);
                    graphics.DrawString($"₱{payment:N2}", normalFont, Brushes.Black, xOffset + leftMargin + 160, yPos);
                    yPos += normalFont.Height + 3;

                    decimal change = payment - currentOrder.TotalAmount;
                    graphics.DrawString("Change:", normalFont, Brushes.Black, xOffset + leftMargin, yPos);
                    graphics.DrawString($"₱{change:N2}", normalFont, Brushes.Black, xOffset + leftMargin + 160, yPos);
                    yPos += normalFont.Height + 8;
                }
            }
            else if (currentOrder.PaymentType == "GCash")
            {
                graphics.DrawString("Payment Method:", normalFont, Brushes.Black, xOffset + leftMargin, yPos);
                graphics.DrawString("GCash", normalFont, Brushes.Black, xOffset + leftMargin + 160, yPos);
                yPos += normalFont.Height + 3;

                string gcashRef = GetGCashReference(currentOrder.OrderId);
                if (!string.IsNullOrEmpty(gcashRef))
                {
                    string displayRef = gcashRef.Length > 16 ? gcashRef.Substring(0, 13) + "..." : gcashRef;
                    graphics.DrawString("Reference No:", normalFont, Brushes.Black, xOffset + leftMargin, yPos);
                    graphics.DrawString($"{displayRef}", smallFont, Brushes.Black, xOffset + leftMargin + 160, yPos);
                    yPos += normalFont.Height + 3;
                }

                graphics.DrawString("Status:", normalFont, Brushes.Black, xOffset + leftMargin, yPos);
                graphics.DrawString("PAID", headerFont, Brushes.Black, xOffset + leftMargin + 160, yPos);
                yPos += normalFont.Height + 8;
            }

            // Payment separator
            graphics.DrawLine(new Pen(Color.Black, 1), xOffset + leftMargin, yPos, xOffset + rightMargin, yPos);
            yPos += 8;

            // Thank you message
            string thankYou = "Thank you for your order!";
            SizeF thankYouSize = graphics.MeasureString(thankYou, normalFont);
            graphics.DrawString(thankYou, normalFont, Brushes.Black,
                xOffset + (receiptWidth - thankYouSize.Width) / 2, yPos);
            yPos += thankYouSize.Height + 5;

            // Processed by
            graphics.DrawString($"Processed by: {currentOrder.CreatedBy}", smallFont, Brushes.Black,
                xOffset + leftMargin, yPos);
            yPos += smallFont.Height + 5;

            // Footer separator
            graphics.DrawLine(new Pen(Color.Black, 1), xOffset + leftMargin, yPos, xOffset + rightMargin, yPos);
            yPos += 8;

            // Return policy/note
            string returnPolicy = "Goods sold are not returnable";
            string visitAgain = "Please visit again!";

            SizeF policySize = graphics.MeasureString(returnPolicy, smallFont);
            SizeF visitSize = graphics.MeasureString(visitAgain, smallFont);

            graphics.DrawString(returnPolicy, smallFont, Brushes.Black,
                xOffset + (receiptWidth - policySize.Width) / 2, yPos);
            yPos += smallFont.Height + 3;
            graphics.DrawString(visitAgain, smallFont, Brushes.Black,
                xOffset + (receiptWidth - visitSize.Width) / 2, yPos);

            e.HasMorePages = false;
        }

        // ADD THIS METHOD to format date properly
        private string FormatDateForReceipt(string originalDate)
        {
            try
            {
                // Convert "Monday, November 17, 2025" to "Mon, Nov 17, 2025"
                DateTime date = DateTime.Parse(originalDate);
                return date.ToString("ddd, MMM dd, yyyy"); // Shorter format
            }
            catch
            {
                // Fallback: just take first 15 characters if parsing fails
                return originalDate.Length > 15 ? originalDate.Substring(0, 15) + "..." : originalDate;
            }
        }
        private string GetGCashReference(string orderId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT GCashReference FROM tbl_Sales WHERE OrderID = @OrderID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        object result = cmd.ExecuteScalar();

                        // Debug output to see what's happening
                        string reference = result?.ToString() ?? "";
                        Console.WriteLine($"DEBUG - OrderID: {orderId}, GCashRef: '{reference}'");

                        return reference;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving GCash reference: {ex.Message}");
                return "";
            }
        }
      
        private void SaveGCashReference(string orderId, string gcashReference)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "UPDATE tbl_Sales SET GCashReference = @GCashReference WHERE OrderID = @OrderID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@GCashReference", gcashReference);
                        cmd.Parameters.AddWithValue("@OrderID", orderId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        Console.WriteLine($"DEBUG - Saved GCashRef: '{gcashReference}' for OrderID: {orderId}, Rows affected: {rowsAffected}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving GCash reference: {ex.Message}");
                MessageBox.Show($"Error saving GCash reference: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public class OrderItem
        {
            public string ProductName { get; set; }
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
        }

        public class Order
        {
            public string OrderId { get; set; }
            public string CustomerName { get; set; }
            public string PaymentType { get; set; }
            public DateTime OrderDate { get; set; }
            public string DateDisplay { get; set; }
            public string TimeDisplay { get; set; }
            public decimal TotalAmount { get; set; }
            public string CreatedBy { get; set; }
            public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        }

        // Method to generate Order ID
        private string GenerateOrderId()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT ISNULL(MAX(CAST(REPLACE(OrderID, '#', '') AS INT)), 0) FROM tbl_Sales";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    int maxOrderNumber = (int)cmd.ExecuteScalar();
                    return $"#{(maxOrderNumber + 1).ToString("D4")}";
                }
            }
        }

        // Method to save order to database
        private bool SaveOrderToDatabase(Order order)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            // Insert into tbl_Sales
                            string salesQuery = @"INSERT INTO tbl_Sales 
                                        (OrderID, CustomerName, PaymentType, OrderDate, TotalAmount, DateDisplay, TimeDisplay, CreatedBy) 
                                        OUTPUT INSERTED.SaleID
                                        VALUES (@OrderID, @CustomerName, @PaymentType, @OrderDate, @TotalAmount, @DateDisplay, @TimeDisplay, @CreatedBy)";

                            int saleId;
                            using (SqlCommand salesCmd = new SqlCommand(salesQuery, con, transaction))
                            {
                                salesCmd.Parameters.AddWithValue("@OrderID", order.OrderId);
                                salesCmd.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                                salesCmd.Parameters.AddWithValue("@PaymentType", order.PaymentType);
                                salesCmd.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                                salesCmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                                salesCmd.Parameters.AddWithValue("@DateDisplay", order.DateDisplay);
                                salesCmd.Parameters.AddWithValue("@TimeDisplay", order.TimeDisplay);
                                salesCmd.Parameters.AddWithValue("@CreatedBy", order.CreatedBy);

                                saleId = (int)salesCmd.ExecuteScalar();
                            }

                            // Insert order items into tbl_Sales_Items
                            string itemsQuery = @"INSERT INTO tbl_Sales_Items 
                                        (SaleID, ProductName, Quantity, UnitPrice, TotalPrice) 
                                        VALUES (@SaleID, @ProductName, @Quantity, @UnitPrice, @TotalPrice)";

                            using (SqlCommand itemsCmd = new SqlCommand(itemsQuery, con, transaction))
                            {
                                foreach (var item in order.OrderItems)
                                {
                                    itemsCmd.Parameters.Clear();
                                    itemsCmd.Parameters.AddWithValue("@SaleID", saleId);
                                    itemsCmd.Parameters.AddWithValue("@ProductName", item.ProductName);
                                    itemsCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    itemsCmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                                    itemsCmd.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);

                                    itemsCmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Error saving order: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Connection error: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        // Method to collect order data from form
        private Order CollectOrderData()
        {
            Order order = new Order();

            // Get order details from form controls
            order.OrderId = GenerateOrderId();
            order.CustomerName = txtCustomer.Text.Trim();
            order.PaymentType = GetSelectedPaymentType();
            order.OrderDate = DateTime.Now;
            order.DateDisplay = lblDate.Text;
            order.TimeDisplay = lblTime.Text;
            order.CreatedBy = currentUser;

            // Get total amount from label (remove "Total: ₱" and parse)
            string totalText = lblTotal.Text.Replace("Total: ₱", "").Trim();
            if (decimal.TryParse(totalText, out decimal totalAmount))
            {
                order.TotalAmount = totalAmount;
            }
            else
            {
                order.TotalAmount = 0;
            }

            // Get order items from DataGridView
            order.OrderItems = GetOrderItemsFromDataGrid();

            return order;
        }

        // Method to get selected payment type
        private string GetSelectedPaymentType()
        {
            if (cmbPaymentType != null && cmbPaymentType.SelectedItem != null)
            {
                return cmbPaymentType.SelectedItem.ToString();
            }
            return "Cash"; // Default fallback
        }

        // Method to get order items from DataGridView
        private List<OrderItem> GetOrderItemsFromDataGrid()
        {
            List<OrderItem> items = new List<OrderItem>();

            foreach (DataGridViewRow row in dataGridViewOrderSummary.Rows)
            {
                if (!row.IsNewRow && row.Cells["Product"].Value != null)
                {
                    try
                    {
                        OrderItem item = new OrderItem();
                        item.ProductName = row.Cells["Product"].Value?.ToString() ?? "";

                        // Parse quantity (remove " kg" and parse)
                        string qtyText = row.Cells["Qty"].Value?.ToString().Replace(" kg", "") ?? "0";
                        if (decimal.TryParse(qtyText, out decimal quantity))
                        {
                            item.Quantity = quantity;
                        }
                        else
                        {
                            item.Quantity = 0;
                        }

                        // Parse unit price (remove "₱" and "/kg" and parse)
                        string unitPriceText = row.Cells["Unit Price"].Value?.ToString().Replace("₱", "").Replace("/kg", "") ?? "0";
                        if (decimal.TryParse(unitPriceText, out decimal unitPrice))
                        {
                            item.UnitPrice = unitPrice;
                        }
                        else
                        {
                            item.UnitPrice = 0;
                        }

                        // Parse total price (remove "₱" and parse)
                        string totalPriceText = row.Cells["Total"].Value?.ToString().Replace("₱", "") ?? "0";
                        if (decimal.TryParse(totalPriceText, out decimal totalPrice))
                        {
                            item.TotalPrice = totalPrice;
                        }
                        else
                        {
                            item.TotalPrice = 0;
                        }

                        items.Add(item);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error parsing order item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }

            return items;
        }

        // Method to reset order form after successful order
        private void ResetOrderForm()
        {
            // Clear order items
            orderItems.Clear();
            dataGridViewOrderSummary.DataSource = null;

            // Reset total
            lblTotal.Text = "Total: ₱0.00";

            // Clear customer name
            txtCustomer.Text = "";

            // Clear payment fields
            txtTotalPayment.Text = "";
            txtChange.Text = "₱0.00";

            // Clear any error provider
            errorProvider1.SetError(txtCustomer, "");
        }

        // Add this to initialize order items table
        private void InitializeOrderItemsTable()
        {
            if (orderItems.Columns.Count == 0)
            {
                orderItems.Columns.Add("Product", typeof(string));
                orderItems.Columns.Add("Qty", typeof(string));
                orderItems.Columns.Add("Unit Price", typeof(string));
                orderItems.Columns.Add("Total", typeof(string));
            }
        }

        // Calculate change automatically
        private void CalculateChange()
        {
            try
            {
                // Get payment amount
                if (decimal.TryParse(txtTotalPayment.Text, out decimal payment))
                {
                    // Get total amount - remove everything except numbers and decimal point
                    string totalText = lblTotal.Text.Replace("Total: ", "").Replace("₱", "").Replace("₽", "").Trim();

                    if (decimal.TryParse(totalText, out decimal total))
                    {
                        // Calculate change
                        decimal change = payment - total;

                        // Display change with color coding
                        if (change < 0)
                        {
                            txtChange.Text = $"₱{change:N2}";
                            txtChange.ForeColor = Color.Red; // Red for negative
                            txtChange.BackColor = Color.LightPink; // Light red background
                        }
                        else
                        {
                            txtChange.Text = $"₱{change:N2}";
                            txtChange.ForeColor = Color.Green; // Green for positive
                            txtChange.BackColor = Color.LightGreen; // Light green background
                        }
                    }
                    else
                    {
                        txtChange.Text = "₱0.00";
                        txtChange.ForeColor = Color.Black;
                        txtChange.BackColor = SystemColors.Window;
                    }
                }
                else
                {
                    txtChange.Text = "₱0.00";
                    txtChange.ForeColor = Color.Black;
                    txtChange.BackColor = SystemColors.Window;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating change: {ex.Message}");
                txtChange.Text = "₱0.00";
                txtChange.ForeColor = Color.Black;
                txtChange.BackColor = SystemColors.Window;
            }
        }

        private void txtTotalPayment_TextChanged(object sender, EventArgs e)
        {
            CalculateChange();
        }

        private void btnInventory_Click(object sender, EventArgs e)
        {
            InventoryForm inventoryForm = new InventoryForm(currentUser, currentRole);
            inventoryForm.Show();
            this.Close();
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            SupplierForm supplierForm = new SupplierForm(currentUser, currentRole);
            supplierForm.Show();
            this.Close();
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            MainMenuForm adminForm = new MainMenuForm(currentUser, currentRole);
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

        // Step 3: Modified method with search and pagination
        private void LoadProductsToFlowLayout(string searchTerm = "", int page = 1)
        {
            Console.WriteLine($"Loading products - Search: '{searchTerm}', Page: {page}");

            flowLayoutPanel2.Controls.Clear();
            currentPage = page;
            currentSearchTerm = searchTerm;

            if (currentPage < 1)
            {
                currentPage = 1;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // Step 1: Get total product count
                    string countQuery = "SELECT COUNT(*) FROM tbl_Products WHERE Stock > 0"; // Only show products with stock
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        countQuery += " AND (ProductName LIKE @SearchTerm OR Category LIKE @SearchTerm)";
                    }

                    using (SqlCommand countCmd = new SqlCommand(countQuery, con))
                    {
                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            countCmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                        }
                        totalProducts = Convert.ToInt32(countCmd.ExecuteScalar());
                        Console.WriteLine($"Total products found: {totalProducts}");
                    }

                    // Step 2: Calculate pagination
                    totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
                    if (totalPages == 0) totalPages = 1;

                    if (currentPage > totalPages)
                    {
                        currentPage = totalPages;
                    }

                    Console.WriteLine($"Total pages: {totalPages}, Current page: {currentPage}");

                    // Step 3: Build main query - UPDATED to include ImagePath
                    string baseQuery = @"
                SELECT ProductID, ProductName, Price, Stock, ImagePath, Category, SupplierID 
                FROM tbl_Products 
                WHERE Stock > 0"; // Only show products with available stock

                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        baseQuery += " AND (ProductName LIKE @SearchTerm OR Category LIKE @SearchTerm)";
                    }

                    baseQuery += " ORDER BY ProductName";

                    // Add pagination
                    int skip = (page - 1) * pageSize;
                    baseQuery += $" OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY";

                    Console.WriteLine($"Executing query: {baseQuery}");

                    // Step 4: Load products
                    using (SqlCommand cmd = new SqlCommand(baseQuery, con))
                    {
                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            int productCount = 0;
                            while (reader.Read())
                            {
                                productCount++;
                                ProductItemControl productControl = new ProductItemControl();
                                productControl.ProductID = Convert.ToInt32(reader["ProductID"]);
                                productControl.ProductName = reader["ProductName"].ToString();
                                productControl.Price = Convert.ToDecimal(reader["Price"]);
                                productControl.Stock = Convert.ToInt32(reader["Stock"]);

                                // UPDATED: Load product image with proper handling
                                if (!reader.IsDBNull(reader.GetOrdinal("ImagePath")))
                                {
                                    string imageFileName = reader["ImagePath"].ToString();
                                    Console.WriteLine($"Loading image: {imageFileName}");
                                    Image productImage = LoadProductImage(imageFileName);
                                    productControl.ProductImage = productImage;
                                }
                                else
                                {
                                    Console.WriteLine($"No image for product: {reader["ProductName"]}");
                                    productControl.ProductImage = GetDefaultProductImage();
                                }

                                // Set Admin mode
                                productControl.SetAdminMode(currentRole == "Admin");

                                // Handle Edit event
                                productControl.EditPriceClicked += (sender, productId) =>
                                {
                                    ShowEditPriceDialog(productId);
                                };

                                // Handle Delete event  
                                productControl.DeleteProductClicked += (sender, productId) =>
                                {
                                    DeleteProduct(productId);
                                };

                                // Handle product image click event
                                productControl.ProductImageClicked += (sender, e) =>
                                {
                                    AddProductToOrder(e.ProductID, e.Name, e.Price, e.Quantity);
                                };

                                flowLayoutPanel2.Controls.Add(productControl);
                                Console.WriteLine($"✅ Added product: {productControl.ProductName}");
                            }
                            Console.WriteLine($"Loaded {productCount} products");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading products: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            Console.WriteLine($"=== PAGINATION DEBUG ===");
            Console.WriteLine($"Total Products: {totalProducts}");
            Console.WriteLine($"Total Pages: {totalPages}");
            Console.WriteLine($"Current Page: {currentPage}");
            Console.WriteLine($"FlowLayout Controls Count: {flowLayoutPanel2.Controls.Count}");
            Console.WriteLine($"=== END DEBUG ===");

            // Step 5: Add pagination controls
            AddPaginationControls();
        }

        private void AddPaginationControls()
        {
            Console.WriteLine("=== ADD PAGINATION CONTROLS START ===");

            // Create modern pagination container
            Panel paginationPanel = new Panel();
            paginationPanel.Name = "paginationPanel";
            paginationPanel.Size = new Size(Math.Max(flowLayoutPanel2.Width - 60, 400), 52);
            paginationPanel.BackColor = Color.Transparent;
            paginationPanel.Margin = new Padding(20, 25, 20, 15);

            // Calculate total width for centering
            int buttonCount = Math.Min(totalPages, 5);
            int totalWidth = 90 + 90 + (buttonCount * 42) + 120;
            int startX = (paginationPanel.Width - totalWidth) / 2;
            startX = Math.Max(startX, 15);

            // Create container for buttons and info
            Panel container = new Panel();
            container.Size = new Size(totalWidth, 52);
            container.Location = new Point(startX, 0);
            container.BackColor = Color.Transparent;

            // Modern Previous button
            Button btnPrev = new Button();
            btnPrev.Text = "← Prev";
            btnPrev.Size = new Size(85, 32);
            btnPrev.Location = new Point(0, 10);
            btnPrev.BackColor = currentPage > 1 ? Color.FromArgb(74, 107, 255) : Color.FromArgb(225, 225, 230);
            btnPrev.ForeColor = currentPage > 1 ? Color.White : Color.FromArgb(150, 150, 150);
            btnPrev.FlatStyle = FlatStyle.Flat;
            btnPrev.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            btnPrev.Cursor = currentPage > 1 ? Cursors.Hand : Cursors.Default;
            btnPrev.FlatAppearance.BorderSize = 0;
            btnPrev.Enabled = currentPage > 1;
            btnPrev.Click += (sender, e) => LoadProductsToFlowLayout(currentSearchTerm, currentPage - 1);
            container.Controls.Add(btnPrev);

            int buttonStartX = 95;

            // Modern page number buttons
            if (totalPages > 0)
            {
                int startPage = Math.Max(1, currentPage - 2);
                int endPage = Math.Min(totalPages, startPage + 4);

                if (endPage - startPage < 4 && startPage > 1)
                {
                    startPage = Math.Max(1, endPage - 4);
                }

                for (int i = startPage; i <= endPage; i++)
                {
                    bool isActive = i == currentPage;
                    Button btnPage = new Button();
                    btnPage.Text = i.ToString();
                    btnPage.Size = new Size(36, 32);
                    btnPage.Location = new Point(buttonStartX, 10);
                    btnPage.BackColor = isActive ? Color.FromArgb(74, 107, 255) : Color.Transparent;
                    btnPage.ForeColor = isActive ? Color.White : Color.FromArgb(100, 100, 100);
                    btnPage.FlatStyle = FlatStyle.Flat;
                    btnPage.Font = new Font("Segoe UI", 9, isActive ? FontStyle.Bold : FontStyle.Regular);
                    btnPage.Cursor = Cursors.Hand;
                    btnPage.FlatAppearance.BorderSize = 0;

                    int pageNum = i;
                    btnPage.Click += (sender, e) => LoadProductsToFlowLayout(currentSearchTerm, pageNum);

                    container.Controls.Add(btnPage);
                    buttonStartX += 42;
                }
            }

            // Modern Next button
            Button btnNext = new Button();
            btnNext.Text = "Next →";
            btnNext.Size = new Size(85, 32);
            btnNext.Location = new Point(buttonStartX, 10);
            bool nextEnabled = currentPage < totalPages && totalPages > 0;
            btnNext.BackColor = nextEnabled ? Color.FromArgb(74, 107, 255) : Color.FromArgb(225, 225, 230);
            btnNext.ForeColor = nextEnabled ? Color.White : Color.FromArgb(150, 150, 150);
            btnNext.FlatStyle = FlatStyle.Flat;
            btnNext.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            btnNext.Cursor = nextEnabled ? Cursors.Hand : Cursors.Default;
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.Enabled = nextEnabled;
            btnNext.Click += (sender, e) => LoadProductsToFlowLayout(currentSearchTerm, currentPage + 1);
            container.Controls.Add(btnNext);

            // Modern info label
            Label lblPageInfo = new Label();
            string infoText;
            if (totalProducts == 0)
            {
                infoText = "No products found";
            }
            else if (totalPages == 1)
            {
                infoText = $"{totalProducts} product{(totalProducts > 1 ? "s" : "")}";
            }
            else
            {
                infoText = $"Page {currentPage} of {totalPages}•{totalProducts} prod.";
            }
            lblPageInfo.Text = infoText;
            lblPageInfo.AutoSize = true;
            lblPageInfo.Location = new Point(buttonStartX + 95, 16);
            lblPageInfo.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblPageInfo.ForeColor = Color.FromArgb(120, 120, 120);
            container.Controls.Add(lblPageInfo);

            paginationPanel.Controls.Add(container);
            flowLayoutPanel2.Controls.Add(paginationPanel);

            Console.WriteLine("✅ Modern pagination controls added successfully");
            Console.WriteLine("=== ADD PAGINATION CONTROLS END ===");
        }

        private void AddProductToOrder(int productId, string productName, decimal price, decimal quantity)
        {
            // ADD STOCK CHECK FIRST - updated to use decimal
            if (!CheckStockAvailability(productName, quantity))
            {
                MessageBox.Show($"Insufficient stock for {productName}. Please check inventory.",
                    "Stock Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Initialize order items table if not exists
            InitializeOrderItemsTable();

            // Check if product already exists in order
            DataRow[] existingRows = orderItems.Select($"Product = '{productName.Replace("'", "''")}'");

            if (existingRows.Length > 0)
            {
                // Update existing item quantity
                DataRow row = existingRows[0];
                decimal currentQty = decimal.Parse(row["Qty"].ToString().Replace(" kg", ""));
                decimal newQty = currentQty + quantity;

                // CHECK STOCK AGAIN FOR UPDATED QUANTITY
                if (!CheckStockAvailability(productName, newQty))
                {
                    MessageBox.Show($"Insufficient stock for {productName}. Available quantity exceeded.",
                        "Stock Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                row["Qty"] = $"{newQty} kg";
                row["Total"] = $"₱{(newQty * price):N2}";
            }
            else
            {
                // Add new item to order
                DataRow newRow = orderItems.NewRow();
                newRow["Product"] = productName;
                newRow["Qty"] = $"{quantity} kg";
                newRow["Unit Price"] = $"₱{price:N2}/kg";
                newRow["Total"] = $"₱{(price * quantity):N2}";
                orderItems.Rows.Add(newRow);
            }

            // Refresh DataGridView
            dataGridViewOrderSummary.DataSource = null;
            dataGridViewOrderSummary.AutoGenerateColumns = true;
            dataGridViewOrderSummary.DataSource = orderItems;

            // Update total
            UpdateOrderTotal();
        }

        private void UpdateOrderTotal()
        {
            decimal total = 0;
            foreach (DataRow row in orderItems.Rows)
            {
                // Remove ₱ symbol and parse the total
                string totalText = row["Total"].ToString().Replace("₱", "").Trim();
                if (decimal.TryParse(totalText, out decimal itemTotal))
                {
                    total += itemTotal;
                }
            }
            lblTotal.Text = $"Total: ₱{total:N2}";

            // Recalculate change when total updates
            CalculateChange();
        }

        private Image LoadProductImage(string imageFileName)
        {
            try
            {
                if (string.IsNullOrEmpty(imageFileName))
                {
                    Console.WriteLine("Image filename is empty, using default image");
                    return GetDefaultProductImage();
                }

                string imagesFolder = Path.Combine(Application.StartupPath, "ProductImages");
                string fullImagePath = Path.Combine(imagesFolder, imageFileName);

                Console.WriteLine($"Looking for image at: {fullImagePath}");

                if (File.Exists(fullImagePath))
                {
                    Console.WriteLine($"Image found: {fullImagePath}");
                    // Load image without locking the file
                    using (FileStream fs = new FileStream(fullImagePath, FileMode.Open, FileAccess.Read))
                    {
                        return Image.FromStream(fs);
                    }
                }
                else
                {
                    Console.WriteLine($"Image not found: {fullImagePath}");
                    return GetDefaultProductImage();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image '{imageFileName}': {ex.Message}");
                return GetDefaultProductImage();
            }
        }

        private void DeleteProduct(int productId)
        {
            string productName = "";

            // Get product name for confirmation message
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT ProductName FROM tbl_Products WHERE ProductID = @ProductID";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productId);
                    productName = cmd.ExecuteScalar()?.ToString() ?? "Unknown Product";
                }
            }

            // Confirmation dialog
            DialogResult result = MessageBox.Show(
                $"Are you sure you want to PERMANENTLY delete '{productName}'?",
                "Confirm Permanent Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        // CHANGED TO HARD DELETE
                        string query = "DELETE FROM tbl_Products WHERE ProductID = @ProductID";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@ProductID", productId);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Product deleted permanently!", "Success",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                                RefreshProducts(); // This maintains search and pagination
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting product: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowEditPriceDialog(int productId)
        {
            string productName = "";
            decimal currentPrice = 0;

            // Get current product details from database
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT ProductName, Price FROM tbl_Products WHERE ProductID = @ProductID";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            productName = reader["ProductName"].ToString();
                            currentPrice = Convert.ToDecimal(reader["Price"]);
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(productName))
            {
                MessageBox.Show("Product not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Show input dialog for new price
            using (var editForm = new Form())
            {
                editForm.Text = "Edit Product Price";
                editForm.Size = new Size(300, 200);
                editForm.StartPosition = FormStartPosition.CenterScreen;
                editForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                editForm.MaximizeBox = false;
                editForm.MinimizeBox = false;

                // Product info label
                var lblInfo = new Label()
                {
                    Text = $"Product: {productName}",
                    Location = new Point(20, 20),
                    Size = new Size(250, 20),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                // Current price label
                var lblCurrent = new Label()
                {
                    Text = $"Current Price: ₱{currentPrice:N2}",
                    Location = new Point(20, 50),
                    Size = new Size(250, 20)
                };

                // New price label
                var lblNew = new Label()
                {
                    Text = "New Price:",
                    Location = new Point(20, 80),
                    Size = new Size(80, 20)
                };

                // Numeric input for new price
                var numNewPrice = new NumericUpDown()
                {
                    Location = new Point(100, 78),
                    Size = new Size(120, 20),
                    Minimum = 0.1m,
                    Maximum = 10000,
                    DecimalPlaces = 2,
                    Value = currentPrice
                };

                // OK button
                var btnOK = new Button()
                {
                    Text = "Update",
                    Location = new Point(70, 120),
                    Size = new Size(75, 30),
                    DialogResult = DialogResult.OK
                };

                // Cancel button
                var btnCancel = new Button()
                {
                    Text = "Cancel",
                    Location = new Point(155, 120),
                    Size = new Size(75, 30),
                    DialogResult = DialogResult.Cancel
                };

                // Add controls to form
                editForm.Controls.AddRange(new Control[] { lblInfo, lblCurrent, lblNew, numNewPrice, btnOK, btnCancel });
                editForm.AcceptButton = btnOK;
                editForm.CancelButton = btnCancel;

                // Focus on price input
                numNewPrice.Select(0, numNewPrice.Text.Length);

                // Show dialog and process result
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    decimal newPrice = numNewPrice.Value;
                    if (newPrice != currentPrice)
                    {
                        UpdateProductPrice(productId, newPrice, productName);
                    }
                }
            }
        }


        private Image GetDefaultProductImage()
        {
            // Create a modern default placeholder image
            Bitmap bmp = new Bitmap(160, 140);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(240, 240, 240));

                // Draw a vegetable icon
                using (Pen pen = new Pen(Color.FromArgb(180, 180, 180), 2))
                {
                    g.DrawEllipse(pen, 50, 30, 60, 60); // Main circle
                    g.DrawLine(pen, 80, 90, 80, 110);   // Stem
                }

                using (Font font = new Font("Segoe UI", 8))
                using (StringFormat sf = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    g.DrawString("No Image", font, Brushes.DarkGray,
                        new Rectangle(0, 0, 160, 140), sf);
                }
            }
            return bmp;
        }

        private void UpdateProductPrice(int productId, decimal newPrice, string productName)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "UPDATE tbl_Products SET Price = @Price WHERE ProductID = @ProductID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Price", newPrice);
                        cmd.Parameters.AddWithValue("@ProductID", productId);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show($"Price updated successfully!\n\nProduct: {productName}\nNew Price: ₱{newPrice:N2}",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Refresh the product list
                            RefreshProducts();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating price: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ADD THIS METHOD: Check stock availability before adding to order
        private bool CheckStockAvailability(string productName, decimal quantity)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                // REMOVED IsActive condition
                string query = "SELECT Stock FROM tbl_Products WHERE ProductName = @ProductName";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ProductName", productName);

                object result = cmd.ExecuteScalar();
                if (result == null) return false;

                int currentStock = Convert.ToInt32(result);
                return currentStock >= quantity;
            }
        }

        // ADD THIS METHOD: Get SaleID for stock transactions
        private int GetSaleId(string orderId, SqlConnection con, SqlTransaction transaction)
        {
            string query = "SELECT SaleID FROM tbl_Sales WHERE OrderID = @OrderID";
            SqlCommand cmd = new SqlCommand(query, con, transaction);
            cmd.Parameters.AddWithValue("@OrderID", orderId);
            return (int)cmd.ExecuteScalar();
        }

        // ADD THIS METHOD: Update stock after order is placed
        private bool UpdateStockForOrder(Order order)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            // Get the SaleID for this order
                            int saleId = GetSaleId(order.OrderId, con, transaction);

                            foreach (var item in order.OrderItems)
                            {
                                using (SqlCommand cmd = new SqlCommand("sp_DeductStockForSale", con, transaction))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@ProductName", item.ProductName);
                                    cmd.Parameters.AddWithValue("@Quantity", item.Quantity); // Keep as decimal
                                    cmd.Parameters.AddWithValue("@SaleID", saleId);

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception($"Stock update failed: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating stock: {ex.Message}", "Stock Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        private void btnGeneral_Click(object sender, EventArgs e)
        {
            Registration registrationForm = new Registration();
            registrationForm.ShowDialog();
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
        private void InitializePaymentComboBox()
        {
            cmbPaymentType.Items.Clear();
            cmbPaymentType.Items.Add("Cash");
            cmbPaymentType.Items.Add("GCash");
            cmbPaymentType.SelectedIndex = 0; // Default to Cash
        }
        private void OrderForm_Load(object sender, EventArgs e)
        {
            LoadProductsToFlowLayout();
            InitializeOrderItemsTable();
            InitializeOrderNumberSystem();

            if (cmbPaymentType.Items.Count == 0)
            {
                cmbPaymentType.Items.Add("Cash");
                cmbPaymentType.Items.Add("GCash");
                cmbPaymentType.SelectedIndex = 0;
            }
            txtTotalPayment.TextChanged += txtTotalPayment_TextChanged;

            flowLayoutPanel2.FlowDirection = FlowDirection.LeftToRight;

            RoundCorners(btnPlaceOrder, 20);
            RoundPanel(panel3, 20);
            RoundPanel(flowLayoutPanel2, 20);
            timer1.Start();
            txtBarcode.Focus();
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
            lblDate1.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
            lblTime1.Text = DateTime.Now.ToString("hh:mm:ss tt");
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

        private void lblDate_Click(object sender, EventArgs e)
        {

        }

        private void dataGridViewOrderSummary_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click_1(object sender, EventArgs e)
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

        private void btnPlaceOrder_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate customer name
                if (string.IsNullOrWhiteSpace(txtCustomer.Text))
                {
                    MessageBox.Show("Please enter customer name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCustomer.Focus();
                    return;
                }

                // Validate customer name format
                Regex regex = new Regex(@"^[a-zA-Z\s]+$");
                if (!regex.IsMatch(txtCustomer.Text))
                {
                    MessageBox.Show("Please enter only letters and spaces for customer name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCustomer.Focus();
                    return;
                }

                // Validate order has items
                if (orderItems.Rows.Count == 0)
                {
                    MessageBox.Show("Please add products to the order.", "No Items", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate payment based on payment type
                string paymentType = GetSelectedPaymentType();

                if (paymentType == "Cash")
                {
                    // Validate cash payment
                    if (string.IsNullOrWhiteSpace(txtTotalPayment.Text))
                    {
                        MessageBox.Show("Please enter payment amount for cash payment.", "Payment Required",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtTotalPayment.Focus();
                        return;
                    }

                    if (IsChangeNegative())
                    {
                        MessageBox.Show("Payment amount is insufficient. Please enter a higher payment amount.",
                            "Insufficient Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtTotalPayment.Focus();
                        txtTotalPayment.SelectAll();
                        return;
                    }
                }
                // For GCash, validation happens in CompleteOrderPlacement

                // Collect order data
                Order order = CollectOrderData();
                currentOrder = order; // Store for printing

                // Save to database and handle payment
                CompleteOrderPlacement(order);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error placing order: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool IsChangeNegative()
        {
            try
            {
                // Get payment amount
                if (decimal.TryParse(txtTotalPayment.Text, out decimal payment))
                {
                    // Get total amount - remove everything except numbers and decimal point
                    string totalText = lblTotal.Text.Replace("Total: ", "").Replace("₱", "").Replace("₽", "").Trim();

                    if (decimal.TryParse(totalText, out decimal total))
                    {
                        // Calculate change
                        decimal change = payment - total;

                        // Return true if change is negative (insufficient payment)
                        return change < 0;
                    }
                }
                // If we can't parse the amounts or payment field is empty, treat as negative
                return true;
            }
            catch (Exception)
            {
                // If there's any error, treat as negative to prevent order completion
                return true;
            }
        }
        private void PrintReceipt()
        {
            try
            {
                if (currentOrder == null)
                {
                    MessageBox.Show("No order data available for printing.", "Print Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // SIMPLIFIED: Just show print dialog directly
                PrintDialog printDialog = new PrintDialog();
                printDialog.Document = printDocument;

                // THIS IS THE KEY LINE - ShowDialog(this) makes it modal and centered
                DialogResult result = printDialog.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    printDocument.Print();
                    MessageBox.Show("Receipt sent to printer!", "Print Successful",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Printing cancelled.", "Print Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing receipt: {ex.Message}", "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
       
        private void ShowPrintPreview()
        {
            if (currentOrder == null)
            {
                MessageBox.Show("No order data available for preview.", "Preview Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PrintPreviewDialog previewDialog = new PrintPreviewDialog();
            previewDialog.Document = printDocument;
            previewDialog.ShowDialog();
        }
        private void btnBarcode_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Barcode scanner functionality would open here");
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();
            Console.WriteLine($"Search button clicked. Term: '{searchTerm}'");
            LoadProductsToFlowLayout(searchTerm, 1);
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Console.WriteLine("Enter key pressed in search box");
                btnSearch_Click(sender, e);
                e.Handled = true;
            }
        }

        private void RefreshProducts()
        {
            Console.WriteLine("Refreshing product list...");
            LoadProductsToFlowLayout(currentSearchTerm, currentPage);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();
            Console.WriteLine($"Search button clicked. Term: '{searchTerm}'");
            LoadProductsToFlowLayout(searchTerm, 1);
        }

        private void txtBarcode_TextChanged(object sender, EventArgs e)
        {

        }
        private string ScanBarcodeFromImage(string imagePath)
        {
            try
            {
                var barcodeReader = new BarcodeReader();
                barcodeReader.Options.TryHarder = true;

                using (Bitmap bitmap = new Bitmap(imagePath))
                {
                    var result = barcodeReader.Decode(bitmap);
                    return result?.Text;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Scan failed: {ex.Message}");
            }
        }
        private void FindProductByBarcode(string barcode)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT ProductID, ProductName, Price, Stock FROM tbl_Products WHERE Barcode = @Barcode AND Stock > 0";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Barcode", barcode);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int productId = Convert.ToInt32(reader["ProductID"]);
                                string productName = reader["ProductName"].ToString();
                                decimal price = Convert.ToDecimal(reader["Price"]);
                                int stock = Convert.ToInt32(reader["Stock"]);

                                // Ask for quantity using a custom dialog
                                decimal quantity = ShowQuantityDialog(productName, stock);

                                if (quantity > 0)
                                {
                                    if (quantity <= stock)
                                    {
                                        AddProductToOrder(productId, productName, price, quantity);
                                        MessageBox.Show($"'{productName}' ({quantity} kg) added to order!\nTotal: ₱{(price * quantity):N2}",
                                            "Product Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    else
                                    {
                                        MessageBox.Show($"Insufficient stock! Available: {stock} kg",
                                            "Stock Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("No product found with this barcode or product is out of stock.", "Not Found",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finding product: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private decimal ShowQuantityDialog(string productName, int maxStock)
        {
            using (var quantityForm = new Form())
            {
                quantityForm.Text = "Enter Quantity";
                quantityForm.Size = new Size(300, 180);
                quantityForm.StartPosition = FormStartPosition.CenterScreen;
                quantityForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                quantityForm.MaximizeBox = false;
                quantityForm.MinimizeBox = false;

                // Product info label
                var lblInfo = new Label()
                {
                    Text = $"{productName}\nAvailable: {maxStock} kg",
                    Location = new Point(20, 20),
                    Size = new Size(250, 40),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                // Quantity label
                var lblQuantity = new Label()
                {
                    Text = "Quantity (kg):",
                    Location = new Point(20, 70),
                    Size = new Size(80, 20)
                };

                // Numeric input for quantity
                var numQuantity = new NumericUpDown()
                {
                    Location = new Point(110, 68),
                    Size = new Size(100, 20),
                    Minimum = 0.1m,
                    Maximum = maxStock,
                    DecimalPlaces = 2,
                    Value = 1.0m
                };

                // OK button
                var btnOK = new Button()
                {
                    Text = "OK",
                    Location = new Point(70, 110),
                    Size = new Size(75, 30),
                    DialogResult = DialogResult.OK
                };

                // Cancel button
                var btnCancel = new Button()
                {
                    Text = "Cancel",
                    Location = new Point(155, 110),
                    Size = new Size(75, 30),
                    DialogResult = DialogResult.Cancel
                };

                // Add controls to form
                quantityForm.Controls.AddRange(new Control[] { lblInfo, lblQuantity, numQuantity, btnOK, btnCancel });
                quantityForm.AcceptButton = btnOK;
                quantityForm.CancelButton = btnCancel;

                // Focus on quantity input
                numQuantity.Select(0, numQuantity.Text.Length);

                // Show dialog and return result
                if (quantityForm.ShowDialog() == DialogResult.OK)
                {
                    return numQuantity.Value;
                }
                else
                {
                    return 0; // User cancelled
                }
            }
        }

        private void TxtBarcode_TextChanged_1(object sender, EventArgs e)
        {
            string barcode = txtBarcode.Text.Trim();

            if (!string.IsNullOrEmpty(barcode) && barcode.Length >= 10)
            {
                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                timer.Interval = 300; // 0.3 second delay
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    timer.Dispose();

                    FindProductByBarcode(barcode);

                    // Clear for next scan
                    txtBarcode.Text = "";
                    txtBarcode.Focus();
                };
                timer.Start();
            }
        }

        private void btnScanBarcode_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
                    openFileDialog.Title = "Select barcode image to scan";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string barcodeText = ScanBarcodeFromImage(openFileDialog.FileName);

                        if (!string.IsNullOrEmpty(barcodeText))
                        {
                            txtBarcode.Text = barcodeText;
                            // AUTO-ADD TO ORDER - this is what you want!
                            FindProductByBarcode(barcodeText);
                        }
                        else
                        {
                            MessageBox.Show("No barcode found in the selected image.", "Scan Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error scanning barcode: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtBarcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                string barcode = txtBarcode.Text.Trim();

                if (!string.IsNullOrEmpty(barcode))
                {
                    FindProductByBarcode(barcode);
                    txtBarcode.Text = "";
                }
                e.Handled = true;
            }
        }

       

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtSearch_KeyPress_1(object sender, KeyPressEventArgs e)
        {
           
        }

        private void txtSearch_Click(object sender, EventArgs e)
        {
            txtSearch.ForeColor = Color.Black;
            txtSearch.Clear();
        }

        private void btnPreviewOrder_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate required fields first
                if (string.IsNullOrWhiteSpace(txtCustomer.Text))
                {
                    MessageBox.Show("Please enter customer name.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCustomer.Focus();
                    return;
                }

                // Validate customer name format
                Regex regex = new Regex(@"^[a-zA-Z\s]+$");
                if (!regex.IsMatch(txtCustomer.Text))
                {
                    MessageBox.Show("Please enter only letters and spaces for customer name.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCustomer.Focus();
                    return;
                }

                // Validate order has items
                if (orderItems.Rows.Count == 0)
                {
                    MessageBox.Show("Please add products to the order.", "No Items",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Collect order data for preview
                Order order = CollectOrderData();
                currentOrder = order; // Store for preview/printing

                // Show preview
                ShowOrderPreview(order);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating preview: {ex.Message}", "Preview Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ShowOrderPreview(Order order)
        {
            using (OrderPreviewForm previewForm = new OrderPreviewForm(order))
            {
                DialogResult result = previewForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // User clicked "Print Receipt" in preview
                    PrintReceipt();

                    // Ask if they want to place the order after printing
                    DialogResult placeOrderResult = MessageBox.Show(
                        "Do you want to place this order now?",
                        "Place Order",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (placeOrderResult == DialogResult.Yes)
                    {
                        CompleteOrderPlacement(order);
                    }
                }
                else
                {
                    // User clicked "Back to Edit" - just return to form for editing
                    MessageBox.Show("You can continue editing your order.",
                        "Order Preview Closed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }
        private void CompleteOrderPlacement(Order order)
        {
            try
            {
                string gcashReference = null;

                // Handle GCash payment - show reference number dialog FIRST
                if (order.PaymentType == "GCash")
                {
                    gcashReference = ShowGCashReferenceDialog();

                    if (string.IsNullOrEmpty(gcashReference))
                    {
                        MessageBox.Show("GCash payment cancelled. Order not placed.", "Payment Cancelled",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return; // Don't proceed with order placement
                    }
                }

                // Save to database FIRST
                bool success = SaveOrderToDatabase(order);

                if (success)
                {
                    // NOW save GCash reference to database
                    if (order.PaymentType == "GCash" && !string.IsNullOrEmpty(gcashReference))
                    {
                        SaveGCashReference(order.OrderId, gcashReference);

                        // Update currentOrder for printing
                        if (currentOrder != null)
                        {
                            // Add GCashReference property to Order class or store it temporarily
                            // For now, we'll ensure the database has it and refresh
                        }
                    }

                    // Update stock levels
                    bool stockUpdated = UpdateStockForOrder(order);

                    if (stockUpdated)
                    {
                        // Show payment summary
                        string paymentSummary = $"Order placed successfully!\nOrder ID: {order.OrderId}";

                        if (order.PaymentType == "Cash")
                        {
                            if (decimal.TryParse(txtTotalPayment.Text, out decimal payment))
                            {
                                decimal change = payment - order.TotalAmount;
                                paymentSummary += $"\nCash: ₱{payment:N2}";
                                paymentSummary += $"\nChange: ₱{change:N2}";
                            }
                        }
                        else if (order.PaymentType == "GCash")
                        {
                            paymentSummary += $"\nPayment: GCash";
                            if (!string.IsNullOrEmpty(gcashReference))
                            {
                                paymentSummary += $"\nReference: {gcashReference}";
                            }
                        }

                        // Ask if user wants to print receipt for BOTH payment types
                        DialogResult printResult = MessageBox.Show(
                            $"{paymentSummary}\n\nDo you want to print the receipt?",
                            "Order Success",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (printResult == DialogResult.Yes)
                        {
                            // IMPORTANT: Refresh the GCash reference from database before printing
                            if (order.PaymentType == "GCash")
                            {
                                // Small delay to ensure database is updated
                                System.Threading.Thread.Sleep(100);
                            }
                            PrintReceipt();
                        }

                        // Update order number display
                        UpdateOrderNumberDisplay();

                        ResetOrderForm();
                    }
                    else
                    {
                        MessageBox.Show("Order was saved but stock update failed. Please contact administrator.",
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error placing order: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            UpdateOrderNumberDisplay();
        }
        private void InitializeOrderNumberSystem()
        {
            UpdateOrderNumberDisplay();
            StyleOrderNumberLabel();
        }

        private void UpdateOrderNumberDisplay()
        {
            string nextOrderId = GenerateOrderId();
            lblOrderNumber.Text = $"Next Order: {nextOrderId}";
        }

        private void StyleOrderNumberLabel()
        {
            lblOrderNumber.TextAlign = ContentAlignment.MiddleCenter;
            lblOrderNumber.ForeColor = Color.DarkGreen;
            lblOrderNumber.BackColor = Color.Transparent;
            lblOrderNumber.Font = new Font("Segoe UI", 11, FontStyle.Bold); 
            lblOrderNumber.BorderStyle = BorderStyle.None; 
            lblOrderNumber.Padding = new Padding(0);

          
            lblOrderNumber.Region = null;
        }

        private void RoundLabelCorners(Label label, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(new Rectangle(0, 0, radius, radius), 180, 90);
            path.AddLine(radius, 0, label.Width - radius, 0);
            path.AddArc(new Rectangle(label.Width - radius, 0, radius, radius), 270, 90);
            path.AddLine(label.Width, radius, label.Width, label.Height - radius);
            path.AddArc(new Rectangle(label.Width - radius, label.Height - radius, radius, radius), 0, 90);
            path.AddLine(label.Width - radius, label.Height, radius, label.Height);
            path.AddArc(new Rectangle(0, label.Height - radius, radius, radius), 90, 90);
            path.CloseFigure();

            label.Region = new Region(path);
        }

        private void lblOrderNumber_Click(object sender, EventArgs e)
        {
            UpdateOrderNumberDisplay();
            ShowOrderNumberDetails();
        }
        private void ShowOrderNumberDetails()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string nextOrder = GenerateOrderId();

                    string todayQuery = "SELECT COUNT(*) FROM tbl_Sales WHERE CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)";
                    string totalQuery = "SELECT COUNT(*) FROM tbl_Sales";

                    int todayOrders = 0;
                    int totalOrders = 0;

                    using (SqlCommand cmd = new SqlCommand(todayQuery, con))
                    {
                        todayOrders = (int)cmd.ExecuteScalar();
                    }

                    using (SqlCommand cmd = new SqlCommand(totalQuery, con))
                    {
                        totalOrders = (int)cmd.ExecuteScalar();
                    }

                    string details = $"Next Order: {nextOrder}\n" +
                                   $"Today's Orders: {todayOrders}\n" +
                                   $"Total Orders: {totalOrders}";

                    MessageBox.Show(details, "Order Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving order details: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ShowGCashReferenceDialog()
        {
            using (var gcashForm = new Form())
            {
                gcashForm.Text = "GCash Payment - Reference Number";
                gcashForm.Size = new Size(400, 200);
                gcashForm.StartPosition = FormStartPosition.CenterScreen;
                gcashForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                gcashForm.MaximizeBox = false;
                gcashForm.MinimizeBox = false;

                // Instruction label
                var lblInstruction = new Label()
                {
                    Text = "Please enter the GCash reference number:",
                    Location = new Point(20, 20),
                    Size = new Size(350, 20),
                    Font = new Font("Segoe UI", 10, FontStyle.Regular)
                };

                // Reference number input
                var txtReference = new TextBox()
                {
                    Location = new Point(20, 50),
                    Size = new Size(340, 25),
                    Font = new Font("Segoe UI", 10, FontStyle.Regular),
                    Text = "Enter GCash reference number...",
                    ForeColor = Color.Gray,
                    MaxLength = 20
                };

                // Store original text for placeholder behavior
                bool isPlaceholder = true;

                // Handle focus events for placeholder behavior
                txtReference.Enter += (s, ev) =>
                {
                    if (isPlaceholder)
                    {
                        txtReference.Text = "";
                        txtReference.ForeColor = Color.Black;
                        isPlaceholder = false;
                    }
                };

                txtReference.Leave += (s, ev) =>
                {
                    if (string.IsNullOrWhiteSpace(txtReference.Text))
                    {
                        txtReference.Text = "Enter GCash reference number...";
                        txtReference.ForeColor = Color.Gray;
                        isPlaceholder = true;
                    }
                };

                // Validation label
                var lblValidation = new Label()
                {
                    Text = "Reference number must be 10-20 characters",
                    Location = new Point(20, 80),
                    Size = new Size(350, 20),
                    Font = new Font("Segoe UI", 8, FontStyle.Italic),
                    ForeColor = Color.Gray
                };

                // OK button
                var btnOK = new Button()
                {
                    Text = "Confirm",
                    Location = new Point(120, 110),
                    Size = new Size(80, 30),
                    DialogResult = DialogResult.OK,
                    BackColor = Color.FromArgb(0, 123, 255),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                // Cancel button
                var btnCancel = new Button()
                {
                    Text = "Cancel",
                    Location = new Point(210, 110),
                    Size = new Size(80, 30),
                    DialogResult = DialogResult.Cancel,
                    BackColor = Color.FromArgb(108, 117, 125),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular)
                };

                // Add controls to form
                gcashForm.Controls.AddRange(new Control[] { lblInstruction, txtReference, lblValidation, btnOK, btnCancel });
                gcashForm.AcceptButton = btnOK;
                gcashForm.CancelButton = btnCancel;

                // Focus on reference input
                txtReference.Focus();
                txtReference.SelectAll();

                // Show dialog and return result
                if (gcashForm.ShowDialog() == DialogResult.OK)
                {
                    string reference = txtReference.Text.Trim();

                    // Check if it's still the placeholder text
                    if (isPlaceholder || reference == "Enter GCash reference number..." || string.IsNullOrWhiteSpace(reference))
                    {
                        MessageBox.Show("Please enter a GCash reference number.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return ShowGCashReferenceDialog(); // Show dialog again
                    }

                    if (reference.Length < 10 || reference.Length > 20)
                    {
                        MessageBox.Show("Reference number must be between 10-20 characters.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return ShowGCashReferenceDialog(); // Show dialog again
                    }

                    return reference;
                }
                else
                {
                    return null; // User cancelled
                }
            }
        }
    }

        }