namespace Vegetable_Ordering_System
{
    partial class EditStock
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_addstock = new System.Windows.Forms.Button();
            this.txtVegetableName = new System.Windows.Forms.TextBox();
            this.txtQuantity = new System.Windows.Forms.TextBox();
            this.txtPrice = new System.Windows.Forms.TextBox();
            this.lblPrice = new System.Windows.Forms.Label();
            this.lblQuantity = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblUnit = new System.Windows.Forms.Label();
            this.cmbUnit = new System.Windows.Forms.ComboBox();
            this.txtTotalCost = new System.Windows.Forms.TextBox();
            this.lblTotalCost = new System.Windows.Forms.Label();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.dtpDateDelivery = new System.Windows.Forms.DateTimePicker();
            this.lblSupplierName = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.cmbSupplierName = new System.Windows.Forms.ComboBox();
            this.txtImagePath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.picProductImage = new System.Windows.Forms.PictureBox();
            this.lblImage = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picProductImage)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.label1.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 38);
            this.label1.TabIndex = 20;
            this.label1.Text = "Edit Stock";
            // 
            // btn_addstock
            // 
            this.btn_addstock.BackColor = System.Drawing.Color.LimeGreen;
            this.btn_addstock.Location = new System.Drawing.Point(764, 555);
            this.btn_addstock.Name = "btn_addstock";
            this.btn_addstock.Size = new System.Drawing.Size(108, 36);
            this.btn_addstock.TabIndex = 22;
            this.btn_addstock.Text = "Edit Stock";
            this.btn_addstock.UseVisualStyleBackColor = false;
            // 
            // txtVegetableName
            // 
            this.txtVegetableName.Location = new System.Drawing.Point(52, 157);
            this.txtVegetableName.Multiline = true;
            this.txtVegetableName.Name = "txtVegetableName";
            this.txtVegetableName.Size = new System.Drawing.Size(287, 24);
            this.txtVegetableName.TabIndex = 23;
            this.txtVegetableName.TextChanged += new System.EventHandler(this.txtVegetableName_TextChanged);
            this.txtVegetableName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtVegetableName_KeyPress);
            // 
            // txtQuantity
            // 
            this.txtQuantity.Location = new System.Drawing.Point(51, 223);
            this.txtQuantity.Multiline = true;
            this.txtQuantity.Name = "txtQuantity";
            this.txtQuantity.Size = new System.Drawing.Size(134, 24);
            this.txtQuantity.TabIndex = 25;
            this.txtQuantity.TextChanged += new System.EventHandler(this.txtQuantity_TextChanged);
            this.txtQuantity.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtQuantity_KeyPress);
            // 
            // txtPrice
            // 
            this.txtPrice.Location = new System.Drawing.Point(214, 223);
            this.txtPrice.Multiline = true;
            this.txtPrice.Name = "txtPrice";
            this.txtPrice.Size = new System.Drawing.Size(121, 24);
            this.txtPrice.TabIndex = 25;
            this.txtPrice.TextChanged += new System.EventHandler(this.txtPrice_TextChanged);
            this.txtPrice.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPrice_KeyPress);
            // 
            // lblPrice
            // 
            this.lblPrice.AutoSize = true;
            this.lblPrice.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.lblPrice.Location = new System.Drawing.Point(210, 197);
            this.lblPrice.Name = "lblPrice";
            this.lblPrice.Size = new System.Drawing.Size(47, 23);
            this.lblPrice.TabIndex = 26;
            this.lblPrice.Text = "Price";
            // 
            // lblQuantity
            // 
            this.lblQuantity.AutoSize = true;
            this.lblQuantity.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQuantity.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.lblQuantity.Location = new System.Drawing.Point(47, 197);
            this.lblQuantity.Name = "lblQuantity";
            this.lblQuantity.Size = new System.Drawing.Size(77, 23);
            this.lblQuantity.TabIndex = 26;
            this.lblQuantity.Text = "Quantity";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.label11.Location = new System.Drawing.Point(48, 131);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(137, 23);
            this.label11.TabIndex = 26;
            this.label11.Text = "Vegetable Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.label2.Location = new System.Drawing.Point(49, 86);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(136, 28);
            this.label2.TabIndex = 27;
            this.label2.Text = "Stock Details";
            // 
            // lblUnit
            // 
            this.lblUnit.AutoSize = true;
            this.lblUnit.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUnit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.lblUnit.Location = new System.Drawing.Point(397, 197);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = new System.Drawing.Size(42, 23);
            this.lblUnit.TabIndex = 29;
            this.lblUnit.Text = "Unit";
            // 
            // cmbUnit
            // 
            this.cmbUnit.FormattingEnabled = true;
            this.cmbUnit.Location = new System.Drawing.Point(401, 221);
            this.cmbUnit.Name = "cmbUnit";
            this.cmbUnit.Size = new System.Drawing.Size(121, 24);
            this.cmbUnit.TabIndex = 30;
            this.cmbUnit.SelectedIndexChanged += new System.EventHandler(this.cmbUnit_SelectedIndexChanged);
            // 
            // txtTotalCost
            // 
            this.txtTotalCost.Location = new System.Drawing.Point(548, 223);
            this.txtTotalCost.Name = "txtTotalCost";
            this.txtTotalCost.Size = new System.Drawing.Size(121, 22);
            this.txtTotalCost.TabIndex = 31;
            this.txtTotalCost.TextChanged += new System.EventHandler(this.txtTotalCost_TextChanged_1);
            // 
            // lblTotalCost
            // 
            this.lblTotalCost.AutoSize = true;
            this.lblTotalCost.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalCost.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.lblTotalCost.Location = new System.Drawing.Point(544, 197);
            this.lblTotalCost.Name = "lblTotalCost";
            this.lblTotalCost.Size = new System.Drawing.Size(85, 23);
            this.lblTotalCost.TabIndex = 32;
            this.lblTotalCost.Text = "Total Cost";
            // 
            // cmbCategory
            // 
            this.cmbCategory.FormattingEnabled = true;
            this.cmbCategory.Location = new System.Drawing.Point(401, 157);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(268, 24);
            this.cmbCategory.TabIndex = 34;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.label6.Location = new System.Drawing.Point(397, 131);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(81, 23);
            this.label6.TabIndex = 33;
            this.label6.Text = "Category";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.label7.Location = new System.Drawing.Point(396, 261);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(226, 28);
            this.label7.TabIndex = 35;
            this.label7.Text = "Delivery & Supplier Info";
            // 
            // dtpDateDelivery
            // 
            this.dtpDateDelivery.CalendarForeColor = System.Drawing.Color.Black;
            this.dtpDateDelivery.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.dtpDateDelivery.CalendarTitleForeColor = System.Drawing.Color.White;
            this.dtpDateDelivery.Location = new System.Drawing.Point(400, 334);
            this.dtpDateDelivery.Name = "dtpDateDelivery";
            this.dtpDateDelivery.ShowUpDown = true;
            this.dtpDateDelivery.Size = new System.Drawing.Size(229, 22);
            this.dtpDateDelivery.TabIndex = 36;
            // 
            // lblSupplierName
            // 
            this.lblSupplierName.AutoSize = true;
            this.lblSupplierName.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSupplierName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.lblSupplierName.Location = new System.Drawing.Point(397, 369);
            this.lblSupplierName.Name = "lblSupplierName";
            this.lblSupplierName.Size = new System.Drawing.Size(123, 23);
            this.lblSupplierName.TabIndex = 38;
            this.lblSupplierName.Text = "Supplier Name";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.label9.Location = new System.Drawing.Point(397, 305);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(114, 23);
            this.label9.TabIndex = 39;
            this.label9.Text = "Delivery Date";
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(418, 491);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(79, 33);
            this.btnSave.TabIndex = 40;
            this.btnSave.Text = "UPDATE";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.Color.White;
            this.btnClear.FlatAppearance.BorderSize = 0;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.ForeColor = System.Drawing.Color.DimGray;
            this.btnClear.Location = new System.Drawing.Point(503, 491);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(80, 33);
            this.btnClear.TabIndex = 41;
            this.btnClear.Text = "RESET";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.ForeColor = System.Drawing.Color.DimGray;
            this.btnCancel.Location = new System.Drawing.Point(589, 491);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 33);
            this.btnCancel.TabIndex = 42;
            this.btnCancel.Text = "CANCEL";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.panel1.Location = new System.Drawing.Point(0, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(701, 65);
            this.panel1.TabIndex = 43;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // cmbSupplierName
            // 
            this.cmbSupplierName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSupplierName.FormattingEnabled = true;
            this.cmbSupplierName.Location = new System.Drawing.Point(400, 410);
            this.cmbSupplierName.Name = "cmbSupplierName";
            this.cmbSupplierName.Size = new System.Drawing.Size(269, 24);
            this.cmbSupplierName.TabIndex = 44;
            // 
            // txtImagePath
            // 
            this.txtImagePath.Location = new System.Drawing.Point(52, 287);
            this.txtImagePath.Name = "txtImagePath";
            this.txtImagePath.ReadOnly = true;
            this.txtImagePath.Size = new System.Drawing.Size(283, 22);
            this.txtImagePath.TabIndex = 47;
            // 
            // btnBrowse
            // 
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnBrowse.ForeColor = System.Drawing.Color.Black;
            this.btnBrowse.Location = new System.Drawing.Point(119, 494);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(150, 30);
            this.btnBrowse.TabIndex = 46;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // picProductImage
            // 
            this.picProductImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picProductImage.Location = new System.Drawing.Point(119, 338);
            this.picProductImage.Name = "picProductImage";
            this.picProductImage.Size = new System.Drawing.Size(150, 150);
            this.picProductImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picProductImage.TabIndex = 45;
            this.picProductImage.TabStop = false;
            // 
            // lblImage
            // 
            this.lblImage.AutoSize = true;
            this.lblImage.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblImage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.lblImage.Location = new System.Drawing.Point(47, 261);
            this.lblImage.Name = "lblImage";
            this.lblImage.Size = new System.Drawing.Size(62, 23);
            this.lblImage.TabIndex = 49;
            this.lblImage.Text = "Image:";
            this.lblImage.Click += new System.EventHandler(this.label3_Click);
            // 
            // EditStock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(698, 551);
            this.Controls.Add(this.lblImage);
            this.Controls.Add(this.txtImagePath);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.picProductImage);
            this.Controls.Add(this.cmbSupplierName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.lblSupplierName);
            this.Controls.Add(this.dtpDateDelivery);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.cmbCategory);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lblTotalCost);
            this.Controls.Add(this.txtTotalCost);
            this.Controls.Add(this.cmbUnit);
            this.Controls.Add(this.lblUnit);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblPrice);
            this.Controls.Add(this.lblQuantity);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtPrice);
            this.Controls.Add(this.txtQuantity);
            this.Controls.Add(this.txtVegetableName);
            this.Controls.Add(this.btn_addstock);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.Name = "EditStock";
            this.Text = "Edit Stock";
            this.Load += new System.EventHandler(this.EditStock_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picProductImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_addstock;
        private System.Windows.Forms.TextBox txtVegetableName;
        private System.Windows.Forms.TextBox txtQuantity;
        private System.Windows.Forms.TextBox txtPrice;
        private System.Windows.Forms.Label lblPrice;
        private System.Windows.Forms.Label lblQuantity;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblUnit;
        private System.Windows.Forms.ComboBox cmbUnit;
        private System.Windows.Forms.TextBox txtTotalCost;
        private System.Windows.Forms.Label lblTotalCost;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.DateTimePicker dtpDateDelivery;
        private System.Windows.Forms.Label lblSupplierName;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.ComboBox cmbSupplierName;
        private System.Windows.Forms.Label lblImage;
        private System.Windows.Forms.TextBox txtImagePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.PictureBox picProductImage;
    }
}