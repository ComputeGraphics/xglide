using fltstd26.core;
using fltstd26.etc;

namespace fltstd26.assistant.profiles;

public partial class PriceCatCreator : ContentPage
{
    private TaskCompletionSource<Sheets.PriceCat?>? _tcs;
    private readonly Sheets.PriceCat? p;
    public PriceCatCreator(Sheets.PriceCat? preprice)
    {
        InitializeComponent();
        if (preprice != null)
        {
            NameEntry.Text = preprice.Name;
            PriceEntry.Text = GSettings.UnformatPrice(preprice.Price);
            p = preprice;
        }

    }

    private void AddClick(object sender,EventArgs e)
    {
        int price = GSettings.InterpretePrice(PriceEntry.Text);
        Sheets.PriceCat pc = new()
        {
            Id = p == null ? 0 : p.Id,
            Name = p == null || GSettings.ValueChanged(p.Name,NameEntry.Text) ? NameEntry.Text : p.Name,
            Price = p == null || GSettings.ValueChanged(GSettings.UnformatPrice(p.Price),PriceEntry.Text) ? price : p.Price
        };

        _tcs?.SetResult(pc);
        Navigation.PopModalAsync();
    }

    private void CancelClick(object sender,EventArgs e)
    {
        _tcs?.SetResult(null);
        Navigation.PopModalAsync();
    }

    public Task<Sheets.PriceCat?> ShowAndSelect()
    {
        _tcs = new TaskCompletionSource<Sheets.PriceCat?>();
        return _tcs.Task;
    }
}