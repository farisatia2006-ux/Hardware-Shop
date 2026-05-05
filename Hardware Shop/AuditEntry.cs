using System;

namespace Hardware_Shop
{
    // ============================================================
    //  ARRAY OF OBJECTS — OOP Requirement
    //  AuditEntry هو الـ object اللي هنعمل منه array في AuditLogger
    //  وده بيطبق مفهوم Array of Objects بشكل حقيقي وعملي.
    // ============================================================

    /// <summary>
    /// Represents a single audit log entry that records a user action.
    /// Instances of this class are stored in arrays inside AuditLogger.
    /// Demonstrates: Encapsulation (private setters), Array of Objects.
    /// </summary>
    public class AuditEntry
    {
        // Encapsulation: properties with public get, private set
        public DateTime Timestamp { get; private set; }
        public string Action { get; private set; }
        public string Entity { get; private set; }
        public string Details { get; private set; }

        // Constructor enforces that all fields are set at creation time
        public AuditEntry(string action, string entity, string details = "")
        {
            Timestamp = DateTime.Now;
            Action = action;
            Entity = entity;
            Details = details;
        }

        /// <summary>
        /// Returns a formatted string representation of this audit entry.
        /// Used when writing to the log file.
        /// </summary>
        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Action} | {Entity} | {Details}";
        }
    }
}
