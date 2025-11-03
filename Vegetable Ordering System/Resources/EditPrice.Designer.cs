namespace Vegetable_Ordering_System.Resources
{
    partial class EditPrice
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
            this.lblCurrentPrice = new System.Windows.Forms.Label();
            this.lblNewPrice = new System.Windows.Forms.Label();
            this.nudNewPrice = new System.Windows.Forms.NumericUpDown();
            this.btnUpdatePrice = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudNewPrice)).BeginInit();
            this.SuspendLayout();
            // 
            // lblProductName
            // 
            this.lblProductName.AutoSize = true;
            this.lblProductName.Location = new System.Drawing.Point(13, 13);
            this.lblProductName.Name = "lblProductName";
            this.lblProductName.Size = new System.Drawing.Size(168, 16);
            this.lblProductName.TabIndex = 0;
            this.lblProductName.Text = "$\"Product: {productName}\"";
            // 
            // lblCurrentPrice
            // 
            this.lblCurrentPrice.AutoSize = true;
            this.lblCurrentPrice.Location = new System.Drawing.Point(30, 135);
            this.lblCurrentPrice.Name = "lblCurrentPrice";
            this.lblCurrentPrice.Size = new System.Drawing.Size(129, 16);
            this.lblCurrentPrice.TabIndex = 1;
            this.lblCurrentPrice.Text = "Current Price: ₱50.00";
            // 
            // lblNewPrice
            // 
            this.lblNewPrice.AutoSize = true;
            this.lblNewPrice.Location = new System.Drawing.Point(97, 218);
            this.lblNewPrice.Name = "lblNewPrice";
            this.lblNewPrice.Size = new System.Drawing.Size(71, 16);
            this.lblNewPrice.TabIndex = 2;
            this.lblNewPrice.Text = "New Price:";
            // 
            // nudNewPrice
            // 
            this.nudNewPrice.Location = new System.Drawing.Point(105, 226);
            this.nudNewPrice.Name = "nudNewPrice";
            this.nudNewPrice.Size = new System.Drawing.Size(120, 22);
            this.nudNewPrice.TabIndex = 3;
            // 
            // btnUpdatePrice
            // 
            this.btnUpdatePrice.Location = new System.Drawing.Point(33, 328);
            this.btnUpdatePrice.Name = "btnUpdatePrice";
            this.btnUpdatePrice.Size = new System.Drawing.Size(125, 23);
            this.btnUpdatePrice.TabIndex = 4;
            this.btnUpdatePrice.Text = "Update Price";
            this.btnUpdatePrice.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(143, 371);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(125, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // EditPrice
            // 
            this.AcceptButton = this.btnUpdatePrice;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(322, 452);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnUpdatePrice);
            this.Controls.Add(this.nudNewPrice);
            this.Controls.Add(this.lblNewPrice);
            this.Controls.Add(this.lblCurrentPrice);
            this.Controls.Add(this.lblProductName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "EditPrice";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Product Price";
            ((System.ComponentModel.ISupportInitialize)(this.nudNewPrice)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblProductName;
        private System.Windows.Forms.Label lblCurrentPrice;
        private System.Windows.Forms.Label lblNewPrice;
        private System.Windows.Forms.NumericUpDown nudNewPrice;
        private System.Windows.Forms.Button btnUpdatePrice;
        private System.Windows.Forms.Button btnCancel;
    }
}