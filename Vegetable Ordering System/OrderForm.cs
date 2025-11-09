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
            // If you have payment type controls, implement here
            // For now, returning "Cash" as default
            return "Cash";
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

                        // Display change
                        txtChange.Text = $"₱{change:N2}";
                    }
                    else
                    {
                        txtChange.Text = "₱0.00";
                    }
                }
                else
                {
                    txtChange.Text = "₱0.00";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating change: {ex.Message}");
                txtChange.Text = "₱0.00";
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
                // REMOVED any reference to IsActive
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

            // Show your custom form
            EditPriceForm editForm = new EditPriceForm();
            editForm.ProductId = productId;
            editForm.LoadProductData(productName, currentPrice); // Pass data to form

            if (editForm.ShowDialog() == DialogResult.OK)
            {
                // Get the new price from the form and update database
                decimal newPrice = editForm.NewPrice;
                UpdateProductPrice(productId, newPrice, productName);
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
            LoadProductsToFlowLayout();
            InitializeOrderItemsTable();

            // Wire up the TextChanged event
            txtTotalPayment.TextChanged += txtTotalPayment_TextChanged;

            flowLayoutPanel2.FlowDirection = FlowDirection.LeftToRight;
            RoundCorners(btnBarcode, 20);
            RoundCorners(btnPlaceOrder, 20);
            RoundPanel(panel3, 20);
            RoundPanel(flowLayoutPanel2, 20);
            timer1.Start();
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

                // Validate customer name contains only letters and spaces
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

                // Collect order data
                Order order = CollectOrderData();

                // Save to database
                bool success = SaveOrderToDatabase(order);

                if (success)
                {
                    // UPDATE STOCK LEVELS
                    bool stockUpdated = UpdateStockForOrder(order);

                    if (stockUpdated)
                    {
                        MessageBox.Show($"Order placed successfully!\nOrder ID: {order.OrderId}", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
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

    
        }
    
}