namespace SlipperIS.Core.Models;

public class SalesOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public DateTime DeliveryDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Shipped, Completed, Cancelled
    public string PaymentStatus { get; set; } = "Unpaid";
    public string ShippingAddress { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public virtual Customer? Customer { get; set; }
    public virtual User? CreatedByUser { get; set; }
    public virtual ICollection<SalesOrderDetail> OrderDetails { get; set; } = new List<SalesOrderDetail>();
}
