using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vegetable_Ordering_System
{
    public partial class AddStock : Form
    {
        string connectionString = @"Data Source=LAPTOP-B8MV83P4\SQLEXPRESS01;Initial Catalog=db_vegetableOrdering;Integrated Security=True;";
        private InventoryForm _parentform;

        public AddStock(InventoryForm parentform)
        {
            InitializeComponent();
            _parentform = parentform;
        }

        private void btn_addstock_Click(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            SqlCommand command = new SqlCommand(
                "INSERT INTO tbl_Vegetables (VegetableName, Quantity, Price) VALUES (@name, @quantity, @price)", conn);

           
            command.Parameters.AddWithValue("@name", textBox1.Text);
            command.Parameters.AddWithValue("@quantity", textBox3.Text);
            command.Parameters.AddWithValue("@price", textBox4.Text);

            command.ExecuteNonQuery();

            _parentform.LoadVegetables();

            conn.Close();

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
