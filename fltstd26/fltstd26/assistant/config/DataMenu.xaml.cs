using CommunityToolkit.Maui.Behaviors;
using fltstd26.etc;
using fltstd26.Resources.Texts;

namespace fltstd26.assistant.config;

public partial class DataMenu : ContentView
{
	private etc.ConfigSettings _instance;
	public DataMenu(etc.ConfigSettings cfg)
	{
		_instance = cfg;
		InitializeComponent();
	}

	private void AddAdditional(object sender, EventArgs e)
	{
		System.Diagnostics.Debug.WriteLine("Adding new Additional");
		Grid g = new()
		{
			ColumnDefinitions = { new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Auto) }
		};
		Entry t = new()
		{
			Placeholder = Lang.xplan_name,
			MinimumWidthRequest = 180,
			IsReadOnly = false,
		};
		g.Add(t);
		ImageButton i = new()
		{
			Source = "check.png",
			Behaviors = 
			{
				new IconTintColorBehavior()
				{
					TintColor = GSettings.DarkMode ? GSettings.GetColour("White") : GSettings.GetColour("Black"),
				}
			}
        };
		g.Add(i,1);
		Border b = new()
		{
			Content = g,
		};
		i.Clicked += (s,e) => ClickAdditional(b,t,i);
		AdditionalStack.Children.Add(b);
    }

	private void ClickAdditional(Border b, Entry t, ImageButton i)
	{
		if (t.IsReadOnly)
		{
			_instance.Additionals.Remove(t.Text);
			AdditionalStack.Remove(b);
		}
		else
		{
			i.Source = "bin.png";
			t.IsReadOnly = true;
			_instance.Additionals.Add(t.Text);
		}
	}
}