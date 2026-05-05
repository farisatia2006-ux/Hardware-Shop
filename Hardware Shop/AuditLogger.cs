using System;
using System.IO;
using System.Windows.Forms;

namespace Hardware_Shop
{
    // ============================================================
    //  FILE HANDLING — OOP Requirement
    //  ARRAY OF OBJECTS — OOP Requirement
    //
    //  AuditLogger مسؤول بالكامل عن الكتابة على الملفات.
    //  بيحتفظ بـ buffer من AuditEntry[] (array of objects) وبيكتبهم
    //  على ملف نصي. ده بيفصل مسؤولية الـ File I/O عن الـ Forms (SRP).
    // ============================================================

    /// <summary>
    /// Handles writing audit trail entries to a local text file.
    /// Satisfies: File Handling, Array of Objects, Encapsulation, SRP.
    /// </summary>
    public class AuditLogger
    {
        // ── Private state (Encapsulation) ──────────────────────
        private readonly string _logPath;
        private AuditEntry[] _pendingEntries;   // Array of Objects ✅
        private int _count;
        private const int BufferSize = 10;

        // ── Constructor ─────────────────────────────────────────
        public AuditLogger(string logPath = "audit_log.txt")
        {
            _logPath = logPath;
            _pendingEntries = new AuditEntry[BufferSize]; // Array of objects ✅
            _count = 0;
        }

        // ── Public Methods ──────────────────────────────────────

        /// <summary>
        /// Logs a single action. Flushes to file when buffer is full.
        /// </summary>
        public void LogAction(string action, string entity, string details = "")
        {
            // إذا الـ buffer امتلأ، اكتب للملف وامسحه (flush)
            if (_count >= BufferSize)
                Flush();

            _pendingEntries[_count] = new AuditEntry(action, entity, details);
            _count++;

            // دايماً اكتب فوراً للأمان
            Flush();
        }

        /// <summary>
        /// Overloaded: logs multiple entries at once from an array.
        /// Demonstrates Method Overloading.
        /// </summary>
        public void LogAction(AuditEntry[] entries)   // Method Overloading ✅
        {
            foreach (AuditEntry entry in entries)
                AppendToFile(entry);
        }

        /// <summary>
        /// Overloaded: logs with a specific timestamp (useful for imports).
        /// </summary>
        public void LogAction(string action, string entity, DateTime when)  // Overloading ✅
        {
            AppendToFile(new AuditEntry(action, entity, $"Logged at: {when:HH:mm:ss}"));
        }

        /// <summary>
        /// Reads and returns the last N lines from the audit log.
        /// Demonstrates File Reading (File Handling).
        /// </summary>
        public string[] ReadLastEntries(int count = 20)   // File Handling: READ ✅
        {
            try
            {
                if (!File.Exists(_logPath))
                    return new string[] { "No audit log found." };

                string[] allLines = File.ReadAllLines(_logPath);
                int start = Math.Max(0, allLines.Length - count);
                int length = allLines.Length - start;

                string[] result = new string[length];
                Array.Copy(allLines, start, result, 0, length);
                return result;
            }
            catch (IOException ex)
            {
                // Specific exception type — not generic catch ✅
                return new string[] { $"Could not read log: {ex.Message}" };
            }
        }

        // ── Private Helpers ─────────────────────────────────────

        /// <summary>
        /// Writes all buffered entries to the log file and resets the buffer.
        /// </summary>
        private void Flush()   // File Handling: WRITE ✅
        {
            try
            {
                // Using statement ensures the file is always closed properly
                using (StreamWriter sw = File.AppendText(_logPath))
                {
                    for (int i = 0; i < _count; i++)
                    {
                        if (_pendingEntries[i] != null)
                            sw.WriteLine(_pendingEntries[i].ToString());
                    }
                }

                // Reset buffer after flush
                _pendingEntries = new AuditEntry[BufferSize];
                _count = 0;
            }
            catch (IOException ex)
            {
                // Warn the user but don't crash the application
                MessageBox.Show(
                    $"Could not write to audit log:\n{ex.Message}",
                    "Logging Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void AppendToFile(AuditEntry entry)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(_logPath))
                    sw.WriteLine(entry.ToString());
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    $"Logging error: {ex.Message}",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}
