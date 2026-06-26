using fltstd26.core;
using fltstd26.etc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.board
{
    internal static class BoardController
    {

        internal static void Translate(List<Sheets.Flt> lst)
        {
            foreach ((string, string, int) column in USettings.Columns)
            {

            }
        }

        private static View GetInfo<T>(string prop,T obj)
        {
            Label lbl = new()
            {
                FontSize = USettings.ElementSize,
                Text = "N/A"
            };
            PropertyInfo? p = typeof(T).GetProperty(prop);
            if (p == null) return lbl;
            lbl.Text = p.GetValue(obj)?.ToString();
            return lbl;
        }

        private static View GetCtr(string cat,Sheets.Flt obj)
        {
            switch (cat)
            {
                case 
            }
        }
    }
}
