using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vegetable_Ordering_System
{
    public partial class OrderPreviewForm : Form
    {
        private OrderForm.Order order;
        public OrderPreviewForm(OrderForm.Order order)
        {
            InitializeComponent();
            this.order = order;
            DisplayOrderDetails();
        }

        private void OrderPreviewForm_Load(object sender, EventArgs e)
        {

        }
        private void DisplayOrderDetails()
        {
            this.Text = "Receipt Preview";
            this.Size = new Size(320, 700); // Similar to receipt width
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            // Create main panel
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.AutoScroll = true;
            mainPanel.BackColor = Color.White;
            mainPanel.Padding = new Padding(10);

            int yPos = 20;

            // Company Header
            Label lblCompany = new Label();
            lblCompany.Text = "VEGETABLE ORDERING SYSTEM";
            lblCompany.Font = new Font("Courier New", 12, FontStyle.Bold);
            lblCompany.ForeColor = Color.Black;
            lblCompany.AutoSize = true;
            lblCompany.Location = new Point(40, yPos);
            mainPanel.Controls.Add(lblCompany);
            yPos += 30;

            // Address
            Label lblAddress = new Label();
            lblAddress.Text = "Fresh Vegetables Market";
            lblAddress.Font = new Font("Courier New", 8, FontStyle.Regular);
            lblAddress.AutoSize = true;
            lblAddress.Location = new Point(70, yPos);
            mainPanel.Controls.Add(lblAddress);
            yPos += 15;

            // Contact
            Label lblContact = new Label();
            lblContact.Text = "Contact: 0912-345-6789";
            lblContact.Font = new Font("Courier New", 8, FontStyle.Regular);
            lblContact.AutoSize = true;
            lblContact.Location = new Point(55, yPos);
            mainPanel.Controls.Add(lblContact);
            yPos += 25;

            // Separator
            Panel sep1 = CreateSeparator(280);
            sep1.Location = new Point(10, yPos);
            mainPanel.Controls.Add(sep1);
            yPos += 15;

            // Order Information
            AddDetailLabel(mainPanel, "ORDER RECEIPT", 10, yPos, new Font("Courier New", 10, FontStyle.Bold));
            yPos += 25;

            AddTwoColumnLabel(mainPanel, "Order ID:", order.OrderId, 10, yPos);
            yPos += 20;
            AddTwoColumnLabel(mainPanel, "Date:", order.DateDisplay, 10, yPos);
            yPos += 20;
            AddTwoColumnLabel(mainPanel, "Time:", order.TimeDisplay, 10, yPos);
            yPos += 20;
            AddTwoColumnLabel(mainPanel, "Customer:", order.CustomerName, 10, yPos);
            yPos += 20;
            AddTwoColumnLabel(mainPanel, "Payment:", order.PaymentType, 10, yPos);
            yPos += 20;

            if (order.PaymentType == "GCash")
            {
                string gcashRef = GenerateGCashReference();
                AddTwoColumnLabel(mainPanel, "GCash Ref:", gcashRef, 10, yPos);
                yPos += 20;
            }

            yPos += 10;

            // Items separator
            Panel sep2 = CreateSeparator(280);
            sep2.Location = new Point(10, yPos);
            mainPanel.Controls.Add(sep2);
            yPos += 15;

            // Column headers
            AddDetailLabel(mainPanel, "ITEM", 10, yPos, new Font("Courier New", 9, FontStyle.Bold));
            AddDetailLabel(mainPanel, "QTY", 130, yPos, new Font("Courier New", 9, FontStyle.Bold));
            AddDetailLabel(mainPanel, "AMOUNT", 190, yPos, new Font("Courier New", 9, FontStyle.Bold));
            yPos += 20;

            // Line under headers
            Panel sep3 = CreateSeparator(280);
            sep3.Location = new Point(10, yPos);
            mainPanel.Controls.Add(sep3);
            yPos += 10;

            // Order items
            foreach (var item in order.OrderItems)
            {
                // Product name
                string productName = item.ProductName;
                if (productName.Length > 20)
                {
                    productName = productName.Substring(0, 17) + "...";
                }

                AddDetailLabel(mainPanel, productName, 10, yPos, new Font("Courier New", 9, FontStyle.Regular));
                AddDetailLabel(mainPanel, $"{item.Quantity}kg", 130, yPos, new Font("Courier New", 9, FontStyle.Regular));
                AddDetailLabel(mainPanel, $"₱{item.TotalPrice:N2}", 190, yPos, new Font("Courier New", 9, FontStyle.Regular));
                yPos += 15;

                // Unit price
                AddDetailLabel(mainPanel, $"@₱{item.UnitPrice:N2}/kg", 15, yPos, new Font("Courier New", 8, FontStyle.Regular));
                yPos += 15;
            }

            yPos += 10;

            // Total separator (double line)
            Panel sep4 = CreateSeparator(280);
            sep4.Location = new Point(10, yPos);
            mainPanel.Controls.Add(sep4);
            yPos += 12;

            // Total amount
            AddDetailLabel(mainPanel, "TOTAL:", 130, yPos, new Font("Courier New", 10, FontStyle.Bold));
            AddDetailLabel(mainPanel, $"₱{order.TotalAmount:N2}", 190, yPos, new Font("Courier New", 10, FontStyle.Bold));
            yPos += 30;

            // Thank you message
            Label lblThankYou = new Label();
            lblThankYou.Text = "Thank you for your order!";
            lblThankYou.Font = new Font("Courier New", 9, FontStyle.Regular);
            lblThankYou.AutoSize = true;
            lblThankYou.Location = new Point(70, yPos);
            mainPanel.Controls.Add(lblThankYou);
            yPos += 20;

            // Processed by
            AddDetailLabel(mainPanel, $"Processed by: {order.CreatedBy}", 10, yPos, new Font("Courier New", 8, FontStyle.Regular));
            yPos += 20;

            // Footer separator
            Panel sep5 = CreateSeparator(280);
            sep5.Location = new Point(10, yPos);
            mainPanel.Controls.Add(sep5);
            yPos += 15;

            // Footer notes
            AddDetailLabel(mainPanel, "Goods sold are not returnable", 45, yPos, new Font("Courier New", 8, FontStyle.Regular));
            yPos += 15;
            AddDetailLabel(mainPanel, "Please visit again!", 85, yPos, new Font("Courier New", 8, FontStyle.Regular));
            yPos += 30;

            // Action buttons
            Button btnPrint = new Button();
            btnPrint.Text = "Print Receipt";
            btnPrint.Size = new Size(100, 35);
            btnPrint.Location = new Point(40, yPos);
            btnPrint.BackColor = Color.FromArgb(40, 167, 69);
            btnPrint.ForeColor = Color.White;
            btnPrint.FlatStyle = FlatStyle.Flat;
            btnPrint.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnPrint.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };

            Button btnCancel = new Button();
            btnCancel.Text = "Back to Edit";
            btnCancel.Size = new Size(100, 35);
            btnCancel.Location = new Point(160, yPos);
            btnCancel.BackColor = Color.FromArgb(108, 117, 125);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            mainPanel.Controls.Add(btnPrint);
            mainPanel.Controls.Add(btnCancel);

            this.Controls.Add(mainPanel);
        }

        private Panel CreateSeparator(int width)
        {
            Panel separator = new Panel();
            separator.BackColor = Color.Black;
            separator.Size = new Size(width, 1);
            return separator;
        }

        private void AddDetailLabel(Panel parent, string text, int x, int y, Font font)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = font;
            label.AutoSize = true;
            label.Location = new Point(x, y);
            parent.Controls.Add(label);
        }

        private void AddTwoColumnLabel(Panel parent, string leftText, string rightText, int x, int y)
        {
            Label leftLabel = new Label();
            leftLabel.Text = leftText;
            leftLabel.Font = new Font("Courier New", 9, FontStyle.Regular);
            leftLabel.AutoSize = true;
            leftLabel.Location = new Point(x, y);
            parent.Controls.Add(leftLabel);

            Label rightLabel = new Label();
            rightLabel.Text = rightText;
            rightLabel.Font = new Font("Courier New", 9, FontStyle.Bold);
            rightLabel.AutoSize = true;
            rightLabel.Location = new Point(x + 150, y);
            parent.Controls.Add(rightLabel);
        }

        private string GenerateGCashReference()
        {
            Random random = new Random();
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string randomNum = random.Next(1000, 9999).ToString();
            return $"GC{timestamp}{randomNum}";
        }

    }
    }
