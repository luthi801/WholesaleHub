using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.Models
{
    public class AuditLog
    {
        [Key]
        public int LogID { get; set; }

        public int? UserID { get; set; }
        public User? User { get; set; }

        [Required, StringLength(100)]
        public string ActionPerformed { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}