namespace SlipperIS.Core.Models;

public class SalesOrderDetail
{
    public int Id { get; set; }
    public int SalesOrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercentage { get; set; } = 0;
    public decimal LineAmount { get; set; }
    public string Remarks { get; set; } = string.Empty;

    public virtual SalesOrder? SalesOrder { get; set; }
    public virtual Product? Product { get; set; }
}
