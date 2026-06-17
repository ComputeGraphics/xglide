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
         *  4 - Range Creation
         *  5 - Range Deletion
         *  
         *  if any other action after redo -> pop action stack
         *  if any other action after undo -> pop redo stack
        */

        private static bool RedoLock = false;
        private static bool UndoLock = false;
        private static readonly Stack<Stack<DatabaseAction>> ActionStack = new();
        private static readonly Stack<Stack<DatabaseAction>> RedoStack = new();

        /// <summary>
        /// Action Codes:
        /// 0 - Full DB Restore,
        /// 1 - Insert,
        /// 2 - Delete,
        /// 3 - Update
        /// </summary>
        /// <param name="a">Einzelne Aktion - NULL, wenn nicht benötigt</param>
        /// <param name="s">Aktionsstack - NULL oder nich angegeben, wenn nicht benötigt</param>
        public static void PushAction(DatabaseAction? a,Stack<DatabaseAction>? s = null)
        {
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
            else if (a != null)
            {
                Stack<DatabaseAction> temp = new();
                temp.Push(a);
                ActionStack.Push(temp);
            }

            ConProc.ReportActionStack("ActionStack",ActionStack,UndoLock);
            ConProc.ReportActionStack("RedoStack",RedoStack,RedoLock);
        }

        public static void PopAll()
        {
            ActionStack.Pop();
            RedoStack.Pop();
            RedoLock = false;
            UndoLock = false;
        }

        public static void Undo()
        {
            if (ActionStack.TryPeek(out Stack<DatabaseAction>? SingleStack) && SingleStack != null)
            {
                // Deep Copy
                //Stack<DatabaseAction>? icannottakethisanymore = Sheets.Clone(SingleStack);
                //if(icannottakethisanymore != null) RedoStack.Push(icannottakethisanymore);
                RedoStack.Push(SingleStack);
                for (short i = (short)(SingleStack.Count - 1); i >= 0; i--)
                {
                    if (SingleStack.TryPeek(out DatabaseAction? StraightAction) && StraightAction != null)
                    {
                        switch (StraightAction.ActionID)
                        {
                            case 0:
                                RData.Restore("",true);
                                break;
                            case 1:
                                System.Diagnostics.Debug.WriteLine("Revoking Insertion");
                                RData.Delete(StraightAction.ObjectID,StraightAction.DataType);
                                break;
                            case 2:
                                if (StraightAction.PreviousValue != null)
                                {
                                    //Caution of ID Mistmatch
                                    System.Diagnostics.Debug.WriteLine("Revoking Deletion");
                                    //RData.Insert(StraightAction.PreviousValue,StraightAction.DataType);
                                }
                                break;
                            case 3:
                                if (StraightAction.PreviousValue != null && StraightAction.CurrentValue != null)
                                {
                                    System.Diagnostics.Debug.WriteLine("Revoking Update");
                                    RData.Update(StraightAction.PreviousValue,StraightAction.DataType);
                                }
                                break;
                        }
                        SingleStack.Pop();
                    }
                }
            }
            UndoLock = true;
            if (ActionStack.Count > 0) ActionStack.Pop();
        }

        public static void Redo()
        {
            if (RedoStack.TryPeek(out Stack<DatabaseAction>? SingleStack) && SingleStack != null)
            {
                // Deep Copy
                //Stack<DatabaseAction>? icannottakethisanymore = Sheets.Clone(SingleStack);
                //if (icannottakethisanymore != null) ActionStack.Push(icannottakethisanymore);
                ActionStack.Push(SingleStack);
                for (short i = (short)(SingleStack.Count - 1); i >= 0; i--)
                {
                    if (SingleStack.TryPeek(out DatabaseAction? StraightAction) && StraightAction != null)
                    {
                        switch (StraightAction.ActionID)
                        {
                            case 0:
                                //Placeholder
                                RData.Restore("",true);
                                break;
                            case 1:
                                if (StraightAction.PreviousValue != null)
                                {
                                    //Caution of ID Mistmatch
                                    //RData.Insert(StraightAction.PreviousValue,StraightAction.DataType);
                                }
                                break;
                            case 2:
                                RData.Delete(StraightAction.ObjectID,StraightAction.DataType);

                                break;
                            case 3:
                                if (StraightAction.PreviousValue != null && StraightAction.CurrentValue != null)
                                {
                                    RData.Update(StraightAction.CurrentValue,StraightAction.DataType);
                                }
                                break;
                        }
                        SingleStack.Pop();
                    }
                }
            }
            RedoLock = true;
            if(RedoStack.Count > 0) RedoStack.Pop();
        }
    }


    public record DatabaseAction
    {
        public required byte ActionID { get; init; }
        public required Type DataType { get; init; }
        public required int ObjectID { get; init; }
        public object? PreviousValue { get; init; }
        public object? CurrentValue { get; init; }
    }
}
