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
            public byte Interval { get; set; }
            [Column("pricecat")]
            public byte PriceCat { get; set; }
            [Column("autoassign")]
            public bool AutoAssign { get; set; }
        }

        [Table("Slots")]
        public class Slots
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
            public int EId { get; set; }
            [Column("lfz")]
            public int Lfz { get; set; }
            [Column("slot")]
            public int Slot { get; set; }
            [Column("status")]
            public byte Status { get; set; }
            [Column("add")]
            public string? Add { get; set; }
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
            public int Price { get; set; } //Price Cat for negative - Price Absolute for positive  
            [Column("persistent")]
            public bool Persistent { get; set; }
        }

        [Table("PriceCat")]
        public class PriceCat
        {
            [PrimaryKey, AutoIncrement]
            [Column("id")]
            public int Id { get; set; }
            [Column("Price")]
            public int Price { get; set; }
        }
    }
}
