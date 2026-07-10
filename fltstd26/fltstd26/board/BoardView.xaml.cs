using fltstd26.etc;

namespace fltstd26.board;

public partial class BoardView : ContentView
{
	//Scheduler? FlashClock = null;
	public DateTime StartTime;
	//FL - Flash Light
	// 0 - aus, 1 - grün, 2 - rot
	// 4 - grün blinken, 5 - rot blinken, 6 - switch blinken
	private Border? StatusField;
	public BoardView(View[] contents, double[] widths, byte FL, DateTime time)
	{
		InitializeComponent();
		StartTime = time;
        if (contents.Length == widths.Length)
        {
            for (int i = 0; i < contents.Length; i++)
            {
                ColumnContainer.AddColumnDefinition(new ColumnDefinition(widths[i]));
                ColumnContainer.Add(contents[i],ColumnContainer.ColumnDefinitions.Count - 1);
            }
            StatusField = contents.OfType<Border>().FirstOrDefault();
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
        RedLight.TextColor = Colors.DarkGray;
        GreenLight.TextColor = Colors.DarkGray;
        switch (FL)
		{
			case 1:
                GreenLight.TextColor = Colors.ForestGreen;
				break;
			case 2:
                RedLight.TextColor = Colors.IndianRed;
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

	public void UpdateStatus(int status)
	{
		if (StatusField != null)
		{
            byte BGCode = 0;
            if (USettings.StatusBG_Green.Contains(status)) BGCode = 1;
            else if (USettings.StatusBG_Red.Contains(status)) BGCode = 2;

			if(StatusField.Content is Label l)
			{
				l.Text = GSettings.Status[status];
			}
            StatusField.BackgroundColor = BGCode == 2 ? Colors.DarkRed : (BGCode == 1 ? Colors.ForestGreen : Colors.Transparent);
		}
	}

	public void TerminateFlash() => BoardTimeServ.Unload(Id);
}