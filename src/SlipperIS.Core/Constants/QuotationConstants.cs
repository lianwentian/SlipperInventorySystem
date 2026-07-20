namespace SlipperIS.Core.Constants;

public static class RoleConstants
{
    public const string ADMIN = "Admin";
    public const string SALES = "Sales";
    public const string WAREHOUSE = "Warehouse";
    public const string FINANCE = "Finance";
}

public static class QuotationConstants
{
    public const string STATUS_DRAFT = "Draft";
    public const string STATUS_SENT = "Sent";
    public const string STATUS_ACCEPTED = "Accepted";
    public const string STATUS_REJECTED = "Rejected";
    public const string STATUS_EXPIRED = "Expired";
    public const string STATUS_CONVERTED = "Converted";

    public const int DEFAULT_VALIDITY_DAYS = 30;
    public const string QUOTATION_PREFIX = "QT";
}
