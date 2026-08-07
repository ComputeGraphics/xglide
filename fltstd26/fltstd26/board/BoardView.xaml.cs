using fltstd26.etc;

namespace fltstd26.board;

public partial class BoardView : ContentView
{
	//Scheduler? FlashClock = null;
	public readonly DateTime StartTime;
	public readonly int FlightID;
	public readonly int AddColumn;
	//FL - Flash Light
	// 0 - aus, 1 - grün, 2 - rot
	// 4 - grün blinken, 5 - rot blinken, 6 - switch blinken
	private readonly Border? StatusField;
	public BoardView(int id, View[] contents, double[] widths, int addcolumn, byte FL, DateTime time)
	{
		InitializeComponent();
		FlightID = id;
		StartTime = time;
		AddColumn = addcolumn;
        if (contents.Length >= widths.Length)
        {
			int addcount = USettings.Instance.Additionals.Count;
            //System.Diagnostics.Debug.WriteLine("Add Column is " + addcolumn.ToString());
            for (int i = 0; i < contents.Length; i++)
            {
				//System.Diagnostics.Debug.WriteLine("Looping through Columns " + i.ToString() + "/" + contents.Length.ToString());
				if(i == addcolumn)
				{
					double addwidth = widths[i] / addcount;
					for(;i-addcolumn < addcount; i++)
					{
                        //System.Diagnostics.Debug.WriteLine("Looping through Additional Columns " + i.ToString() + "/" + contents.Length.ToString());
                        ColumnContainer.AddColumnDefinition(new ColumnDefinition(addwidth));
                        ColumnContainer.Add(contents[i],ColumnContainer.ColumnDefinitions.Count - 1);
                    }
					i--;
                }
				else
				{
                    ColumnContainer.AddColumnDefinition(new ColumnDefinition(widths[addcolumn != -1 && i > addcolumn ? i- addcount+1 : i]));
                    ColumnContainer.Add(contents[i],ColumnContainer.ColumnDefinitions.Count - 1);
                }

            }
            StatusField = contents.OfType<Border>().FirstOrDefault();
            UpdateFlash(FL);
        }
    }

	public void UpdateColumn(int no, View content)
	{
		IView? prev = ColumnContainer.Children.FirstOrDefault(v => ColumnContainer.GetColumn(v) == no && ColumnContainer.GetRow(v) == 0);
		if (prev != null)
		{
			ColumnContainer.Children.Remove(prev);
			ColumnContainer.Add(content,no);
		}
    }

	public IList<IView> GetColumns() => ColumnContainer.Children;

	public List<FlipChar> GetColumnFlips()
	{
		List<FlipChar> chars = [];
		foreach(HorizontalStackLayout hsl in ColumnContainer.Children.OfType<HorizontalStackLayout>())
		{
			chars.AddRange(hsl.Children.OfType<FlipChar>());
            /*foreach (IView item in hsl.Children)
            {
				//System.Diagnostics.Debug.WriteLine("HSL Content: " + item.GetType().Name);
				if(item is FlipChar c) chars.Add(c);
            }*/
        }
        return chars;
	} 

    public IView? GetColumn(int no) => ColumnContainer.Children.FirstOrDefault(v => ColumnContainer.GetColumn(v) == no && ColumnContainer.GetRow(v) == 0);

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
			System.Diagnostics.Debug.WriteLine("Updating Status Field of " + FlightID.ToString());
            byte BGCode = 0;
            if (USettings.Instance.StatusBG_Green.Contains(status)) BGCode = 1;
            else if (USettings.Instance.StatusBG_Red.Contains(status)) BGCode = 2;

			if(StatusField.Content is Label l)
			{
				l.Text = GSettings.Status[status];
			}
            StatusField.BackgroundColor = BGCode == 2 ? Colors.DarkRed : (BGCode == 1 ? Colors.ForestGreen : Colors.Transparent);
		}
	}

	public void TerminateFlash() => BoardTimeServ.Unload(Id);
}