namespace fltstd26.board;

public partial class BoardView : ContentView
{
	//FL - Flash Light
	// 0 - aus, 1 - grün, 2 - rot, 3 - beide
	public BoardView(View[] contents, int[] widths, byte FL)
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
        GreenLight.TextColor = FL == 1 || FL == 3 ? Colors.ForestGreen : Colors.DarkGray;
        RedLight.TextColor = FL == 2 || FL == 3 ? Colors.IndianRed : Colors.DarkGray;
    }
}