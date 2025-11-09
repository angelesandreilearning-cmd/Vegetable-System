using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
namespace Vegetable_Ordering_System
{
    public partial class Logs : Form
    {
        string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";

        private string currentUser;
        private string currentRole;

        public Logs(string username, string role)
        {
            InitializeComponent();
            currentUser = username;
            currentRole = role;
        }

        private void Logs_Load(object sender, EventArgs e)
        {
            InitializeDatePickers();
            LoadTransactionData();
            LoadComboBoxes();
            InitializeComboBoxPlaceholders();
        }

        private void LoadTransactionData(string query = null, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    if (string.IsNullOrEmpty(query))
                    {
                        query = @"SELECT 
                                    SaleID,
                                    OrderID,
                                    CustomerName,
                                    PaymentType,
                                    OrderDate,
                                    TotalAmount,
                                    DateDisplay,
                                    TimeDisplay,
                                    Status,
                                    CreatedBy,
                                    CreatedAt
                                  FROM tbl_Sales
                                  ORDER BY SaleID DESC";
                    }

                    SqlCommand cmd = new SqlCommand(query, con);
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvLogs.DataSource = dt;

                    dgvLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgvLogs.ReadOnly = true;
                    dgvLogs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading transactions: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void InitializeComboBoxPlaceholders()
        {
            SetComboPlaceholder(cmbUser, "User");
            SetComboPlaceholder(cmbStatus, "Status");
            SetComboPlaceholder(cmbPayment, "Payment");
        }

        private void SetComboPlaceholder(ComboBox combo, string placeholder)
        {
            combo.ForeColor = Color.Gray;
            combo.Text = placeholder;

            combo.GotFocus += (s, e) =>
            {
                if (combo.Text == placeholder)
                {
                    combo.Text = "";
                    combo.ForeColor = Color.Black;
                }
            };

            combo.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(combo.Text))
                {
                    combo.Text = placeholder;
                    combo.ForeColor = Color.Gray;
                }
            };
        }
        private void LoadComboBoxes()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Load users from tbl_Users - use Name instead of Username
                    SqlCommand cmdUser = new SqlCommand("SELECT UserID, Name, Role FROM tbl_Users WHERE Name IS NOT NULL AND Name != ''", con);
                    SqlDataReader drUser = cmdUser.ExecuteReader();
                    cmbUser.Items.Clear();
                    while (drUser.Read())
                    {
                        // Store both name and role for display
                        string userInfo = $"{drUser["Name"]} ({drUser["Role"]})";
                        cmbUser.Items.Add(userInfo);
                    }
                    drUser.Close();

                    // Load statuses from database
                    SqlCommand cmdStatus = new SqlCommand("SELECT DISTINCT Status FROM tbl_Sales WHERE Status IS NOT NULL AND Status != ''", con);
                    SqlDataReader drStatus = cmdStatus.ExecuteReader();
                    cmbStatus.Items.Clear();
                    while (drStatus.Read())
                    {
                        cmbStatus.Items.Add(drStatus["Status"].ToString());
                    }
                    drStatus.Close();

                    // Load payment types from database
                    SqlCommand cmdPayment = new SqlCommand("SELECT DISTINCT PaymentType FROM tbl_Sales WHERE PaymentType IS NOT NULL AND PaymentType != ''", con);
                    SqlDataReader drPayment = cmdPayment.ExecuteReader();
                    cmbPayment.Items.Clear();
                    while (drPayment.Read())
                    {
                        cmbPayment.Items.Add(drPayment["PaymentType"].ToString());
                    }
                    drPayment.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading combo boxes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeDatePickers()
        {
            // Set to 2023 dates by default
            dtpFrom.Value = new DateTime(2023, 1, 1);
            dtpTo.Value = new DateTime(2023, 12, 31);

            // Always show actual dates, no placeholders
            dtpFrom.Format = DateTimePickerFormat.Short;
            dtpTo.Format = DateTimePickerFormat.Short;
            dtpFrom.ForeColor = Color.Black;
            dtpTo.ForeColor = Color.Black;
        }



        private void SearchTransactions()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"SELECT 
                SaleID,
                OrderID,
                CustomerName,
                PaymentType,
                OrderDate,
                TotalAmount,
                DateDisplay,
                TimeDisplay,
                Status,
                CreatedBy,
                CreatedAt
            FROM tbl_Sales 
            WHERE 1=1";

                    List<SqlParameter> parameters = new List<SqlParameter>();

