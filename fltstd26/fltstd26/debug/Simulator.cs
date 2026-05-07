using fltstd26.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.debug
{
    internal class Simulator
    {
        public Simulator()
        {
            RData.Init();
        }

        public static List<(string, string, string)> content = new()
            {
                ("plane.png","Option 1","Description for option 1"),
                ("control.png","Option 2","Description for option 2"),
                ("copy.png","Option 3","Description for option 3"),
                ("slot.png","Option 4","Description for option 4"),
                ("target.png","Option 5","Description for option 5"),
                ("add.png","Option 6","Description for option 6"),
                ("plane.png","Option 1","Description for option 1"),
                ("control.png","Option 2","Description for option 2"),
                ("copy.png","Option 3","Description for option 3"),
                ("slot.png","Option 4","Description for option 4"),
                ("target.png","Option 5","Description for option 5"),
                ("add.png","Option 6","Description for option 6"),
            };

    }
}
