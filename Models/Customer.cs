using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.Models
{
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }

        public int? UserID { get; set; }
        public User? User { get; set; }

        [Required, StringLength(50)]
        public string CompanyName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string ContactPerson { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(50)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Address { get; set; } = string.Empty;
    }
}