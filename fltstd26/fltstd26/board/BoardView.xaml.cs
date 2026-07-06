using fltstd26.etc;
using fltstd26.system;

namespace fltstd26.board;

public partial class BoardView : ContentView
{
	//Scheduler? FlashClock = null;

	//FL - Flash Light
	// 0 - aus, 1 - grün, 2 - rot
	// 4 - grün blinken, 5 - rot blinken, 6 - switch blinken
	public BoardView(View[] contents, double[] widths, byte FL)
	{
		InitializeComponent();
        if (contents.Length == widths.Length)
        {
            for (int i = 0; i < contents.Length; i++)
            {
                ColumnContainer.AddColumnDefinition(new ColumnDefinition(widths[i]));
                ColumnContainer.Add(contents[i],ColumnContainer.ColumnDefinitions.Count - 1);
            }
            UpdateFlash(FL);
        }
    }

	public void UpdateColumn(int no, View content)
	{
		IView? prev = ColumnContainer.Children.FirstOrDefault(v => ColumnContainer.GetColumn(v) == 1 && ColumnContainer.GetRow(v) == 0);
		if (prev != null)
		{
			ColumnContainer.Children.Remove(prev);
			ColumnContainer.Add(content,no);
		}
    }

	public void UpdateFlash(byte FL)
	{
		BoardTimeServ.Unload(this.Id);
        switch (FL)
		{
			case 0:
                RedLight.TextColor = Colors.DarkGray;
                GreenLight.TextColor = Colors.DarkGray;
				break;
			case 1:
                RedLight.TextColor = Colors.DarkGray;
                GreenLight.TextColor = Colors.ForestGreen;
				break;
			case 2:
                RedLight.TextColor = Colors.IndianRed;
                GreenLight.TextColor = Colors.DarkGray;
				break;
			case 4:
				BoardTimeServ.Clock(this.Id, () => GreenLight.TextColor = GreenLight.TextColor == Colors.DarkGray ? Colors.ForestGreen : Colors.DarkGray);
				break;
            case 5:
				BoardTimeServ.Clock(this.Id,() => RedLight.TextColor = RedLight.TextColor == Colors.DarkGray ? Colors.IndianRed : Colors.DarkGray);      
                break;
			case 6:
                RedLight.TextColor = Colors.IndianRed;
                BoardTimeServ.Clock(this.Id,
					() => {
                        GreenLight.TextColor = GreenLight.TextColor == Colors.DarkGray ? Colors.ForestGreen : Colors.DarkGray;
                        RedLight.TextColor = RedLight.TextColor == Colors.DarkGray ? Colors.IndianRed : Colors.DarkGray;
						});
                break;
        }
    }

	public void TerminateFlash() => BoardTimeServ.Unload(Id);
}