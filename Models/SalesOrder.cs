using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.Models
{
    public class SalesOrder
    {
        [Key]
        public int SalesOrderID { get; set; }

        public int Id { get; set; }
        public int CustomerID { get; set; }
        public Customer? Customer { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();
        public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
        public AccountsReceivable? AccountsReceivable { get; set; }
    }

    public class SalesOrderItemInput
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreateSalesOrderInput
    {
        public string CustomerName { get; set; } = string.Empty;
        public IEnumerable<SalesOrderItemInput>? Items { get; set; }
    }
}