using fltstd26.etc;

namespace fltstd26.system;

public partial class XConsole : Window
{
	private static XConsole? instance;
    public XConsole()
	{
		InitializeComponent();
        GSettings.XConsoleOpen = true;
        instance = this;
        ConProc.Log("Welcome to the XConsole!");
    }

	public static void Update()
	{
		if (instance == null) return;
        instance!.XConsoleContent.Text = string.Empty;
        foreach (string entry in ConProc.GetLog())
		{
            instance!.XConsoleContent.Text += entry + Environment.NewLine;
        }
    }
}