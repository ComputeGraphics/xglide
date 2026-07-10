using fltstd26.core;
using fltstd26.etc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace fltstd26.system
{
    public static class AutoAct
    {
        /* Action ID Dict:
         * 
         *  0 - Full DB Restore
         *  1 - Insert
         *  2 - Delete
         *  3 - Update
         *  
         *  if any other action after redo -> pop action stack
         *  if any other action after undo -> pop redo stack
        */
        private static readonly Dictionary<int,(string, int)> ForeignKeyBuffer = [];
        internal static int ActionIncrementor = 0;

        private static bool RedoLock = false;
        private static bool UndoLock = false;
        private static readonly Stack<List<DatabaseAction>> ActionStack = new();
        private static readonly Stack<List<DatabaseAction>> RedoStack = new();
        internal static Tuple<Button,Button>? ActionButtons = null;

        /// <summary>
        /// Foreign Key Creation Actions before!
        /// Action Codes:
        /// 0 - Full DB Restore,
        /// 1 - Insert,
        /// 2 - Delete,
        /// 3 - Update,
        /// </summary>
        /// <param name="a">Einzelne Aktion - NULL, wenn nicht benötigt</param>
        /// <param name="s">Aktionsstack - NULL oder nich angegeben, wenn nicht benötigt</param>
        public static void PushAction(DatabaseAction? a,List<DatabaseAction>? s = null)
        {
            if (ActionButtons != null)
            {
                //ActionButtons.Item1.IsEnabled = !RedoLock && ActionStack.Count != 0;
                //ActionButtons.Item2.IsEnabled = !UndoLock && RedoStack.Count != 0;
            }
            if (RedoLock)
            {
                //GSettings.main?.RefreshActionButtons(false,true);
                ActionStack.Clear();
            }
            if (UndoLock)
            {
                //GSettings.main?.RefreshActionButtons(true,false);
                RedoStack.Clear();
            }
            if (s != null) ActionStack.Push(s);
            else if (a != null) ActionStack.Push([a]);

            ConProc.ReportActionStack("ActionStack",ActionStack,UndoLock);
            ConProc.ReportActionStack("RedoStack",RedoStack,RedoLock);
        }

        public static void Clear()
        {
            ActionStack.Clear();
            RedoStack.Clear();
            RedoLock = false;
            UndoLock = false;
        }

        public static bool Undo()
        {
            if (ActionStack.TryPeek(out List<DatabaseAction>? SingleStack) && SingleStack != null)
            {
                List<DatabaseAction> Modified = [];
                List<int> ids = Act(SingleStack,true);
                //if (ForeignKeyBuffer.Count != 0) ids = Act([.. SingleStack.Where(x => ForeignKeyBuffer.ContainsKey(x.ID))],true);
                System.Diagnostics.Debug.WriteLine($"Stack Count: {SingleStack.Count}; ID Count: {ids.Count}");
                ids.ForEach(x => System.Diagnostics.Debug.Write(x + ","));
                System.Diagnostics.Debug.WriteLine("");
                SingleStack.ForEach(x => System.Diagnostics.Debug.Write(x + ","));
                for (int i = 0; i < SingleStack.Count; i++) Modified.Add(SingleStack[i] with { ObjectID = ids[i] });
                RedoStack.Push(Modified);
                UndoLock = true;
                if (ActionStack.Count > 0) ActionStack.Pop();
                return SingleStack.Select(x => x.DataType).Intersect(GSettings.NecessaryResetTypes).Any();
            }
            return false;
            //ConProc.ReportActionStack("ActionStack",ActionStack,UndoLock);
            //ConProc.ReportActionStack("RedoStack",RedoStack,RedoLock);
        }

        public static bool Redo()
        {
            if (RedoStack.TryPeek(out List<DatabaseAction>? SingleStack) && SingleStack != null)
            {
                List<DatabaseAction> Modified = [];
                List<int> ids = Act(SingleStack,false);
                //if (ForeignKeyBuffer.Count != 0) ids = Act([.. SingleStack.Where(x => ForeignKeyBuffer.ContainsKey(x.ID))],false);
                for (int i = 0; i < SingleStack.Count; i++) Modified.Add(SingleStack[i] with { ObjectID = ids[i] });
                ActionStack.Push(Modified);
                RedoLock = true;
                if (RedoStack.Count > 0) RedoStack.Pop();
                return SingleStack.Select(x => x.DataType).Intersect(GSettings.NecessaryResetTypes).Any();
            }
            return false;
            //ConProc.ReportActionStack("ActionStack",ActionStack,UndoLock);
            //ConProc.ReportActionStack("RedoStack",RedoStack,RedoLock);
        }

        private static List<int> Act(List<DatabaseAction> actions,bool undo)
        {
            byte[] Reversed = [0,2,1,3];
            List<int> StackReturn = [];
            for (int i = actions.Count - 1; i >= 0; i--)
            {
                System.Diagnostics.Debug.WriteLine($"Link ID {actions[i].ID}: Performing Action {actions[i].ActionID} with Object {actions[i].ObjectID}");
                //if (undo) RedoStack.Push(actions); else ActionStack.Push(actions);
                int NewID = actions[i].ObjectID;

                switch (undo ? Reversed[actions[i].ActionID] : actions[i].ActionID)
                {
                    case 0:
                        //Placeholder
                        RData.Restore("",true,undo);
                        break;
                    case 1:
                        if (actions[i].PreviousValue != null)
                        {
                            //Caution of ID Mistmatch
                            int Id = RData.Insert(actions[i].PreviousValue!,actions[i].DataType);
                            System.Diagnostics.Debug.WriteLine("New ID: " + Id.ToString());
                            if (actions[i].ForeignKeyName != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Action {actions[i].LinkAction}: {Id} for {actions[i].ForeignKeyName} added");
                                ForeignKeyBuffer.TryAdd(actions[i].LinkAction,(actions[i].ForeignKeyName!, Id));
                            }
                            //Object ID update
                            NewID = Id;
                        }
                        break;
                    case 2:
                        RData.Delete(actions[i].ObjectID,actions[i].DataType);
                        break;
                    case 3:
                        if (actions[i].PreviousValue != null && actions[i].CurrentValue != null)
                        {
                            RData.Update(undo ? actions[i].PreviousValue! : actions[i].CurrentValue!,actions[i].DataType);
                        }
                        break;
                }

                if (ForeignKeyBuffer.TryGetValue(actions[i].ID,out (string, int) link))
                {
                    System.Diagnostics.Debug.WriteLine($"Action {actions[i].ID}: Link {NewID} with {link.Item2} in {link.Item1}");
                    RData.UpdateProperty<int>(NewID,link.Item2,link.Item1,actions[i].DataType);
                    ForeignKeyBuffer.Remove(actions[i].ID);
                }

                StackReturn.Add(NewID);
            }
            StackReturn.Reverse();
            return StackReturn;
        }
    }



    //ForeignKeyName is Property (NOT DATABASE) Name
    public record DatabaseAction
    {
        public int ID = AutoAct.ActionIncrementor++;
        public string? ForeignKeyName { get; init; }
        public int LinkAction { get; init; }
        public required byte ActionID { get; init; }
        public required Type DataType { get; init; }
        public required int ObjectID { get; init; }
        public object? PreviousValue { get; init; }
        public object? CurrentValue { get; init; }
    }
}
