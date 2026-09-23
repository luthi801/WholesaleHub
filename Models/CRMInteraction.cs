using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesaleHub.Models
{
    public class CRMInteraction
    {
        [Key]
        public int CRMID { get; set; }

        [Required]
        public int CustomerID { get; set; }

        public int? CreatedBy { get; set; }

        [Required, StringLength(30)]
        public string InteractionType { get; set; } = string.Empty; // Phone Call, Email, Site Visit, Complaint, Follow-up

        [Required, StringLength(255)]
        public string Notes { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; } = DateTime.Now;

        [ForeignKey("CustomerID")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User? User { get; set; }
    }
}