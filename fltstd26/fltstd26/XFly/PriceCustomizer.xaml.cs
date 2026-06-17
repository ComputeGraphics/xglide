using fltstd26.etc;
namespace fltstd26.XFly;

public partial class PriceCustomizer : ContentPage
{
    private TaskCompletionSource<int?>? _tcs;

    int _price = 0;
    public PriceCustomizer(int price, string name)
	{
		InitializeComponent();
        TGT_Title.Text = name;
        _price = price;
        TGT_Price_Entry.Text = GSettings.UnformatPrice(price);
    }

    public Task<int?> ShowAndSelect()
    {
        _tcs = new TaskCompletionSource<int?>();
        return _tcs.Task;
    }

    public void OnCancel(object sender,EventArgs e)
    {
        _tcs?.SetResult(null);
        Navigation.PopModalAsync();
    }
	public void OnConfirm(object sender,EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine(TGT_Price_Entry.Text ?? "null");
        if (!TGT_Price_Dropdown_Enable.IsChecked && ValueChanged(GSettings.UnformatPrice(_price),TGT_Price_Entry.Text) && Int32.TryParse(TGT_Price_Entry.Text,out int p))
        {
            System.Diagnostics.Debug.WriteLine("Here1");
            _price = GSettings.InterpretePrice(TGT_Price_Entry.Text.Trim());
            _tcs?.SetResult(_price);
        }
        else if(!TGT_Price_Dropdown_Enable.IsChecked)
        {
            System.Diagnostics.Debug.WriteLine("Here2");
            _tcs?.SetResult(0);
        }
        else _tcs?.SetResult(_price);
        Navigation.PopModalAsync();
    }

    static Func<string?,string?,bool> ValueChanged => (prev,aft) => aft != null && aft.Trim() != String.Empty && (prev == null || prev.Trim() != aft.Trim());
}