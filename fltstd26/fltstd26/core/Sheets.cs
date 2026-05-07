using SQLite;
using fltstd26.etc;

namespace fltstd26.core
{
    public class Sheets
    {
        [Table("Lfz")]
        public class Lfz
        {
            [PrimaryKey, AutoIncrement]
            [Column("id")]
            public int Id { get; set; }
            [Indexed]
            [Column("reg")]
            public string? Reg { get; set; }
            [Column("type")]
            public string? Type { get; set; }
            [Column("seats")]
            public byte Seats { get; set; }
            [Column("interval")]
            public byte Interval { get; set; } //Zeit zwischen jedem Flug. Im Slotting System nicht berücksichtigt
            [Column("pricecat")]
            public byte PriceCat { get; set; }
            [Column("avail")]
            public byte[]? AvailTimes { get; set; }
            [Column("autoassign")]
            public bool AutoAssign { get; set; }
        }

        [Table("Slot")]
        public class Slot
        {
            [PrimaryKey, AutoIncrement]
            [Column("id")]
            public int Id { get; set; }
            [Indexed]
            [Column("stime")]
            public DateTime STime { get; set; }
            [Column("ftime")]
            public DateTime FTime { get; set; }
            [Column("length")]
            public int Length { get; set; }
        }

        [Table("Flt")]
        public class Flt
        {
            [PrimaryKey, AutoIncrement]
            [Column("id")]
            public int Id { get; set; }
            [Indexed]
            [Column("eid")]
            public string? EId { get; set; }
            [Column("lfz")]
            public int Lfz { get; set; }
            [Column("slot")]
            public int Slot { get; set; }
            [Column("status")]
            public byte Status { get; set; }
            [Column("add")]
            public string? Add { get; set; } //Additional Info separated by ';'
        }

        [Table("Target")]
        public class Target
        {
            [PrimaryKey, AutoIncrement]
            [Column("id")]
            public int Id { get; set; }
            [Indexed]
            [Column("lid")] 
            public int LId { get; set; } //Linked FLT Id
            [Column("name")]
            public string? Name { get; set; }
            [Column("weight")]
            public int Weight { get; set; }
            [Column("quickticket")]
            public bool QuickTicket { get; set; }
            [Column("price")]
            public int Price { get; set; } //Price Cat for negative - Price Absolute for positive - Price 0 for auto
            [Column("persistent")]
            public bool Persistent { get; set; } //Flight will not be deleted by Software and cannot be moved by User.
        }

        [Table("PriceCat")]
        public class PriceCat
        {
            [PrimaryKey, AutoIncrement]
            [Column("id")]
            public int Id { get; set; }
            [Column("name")]
            public string? Name { get; set; }

            [Column("price")]
            public int Price { get; set; }
        }
    }
}
