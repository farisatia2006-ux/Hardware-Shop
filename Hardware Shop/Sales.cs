using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;

namespace Hardware_Shop
{
    // ============================================================
    //  Sales.cs — REFACTORED
    //
    //  التغييرات من منظور OOP:
    //  • ترث من BaseDataForm (Inheritance) ✅
    //  • DisplayData / ResetFields تعمل override (Polymorphism) ✅
    //  • بيمشي للـ DatabaseHelper (Encapsulation) ✅
    //  • بيمشي للـ AuditLogger (File Handling) ✅
    //  • Exception Handling محسّن ✅
    //  • بيمشي للـ NavigateTo (INavigable) ✅
    //  • تم تصحيح البق في UpdateBtn: Quantity * → Quantity + ✅
    //  • تم إضافة رسالة خطأ للـ catch الفارغ ✅
    // ============================================================

    /// <summary>
    /// Sales management form.
    /// Inherits from BaseDataForm — gains DB, logging, grid styling, navigation.
    /// Demonstrates: Inheritance, Polymorphism, Exception Handling,
    ///               File Handling (via AuditLogger), Encapsulation.
    /// </summary>
    public partial class Sales : BaseDataForm  // Inheritance ✅
    {
        string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Hardware_Shop_MS.mdf;Integrated Security=True;";
        // ── Print support (encapsulated fields) ──────────────────
        private readonly PrintDocument _printDocument = new PrintDocument();
        private int _currentRow = 0;
        private int[] _columnWidths;

        // ── Constructor ──────────────────────────────────────────
        public Sales()
        {
            InitializeComponent();
            DisplayData();
        }

        // ────────────────────────────────────────────────────────
        // POLYMORPHISM — Overriding abstract methods ✅
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Loads and displays all sales in the DataGridView.
        /// Overrides abstract DisplayData() from BaseDataForm. ✅
        /// </summary>
        protected override void DisplayData()  // Polymorphism ✅
        {
            try
            {
                DataTable dt = _db.ExecuteQuery("SELECT * FROM Sales");
                dataGridView1.DataSource = dt;
            }
            catch (ApplicationException ex)
            {
                ShowMessage(ex.Message, "Load Error", MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Clears all sales input fields.
        /// Overrides abstract ResetFields() from BaseDataForm. ✅
        /// </summary>
        protected override void ResetFields()  // Polymorphism ✅
        {
            CusIdCB.Text = "";
            CusNameTb.Text = "";
            ProaldCb.Text = "";
            ProNameTb.Text = "";
            QuenTb.Text = "";
            PriceTb.Text = "";
            dateTimePicker1.Value = DateTime.Now;
        }

        // ────────────────────────────────────────────────────────
        // LOAD — populate ComboBoxes from DB
        // ────────────────────────────────────────────────────────

        private void Sales_Load(object sender, EventArgs e)
        {
            LoadCustomerIDs();
            LoadProductIDs();
            ApplyGridStyle(dataGridView1);  // Inherited from BaseDataForm ✅
            _printDocument.PrintPage += PrintDocument_PrintPage;
        }

        private void LoadCustomerIDs()
        {
            try
            {
                DataTable dt = _db.ExecuteQuery("SELECT CustomerID FROM Customers");
                foreach (DataRow row in dt.Rows)
                    CusIdCB.Items.Add(row["CustomerID"].ToString());
            }
            catch (ApplicationException ex)
            {
                ShowMessage($"Could not load customers:\n{ex.Message}",
                            "Load Error", MessageBoxIcon.Error);
            }
        }

        private void LoadProductIDs()
        {
            try
            {
                DataTable dt = _db.ExecuteQuery("SELECT ProductID FROM Products");
                foreach (DataRow row in dt.Rows)
                    ProaldCb.Items.Add(row["ProductID"].ToString());
            }
            catch (ApplicationException ex)
            {
                ShowMessage($"Could not load products:\n{ex.Message}",
                            "Load Error", MessageBoxIcon.Error);
            }
        }

        // ────────────────────────────────────────────────────────
        // COMBOBOX CHANGE EVENTS
        // ────────────────────────────────────────────────────────

        private void CusIdCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                object result = _db.ExecuteScalar(
                    "SELECT CName FROM Customers WHERE CustomerID = @ID",
                    new Dictionary<string, object> { { "@ID", CusIdCB.Text } });

                CusNameTb.Text = result?.ToString() ?? "";
            }
            catch (ApplicationException ex)
            {
                ShowMessage(ex.Message, "Error", MessageBoxIcon.Error);
            }
        }

        private void ProaldCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = _db.ExecuteQuery(
                    "SELECT ProductName, Price FROM Products WHERE ProductID = @ID",
                    new Dictionary<string, object> { { "@ID", ProaldCb.Text } });

                if (dt.Rows.Count > 0)
                {
                    ProNameTb.Text = dt.Rows[0]["ProductName"].ToString();
                    PriceTb.Text = dt.Rows[0]["Price"].ToString();
                }
            }
            catch (ApplicationException ex)
            {
                ShowMessage(ex.Message, "Error", MessageBoxIcon.Error);
            }
        }

