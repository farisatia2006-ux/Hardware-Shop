using DevExpress.Data;
using DevExpress.Utils.Taskbar.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Hardware_Shop
{
    public partial class Sales : Form
    {
        public Sales()
        {
            InitializeComponent();
            DisplaySales();

        }

        SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\USER\OneDrive\المستندات\Hardware Shop MS.mdf"";Integrated Security=True;Connect Timeout=30");
        private void DisplaySales()
        {
            try
            {
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Sales", con);
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


            CusIdCB.Text = "";
            CusNameTb.Text = "";
            ProaldCb.Text = "";
            ProNameTb.Text = "";
            QuenTb.Text = "";
            PriceTb.Text = "";
            dateTimePicker1.Text = "";


        }

        private void LoadCustomerIDs()
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT CustomerID FROM Customers", con);
                SqlDataReader sqlDataReader = cmd.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    CusIdCB.Items.Add(sqlDataReader["CustomerID"].ToString());
                }
                sqlDataReader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { con.Close(); }
        }

        private void LoadProductIDs()
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT ProductID FROM Products", con);
                SqlDataReader sqlDataReader = cmd.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    ProaldCb.Items.Add(sqlDataReader["ProductID"].ToString());
                }
                sqlDataReader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { con.Close(); }
        }




        private void label2_Click(object sender, EventArgs e)
        {
            Product product = new Product();
            product.Show();
            this.Hide();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            Customers customers = new Customers();
            customers.Show();
            this.Hide();
        }

        private void label5_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Hide();
        }

        private void Cross_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Clrbtn_Click(object sender, EventArgs e)
        {
            ResetFields();
        }

        private void CusIdCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            con.Open();
            SqlCommand sqlCommand = new SqlCommand("SELECT CName FROM Customers WHERE CustomerID = @ID", con);
            sqlCommand.Parameters.AddWithValue("@ID", CusIdCB.Text);
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            if (sqlDataReader.Read())
            {
                CusNameTb.Text = sqlDataReader["CName"].ToString();

            }
            con.Close();
        }

        private void ProaldCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            con.Open();
            SqlCommand sqlCommand = new SqlCommand("SELECT ProductName ,Price FROM Products WHERE ProductID = @ID", con);
            sqlCommand.Parameters.AddWithValue("@ID", ProaldCb.Text);
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            if (sqlDataReader.Read())
            {
                ProNameTb.Text = sqlDataReader["ProductName"].ToString();
                PriceTb.Text = sqlDataReader["Price"].ToString();

            }
            con.Close();
        }

        private void Sales_Load(object sender, EventArgs e)
        {
            LoadCustomerIDs();
            LoadProductIDs();

            Color customColor = Color.FromArgb(44, 62, 80); // Custom color

            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = customColor;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            dataGridView1.DefaultCellStyle.SelectionBackColor = customColor;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;

            dataGridView1.GridColor = Color.LightGray;

            PrintDocument.PrintPage += new PrintPageEventHandler(PrintDocument_PrintPage);

        }

        private void Addbtn_Click(object sender, EventArgs e)
        {
            if (CusIdCB.Text == "" || ProaldCb.Text == "" || QuenTb.Text == "")
            {
                MessageBox.Show("Missing Information");
            }
            int quantitySold;
            if (!int.TryParse(QuenTb.Text, out quantitySold) || quantitySold <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.");
                return;
            }

            try
            {
                con.Open();
                SqlCommand sqlCommand = new SqlCommand("SELECT Quantity ,Price FROM Products WHERE ProductID = @PID", con);
                sqlCommand.Parameters.AddWithValue("@PID", ProaldCb.Text);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                if (!reader.Read())
                {
                    MessageBox.Show("Product not found.");
                    con.Close();
                    return;

                }
                int availableQuantity = Convert.ToInt32(reader["Quantity"]);
                decimal unitPrice = Convert.ToDecimal(reader["Price"]);
                reader.Close();

                if (quantitySold > availableQuantity)
                {
                    MessageBox.Show("Stock not available.");
                    return;
                }
                decimal totalPrice = quantitySold * unitPrice;

                SqlCommand insert = new SqlCommand("INSERT INTO Sales (CustomerID, CustomerName,ProductID ,ProductName,QuantitySold , TotalAmount,SaleDate) VALUES (@CID, @CN, @PID, @PN, @QS, @TA,@SD)", con);
                insert.Parameters.AddWithValue("@CID", CusIdCB.Text);
                insert.Parameters.AddWithValue("@CN", CusNameTb.Text);
                insert.Parameters.AddWithValue("@PID", ProaldCb.Text);
                insert.Parameters.AddWithValue("@PN", ProNameTb.Text);
                insert.Parameters.AddWithValue("@QS", QuenTb.Text);
                insert.Parameters.AddWithValue("@TA", totalPrice);
                insert.Parameters.AddWithValue("@SD", dateTimePicker1.Value);
                insert.ExecuteNonQuery();

                SqlCommand update = new SqlCommand("UPDATE Products SET Quantity = Quantity - @Qty WHERE ProductID = @PID", con);
                update.Parameters.AddWithValue("@Qty", quantitySold);
                update.Parameters.AddWithValue("@PID", ProaldCb.Text);
                update.ExecuteNonQuery();
                con.Close();

                DisplaySales();
                ResetFields();
                MessageBox.Show("Sale recorded successfully.");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { con.Close(); }
        }

        private void QuenTb_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ProaldCb.Text) && int.TryParse(QuenTb.Text, out int qty))
            {
                try
                {
                    con.Open();
                    SqlCommand S = new SqlCommand("SELECT Price FROM Products WHERE ProductID = @ID", con);
                    S.Parameters.AddWithValue("@ID", ProaldCb.Text);
                    object result = S.ExecuteScalar();
                    con.Close();

                    if (result != null && decimal.TryParse(result.ToString(), out decimal unitPrice))
                    {
                        decimal total = qty * unitPrice;
                        PriceTb.Text = total.ToString("0.00");
                    }
                    else
                    {
                        PriceTb.Text = "0.00";
                    }
                }
                catch (Exception ex)
                {
                    con.Close();
                    PriceTb.Text = "0.00";
                }
                finally
                {
                    con.Close();
                }
            }
            else
            {
                PriceTb.Text = "0.00";
            }

        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int saleId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["SaleID"].Value);// Implement the logic to update the selected sale record based on the input fields
                                                                                                  // You can use a similar approach as in the Addbtn_Click event handler to update the record in the database
                                                                                                  // After updating, call DisplaySales() to refresh the data grid view

                int oldQty = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["QuantitySold"].Value);
                string productId = dataGridView1.SelectedRows[0].Cells["ProductID"].Value.ToString();




                try
                {
                     

                    con.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE Products SET Quantity = Quantity * @oldQty WHERE ProductID = @PID ", con);
                    cmd.Parameters.AddWithValue("@oldQty", oldQty);
                    cmd.Parameters.AddWithValue("@PID", productId);
                    cmd.ExecuteNonQuery();

                    SqlCommand sqlcommand = new SqlCommand("SELECT Quantity FROM Products WHERE ProductID = @PID", con);
                    sqlcommand.Parameters.AddWithValue("@PID", ProaldCb.Text);

                    int currentStock = Convert.ToInt32(sqlcommand.ExecuteScalar());

                    int newQty = Convert.ToInt32(QuenTb.Text);

                    if (newQty > currentStock)
                    {
                        MessageBox.Show("Stock not available.");
                       
                        return;
                    }

                    decimal unitPrice = Convert.ToDecimal(PriceTb.Text);
                    decimal total = newQty * unitPrice;

                    SqlCommand sql = new SqlCommand(@"UPDATE Sales SET CustomerID = @CID, CustomerName = @CN, ProductID = @PID, ProductName = @PN, QuantitySold = @QS, TotalAmount = @TA, SaleDate = @SD WHERE SaleID = @SID", con);
                    sql.Parameters.AddWithValue("@CID", CusIdCB.Text);
                    sql.Parameters.AddWithValue("@CN", CusNameTb.Text);
                    sql.Parameters.AddWithValue("@PID", ProaldCb.Text);
                    sql.Parameters.AddWithValue("@PN", ProNameTb.Text);
                    sql.Parameters.AddWithValue("@QS", newQty);
                    sql.Parameters.AddWithValue("@TA", total);
                    sql.Parameters.AddWithValue("@SD", dateTimePicker1.Value);
                    sql.Parameters.AddWithValue("@SID", saleId);
                    sql.ExecuteNonQuery();

                    SqlCommand sqlCommand = new SqlCommand("UPDATE Products SET Quantity = Quantity - @NewQty WHERE ProductID = @PID", con);
                    sqlCommand.Parameters.AddWithValue("@NewQty", newQty);
                    sqlCommand.Parameters.AddWithValue("@PID", ProaldCb.Text);
                    sqlCommand.ExecuteNonQuery();

                    MessageBox.Show("Sale updated successfully.");
                    DisplaySales();
                    ResetFields();
                    
                }
                catch (Exception ex)
                {

                }
                finally
                { con.Close(); }


            }
        }

        private void DelBtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int saleId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["SaleID"].Value);

                int qtyToRestore = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["QuantitySold"].Value);
                string productId = dataGridView1.SelectedRows[0].Cells["ProductID"].Value.ToString();

                DialogResult dr = MessageBox.Show("Are you sure you want to delete this sale?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr != DialogResult.Yes) return;

                try
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("UPDATE Products SET Quantity = Quantity *@oldQty WHERE ProductID = @PID ", con);
                    cmd.Parameters.AddWithValue("@oldQty", qtyToRestore);
                    cmd.Parameters.AddWithValue("@PID", productId);
                    cmd.ExecuteNonQuery();

                    SqlCommand cmd2 = new SqlCommand("DELETE FROM Sales WHERE SaleID = @SaleID", con);
                    cmd2.Parameters.AddWithValue("@SaleID", saleId);
                    cmd2.ExecuteNonQuery();
                    

                    MessageBox.Show("Sale deleted successfully.");


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
            else { MessageBox.Show("Please select a sale to delete."); }

            



        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.Index != -1)
            {
                CusIdCB.Text = dataGridView1.CurrentRow.Cells["CustomerID"].Value.ToString();
                CusNameTb.Text = dataGridView1.CurrentRow.Cells["CustomerName"].Value.ToString();
                ProaldCb.Text = dataGridView1.CurrentRow.Cells["ProductID"].Value.ToString();
                ProNameTb.Text = dataGridView1.CurrentRow.Cells["ProductName"].Value.ToString();
                QuenTb.Text = dataGridView1.CurrentRow.Cells["QuantitySold"].Value.ToString();
                PriceTb.Text = dataGridView1.CurrentRow.Cells["TotalAmount"].Value.ToString();
                dateTimePicker1.Text = dataGridView1.CurrentRow.Cells["SaleDate"].Value.ToString();
            }
        }


        private PrintDocument PrintDocument = new PrintDocument();
        private int currentRow = 0;
        private int[] columnWidth;

        private void CalculateColumnWidths()
        {
            columnWidth = new int[dataGridView1.Columns.Count];
            using (Graphics g = this.CreateGraphics())

                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    int maxWidth = (int)g.MeasureString(dataGridView1.Columns[i].HeaderText, new Font("Segoe UI", 9)).Width + 20;

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Cells[i].Value != null)
                        {
                            string cellText = row.Cells[i].Value.ToString();
                            int cellWidth = (int)g.MeasureString(cellText, new Font("Segoe UI", 9)).Width + 20;
                            if (cellWidth > maxWidth)
                                maxWidth = cellWidth;
                        }
                    }
                    columnWidth[i] = maxWidth;
                }
        }
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            int rowHeight = 25;
            int topMargin = e.MarginBounds.Top + 60;
            int yPosition = topMargin;
            Font headerFont = new Font("Segoe UI", 14, FontStyle.Bold);
            Font cellFont = new Font("Segoe UI", 9);
            Font tableHeaderFont = new Font("Segoe UI", 9, FontStyle.Bold);

            CalculateColumnWidths();

            int tableWidth = columnWidth.Sum();
            int leftMargin = e.MarginBounds.Left + (e.MarginBounds.Width - tableWidth) / 2;

            string heading = "Sales Report ";
            SizeF headingSize = e.Graphics.MeasureString(heading, headerFont);
            float headingX = e.MarginBounds.Left + (e.MarginBounds.Width - headingSize.Width) / 2;
            e.Graphics.DrawString(heading, headerFont, Brushes.Black, headingX, e.MarginBounds.Top);

            int x = leftMargin;


            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                string headerText = dataGridView1.Columns[i].HeaderText;
                Rectangle headerRect = new Rectangle(x, yPosition, columnWidth[i], rowHeight);
                e.Graphics.FillRectangle(Brushes.DarkSlateGray, headerRect);
                e.Graphics.DrawRectangle(Pens.Black, headerRect);
                e.Graphics.DrawString(headerText, tableHeaderFont, Brushes.White, headerRect);
                x += columnWidth[i];
            }

            yPosition += rowHeight;
            while (currentRow < dataGridView1.Rows.Count)
            {
                DataGridViewRow row = dataGridView1.Rows[currentRow];
                if (!row.IsNewRow)
                    x = leftMargin;
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    string cellText = row.Cells[i].Value?.ToString() ?? "";
                    Rectangle cellRect = new Rectangle(x, yPosition, columnWidth[i], rowHeight);
                    e.Graphics.DrawRectangle(Pens.Black, cellRect);
                    e.Graphics.DrawString(cellText, cellFont, Brushes.Black, cellRect);
                    x += columnWidth[i];
                }

                yPosition += rowHeight;

                if (yPosition + rowHeight > e.MarginBounds.Bottom)
                {
                    currentRow++;
                    e.HasMorePages = true;
                    return;
                }

                currentRow++;
            }


            






            currentRow = 0;
            e.HasMorePages = false;

        }

        

        private void ReportBtn_Click_1(object sender, EventArgs e)
        {
            currentRow = 0;
            CalculateColumnWidths();
            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = PrintDocument;
            preview.ShowDialog();
        }
    }
}

