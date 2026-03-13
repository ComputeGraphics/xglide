using fltstd26;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace fltstd26.etc
{
    public class USettings
    {
        public static Dictionary<int, (string, int)> PriceCategories = [];

        public static List<Types.LFZ> allLFZ = [];
        public static List<Types.FTS> allFTS = [];

        public static bool XConsoleOpen = false;
    }
}
