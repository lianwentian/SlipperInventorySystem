namespace SlipperIS.Core.Models;

public class Quotation
{
    public int Id { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime QuotationDate { get; set; } = DateTime.Now;
    public DateTime ValidUntilDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = "Draft";
    public string Remarks { get; set; } = string.Empty;
    public int? ConvertedToOrderId { get; set; }
    public bool ShowCostPrice { get; set; } = false;
    public bool ShowDiscount { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public virtual Customer? Customer { get; set; }
    public virtual User? CreatedByUser { get; set; }
    public virtual ICollection<QuotationDetail> QuotationDetails { get; set; } = new List<QuotationDetail>();
}
