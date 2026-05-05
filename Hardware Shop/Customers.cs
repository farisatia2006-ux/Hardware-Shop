using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace Hardware_Shop
{
    // ============================================================
    //  Customers.cs — REFACTORED
    //
    //  التغييرات من منظور OOP:
    //  • ترث من BaseDataForm (Inheritance) ✅
    //  • DisplayData / ResetFields تعمل override (Polymorphism) ✅
    //  • SqlConnection اتنقلت لـ DatabaseHelper (Encapsulation) ✅
    //  • Navigation بـ NavigateTo بدل 3 أسطر مكررة ✅
    //  • Exception Handling بـ specific exceptions ✅
    //  • AuditLogger بيسجل كل العمليات (File Handling) ✅
    // ============================================================

    /// <summary>
    /// Customers management form.
    /// Inherits from BaseDataForm — gains DB access, logging, grid styling, navigation.
    /// Demonstrates: Inheritance, Polymorphism, Encapsulation, Exception Handling,
    ///               File Handling (via AuditLogger).
    /// </summary>
    public partial class Customers : BaseDataForm  // Inheritance ✅
    {
        // ── Constructor ──────────────────────────────────────────
        public Customers()
        {
            InitializeComponent();
            DisplayData();   // calls overridden abstract method
        }

        // ────────────────────────────────────────────────────────
        // POLYMORPHISM — Overriding abstract methods ✅
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Loads and displays all customers in the DataGridView.
        /// Overrides the abstract DisplayData() from BaseDataForm. ✅
        /// </summary>
        protected override void DisplayData()  // Polymorphism ✅
        {
            try
            {
                // DatabaseHelper.ExecuteQuery — no params overload ✅
                DataTable dt = _db.ExecuteQuery("SELECT * FROM Customers");
                dataGridView1.DataSource = dt;
            }
            catch (ApplicationException ex)
            {
                // Catching specific exception type ✅
                ShowMessage(ex.Message, "Load Error", MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Clears all customer input fields.
        /// Overrides abstract ResetFields() from BaseDataForm. ✅
        /// </summary>
        protected override void ResetFields()  // Polymorphism ✅
        {
            CusNameTb.Text = "";
            CusPhoneTb.Text = "";
            Ptb.Text = "";
        }

        // ────────────────────────────────────────────────────────
        // CRUD OPERATIONS — use DatabaseHelper + AuditLogger
        // ────────────────────────────────────────────────────────

        private void Addbtn_Click(object sender, EventArgs e)
        {
            // Validate inputs before attempting DB operation ✅
            if (!ValidateInputs()) return;

            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "@CName", CusNameTb.Text.Trim() },
                    { "@Phone", CusPhoneTb.Text.Trim() },
                    { "@Email", Ptb.Text.Trim() }
                };

                _db.ExecuteNonQuery(
                    "INSERT INTO Customers (CName, Phone, Email) VALUES (@CName, @Phone, @Email)",
                    parameters);

                // File Handling via AuditLogger ✅
                _logger.LogAction("INSERT", "Customers",
                                  $"Added customer: {CusNameTb.Text.Trim()}");

                ShowMessage("Customer added successfully.");
                DisplayData();
                ResetFields();
            }
            catch (ApplicationException ex)
            {
                ShowMessage($"Could not add customer:\n{ex.Message}",
                            "Insert Error", MessageBoxIcon.Error);
            }
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                ShowMessage("Please select a customer to update.",
                            "No Selection", MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInputs()) return;

            try
            {
                int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);

                var parameters = new Dictionary<string, object>
                {
                    { "@CustomerID", id },
                    { "@CName",  CusNameTb.Text.Trim() },
                    { "@Phone",  CusPhoneTb.Text.Trim() },
                    { "@Email",  Ptb.Text.Trim() }
                };

                _db.ExecuteNonQuery(
                    "UPDATE Customers SET CName=@CName, Phone=@Phone, Email=@Email " +
                    "WHERE CustomerID=@CustomerID",
                    parameters);

                _logger.LogAction("UPDATE", "Customers",
                                  $"Updated CustomerID={id}, Name={CusNameTb.Text.Trim()}");

                ShowMessage("Customer updated successfully.");
                DisplayData();
                ResetFields();
            }
            catch (ApplicationException ex)
            {
                ShowMessage($"Could not update customer:\n{ex.Message}",
                            "Update Error", MessageBoxIcon.Error);
            }
        }

        private void DelBtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                ShowMessage("Please select a customer to delete.",
                            "No Selection", MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to delete this customer?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);

                _db.ExecuteNonQuery(
                    "DELETE FROM Customers WHERE CustomerID=@CustomerID",
                    new Dictionary<string, object> { { "@CustomerID", id } });

                _logger.LogAction("DELETE", "Customers", $"Deleted CustomerID={id}");

                ShowMessage("Customer deleted successfully.");
                DisplayData();
                ResetFields();
            }
            catch (ApplicationException ex)
            {
                ShowMessage($"Could not delete customer:\n{ex.Message}",
                            "Delete Error", MessageBoxIcon.Error);
            }
        }

        // ── UI Event Handlers ─────────────────────────────────────

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
            ApplyGridStyle(dataGridView1);  // Inherited from BaseDataForm ✅
        }

        private void Clrbtn_Click(object sender, EventArgs e) => ResetFields();
        private void Cross_Click(object sender, EventArgs e) => this.Close();

        // Navigation — using inherited NavigateTo (INavigable) ✅
        private void label2_Click(object sender, EventArgs e) => NavigateTo(new Product());
        private void label4_Click(object sender, EventArgs e) => NavigateTo(new Sales());
        private void label5_Click(object sender, EventArgs e) => NavigateTo(new Login());

        // ── Private Validation ────────────────────────────────────

        /// <summary>
        /// Validates that all required customer fields are filled.
        /// Encapsulates validation logic in a single reusable method.
        /// </summary>
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(CusNameTb.Text))
            {
                ShowMessage("Customer name is required.",
                            "Validation", MessageBoxIcon.Warning);
                CusNameTb.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(CusPhoneTb.Text))
            {
                ShowMessage("Phone number is required.",
                            "Validation", MessageBoxIcon.Warning);
                CusPhoneTb.Focus();
                return false;
            }
            return true;
        }
    }
}