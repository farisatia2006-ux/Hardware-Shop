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

namespace Hardware_Shop
{
    public partial class Product : Form
    {
        public Product()
        {
            InitializeComponent();
            DisplayProduct();
        }
        SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\USER\OneDrive\المستندات\Hardware Shop MS.mdf"";Integrated Security=True;Connect Timeout=30");
        private void DisplayProduct()
        {
            try
            {
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Products", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { con.Close(); }
        }



        private void label5_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Hide();
        }


        private void ResetFields()
        {


            ProNameTb.Text = "";
            CatComBob.Text = "";
            Quantb.Text = "";
            Ptb.Text = "";


        }

        private void Cross_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            Customers customers = new Customers();
            customers.Show();
            this.Hide();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            Sales sales = new Sales();
            sales.Show();
            this.Hide();

        }

        private void Clrbtn_Click(object sender, EventArgs e)
        {
            ResetFields();
        }

        private void Addbtn_Click(object sender, EventArgs e)
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Products (ProductName, Category, Quantity, Price) VALUES (@ProductName, @Category, @Quantity, @Price)", con);
                cmd.Parameters.AddWithValue("@ProductName", ProNameTb.Text);
                cmd.Parameters.AddWithValue("@Category", CatComBob.Text);
                cmd.Parameters.AddWithValue("@Quantity", decimal.Parse(Quantb.Text));
                cmd.Parameters.AddWithValue("@Price", decimal.Parse(Ptb.Text));
                cmd.ExecuteNonQuery();
                MessageBox.Show("Product Added Successfully"); // Show a success message after adding the product
                con.Close();
                DisplayProduct(); // Refresh the product list after adding a new product
                ResetFields(); // Clear the input fields after adding the product
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Products (ProductName, Category, Quantity, Price) VALUES (@ProductName, @Category, @Quantity, @Price)", con);
                cmd.Parameters.AddWithValue("@ProductName", ProNameTb.Text);
                cmd.Parameters.AddWithValue("@Category", CatComBob.Text);
                cmd.Parameters.AddWithValue("@Quantity", decimal.Parse(Quantb.Text));
                cmd.Parameters.AddWithValue("@Price", decimal.Parse(Ptb.Text));
                cmd.ExecuteNonQuery();
                MessageBox.Show("Product Added Successfully"); // Show a success message after adding the product
                con.Close();
                DisplayProduct(); // Refresh the product list after adding a new product
                ResetFields(); // Clear the input fields after adding the product
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        private void DelBtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {

                int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);
                try
                {
                    con.Open();
                    
                    SqlCommand cmd = new SqlCommand("DELETE FROM Products WHERE ProductID=@ProductID", con);
                    cmd.Parameters.AddWithValue("@ProductID", id);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Product Deleted Successfully");
                    con.Close();
                    DisplayProduct();
                    ResetFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
            else
            {
                MessageBox.Show("Please select a product to delete.");
            }
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {

        }

        private void Product_Load(object sender, EventArgs e)
        {
            Color customColor = Color.FromArgb(44, 62, 80); // Custom color

            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = customColor;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            dataGridView1.DefaultCellStyle.SelectionBackColor = customColor;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;

            dataGridView1.GridColor = Color.LightGray;
        }
    }
}     

