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
        ConProc.Log("[CONSOLE] Welcome");
    }

	public static void Update()
	{
        try
        {
            if (instance == null) throw new Exception("No Instance found");
            instance!.XConsoleContent.Text = string.Empty;
            foreach (string entry in ConProc.GetLog())
            {
                instance!.XConsoleContent.Text += entry + Environment.NewLine;
            }
        }
        catch (Exception ex)
        {
            ConProc.Log("[CONSOLE] Logging failed: " + ex.Message,2);
        }

    }
}