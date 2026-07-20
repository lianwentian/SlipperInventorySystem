namespace SlipperIS.Core.Models;

public class StockRecord
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string Type { get; set; } = string.Empty; // In, Out, Adjustment, Return
    public string Reason { get; set; } = string.Empty;
    public int? RelatedOrderId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual Product? Product { get; set; }
    public virtual User? CreatedByUser { get; set; }
}
