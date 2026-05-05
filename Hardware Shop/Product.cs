using System;
using System.Windows.Forms;
using Hardware_Shop.Models;
using Hardware_Shop.Services;
using System.Configuration;

namespace Hardware_Shop
{
    public partial class Product : Form
    {
        IProductService service = new ProductService();
        int selectedId = 0;

        public Product()
        {
            InitializeComponent();
            DisplayProduct();
        }

        private void DisplayProduct()
        {
            try
            {
                dataGridView1.DataSource = service.GetProducts();
            }
            catch
            {
                MessageBox.Show("Error loading products");
            }
        }

        private void Addbtn_Click(object sender, EventArgs e)
        {
            try
            {
                CategoryEnum cat = (CategoryEnum)Enum.Parse(typeof(CategoryEnum), CatComBob.Text);

                var product = new ProductModel(
                    ProNameTb.Text,
                    cat,
                    decimal.Parse(Quantb.Text),
                    decimal.Parse(Ptb.Text)
                );

                service.AddProduct(product);

                MessageBox.Show("Added Successfully");
                DisplayProduct();
            }
            catch
            {
                MessageBox.Show("Invalid input");
            }
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            if (selectedId == 0)
            {
                MessageBox.Show("Select product first");
                return;
            }

            CategoryEnum cat = (CategoryEnum)Enum.Parse(typeof(CategoryEnum), CatComBob.Text);

            var product = new ProductModel(
                ProNameTb.Text,
                cat,
                decimal.Parse(Quantb.Text),
                decimal.Parse(Ptb.Text)
            );

            service.UpdateProduct(selectedId, product);

            MessageBox.Show("Updated Successfully");
            DisplayProduct();
        }

        private void DelBtn_Click(object sender, EventArgs e)
        {
            if (selectedId == 0)
            {
                MessageBox.Show("Select product first");
                return;
            }

            service.DeleteProduct(selectedId);

            MessageBox.Show("Deleted Successfully");
            DisplayProduct();
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                selectedId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ProductID"].Value);

                ProNameTb.Text = dataGridView1.CurrentRow.Cells["ProductName"].Value.ToString();
                CatComBob.Text = dataGridView1.CurrentRow.Cells["Category"].Value.ToString();
                Quantb.Text = dataGridView1.CurrentRow.Cells["Quantity"].Value.ToString();
                Ptb.Text = dataGridView1.CurrentRow.Cells["Price"].Value.ToString();
            }
        }

        // Added missing event handlers referenced by the Designer
        private void label4_Click(object sender, EventArgs e)
        {
            // Placeholder: navigate to Sales - implementation depends on other forms
            MessageBox.Show("Sales clicked");
        }

        private void label3_Click(object sender, EventArgs e)
        {
            // Placeholder: navigate to Customers
            MessageBox.Show("Customers clicked");
        }

        private void label5_Click(object sender, EventArgs e)
        {
            // Placeholder: perform logout
            MessageBox.Show("Logout clicked");
        }

        private void Cross_Click(object sender, EventArgs e)
        {
            // Close the form when the 'X' label is clicked
            this.Close();
        }

        private void Clrbtn_Click(object sender, EventArgs e)
        {
            // Clear inputs and reset selection
            ProNameTb.Text = string.Empty;
            CatComBob.SelectedIndex = -1;
            Quantb.Text = string.Empty;
            Ptb.Text = string.Empty;
            selectedId = 0;
        }

        private void Product_Load(object sender, EventArgs e)
        {
            // Ensure products are displayed on load
            DisplayProduct();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Reuse the DoubleClick handler logic
            dataGridView1_DoubleClick(sender, EventArgs.Empty);
        }
    }
}

