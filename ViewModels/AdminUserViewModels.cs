using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.ViewModels
{
    public class AdminUserCreateViewModel
    {
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required, StringLength(255, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Accountant|Warehouse|Customer)$", ErrorMessage = "Select Accountant, Warehouse, or Customer.")]
        public string Role { get; set; } = "Customer";

        [StringLength(50)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(50)]
        public string ContactPerson { get; set; } = string.Empty;

        [EmailAddress, StringLength(50)]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        public string Address { get; set; } = string.Empty;
    }

    public class AdminUserEditViewModel
    {
        public int UserID { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [StringLength(255, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        [RegularExpression("^(Accountant|Warehouse|Customer)$", ErrorMessage = "Select Accountant, Warehouse, or Customer.")]
        public string Role { get; set; } = "Customer";

        [StringLength(50)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(50)]
        public string ContactPerson { get; set; } = string.Empty;

        [EmailAddress, StringLength(50)]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        public string Address { get; set; } = string.Empty;
    }
}