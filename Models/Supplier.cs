using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }

        [Required, StringLength(50)]
        public string SupplierName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string ContactPerson { get; set; } = string.Empty;

        [Required, StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(50)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Address { get; set; } = string.Empty;
    }
}