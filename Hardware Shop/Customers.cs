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
    public partial class Customers : Form
    {
        public Customers()
        {
            InitializeComponent();
            DisplayCustomers();
        }
        SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\USER\OneDrive\المستندات\Hardware Shop MS.mdf"";Integrated Security=True;Connect Timeout=30");
        private void DisplayCustomers()
        {
            try
            {
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Customers", con);
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

        private void ResetFields()
        {


            CusNameTb.Text = "";
            CusPhoneTb.Text = "";
            Ptb.Text = "";


        }















        private void Cross_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Product product = new Product();
            product.Show();
            this.Hide();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            Sales sales = new Sales();
            sales.Show();
            this.Hide();
        }

        private void label5_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            login.Show();
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
                SqlCommand cmd = new SqlCommand("INSERT INTO Customers (CName, Phone, Email) VALUES (@CName, @Phone, @Email)", con);
                cmd.Parameters.AddWithValue("@CName", CusNameTb.Text);
                cmd.Parameters.AddWithValue("@Phone", CusPhoneTb.Text);
                cmd.Parameters.AddWithValue("@Email", Ptb.Text);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Customer Added Successfully"); // Show a success message after adding the customer
                con.Close();
                DisplayCustomers(); // Refresh the customer list after adding a new customer
                ResetFields(); // Clear the input fields after adding the customer
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
            int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Customers SET CName=@CName, Phone=@Phone, Email=@Email WHERE CustomerID=@CustomerID", con);
                cmd.Parameters.AddWithValue("@CustomerID",id);
                cmd.Parameters.AddWithValue("@CName", CusNameTb.Text);
                cmd.Parameters.AddWithValue("@Phone", CusPhoneTb.Text);
                cmd.Parameters.AddWithValue("@Email", Ptb.Text);         
                cmd.ExecuteNonQuery();
                MessageBox.Show("Customer Updated Successfully"); // Show a success message after updating the customer
                con.Close();
                DisplayCustomers(); // Refresh the customer list after updating a customer
                ResetFields(); // Clear the input fields after updating the customer
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

                    SqlCommand cmd = new SqlCommand("DELETE FROM Customers WHERE CustomerID=@CustomerID", con);
                    cmd.Parameters.AddWithValue("@CustomerID", id);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Customer Deleted Successfully");
                    con.Close();
                    DisplayCustomers();
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
                MessageBox.Show("Please select a customer to delete.");
            }
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.Index != -1)
            {
                CusNameTb.Text = dataGridView1.CurrentRow.Cells["CName"].Value.ToString();
                CusPhoneTb.Text = dataGridView1.CurrentRow.Cells["Phone"].Value.ToString();
                Ptb.Text = dataGridView1.CurrentRow.Cells["Email"].Value.ToString();
            }
        }

        private void Customers_Load(object sender, EventArgs e)
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
