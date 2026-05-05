using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Hardware_Shop
{
    // ============================================================
    //  ENCAPSULATION + SRP (Single Responsibility Principle)
    //  DatabaseHelper مسؤول فقط عن التواصل مع قاعدة البيانات.
    //  بيحمي الـ connection string ويوفر API نظيف للـ Forms.
    //  بيلغي التكرار: كان في كل form كود اتصال مكرر.
    //
    //  METHOD OVERLOADING — OOP Requirement
    //  ExecuteQuery موجودة بـ 3 نسخ مختلفة الـ parameters.
    // ============================================================

    /// <summary>
    /// Centralizes all database operations for the Hardware Shop system.
    /// Demonstrates: Encapsulation, Method Overloading, Exception Handling, SRP.
    /// </summary>
    public class DatabaseHelper
    {
        // ── Encapsulated private connection string ───────────────
        private readonly string _connectionString;

        public DatabaseHelper()
        {
            // Connection string in ONE place — all forms share this
            _connectionString =
                @"Data Source=(LocalDB)\MSSQLLocalDB;" +
                @"AttachDbFilename=|DataDirectory|\Hardware_Shop_MS.mdf;" +
                @"Integrated Security=True;Connect Timeout=30";
        }

        // ── Factory: create and open a fresh connection ──────────
        private SqlConnection OpenConnection()
        {
            SqlConnection con = new SqlConnection(_connectionString);
            con.Open();
            return con;
        }

        // ────────────────────────────────────────────────────────
        // METHOD OVERLOADING (Compile-time Polymorphism) ✅
        // ExecuteQuery بـ 3 توقيعات مختلفة
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Overload 1: Executes a SELECT query with no parameters.
        /// </summary>
        public DataTable ExecuteQuery(string sql)
        {
            return ExecuteQuery(sql, new Dictionary<string, object>());
        }

        /// <summary>
        /// Overload 2: Executes a SELECT query with a parameters dictionary.
        /// </summary>
        public DataTable ExecuteQuery(string sql, Dictionary<string, object> parameters)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = OpenConnection())
                using (SqlCommand cmd = BuildCommand(sql, con, parameters))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }
            catch (SqlException ex)
            {
                // Specific SqlException — more informative than generic Exception ✅
                throw new ApplicationException(
                    $"Database query failed.\nSQL: {sql}\nError: {ex.Message}", ex);
            }
            return dt;
        }

        /// <summary>
        /// Overload 3: Executes a SELECT and returns a single scalar value.
        /// </summary>
        public object ExecuteScalar(string sql, Dictionary<string, object> parameters)
        {
            try
            {
                using (SqlConnection con = OpenConnection())
                using (SqlCommand cmd = BuildCommand(sql, con, parameters))
                {
                    return cmd.ExecuteScalar();
                }
            }
            catch (SqlException ex)
            {
                throw new ApplicationException(
                    $"Scalar query failed.\nError: {ex.Message}", ex);
            }
        }

        // ────────────────────────────────────────────────────────
        // ExecuteNonQuery — for INSERT / UPDATE / DELETE
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Executes INSERT, UPDATE, or DELETE and returns rows affected.
        /// </summary>
        public int ExecuteNonQuery(string sql, Dictionary<string, object> parameters)
        {
            try
            {
                using (SqlConnection con = OpenConnection())
                using (SqlCommand cmd = BuildCommand(sql, con, parameters))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new ApplicationException(
                    $"Database operation failed.\nSQL: {sql}\nError: {ex.Message}", ex);
            }
        }

        // ── Private helper: builds a parameterized SqlCommand ────
        private SqlCommand BuildCommand(string sql, SqlConnection con,
                                        Dictionary<string, object> parameters)
        {
            SqlCommand cmd = new SqlCommand(sql, con);
            if (parameters != null)
            {
                foreach (var kv in parameters)
                    cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
            }
            return cmd;
        }
    }
}

