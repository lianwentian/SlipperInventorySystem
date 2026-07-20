namespace SlipperIS.Core.Constants;

public static class PermissionConstants
{
    public const string MODULE_PRODUCTS = "Products";
    public const string MODULE_SALES = "Sales";
    public const string MODULE_QUOTATIONS = "Quotations";
    public const string MODULE_REPORTS = "Reports";
    public const string MODULE_USERS = "Users";
    public const string MODULE_SETTINGS = "Settings";

    public const string ACTION_VIEW = "View";
    public const string ACTION_CREATE = "Create";
    public const string ACTION_EDIT = "Edit";
    public const string ACTION_DELETE = "Delete";
    public const string ACTION_EXPORT = "Export";
    public const string ACTION_IMPORT = "Import";
    public const string ACTION_EDIT_PRICE = "EditPrice";

    public static class Products
    {
        public const string VIEW = MODULE_PRODUCTS + "." + ACTION_VIEW;
        public const string CREATE = MODULE_PRODUCTS + "." + ACTION_CREATE;
        public const string EDIT = MODULE_PRODUCTS + "." + ACTION_EDIT;
        public const string DELETE = MODULE_PRODUCTS + "." + ACTION_DELETE;
        public const string IMPORT = MODULE_PRODUCTS + "." + ACTION_IMPORT;
        public const string EXPORT = MODULE_PRODUCTS + "." + ACTION_EXPORT;
    }

    public static class Sales
    {
        public const string VIEW = MODULE_SALES + "." + ACTION_VIEW;
        public const string CREATE = MODULE_SALES + "." + ACTION_CREATE;
        public const string EDIT = MODULE_SALES + "." + ACTION_EDIT;
        public const string DELETE = MODULE_SALES + "." + ACTION_DELETE;
    }

    public static class Quotations
    {
        public const string VIEW = MODULE_QUOTATIONS + "." + ACTION_VIEW;
        public const string CREATE = MODULE_QUOTATIONS + "." + ACTION_CREATE;
        public const string EDIT = MODULE_QUOTATIONS + "." + ACTION_EDIT;
        public const string DELETE = MODULE_QUOTATIONS + "." + ACTION_DELETE;
        public const string EDIT_PRICE = MODULE_QUOTATIONS + "." + ACTION_EDIT_PRICE;
        public const string EXPORT = MODULE_QUOTATIONS + "." + ACTION_EXPORT;
    }

    public static class Reports
    {
        public const string VIEW = MODULE_REPORTS + "." + ACTION_VIEW;
        public const string EXPORT = MODULE_REPORTS + "." + ACTION_EXPORT;
    }

    public static class Users
    {
        public const string VIEW = MODULE_USERS + "." + ACTION_VIEW;
        public const string CREATE = MODULE_USERS + "." + ACTION_CREATE;
        public const string EDIT = MODULE_USERS + "." + ACTION_EDIT;
        public const string DELETE = MODULE_USERS + "." + ACTION_DELETE;
    }
}
