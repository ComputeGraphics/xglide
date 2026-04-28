using CommunityToolkit.Maui.Behaviors;
using fltstd26.etc;
using fltstd26.XFly;
using System.Threading.Tasks;

namespace fltstd26.system.modals;

public partial class Selector : ContentPage
{
    private TaskCompletionSource<int>? _tcs;
    public Selector(string Title, List<(string, string, string)> Content)
    { 
        InitializeComponent();
        ItemTitle.Text = Title;
        ControlTemplate? style = Resources["SelectorRadioButton"] as ControlTemplate;
        for (int i = 0; i < Content.Count; i++)
        {
            var (icon, title, desc) = Content[i];
            Grid tile = new()
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition() { Width = GridLength.Auto },
                    new ColumnDefinition() { Width = GridLength.Star }
                },
                RowDefinitions =
                {
                    new RowDefinition() { Height = GridLength.Auto },
                    new RowDefinition() { Height = GridLength.Auto }
                },
                Margin = new Thickness(5)
            };
            Label titleLabel = new()
            {
                Text = title,
                FontSize = 20,
                FontAttributes = FontAttributes.Bold
            };
            tile.Add(titleLabel,1,0);
            Label descriptionLabel = new()
            {
                Text = desc,
                FontSize = 16,
            };
            tile.Add(descriptionLabel,1,1);
            Image iconImage = new()
            {
                Source = icon,
                WidthRequest = 56,
                HeightRequest = 56,
                Margin = new Thickness(5)
            };
            iconImage.Behaviors.Add(new IconTintColorBehavior { TintColor = GSettings.DarkMode ? Colors.White : Colors.Black });
            tile.Add(iconImage,0,0);
            tile.SetRowSpan(iconImage,2);
            SelectorRadio radio = new()
            {
                Margin = new Thickness(0,5),
                Padding = new Thickness(10,5),
                Content = tile,
                ControlTemplate = style,
                Current = i
            };
            ItemStack.Add(radio);
        }
    }

    public Task<int> ShowAndSelect()
    {
        _tcs = new TaskCompletionSource<int>();
        return _tcs.Task;
    }

    private void OnConfirm(object sender,EventArgs args)
    {
        _tcs?.SetResult(ItemStack.Children.OfType<SelectorRadio>().Where(r => r.IsChecked).FirstOrDefault()?.Current ?? -1);
        Navigation.PopModalAsync();
    }

    private void OnCancel(object sender,EventArgs args)
    {
        _tcs?.SetResult(-1);
        Navigation.PopModalAsync();
    }
}


public partial class SelectorRadio : RadioButton
{
    public int Current { get; init; }
}
