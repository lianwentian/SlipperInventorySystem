namespace SlipperIS.Core.Models;

public class QuotationDetail
{
    public int Id { get; set; }
    public int QuotationId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal LineAmount { get; set; }
    public decimal DiscountPercentage { get; set; } = 0;
    public decimal DiscountedAmount { get; set; }
    public string Remarks { get; set; } = string.Empty;

    public virtual Quotation? Quotation { get; set; }
    public virtual Product? Product { get; set; }
}
