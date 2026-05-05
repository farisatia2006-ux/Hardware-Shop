//using System;
//using System.Windows.Forms;
//using Hardware_Shop.Models;
//using Hardware_Shop.Services;

//namespace Hardware_Shop
//{
//    public partial class Login : Form
//    {
//        private AdminAuth authService;

//        public Login()
//        {
//            InitializeComponent();
//            authService = new AdminAuth(); // Dependency
//        }

//        private void label4_Click(object sender, EventArgs e)
//        {
//            this.Close();
//        }

//        private void loginbtn_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                User user = new User(UserTb.Text, Passtb.Text);

//                if (authService.Login(user))
//                {
//                    MessageBox.Show("Login successful");

//                    Product product = new Product();
//                    product.Show();
//                    this.Hide();
//                }
//                else
//                {
//                    MessageBox.Show("Invalid username or password");
//                }
//            }
//            catch (Exception)
//            {
//                MessageBox.Show("Something went wrong. Please try again.");
//            }
//        }

//        private void ClrLbl_Click(object sender, EventArgs e)
//        {
//            UserTb.Text = "";
//            Passtb.Text = "";
//        }

//        private void Login_Load(object sender, EventArgs e)
//        {

//        }
//    }
//}
using DevExpress.Xpo.Logger;
using System;
using System.Windows.Forms;

namespace Hardware_Shop
{
    // ============================================================
    //  Login.cs — REFACTORED
    //
    //  التغييرات من منظور OOP:
    //  • ترث من BaseDataForm (Inheritance) ✅
    //  • تطبق INavigable من خلال BaseDataForm ✅
    //  • DisplayData و ResetFields overriding للـ abstract methods
    //    (Polymorphism - runtime) ✅
    //  • Exception Handling محسّن ✅
    //  • الـ credentials انفصلت لـ constants محمية ✅
    // ============================================================

    /// <summary>
    /// Login form — entry point of the application.
    /// Inherits from BaseDataForm to gain shared functionality.
    /// Demonstrates: Inheritance, Polymorphism (abstract method override),
    ///               Encapsulation, Exception Handling.
    /// </summary>
    public partial class Login : BaseDataForm  // Inheritance ✅
    {
        // ── Encapsulated credentials (private constants) ─────────
        // في مشروع حقيقي دول بيتخزنوا في DB أو config مشفّر،
        // لكن لأغراض الـ OOP project دول private constants
        private const string ValidUsername = "Admin";
        private const string ValidPassword = "Password";

        // ── Constructor ──────────────────────────────────────────
        public Login()
        {
            InitializeComponent();
        }

        // ────────────────────────────────────────────────────────
        // POLYMORPHISM — Overriding abstract methods ✅
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Login form has no grid to display — empty implementation of abstract method.
        /// Required by BaseDataForm contract (Abstraction).
        /// </summary>
        protected override void DisplayData()
        {
            // Login form doesn't display tabular data — intentionally empty
        }

        /// <summary>
        /// Clears username and password fields.
        /// Overrides the abstract ResetFields from BaseDataForm (Polymorphism). ✅
        /// </summary>
        protected override void ResetFields()  // Polymorphism ✅
        {
            UserTb.Text = "";
            Passtb.Text = "";
        }

        // ────────────────────────────────────────────────────────
        // EXCEPTION HANDLING — OOP Requirement ✅
        // ────────────────────────────────────────────────────────

        private void loginbtn_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input fields first
                if (string.IsNullOrWhiteSpace(UserTb.Text) ||
                    string.IsNullOrWhiteSpace(Passtb.Text))
                {
                    ShowMessage("Please enter both username and password.",
                                "Login", MessageBoxIcon.Warning);  // Overloaded ShowMessage ✅
                    return;
                }

                if (UserTb.Text == ValidUsername && Passtb.Text == ValidPassword)
                {
                    ShowMessage("Login successful.");  // Overloaded ShowMessage ✅
                    _logger.LogAction("LOGIN", "System", $"User '{UserTb.Text}' logged in.");

                    NavigateTo(new Product());  // INavigable — no more 3-line copy-paste ✅
                }
                else
                {
                    ShowMessage("Invalid username or password.",
                                "Login Failed", MessageBoxIcon.Error);
                    _logger.LogAction("LOGIN_FAIL", "System",
                                      $"Failed attempt for user '{UserTb.Text}'.");
                }
            }
            catch (Exception ex)
            {
                // Exception Handling: specific message, not just ex.Message ✅
                ShowMessage($"An unexpected error occurred during login:\n{ex.Message}",
                            "Error", MessageBoxIcon.Error);
            }
        }

        private void label4_Click(object sender, EventArgs e) => this.Close();

        private void ClrLbl_Click(object sender, EventArgs e) => ResetFields();

        private void Login_Load(object sender, EventArgs e) { }
    }
}
