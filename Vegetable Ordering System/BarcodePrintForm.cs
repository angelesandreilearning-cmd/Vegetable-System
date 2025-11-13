using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;

namespace Vegetable_Ordering_System
{
    public partial class BarcodePrintForm : Form
    {
        private string _barcode;
        private string _productName;
        public BarcodePrintForm(string barcode, string productName)
        {
            InitializeComponent();
            _barcode = barcode;
            _productName = productName;
        }

        private void BarcodePrintForm_Load(object sender, EventArgs e)
        {
            lblProductName.Text = _productName;
            lblBarcodeText.Text = "Barcode: " + _barcode;

            GenerateBarcodeImage();
        }
        private void GenerateBarcodeImage()
        {
            try
            {
                // Use ZXing to generate barcode image
                var barcodeWriter = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Height = 80,
                        Width = 300,
                        Margin = 10
                    }
                };

                var barcodeBitmap = barcodeWriter.Write(_barcode);
                picBarcode.Image = barcodeBitmap;
            }
            catch (Exception ex)
            {
                // Fallback: create text-based barcode if ZXing fails
                CreateTextBarcode();
                MessageBox.Show($"Barcode image generation failed: {ex.Message}\nUsing text barcode instead.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void CreateTextBarcode()
        {
            // Create a simple text-based barcode
            Bitmap bmp = new Bitmap(400, 120);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                // Draw border
                g.DrawRectangle(Pens.Black, 0, 0, 399, 119);

                // Draw product name
                using (Font nameFont = new Font("Arial", 12, FontStyle.Bold))
                {
                    g.DrawString(_productName, nameFont, Brushes.Black, 10, 10);
                }

                // Draw barcode text
                using (Font barcodeFont = new Font("Courier New", 16, FontStyle.Bold))
                {
                    g.DrawString(_barcode, barcodeFont, Brushes.Black, 10, 40);
                }

                // Draw student info
                using (Font infoFont = new Font("Arial", 8, FontStyle.Italic))
                {
                    g.DrawString("Student Project - Vegetable Ordering System",
                        infoFont, Brushes.DarkGray, 10, 90);
                }
            }
            picBarcode.Image = bmp;
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintBarcode();
        }
        private void PrintBarcode()
        {
            try
            {
                PrintDocument printDoc = new PrintDocument();
                printDoc.PrintPage += (s, e) =>
                {
                    Graphics graphics = e.Graphics;
                    Font titleFont = new Font("Arial", 14, FontStyle.Bold);
                    Font barcodeFont = new Font("Courier New", 18, FontStyle.Bold);
                    Font infoFont = new Font("Arial", 9, FontStyle.Regular);

                    float yPos = 50;


                    // Product Name
                    graphics.DrawString(_productName, titleFont, Brushes.Black, 50, yPos);
                    yPos += 30;

                    // Barcode
                    graphics.DrawString(_barcode, barcodeFont, Brushes.Black, 50, yPos);
                    yPos += 40;

                    // Date
                    graphics.DrawString($"Generated on: {DateTime.Now:MMMM dd, yyyy hh:mm tt}",
                        infoFont, Brushes.DarkGray, 50, yPos);
                };

                PrintDialog printDialog = new PrintDialog();
                printDialog.Document = printDoc;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDoc.Print();
                    MessageBox.Show("Barcode printed successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing: {ex.Message}", "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        

        private void SaveBarcodeImage()
        {
            try
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|BMP Image|*.bmp";
                    saveDialog.Title = "Save Barcode Image";
                    saveDialog.FileName = $"{_productName}_barcode_{_barcode}";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        if (picBarcode.Image != null)
                        {
                            // Get the selected file format
                            string extension = Path.GetExtension(saveDialog.FileName).ToLower();
                            System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;

                            switch (extension)
                            {
                                case ".jpg":
                                case ".jpeg":
                                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                                    break;
                                case ".bmp":
                                    format = System.Drawing.Imaging.ImageFormat.Bmp;
                                    break;
                                default:
                                    format = System.Drawing.Imaging.ImageFormat.Png;
                                    break;
                            }

                            picBarcode.Image.Save(saveDialog.FileName, format);
                            MessageBox.Show($"Barcode image saved successfully!\n\nLocation: {saveDialog.FileName}",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("No barcode image to save.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving image: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            SaveBarcodeImage();
        }
    }
}
