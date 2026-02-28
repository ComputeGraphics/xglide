using fltstd26.etc;

namespace fltstd26.system;

public partial class XConsole : Window
{
	private static XConsole? instance;
    public XConsole()
	{
		InitializeComponent();
		instance = this;
        ConProc.Log("Welcome to the XConsole!");
		USettings.XConsoleOpen = true;
    }

	public static void Update()
	{
		if (instance == null) return;
        foreach (string entry in ConProc.GetLog())
		{
            instance!.XConsoleContent.Text += entry + Environment.NewLine;
        }
    }
	private void Window_Closed(object sender, EventArgs e)
	{
		USettings.XConsoleOpen = false;
    }
}