        private void QuenTb_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ProaldCb.Text) &&
                int.TryParse(QuenTb.Text, out int qty))
            {
                try
                {
                    object result = _db.ExecuteScalar(
                        "SELECT Price FROM Products WHERE ProductID = @ID",
                        new Dictionary<string, object> { { "@ID", ProaldCb.Text } });

                    if (result != null && decimal.TryParse(result.ToString(), out decimal price))
                        PriceTb.Text = (qty * price).ToString("0.00");
                    else
                        PriceTb.Text = "0.00";
                }
                catch
                {
                    PriceTb.Text = "0.00";
                }
            }
            else
            {
                PriceTb.Text = "0.00";
            }
        }

        // ────────────────────────────────────────────────────────
        // CRUD OPERATIONS
        // ────────────────────────────────────────────────────────

        private void Addbtn_Click(object sender, EventArgs e)
        {
            if (!ValidateSaleInputs()) return;

            if (!int.TryParse(QuenTb.Text, out int quantitySold) || quantitySold <= 0)
            {
                ShowMessage("Please enter a valid quantity.", "Validation", MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Check available stock
                DataTable stockDt = _db.ExecuteQuery(
                    "SELECT Quantity, Price FROM Products WHERE ProductID = @PID",
                    new Dictionary<string, object> { { "@PID", ProaldCb.Text } });

                if (stockDt.Rows.Count == 0)
                {
                    ShowMessage("Product not found.", "Error", MessageBoxIcon.Error);
                    return;
                }

                int availableQty = Convert.ToInt32(stockDt.Rows[0]["Quantity"]);
                decimal unitPrice = Convert.ToDecimal(stockDt.Rows[0]["Price"]);

                if (quantitySold > availableQty)
                {
                    ShowMessage($"Stock not available. Available: {availableQty}",
                                "Stock Error", MessageBoxIcon.Warning);
                    return;
                }

                decimal totalPrice = quantitySold * unitPrice;

                // Insert sale record
                _db.ExecuteNonQuery(
                    "INSERT INTO Sales (CustomerID, CustomerName, ProductID, ProductName, " +
                    "QuantitySold, TotalAmount, SaleDate) " +
                    "VALUES (@CID, @CN, @PID, @PN, @QS, @TA, @SD)",
                    new Dictionary<string, object>
                    {
                        { "@CID", CusIdCB.Text },
                        { "@CN",  CusNameTb.Text },
                        { "@PID", ProaldCb.Text },
                        { "@PN",  ProNameTb.Text },
                        { "@QS",  quantitySold },
                        { "@TA",  totalPrice },
                        { "@SD",  dateTimePicker1.Value }
                    });

                // Deduct from stock
                _db.ExecuteNonQuery(
                    "UPDATE Products SET Quantity = Quantity - @Qty WHERE ProductID = @PID",
                    new Dictionary<string, object>
                    {
                        { "@Qty", quantitySold },
                        { "@PID", ProaldCb.Text }
                    });

                _logger.LogAction("INSERT", "Sales",
                                  $"CustomerID={CusIdCB.Text}, ProductID={ProaldCb.Text}, " +
                                  $"Qty={quantitySold}, Total={totalPrice:0.00}");

                ShowMessage("Sale recorded successfully.");
                DisplayData();
                ResetFields();
            }
            catch (ApplicationException ex)
            {
                ShowMessage($"Could not record sale:\n{ex.Message}",
                            "Insert Error", MessageBoxIcon.Error);
            }
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                ShowMessage("Please select a sale to update.",
                            "No Selection", MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateSaleInputs()) return;

            try
            {
                int saleId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["SaleID"].Value);
                int oldQty = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["QuantitySold"].Value);
                string productId = dataGridView1.SelectedRows[0].Cells["ProductID"].Value.ToString();

                // Restore old stock first: + old qty back ✅ (BUG FIX: was * instead of +)
                _db.ExecuteNonQuery(
                    "UPDATE Products SET Quantity = Quantity + @OldQty WHERE ProductID = @PID",
                    new Dictionary<string, object>
                    {
                        { "@OldQty", oldQty },
                        { "@PID",    productId }
                    });

                // Check new stock availability
                object stockResult = _db.ExecuteScalar(
                    "SELECT Quantity FROM Products WHERE ProductID = @PID",
                    new Dictionary<string, object> { { "@PID", ProaldCb.Text } });

                int currentStock = Convert.ToInt32(stockResult);
                int newQty = Convert.ToInt32(QuenTb.Text);

                if (newQty > currentStock)
                {
                    ShowMessage($"Stock not available. Available: {currentStock}",
                                "Stock Error", MessageBoxIcon.Warning);
                    // Rollback: re-deduct old qty
                    _db.ExecuteNonQuery(
                        "UPDATE Products SET Quantity = Quantity - @OldQty WHERE ProductID = @PID",
                        new Dictionary<string, object>
                        {
                            { "@OldQty", oldQty },
                            { "@PID",    productId }
                        });
                    return;
                }

                decimal unitPrice = Convert.ToDecimal(PriceTb.Text);
                decimal total = newQty * unitPrice;

                // Update sale record
                _db.ExecuteNonQuery(
                    "UPDATE Sales SET CustomerID=@CID, CustomerName=@CN, ProductID=@PID, " +
                    "ProductName=@PN, QuantitySold=@QS, TotalAmount=@TA, SaleDate=@SD " +
                    "WHERE SaleID=@SID",
                    new Dictionary<string, object>
                    {
                        { "@CID", CusIdCB.Text },
                        { "@CN",  CusNameTb.Text },
                        { "@PID", ProaldCb.Text },
                        { "@PN",  ProNameTb.Text },
                        { "@QS",  newQty },
                        { "@TA",  total },
                        { "@SD",  dateTimePicker1.Value },
                        { "@SID", saleId }
                    });

                // Deduct new qty from stock
                _db.ExecuteNonQuery(
                    "UPDATE Products SET Quantity = Quantity - @NewQty WHERE ProductID = @PID",
                    new Dictionary<string, object>
                    {
                        { "@NewQty", newQty },
                        { "@PID",    ProaldCb.Text }
                    });

                _logger.LogAction("UPDATE", "Sales",
                                  $"SaleID={saleId}, NewQty={newQty}, Total={total:0.00}");

                ShowMessage("Sale updated successfully.");
                DisplayData();
                ResetFields();
            }
            catch (ApplicationException ex)
            {
                // FIXED: was empty catch block — now shows meaningful error ✅
                ShowMessage($"Could not update sale:\n{ex.Message}",
                            "Update Error", MessageBoxIcon.Error);
            }
        }

        private void DelBtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                ShowMessage("Please select a sale to delete.",
                            "No Selection", MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to delete this sale?",
                "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                int saleId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["SaleID"].Value);
                int qtyToRestore = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["QuantitySold"].Value);
                string productId = dataGridView1.SelectedRows[0].Cells["ProductID"].Value.ToString();

                // Restore stock: + qty back ✅ (BUG FIX: was * instead of +)
                _db.ExecuteNonQuery(
                    "UPDATE Products SET Quantity = Quantity + @OldQty WHERE ProductID = @PID",
                    new Dictionary<string, object>
                    {
                        { "@OldQty", qtyToRestore },
                        { "@PID",    productId }
                    });

                // Delete sale
                _db.ExecuteNonQuery(
                    "DELETE FROM Sales WHERE SaleID = @SaleID",
                    new Dictionary<string, object> { { "@SaleID", saleId } });

                _logger.LogAction("DELETE", "Sales",
                                  $"SaleID={saleId}, RestoredQty={qtyToRestore}");

                ShowMessage("Sale deleted successfully.");
                DisplayData();
                ResetFields();
            }
            catch (ApplicationException ex)
            {
                ShowMessage($"Could not delete sale:\n{ex.Message}",
                            "Delete Error", MessageBoxIcon.Error);
            }
        }

        // ── UI Event Handlers ─────────────────────────────────────

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
                dateTimePicker1.Value =
                    Convert.ToDateTime(dataGridView1.CurrentRow.Cells["SaleDate"].Value);
            }
        }

        private void Clrbtn_Click(object sender, EventArgs e) => ResetFields();
        private void Cross_Click(object sender, EventArgs e) => this.Close();

        // Navigation — using inherited NavigateTo (INavigable) ✅
        private void label2_Click(object sender, EventArgs e) => NavigateTo(new Product());
        private void label3_Click(object sender, EventArgs e) => NavigateTo(new Customers());
        private void label5_Click(object sender, EventArgs e) => NavigateTo(new Login());

        // ── Print Report (unchanged logic, cleaned up style) ──────

        private void ReportBtn_Click_1(object sender, EventArgs e)
        {
            _currentRow = 0;
            CalculateColumnWidths();
            PrintPreviewDialog preview = new PrintPreviewDialog
            {
                Document = _printDocument
            };
            preview.ShowDialog();
        }

        private void CalculateColumnWidths()
        {
            _columnWidths = new int[dataGridView1.Columns.Count];
            using (Graphics g = this.CreateGraphics())
            {
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    int maxWidth = (int)g.MeasureString(
                        dataGridView1.Columns[i].HeaderText,
                        new Font("Segoe UI", 9)).Width + 20;

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Cells[i].Value != null)
                        {
                            int cellWidth = (int)g.MeasureString(
                                row.Cells[i].Value.ToString(),
                                new Font("Segoe UI", 9)).Width + 20;
                            if (cellWidth > maxWidth) maxWidth = cellWidth;
                        }
                    }
                    _columnWidths[i] = maxWidth;
                }
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            int rowHeight = 25;
            int topMargin = e.MarginBounds.Top + 60;
            int yPos = topMargin;
            Font headerFont = new Font("Segoe UI", 14, FontStyle.Bold);
            Font cellFont = new Font("Segoe UI", 9);
            Font tableHeaderFont = new Font("Segoe UI", 9, FontStyle.Bold);

            CalculateColumnWidths();

            int tableWidth = _columnWidths.Sum();
            int leftMargin = e.MarginBounds.Left + (e.MarginBounds.Width - tableWidth) / 2;

            // Draw heading
            string heading = "Sales Report";
            SizeF headingSize = e.Graphics.MeasureString(heading, headerFont);
            float headingX = e.MarginBounds.Left + (e.MarginBounds.Width - headingSize.Width) / 2;
            e.Graphics.DrawString(heading, headerFont, Brushes.Black, headingX, e.MarginBounds.Top);

            int x = leftMargin;

            // Draw column headers
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                Rectangle headerRect = new Rectangle(x, yPos, _columnWidths[i], rowHeight);
                e.Graphics.FillRectangle(Brushes.DarkSlateGray, headerRect);
                e.Graphics.DrawRectangle(Pens.Black, headerRect);
                e.Graphics.DrawString(dataGridView1.Columns[i].HeaderText,
                                      tableHeaderFont, Brushes.White, headerRect);
                x += _columnWidths[i];
            }

            yPos += rowHeight;

            // Draw rows
            while (_currentRow < dataGridView1.Rows.Count)
            {
                DataGridViewRow row = dataGridView1.Rows[_currentRow];
                if (!row.IsNewRow)
                {
                    x = leftMargin;
                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {
                        string cellText = row.Cells[i].Value?.ToString() ?? "";
                        Rectangle cellRect = new Rectangle(x, yPos, _columnWidths[i], rowHeight);
                        e.Graphics.DrawRectangle(Pens.Black, cellRect);
                        e.Graphics.DrawString(cellText, cellFont, Brushes.Black, cellRect);
                        x += _columnWidths[i];
                    }
                    yPos += rowHeight;
                }

                if (yPos + rowHeight > e.MarginBounds.Bottom)
                {
                    _currentRow++;
                    e.HasMorePages = true;
                    return;
                }

                _currentRow++;
            }

            _currentRow = 0;
            e.HasMorePages = false;
        }

        // ── Private Validation ────────────────────────────────────

        private bool ValidateSaleInputs()
        {
            if (string.IsNullOrWhiteSpace(CusIdCB.Text))
            {
                ShowMessage("Please select a customer.", "Validation", MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(ProaldCb.Text))
            {
                ShowMessage("Please select a product.", "Validation", MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(QuenTb.Text))
            {
                ShowMessage("Please enter a quantity.", "Validation", MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}
