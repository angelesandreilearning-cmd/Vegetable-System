namespace Vegetable_Ordering_System
{
    partial class EditPriceForm
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
            this.lblProductName = new System.Windows.Forms.Label();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.nudNewPrice = new System.Windows.Forms.NumericUpDown();
            this.lblPrice = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblNewPrice = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudNewPrice)).BeginInit();
            this.SuspendLayout();
            // 
            // lblProductName
            // 
            this.lblProductName.AutoSize = true;
            this.lblProductName.Location = new System.Drawing.Point(24, 32);
            this.lblProductName.Name = "lblProductName";
            this.lblProductName.Size = new System.Drawing.Size(96, 16);
            this.lblProductName.TabIndex = 0;
            this.lblProductName.Text = "Product Name:";
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(27, 144);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(102, 23);
            this.btnUpdate.TabIndex = 1;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // nudNewPrice
            // 
            this.nudNewPrice.Location = new System.Drawing.Point(131, 103);
            this.nudNewPrice.Name = "nudNewPrice";
            this.nudNewPrice.Size = new System.Drawing.Size(120, 22);
            this.nudNewPrice.TabIndex = 2;
            // 
            // lblPrice
            // 
            this.lblPrice.AutoSize = true;
            this.lblPrice.Location = new System.Drawing.Point(24, 65);
            this.lblPrice.Name = "lblPrice";
            this.lblPrice.Size = new System.Drawing.Size(86, 16);
            this.lblPrice.TabIndex = 3;
            this.lblPrice.Text = "Current Price:";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(176, 144);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblNewPrice
            // 
            this.lblNewPrice.AutoSize = true;
            this.lblNewPrice.Location = new System.Drawing.Point(24, 105);
            this.lblNewPrice.Name = "lblNewPrice";
            this.lblNewPrice.Size = new System.Drawing.Size(71, 16);
            this.lblNewPrice.TabIndex = 5;
            this.lblNewPrice.Text = "New Price:";
            // 
            // EditPriceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 185);
            this.Controls.Add(this.lblNewPrice);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblPrice);
            this.Controls.Add(this.nudNewPrice);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.lblProductName);
            this.Name = "EditPriceForm";
            this.Text = "EditPriceForm";
            this.Load += new System.EventHandler(this.EditPriceForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudNewPrice)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblProductName;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.NumericUpDown nudNewPrice;
        private System.Windows.Forms.Label lblPrice;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblNewPrice;
    }
}