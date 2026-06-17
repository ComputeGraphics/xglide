using fltstd26.etc;
using static SQLite.SQLite3;
namespace fltstd26.XFly
{
    internal class Drawer
    {
        internal static FBorder CreateFltCollector(int ID, string? EID)
        {
            VerticalStackLayout vsl = [];
            FBorder outer = new()
            {
                FltId = ID,
                StrokeThickness = 0,
                Content = vsl
            };
            VerticalStackLayout inner = [];
            Label fltLabel = new() {
                Text = EID ?? ID.ToString(),
                HorizontalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.Bold,
                TextColor = GSettings.DarkMode ? Colors.Black : Colors.White,
            };
            Border fltTextFrame = new()
            {
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = GSettings.DarkMode ? Colors.White : Colors.Black,
                StrokeThickness = 0,
                Content = fltLabel
            };
            vsl.Add(fltTextFrame);
            vsl.Add(inner);
            return outer;
        }

        internal static async Task<int> AskForPriceUpdate(int Price,int? LFZ_PriceCat, string Name)
        {
            int pcat = 0;
            if (USettings.AskForNodePriceChange)
            {
                PriceCustomizer pc = new(Price, Name);
                await GSettings.nav!.PushModalAsync(pc);
                await pc.ShowAndSelect().ContinueWith(r =>
                {
                    pcat = r.Result switch
                    {
                        null => 0,
                        0 => -(LFZ_PriceCat ?? USettings.FallbackPriceCat),
                        _ => r.Result.Value,
                    };
                });
            }
            return pcat;
        }

    }
}
