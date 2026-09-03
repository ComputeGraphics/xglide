using fltstd26.core;
using fltstd26.etc;
using fltstd26.Resources.Texts;
using fltstd26.system;
using System.Xml.Linq;

namespace fltstd26.assistant.config;

public partial class ConfigSettings : ContentPage
{
    //private GeneralMenu generalpage;
    //private PlannerMenu plannerpage;
    //private DataMenu datapage;

    private ContentView[]? pages;

    private readonly etc.ConfigSettings? _current;
    private readonly string? _filename;

    private int _selected = 0;
    public ConfigSettings(string? config,string? name)
    {
        if (config != null && name != null)
        {
            string? sv = DskMan.SaveConfig(USettings.Instance,name,true);
            if (sv != null)
            {
                USettings.ConfigName = sv;
                DskMan.LoadConfig(config,true);
                _filename = config;
            }
        }

        etc.ConfigSettings? c = Sheets.Clone(USettings.Instance);
        System.Diagnostics.Debug.WriteLine("Previous Tolerance: " + USettings.Instance.QuickTolerance);
        if (c != null)
        {
            _current = c;
            pages =
                [
                new GeneralMenu(_current),
                new PlannerMenu(_current),
                new DataMenu(_current),
                new DefaultsMenu(_current),
                new UxMenu(_current),
                new OnLineMenu(_current),
                ];
        }

        InitializeComponent();
    }

    internal void SelectedIndexChanged(object sender,TappedEventArgs e)
    {
        if (pages != null)
        {
            IList<SidebarItem> items = [.. Sidebar.Children.OfType<SidebarItem>()];
            System.Diagnostics.Debug.WriteLine("Sidebar Count: " + items.Count.ToString());
            SidebarItem si = (SidebarItem)sender;
            System.Diagnostics.Debug.WriteLine("Selected Element: " + si.ItemId.ToString());

            if (si.ItemId >= 0 && si.ItemId < items.Count && si.ItemId < (pages?.Length ?? 0))
            {
                items[_selected].SetSelection(false);
                items[_selected].Unfocus();

                si.SetSelection(true);
                si.Focus();
                System.Diagnostics.Debug.WriteLine($"Pages Count: {pages!.Length}");
                MenuContainer.Content = pages![si.ItemId];
                _selected = si.ItemId;
            }
        }
    }

    public void ApplyClick(object sender,EventArgs e)
    {
        if (_current != null && _filename != null)
        {
            USettings.Instance = _current;
            DskMan.SaveConfig(USettings.Instance,_filename,false,true);
        }
    }

    public void CancelClick(object sender,EventArgs e)
    {
        Application.Current?.CloseWindow(Window);
    }

}