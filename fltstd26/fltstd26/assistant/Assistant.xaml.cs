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
		Navigation.PushAsync(new assistant.FileManager(false,false));
        //Shell.Current.GoToAsync("//assistant/IABoard");
    }
}