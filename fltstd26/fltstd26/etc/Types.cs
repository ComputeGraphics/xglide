using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.etc
{
    public class Types
    {
        public record struct LFZ
        {
            public int Id;
            public string Reg;
            public string Type;
            public byte Seats;
            public byte Interval;
            public byte PriceCat;
            public bool AutoAssign;
        }

        //Flight Time Slot
        public record struct FTS
        {
            public int Id;
            public DateTime Start;
            public DateTime End;
            public int Length;
        }

        public record struct TGT
        {
            public int Id;
            public string Name;
            public int Weight;
            public int Price; //in Cents
            public bool QuickTicket; //When Slots disabled, QuickTicket ignores Interval,Contigent and does not round Time to next 15min, but uses exact time. 
            public bool Persistent; //Flight will not be deleted by Software and cannot be moved by User.
        }

        public record struct FLT
        {
            public int Id;
            public int eId; //Custom Flight ID to match with external systems by Algorithm or Name = eId
            public LFZ Aircraft;
            public List<TGT> Target;
            public FTS TimeSlot; 
            public byte Status; //Status Number
            public string Add; //Additional Info separated by ';'
        }
    }
}
