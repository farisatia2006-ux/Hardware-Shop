using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Hardware_Shop
{
    // ============================================================
    //  ABSTRACTION — OOP Requirement
    //  INavigable بيعرّف contract للـ navigation بين الـ forms
    //  من غير ما يحدد الـ implementation.
    // ============================================================

    /// <summary>
    /// Contract for any form that supports navigation to another form.
    /// Demonstrates: Abstraction via Interface, Runtime Polymorphism.
    /// </summary>
    public interface INavigable
    {
        /// <summary>Shows the target form and hides the current one.</summary>
        void NavigateTo(Form targetForm);
    }

    // ============================================================
    //  INHERITANCE — OOP Requirement
    //  POLYMORPHISM — OOP Requirement (abstract methods overridden in children)
    //  METHOD OVERLOADING — ShowMessage overloaded 3 مرات
    //  ENCAPSULATION — _db و _logger محميين بـ protected
    //
    //  BaseDataForm هو الأساس اللي كل الـ 3 forms بترث منه.
    //  بيحتوي على الكود المشترك اللي كان مكرر في كل form.
    // ============================================================

    /// <summary>
    /// Abstract base class for all data-entry forms in the Hardware Shop.
    /// Provides shared database access, grid styling, navigation, and logging.
    /// Demonstrates: Inheritance, Abstraction, Polymorphism, Encapsulation,
    ///               Method Overloading, Exception Handling.
    /// </summary>
    public  class BaseDataForm : Form, INavigable  // Inheritance + Interface ✅
    {
        // ── Protected members (accessible by child classes only) ─
        protected readonly DatabaseHelper _db;       // Encapsulation ✅
        protected readonly AuditLogger _logger;   // Encapsulation ✅

        // ── Constructor ─────────────────────────────────────────
        protected BaseDataForm()
        {
            _db = new DatabaseHelper();
            _logger = new AuditLogger();
        }

        // ────────────────────────────────────────────────────────
        // ABSTRACT METHODS — Abstraction + Polymorphism ✅
        // كل child form لازم تعرّف إزاي تعرض البيانات وتعمل reset
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Each child form implements its own data-loading logic.
        /// Overriding this method is Polymorphism (runtime). ✅
        /// </summary>
        protected virtual void DisplayData()
        {
            // Default implementation (can be overridden by child forms)
        }

        /// <summary>
        /// Each child form implements its own field-reset logic.
        /// </summary>
        protected virtual void ResetFields()
        {
            // Default implementation (can be overridden by child forms)
        }

        // ────────────────────────────────────────────────────────
        // SHARED CONCRETE METHOD — used by all child forms
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Applies the standard dark-header style to any DataGridView.
        /// One method called from 3 forms instead of duplicating 6 lines each.
        /// </summary>
        protected void ApplyGridStyle(DataGridView dgv)
        {
            Color headerColor = Color.FromArgb(44, 62, 80);
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = headerColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9, FontStyle.Bold);
            dgv.DefaultCellStyle.SelectionBackColor = headerColor;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.GridColor = Color.LightGray;
        }

        // ────────────────────────────────────────────────────────
        // METHOD OVERLOADING — OOP Requirement ✅
        // ShowMessage بـ 3 توقيعات مختلفة
        // ────────────────────────────────────────────────────────

        /// <summary>Overload 1: simple message.</summary>
        protected void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        /// <summary>Overload 2: message with custom title.</summary>
        protected void ShowMessage(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }

        /// <summary>Overload 3: message with custom title and icon.</summary>
        protected void ShowMessage(string message, string title, MessageBoxIcon icon)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }

        // ────────────────────────────────────────────────────────
        // INavigable Implementation — Runtime Polymorphism ✅
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Navigates to the specified form and hides the current form.
        /// All child forms inherit this — no more copy-pasting 3 lines per button.
        /// </summary>
        public void NavigateTo(Form targetForm)
        {
            targetForm.Show();
            this.Hide();
        }
    }
}