                    // ALWAYS use date range - no placeholder checks needed
                    query += " AND (OrderDate BETWEEN @FromDate AND @ToDate OR CreatedAt BETWEEN @FromDate AND @ToDate)";
                    parameters.Add(new SqlParameter("@FromDate", dtpFrom.Value.Date));
                    parameters.Add(new SqlParameter("@ToDate", dtpTo.Value.Date.AddDays(1).AddSeconds(-1)));

                    // Add user filter if selected and not placeholder
                    if (!string.IsNullOrWhiteSpace(cmbUser.Text) && cmbUser.Text != "User" && cmbUser.SelectedItem != null)
                    {
                        // Extract just the name from "Name (Role)" format
                        string selectedUserName = cmbUser.Text.Split('(')[0].Trim();

                        // We need to get the username that corresponds to this name for filtering
                        string username = GetUsernameByName(selectedUserName);
                        if (!string.IsNullOrEmpty(username))
                        {
                            query += " AND CreatedBy = @User";
                            parameters.Add(new SqlParameter("@User", username));
                        }
                    }

                    // Add status filter if selected and not placeholder
                    if (!string.IsNullOrWhiteSpace(cmbStatus.Text) && cmbStatus.Text != "Status" && cmbStatus.SelectedItem != null)
                    {
                        query += " AND Status = @Status";
                        parameters.Add(new SqlParameter("@Status", cmbStatus.Text));
                    }

                    // Add payment type filter if selected and not placeholder
                    if (!string.IsNullOrWhiteSpace(cmbPayment.Text) && cmbPayment.Text != "Payment" && cmbPayment.SelectedItem != null)
                    {
                        query += " AND PaymentType = @PaymentType";
                        parameters.Add(new SqlParameter("@PaymentType", cmbPayment.Text));
                    }

                    query += " ORDER BY OrderDate DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            dgvLogs.DataSource = dataTable;

                            // Format column headers
                            FormatDataGridViewColumns();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching transactions: {ex.Message}", "Search Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string GetUsernameByName(string name)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT Username FROM tbl_Users WHERE Name = @Name", con);
                    cmd.Parameters.AddWithValue("@Name", name);

