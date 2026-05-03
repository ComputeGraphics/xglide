using fltstd26.etc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.XFly
{
    internal class Drawer
    {
        internal FBorder CreateFltCollector(int ID, string? EID)
        {
            VerticalStackLayout vsl = [];
            FBorder outer = new()
            {
                FltId = ID,
                StrokeThickness = 2,
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

        internal void CreateNode()
        {

        }
    }
}
