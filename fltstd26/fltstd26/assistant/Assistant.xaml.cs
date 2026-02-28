using fltstd26.assistant;

namespace fltstd26;

public partial class Assistant : ContentPage
{
	public Assistant()
	{
		InitializeComponent();
	}

    private void Start_Clicked(object sender,EventArgs e)
    {
		//fltstd26.App.Current.Windows[1].Page = new assistant.IABoard();
        Shell.Current.GoToAsync("//assistant/IABoard");
    }
}