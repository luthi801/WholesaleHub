using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.Models
{
    public class AccountsReceivable
    {
        [Key]
        public int AR_ID { get; set; }

        public int SalesOrderID { get; set; }
        public SalesOrder? SalesOrder { get; set; }

        public int CustomerID { get; set; }
        public Customer? Customer { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentStatus { get; set; } = "Pending";

        public ICollection<CustomerPayment> Payments { get; set; } = new List<CustomerPayment>();

        public decimal OutstandingBalance => TotalAmount - AmountPaid;
    }

}