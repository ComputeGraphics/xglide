using CommunityToolkit.Maui.Behaviors;
using fltstd26.etc;
using fltstd26.XFly;
using System.Threading.Tasks;

namespace fltstd26.system.modals;

public partial class Selector : ContentPage
{
    private TaskCompletionSource<int>? _tcs;
    private int SelectedIndex = -1;
    public Selector(string Title, List<(string, string, string)> Content)
    { 
        InitializeComponent();

        ItemTitle.Text = Title;
        foreach(var item in Content)
        {
            SelectorItem t = new(item.Item1,item.Item2,item.Item3);
            TapGestureRecognizer tp = new();
            tp.Tapped += UpdateSelection;
            t.GestureRecognizers.Add(tp);
            ItemStack.Add(t);
        }
    }

    private void UpdateSelection(object? sender, TappedEventArgs e)
    {
        if(sender is SelectorItem Item)
        {
            if (SelectedIndex != -1 && ItemStack[SelectedIndex] is SelectorItem x) x.UpdateSelectionState(false);
            Item.UpdateSelectionState(true);
            SelectedIndex = ItemStack.IndexOf(Item);
        }
    }

    public Task<int> ShowAndSelect()
    {
        _tcs = new TaskCompletionSource<int>();
        return _tcs.Task;
    }

    private void OnConfirm(object sender,EventArgs args)
    {
        _tcs?.SetResult(SelectedIndex);
        Navigation.PopModalAsync();
    }

    private void OnCancel(object sender,EventArgs args)
    {
        _tcs?.SetResult(-1);
        Navigation.PopModalAsync();
    }
}