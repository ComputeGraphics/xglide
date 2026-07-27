namespace fltstd26.assistant;

public partial class IABoard : ContentPage
{
	private readonly List<CheckBox> visibilityCheck = [];
    private readonly List<Entry> nameEntries = [];
    private readonly List<Entry> bindingEntries = [];
    private readonly List<Entry> widthEntries = [];


    public IABoard()
	{
		InitializeComponent();
	}

    private void IABoard_Add_Clicked(object sender,EventArgs e)
    {
		visibilityCheck.Add(new CheckBox { IsChecked = true, VerticalOptions = LayoutOptions.Center });
		nameEntries.Add(new Entry { Placeholder = "Name", VerticalOptions = LayoutOptions.Center });
        bindingEntries.Add(new Entry { Placeholder = "Binding", VerticalOptions = LayoutOptions.Center });
        widthEntries.Add(new Entry { Placeholder = "Width", VerticalOptions = LayoutOptions.Center });

        IABoard_Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
		IABoard_Grid.Add(visibilityCheck[^1], 0, IABoard_Grid.RowDefinitions.Count - 1);
		IABoard_Grid.Add(nameEntries[^1], 1, IABoard_Grid.RowDefinitions.Count - 1);
        IABoard_Grid.Add(bindingEntries[^1],2,IABoard_Grid.RowDefinitions.Count - 1);
        IABoard_Grid.Add(widthEntries[^1],3,IABoard_Grid.RowDefinitions.Count - 1);
    }
}