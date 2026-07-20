using System.Windows;

namespace SlipperIS.UI.Views;

public partial class StockAdjustDialog : Window
{
    public int Quantity { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    private readonly string _type;

    public StockAdjustDialog(string title, string productName, int currentStock, string type)
    {
        InitializeComponent();
        Title = title;
        _type = type;
        ProductNameText.Text = productName;
        CurrentStockText.Text = currentStock.ToString();

        if (type == "Adjustment")
        {
            QuantityLabel.Text = "设定数量 *";
            QuantityBox.Text = currentStock.ToString();
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(QuantityBox.Text, out var qty) || qty < 0)
        {
            MessageBox.Show("请输入有效的数量（非负整数）。", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Quantity = qty;
        Reason = ReasonBox.Text.Trim();
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