                    object result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting username: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
        }
        // Extract column formatting to a separate method for better organization
        private void FormatDataGridViewColumns()
        {
            if (dgvLogs.Columns.Contains("SaleID"))
                dgvLogs.Columns["SaleID"].HeaderText = "Sale ID";
            if (dgvLogs.Columns.Contains("OrderID"))
                dgvLogs.Columns["OrderID"].HeaderText = "Order ID";
            if (dgvLogs.Columns.Contains("CustomerName"))
                dgvLogs.Columns["CustomerName"].HeaderText = "Customer Name";
            if (dgvLogs.Columns.Contains("PaymentType"))
                dgvLogs.Columns["PaymentType"].HeaderText = "Payment Type";
            if (dgvLogs.Columns.Contains("TotalAmount"))
            {
                dgvLogs.Columns["TotalAmount"].HeaderText = "Total Amount";
                dgvLogs.Columns["TotalAmount"].DefaultCellStyle.Format = "N2";
                dgvLogs.Columns["TotalAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dgvLogs.Columns.Contains("DateDisplay"))
                dgvLogs.Columns["DateDisplay"].HeaderText = "Date Display";
            if (dgvLogs.Columns.Contains("TimeDisplay"))
                dgvLogs.Columns["TimeDisplay"].HeaderText = "Time Display";
            if (dgvLogs.Columns.Contains("Status"))
                dgvLogs.Columns["Status"].HeaderText = "Status";
            if (dgvLogs.Columns.Contains("CreatedBy"))
                dgvLogs.Columns["CreatedBy"].HeaderText = "Created By";
            if (dgvLogs.Columns.Contains("CreatedAt"))
                dgvLogs.Columns["CreatedAt"].HeaderText = "Created At";

            // Optional: Auto-resize columns for better visibility
            dgvLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void dtpTo_ValueChanged(object sender, EventArgs e)
        {
            if (dtpTo.Value >= dtpFrom.Value)
            {
                SearchTransactions();
            }
            else
            {
                MessageBox.Show("To date cannot be less than From date", "Invalid Date Range",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private bool ValidateDates()
        {
            if (dtpFrom.Value > dtpTo.Value)
            {
                MessageBox.Show("From date cannot be greater than To date", "Invalid Date Range",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void btnSearch_Click_1(object sender, EventArgs e)
        {
            if (!ValidateDates())
                return;

            SearchTransactions();
        }

        private void cmbPayment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPayment.SelectedItem != null && cmbPayment.Text != "Payment")
                SearchTransactions();
        }

        private void cmbUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbUser.SelectedItem != null && cmbUser.Text != "User")
                SearchTransactions();
        }

        private void cmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStatus.SelectedItem != null && cmbStatus.Text != "Status")
                SearchTransactions();
        }

        private void btnCLear_Click(object sender, EventArgs e)
        {
            ClearFilters();
        }
        private void ClearFilters()
        {
            try
            {
                // Reset to 2023 dates instead of placeholders
                dtpFrom.Value = new DateTime(2023, 1, 1);
                dtpTo.Value = new DateTime(2023, 12, 31);
                dtpFrom.Format = DateTimePickerFormat.Short;
                dtpTo.Format = DateTimePickerFormat.Short;
                dtpFrom.ForeColor = Color.Black;
                dtpTo.ForeColor = Color.Black;

                // Reset combo boxes
                cmbUser.SelectedIndex = -1;
                cmbUser.Text = "User";
                cmbUser.ForeColor = Color.Gray;

                cmbStatus.SelectedIndex = -1;
                cmbStatus.Text = "Status";
                cmbStatus.ForeColor = Color.Gray;

                cmbPayment.SelectedIndex = -1;
                cmbPayment.Text = "Payment";
                cmbPayment.ForeColor = Color.Gray;

                // Refresh with current filters (which will be 2023 dates)
                SearchTransactions();

                MessageBox.Show("All filters have been cleared", "Clear Filters",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error clearing filters: " + ex.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportToPDF();
        }
        private void ExportToPDF()
        {
            try
            {
                if (dgvLogs.Rows.Count == 0)
                {
                    MessageBox.Show("No data to export!", "Export to PDF",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                saveFileDialog.Title = "Export Transaction History to PDF";
                saveFileDialog.FileName = $"Transaction_History_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Create PDF document (Landscape for better table view)
                    Document document = new Document(PageSize.A4.Rotate(), 20, 20, 30, 30);
                    PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(saveFileDialog.FileName, FileMode.Create));

                    document.Open();

                    // Add title - use fully qualified names for fonts
                    iTextSharp.text.Font titleFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 18, iTextSharp.text.BaseColor.DARK_GRAY);
                    Paragraph title = new Paragraph("TRANSACTION HISTORY REPORT", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 20;
                    document.Add(title);

                    // Add report information with professional formatting
                    iTextSharp.text.Font infoFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10, iTextSharp.text.BaseColor.BLACK);
                    iTextSharp.text.Font labelFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 10, iTextSharp.text.BaseColor.BLACK);

                    Paragraph info = new Paragraph();

                    // Date Range
                    info.Add(new Chunk("Date Range: ", labelFont));
                    info.Add(new Chunk($"{GetDateRangeText()}\n", infoFont));

                    // Applied Filters
                    info.Add(new Chunk("Applied Filters: ", labelFont));
                    info.Add(new Chunk($"{GetProfessionalFilterInfo()}\n", infoFont));

                    // Report Generation Details
                    info.Add(new Chunk("Report Generated: ", labelFont));
                    info.Add(new Chunk($"{DateTime.Now:MMMM dd, yyyy 'at' hh:mm tt}\n", infoFont));

                    // Generated By with proper name formatting
                    info.Add(new Chunk("Generated By: ", labelFont));
                    info.Add(new Chunk($"{GetFormattedUserName()}\n", infoFont));

                    // Total Records
                    info.Add(new Chunk("Total Records: ", labelFont));
                    info.Add(new Chunk($"{dgvLogs.Rows.Count:N0}\n\n", infoFont));

                    info.SpacingAfter = 15;
                    document.Add(info);

                    // Create table
                    PdfPTable table = new PdfPTable(dgvLogs.Columns.Count);
                    table.WidthPercentage = 100;

                    // Fix column widths - use proper float values
                    float[] columnWidths = new float[dgvLogs.Columns.Count];
                    for (int i = 0; i < dgvLogs.Columns.Count; i++)
                    {
                        columnWidths[i] = 1f; // Equal width for all columns
                    }
                    table.SetWidths(columnWidths);

                    // Add table headers
                    iTextSharp.text.Font headerFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 9, iTextSharp.text.BaseColor.WHITE);
                    foreach (DataGridViewColumn column in dgvLogs.Columns)
                    {
                        PdfPCell headerCell = new PdfPCell(new Phrase(column.HeaderText, headerFont));
                        headerCell.BackgroundColor = new iTextSharp.text.BaseColor(70, 130, 180); // Steel blue
                        headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        headerCell.Padding = 5;
                        table.AddCell(headerCell);
                    }

                    // Add table data
                    iTextSharp.text.Font cellFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 8, iTextSharp.text.BaseColor.BLACK);
                    int rowCount = 0;

                    foreach (DataGridViewRow dataRow in dgvLogs.Rows)
                    {
                        if (!dataRow.IsNewRow)
                        {
                            for (int i = 0; i < dgvLogs.Columns.Count; i++)
                            {
                                string cellValue = dataRow.Cells[i].Value?.ToString() ?? "";
                                PdfPCell cell = new PdfPCell(new Phrase(cellValue, cellFont));
                                cell.Padding = 4;

                                // Alternate row colors
                                if (rowCount % 2 == 0)
                                    cell.BackgroundColor = new iTextSharp.text.BaseColor(240, 240, 240);

                                // Right align for numeric columns
                                if (dgvLogs.Columns[i].HeaderText.Contains("Amount") ||
                                    dgvLogs.Columns[i].HeaderText.Contains("ID"))
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                }
                                else
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                }

                                table.AddCell(cell);
                            }
                            rowCount++;
                        }
                    }

                    document.Add(table);

                    // Add footer
                    Paragraph footer = new Paragraph($"\n\nEnd of Report - {dgvLogs.Rows.Count:N0} records exported", infoFont);
                    footer.Alignment = Element.ALIGN_CENTER;
                    document.Add(footer);

                    document.Close();

                    MessageBox.Show($"PDF report generated successfully!\n\nFile: {saveFileDialog.FileName}\nRecords: {dgvLogs.Rows.Count}",
                                  "PDF Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating PDF: {ex.Message}", "PDF Export Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string GetDateRangeText()
        {

            if (dtpFrom.Value.Date == new DateTime(2023, 1, 1) && dtpTo.Value.Date == new DateTime(2023, 12, 31))
                return "Full Year 2023";
            else
                return $"{dtpFrom.Value:MMMM dd, yyyy} to {dtpTo.Value:MMMM dd, yyyy}";
        }

        // Helper method to get filter info
        private string GetFilterInfo()
        {
            return GetProfessionalFilterInfo(); // Use the new professional version
        }

        private void dtpFrom_ValueChanged_1(object sender, EventArgs e)
        {
            if (dtpTo.Value >= dtpFrom.Value)
            {
                SearchTransactions();
            }
            else
            {
                MessageBox.Show("From date cannot be greater than To date", "Invalid Date Range",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dtpTo_ValueChanged_1(object sender, EventArgs e)
        {
            if (dtpTo.Value >= dtpFrom.Value)
            {
                SearchTransactions();
            }
            else
            {
                MessageBox.Show("To date cannot be less than From date", "Invalid Date Range",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private string GetProfessionalFilterInfo()
        {
            List<string> filters = new List<string>();

            if (cmbUser.Text != "User" && !string.IsNullOrWhiteSpace(cmbUser.Text))
            {
                // Already in "Name (Role)" format, so just use it directly
                string userDisplay = cmbUser.Text.Split('(')[0].Trim();
                filters.Add($"User: {userDisplay}");
            }

            if (cmbStatus.Text != "Status" && !string.IsNullOrWhiteSpace(cmbStatus.Text))
                filters.Add($"Status: {cmbStatus.Text}");

            if (cmbPayment.Text != "Payment" && !string.IsNullOrWhiteSpace(cmbPayment.Text))
                filters.Add($"Payment Method: {cmbPayment.Text}");

            return filters.Count > 0 ? string.Join(" • ", filters) : "No filters applied";
        }

        private string GetFormattedUserName()
        {
            try
            {
                // Get the actual name from database instead of username
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT Name FROM tbl_Users WHERE Username = @Username", con);
                    cmd.Parameters.AddWithValue("@Username", currentUser);

                    object result = cmd.ExecuteScalar();
                    string actualName = result?.ToString() ?? currentUser; // Fallback to username if name not found

                    return $"{actualName} ({currentRole})";
                }
            }
            catch (Exception)
            {
                // Fallback if there's an error
                return $"{currentUser} ({currentRole})";
            }
        }
    }
}