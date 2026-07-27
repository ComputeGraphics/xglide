using CommunityToolkit.Maui.Converters;
using fltstd26.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.XFly
{
    internal static class Nexus
    {
        private static byte Level = 0;
        public static string Name = "Host";
        // NEXUS PRIORITIES
        // 0 - master / host (session or server)
        // 1 - vice-master (can change config and run different config)
        // 2 - collaborator (can change profile)
        // 3 - worker (can interact with xplan -> can not issue delays)
        // 4 - read-only (with independent clock)
        // 5 - slave (no clock and run different config)

        // ACTION PRIORITIES
        // 0 - asap
        // 1 - precision (reset clock)
        // 2 - max (config updates)
        // 3 - high (slot, pricecat, lfz update)
        // 4 - action (tgt,flt update)
        // 5 - medium (clock & actionstack sync)
        // 6 - low (regular db sync)
        // 7 - idc (unnecessary stuff)

        // TRANSMISSION TYPES
        // 0 - Sync (Full DB)
        // 1 - Update (DB Actions)
        // 2 - Request (what)
        private static readonly PriorityQueue<List<DatabaseAction>,byte> _localnexus = new();
        private static readonly PriorityQueue<List<DatabaseAction>,byte> _offlinenexus = new();

        public static void PassNEXUS(List<DatabaseAction> a)
        {
            if(Level > 0)
            {
                //Pass to server
                //If Server acks
                //Return db action to apply
                //Do
            }
            else
            {
                //Do and pass DB Action to clients
            }
        }

        internal static void Pack(PriorityQueue<List<DatabaseAction>,byte> a)
        {

        }


        public static void DatabaseToNEXUS(List<DatabaseAction> a)
        {
            if(Level < 4)
            {

            }
        }

        public static void ConfigToNEXUS(List<DatabaseAction> a)
        {
            if(Level < 2)
            {
                AutoAct.PushAction(null,a);
                AutoAct.Act(a,false);
            }
        }
    }

    public class NexusAction
    {
        public required byte Level {  get; init; }
        public byte Priority = 7;
        public string Source = "N/A";
        public readonly List<DatabaseAction> Actions = [];
        
    }
}
