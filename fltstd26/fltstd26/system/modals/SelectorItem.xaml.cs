using fltstd26.etc;

namespace fltstd26.system.modals;

public partial class SelectorItem : ContentView
{
    public SelectorItem(string CIcon,string CTitle,string CSubtitle)
    {
        InitializeComponent();
        Icon.Source = CIcon;
        Title.Text = CTitle;
        Description.Text = CSubtitle;

    }

    public void UpdateSelectionState(bool state)
    {
        Check.IsVisible = state;
        CheckCircle.Stroke = state ? GSettings.GetColour("Primary") : (GSettings.DarkMode ? GSettings.GetColour("Gray800") : GSettings.GetColour("Gray200"));
        ElementBorder.Stroke = state ? GSettings.GetColour("Secondary") : (GSettings.DarkMode ? GSettings.GetColour("Gray500") : GSettings.GetColour("Gray200"));
    }
}