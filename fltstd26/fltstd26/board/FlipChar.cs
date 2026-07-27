using fltstd26.etc;
using Microsoft.Maui.Controls.Shapes;

namespace fltstd26.board;

public partial class FlipChar : ContentView
{
    public bool AtTarget { get => UpperChar == TargetChar && Letter.Text == OverwriteLetter.Text; }
    public char Get { get => USettings.Instance.Alphabet[UpperChar]; }
    private int UpperChar;
    private int TargetChar;

    private readonly Border Outer;
    private readonly Grid Case;
    private readonly Label Letter;
    private readonly Label OverwriteLetter;
    public FlipChar(short size)
    {
        Label lb = GetLetter(size,USettings.Instance.Alphabet.First());
        lb.Padding = new Thickness(0,4,0,0);
        Letter = lb;
        Label lf = GetLetter(size,USettings.Instance.Alphabet.First());
        //lf.BackgroundColor = Colors.Pink;
        OverwriteLetter = lf;
        Grid g = new()
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            ColumnDefinitions = { new ColumnDefinition() },
            RowDefinitions = { new RowDefinition(GridLength.Star) },
        };
        g.Add(lf);
        g.Add(lb);
        Loaded += (s,e) => { InitLetter(); };
        Border b = new()
        {
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(4)
            },
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            StrokeThickness = 0,
            BackgroundColor = GSettings.GetColour("Gray900"),
            Content = g
        };
        Content = b;
        Outer = b;
        Case = g;
    }

    internal int SetTarget(char letter)
    {
        int index = USettings.Instance.Alphabet.IndexOf(letter);
        TargetChar = index < 0 ? USettings.Instance.Alphabet.Last() : index;
        return index;
    }

    //Returns reached
    public bool UpdateLetter()
    {
        if (UpperChar != TargetChar || Letter.Text != OverwriteLetter.Text)
        {
            if (Letter.Text != OverwriteLetter.Text)
            {
                Letter.Text = OverwriteLetter.Text;
            }
            else
            {
                if (++UpperChar >= USettings.Instance.Alphabet.Length) UpperChar = 0;
                OverwriteLetter.Text = USettings.Instance.Alphabet[UpperChar].ToString();
            }
            return false;
        }
        return true;
    }

    private void InitLetter()
    {
        
        //Case.WidthRequest = USettings.Instance.LetterWidth;
        //Letter.WidthRequest = USettings.Instance.LetterWidth;
        //OverwriteLetter.WidthRequest = USettings.Instance.LetterWidth;
        Case.HeightRequest = Case.Height;
        double split = Case.Height / 2;
        if (Case.Width <= 0 || Case.Height <= 0)
            return;
        OverwriteLetter.Clip = new RectangleGeometry(
            new Rect(0,0,USettings.Instance.LetterWidth,split));
        Letter.Clip = new RectangleGeometry(
            new Rect(0,split,USettings.Instance.LetterWidth,Case.Height - split));
        Outer.HeightRequest = Case.Height + 8;
        Outer.WidthRequest = USettings.Instance.LetterWidth + 2;
    }

    private static Label GetLetter(short FontSize,char Letter)
    {
        return new()
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            FontFamily = "ZenDots",
            LineHeight = 0,
            Padding = 0,
            MaxLines = 1,
            FontAttributes = FontAttributes.Bold,
            //BackgroundColor = Colors.Red,
            //TextColor = GSettings.GetColour("Gray900"),
            TextColor = GSettings.GetColour("White"),
            Text = Letter.ToString(),
            FontSize = FontSize,
        };
    }

    private static Border GetBorder()
    {
        return new()
        {
            StrokeThickness = 0,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            BackgroundColor = GSettings.GetColour("Gray900")
        };
    }
}