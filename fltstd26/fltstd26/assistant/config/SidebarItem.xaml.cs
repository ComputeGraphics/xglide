namespace fltstd26.assistant.config;

public partial class SidebarItem : ContentView
{
    /*public string Icon
    {
        get { return (string)GetValue(IconProperty); }
        set { SetValue(IconProperty,value); }
    }
    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty,value); }
    }
    public string Subtitle
    {
        get { return (string)GetValue(SubtitleProperty); }
        set { SetValue(SubtitleProperty,value); }
    }

    //public readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon),typeof(string),typeof(VisualElement),"info.png",propertyChanged: OnChange);
    //public readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title),typeof(string),typeof(VisualElement),"N/A");
    public readonly BindableProperty SubtitleProperty = BindableProperty.Create(nameof(Subtitle),typeof(string),typeof(VisualElement),"N/A");
    */

    public string Icon
    {
        get => IconControl.Source.ToString() ?? "";
        set => IconControl.Source = value;
    }
    public string Title { 
        get => TitleControl.Text;
        set => TitleControl.Text = value; 
    }
    public string Subtitle
    {
        get => SubtitleControl.Text;
        set {
            if(value == "$")
            {
                GridControl.SetRowSpan(TitleControl,2);
                SubtitleControl.Text = "";
            }
            else SubtitleControl.Text = value;
        }
    }
    public int ItemId { get; init; }
    public event EventHandler<TappedEventArgs>? Clicked;
    public SidebarItem()
    {
        InitializeComponent();
        TapGestureRecognizer tgr = new();
        GridControl.GestureRecognizers.Add(tgr);

        tgr.Tapped += (s,e) => Clicked?.Invoke(this,e);
        //IconControl.Source = Icon;
        //TitleControl.Text = Title;
        //SubtitleControl.Text = Subtitle;
    }

    public void SetSelection(bool state)
    {
        IndicatorControl.IsVisible = state;
    }

    /*private static void OnChange(BindableObject bindable,object oldValue,object newValue)
    {
        if (bindable is Image img) img.Source = (string)newValue;
    }*/
}