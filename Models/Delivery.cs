using System;
using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.Models
{
    public class Delivery
    {
        [Key]
        public int DeliveryID { get; set; }

        public int Id { get; set; }
        public int SalesOrderID { get; set; }
        public int UserID { get; set; }
        public User? User { get; set; }
        public SalesOrder? SalesOrder { get; set; }
        public string Status { get; set; } = string.Empty;
        public string DeliveryStatus { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; } = DateTime.UtcNow;
    }
}