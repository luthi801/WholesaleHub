using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.Models
{
    public class CustomerCrmInteraction
    {
        [Key]
        public int InteractionID { get; set; }

        public int CustomerID { get; set; }
        public Customer? Customer { get; set; }

        public int UserID { get; set; }
        public User? User { get; set; }

        [Required, StringLength(30)]
        public string InteractionType { get; set; } = string.Empty; // Email, Call, Meeting, Complaint

        [Required, StringLength(255)]
        public string Notes { get; set; } = string.Empty;

        public DateTime InteractionDate { get; set; } = DateTime.Now;
    }
}