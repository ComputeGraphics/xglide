using CommunityToolkit.Maui.Behaviors;
using fltstd26.board;
using fltstd26.core;
using fltstd26.debug;
using fltstd26.etc;
using fltstd26.etc.online;
using fltstd26.Resources.Texts;
using fltstd26.system;
using fltstd26.XFly;
using System.Text;
namespace fltstd26
{
    public partial class XMain : ContentPage
    {
        //CLEANUP FLIGHTS REVERSABLE MACHEN!!!!!!


        private readonly List<int> HiddenSlots = [];
        private readonly List<List<Border>> Cells = [];
        private readonly List<Border> Slots = [];
        internal static readonly List<FBorder> FlightCollectors = [];
        private readonly List<List<TargetStack>> StackContainers = [];
        private readonly List<int> CurrentPriceCatIds = [];
        private readonly List<int> CurrentAircraftIds = [];
        private readonly Dictionary<int,int> SlotTickLink = [];

        private readonly Dictionary<Guid,XBlock> NodeLibrary = [];
        private Guid copyBuffer = new(new byte[16]);
        private Guid focusedID = new(new byte[16]);
        internal bool XPlanOpen = false;

        public XMain()
        {
            InitializeComponent();
            GSettings.nav = Navigation;
            TimeServ.Init();
            AutoAct.ActionButtons = new(XPlan_UndoButton,XPlan_RedoButton);
            DskMan.Init();
            RData.Init();
            OGN.Sync();
            DskMan.SaveConfig("Test");
            USettings.FinalizeConfig();
            System.Diagnostics.Debug.WriteLine(Application.Current!.RequestedTheme.ToString());
            //System.Diagnostics.Debug.WriteLine(RData.Insert<Sheets.Target>(new() { Name = "Test" }));
            //System.Diagnostics.Debug.WriteLine(RData.GetWhere<Sheets.Slot>($"id=2").First().Length);
            //Application.Current!.UserAppTheme = AppTheme.Light;

        }


        private (int, int) GetCellIndex(int slotid,int? lfzid)
        {
            int rowIndex = RData.GetSlotsTable().Where(x => !HiddenSlots.Contains(x.Id)).ToList().FindIndex(fts => fts.Id.Equals(slotid));
            int colIndex = lfzid == null ? -1 : RData.GetAircraftTable().FindIndex(lfz => lfz.Id.Equals(lfzid));
            return (rowIndex, colIndex);
        }

        private void SidebarRefresh()
        {
            //LFZ & PRICE Selector füllen
            TGT_LFZ_Dropdown.Items.Clear();
            TGT_Price_Dropdown.Items.Clear();
            CurrentPriceCatIds.Clear();
            CurrentAircraftIds.Clear();
            FLTAddsEntryContainer.Clear();
            RData.GetAircraftTable().ForEach(x =>
            {
                CurrentAircraftIds.Add(x.Id);
                TGT_LFZ_Dropdown.Items.Add(x.Reg);
            });
            TGT_Price_Dropdown.Items.Add(Lang.custom);
            RData.GetPriceTable().ForEach(x =>
            {
                CurrentPriceCatIds.Add(x.Id);
                TGT_Price_Dropdown.Items.Add($"{x.Name} ({GSettings.UnformatPrice(x.Price)})");
            });

            FLT_Length_Entry.Placeholder = Lang.xplan_length.ToString() + $" ({USettings.DefaultFltLength} min)";
            TGT_Weight_Entry.Placeholder = Lang.xplan_weight.ToString() + $" ({USettings.DefaultTgtWeight})";
            USettings.Additionals.ForEach(x => FLTAddsEntryContainer.Add(new Entry() { Placeholder = x }));
        }
        private void XPlanClear(bool ClearSheet)
        {
            NodeLibrary.Clear();
            copyBuffer = new(new byte[16]);
            focusedID = new(new byte[16]);
            StackContainers.ForEach(x => x.ForEach(x => x.Clear()));
            if (ClearSheet)
            {
                XPlan.GestureRecognizers.Clear();
                XPlan.ColumnDefinitions.Clear();
                XPlan.RowDefinitions.Clear();
                XPlan.Children.Clear();
                Slots.Clear();
                Cells.Clear();
                StackContainers.Clear();
                FlightCollectors.Clear();
                HiddenSlots.Clear();
                XPlanOpen = false;
            }
        }
        private void XPlanRefresh()
        {
            if (XPlanOpen)
            {
                List<Sheets.Flt> allFLT = RData.GetFlightTable();
                List<Sheets.Target> allTGT = RData.GetTargetTable();
                List<Sheets.Slot> allSLT = RData.GetSlotsTable();
                if (BoardController.Boards.Count > 0) BoardController.SynchronizeWithFlight(allFLT,allTGT);
                //Alle Nodes löschen
                XPlanClear(false);

                //Datenbank Nodes einfügen
                if (allFLT.Count != 0 && allTGT.Count != 0)
                {
                    AddFlightNo(allFLT,allSLT);
                    foreach (var t in allTGT)
                    {
                        Sheets.Flt? f = allFLT.Find(x => x.Id == t.LId);
                        if (f == null)
                        {
                            ConProc.Log("[XPLAN-SYNC] Target " + t.Id.ToString() + " konnte kein Flug zugeordnet werden",2);
                            continue;
                        }

                        Sheets.Slot? s = allSLT.Find(x => x.Id == f.Slot);
                        if (s == null)
                        {
                            ConProc.Log("[XPLAN-SYNC] Target " + t.Id.ToString() + " konnte kein Slot zugeordnet werden",2);
                            continue;
                        }
                        AddNode(s,f.Lfz,t);
                    }
                }
            }
        }
        private void AddFlightNo(List<Sheets.Flt> flts,List<Sheets.Slot> slts)
        {
            FlightCollectors.Clear();
            List<Sheets.Lfz> acs = RData.GetAircraftTable();

            foreach (Sheets.Flt flt in flts)
            {
                int occupied = RData.GetTargetTable().Where(x => x.LId == flt.Id).Count();
                (int, int) coords = GetCellIndex(flt.Slot,flt.Lfz);
                if (coords.Item1 == -1 || coords.Item2 == -1)
                {
                    ConProc.Log($"[XPLAN-RENDERER] Dem Flug {flt.Id} konnte keine Zelle zugeordnet werden",2);
                    continue;
                }
                FBorder fltcollect = new(flt,occupied,acs[coords.Item2].Seats);
                StackContainers.ElementAt(coords.Item1).ElementAt(coords.Item2).Children.Add(fltcollect);
                FlightCollectors.Add(fltcollect);
                if (flt.Status == 13) GSettings.StatusLink.TryAdd(flt.Id,11);
            }
        }



        internal void XPlanRestart()
        {

            /*try
            {*/
            List<IView> DateDisplay = [];
            XPlanClear(true);
            Color stroke = GSettings.DarkMode ? Colors.DarkGray : Colors.LightGray;
            List<Sheets.Lfz> allLFZ = RData.GetAircraftTable();
            SidebarRefresh();

            //XPLAN AUFBAUEN
            TapGestureRecognizer Deselector = new();
            Deselector.Tapped += NodeDeselectionHandler;
            XPlan.GestureRecognizers.Add(Deselector);
            XPlan.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            XPlan.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            ImageButton refreshButton = new()
            {
                BackgroundColor = Colors.Transparent,
                Source = "refresh.png",
                Aspect = Aspect.AspectFit,
                Behaviors =
                    {
                        new IconTintColorBehavior { TintColor = GSettings.DarkMode ? Colors.White : Colors.Black }
                    },
            };
            refreshButton.Clicked += (s,e) => XPlanRestart();
            XPlan.Add(refreshButton,0,0);
            foreach (Sheets.Lfz lfz in allLFZ)
            {
                XPlan.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                VerticalStackLayout v = [ new Label()
                    {
                        Text = lfz.Reg,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 24,
                    }, new Label()
                    {
                        Text = lfz.Type,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        FontSize = 16,
                    },];
                v.Margin = new Thickness(0,10);
                XPlan.Add(v,XPlan.ColumnDefinitions.Count - 1,0);
            }
            List<DateTime> drawnDays = [];
            List<Sheets.Slot> slts = RData.GetSlotsTable();
            DateTime now = DateTime.Now;
            foreach (Sheets.Slot fts in slts)
            {

                if (!USettings.HidePastSlots || fts.FTime >= now)
                {

                    List<Border> borders = [];
                    List<TargetStack> containersRow = [];
                    XPlan.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    if (!drawnDays.Contains(fts.STime.Date))
                    {
                        HorizontalStackLayout hsl = new()
                        {
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center,
                            Margin = new Thickness(0,10)
                        };
                        IconTintColorBehavior itb = new()
                        {
                            TintColor = GSettings.DarkMode ? Colors.White : Colors.Black,
                        };
                        Image ico = new()
                        {
                            Source = "calendar.png",
                            Aspect = Aspect.AspectFit,
                            Margin = new Thickness(10,0),
                            VerticalOptions = LayoutOptions.Center,
                        };
                        ico.Behaviors.Add(itb);
                        hsl.Add(ico);
                        hsl.Add(new Label()
                        {
                            Text = fts.STime.Date.ToShortDateString(),
                            FontAttributes = FontAttributes.Bold,
                            VerticalOptions = LayoutOptions.Center,
                        });
                        XPlan.Add(hsl,1,XPlan.RowDefinitions.Count - 1);
                        XPlan.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        DateDisplay.Add(hsl);
                        drawnDays.Add(fts.STime.Date);
                    }




                    Grid slot = new()
                    {
                        Padding = new Thickness(4),
                        ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto)
                    },
                        RowDefinitions =
                    {
                        new RowDefinition(),
                        new RowDefinition()
                    }
                    };

                    VerticalStackLayout v = [
                            new Label()
                    {
                        Text = $"{fts.STime:HH:mm}",
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.End,
                        FontSize = 20,
                    },

                    new Label()
                    {
                        Text = $"- {fts.FTime:HH:mm}",
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.End,
                        FontSize = 20,
                    },

                    new Label()
                    {
                        Text = $"({fts.Length} min)",
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        FontSize = 18,
                    }
                    ];
                    v.Margin = new Thickness(5,10);
                    v.VerticalOptions = LayoutOptions.Center;

                    slot.Add(v,0,0);
                    slot.SetRowSpan(v,2);

                    Button min5 = new()
                    {
                        Text = "+5",
                        CornerRadius = 0,
                        Margin = new Thickness(0,0,2,4),
                        FontSize = 12,
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.End,
                    };
                    min5.Clicked += (s,e) => SlotDelayInteraction(fts.Id,5);
                    slot.Add(min5,1,0);
                    Button min10 = new()
                    {
                        Text = "+10",
                        CornerRadius = 0,
                        Margin = new Thickness(0,4,2,0),
                        FontSize = 12,
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.Start,
                    };
                    min10.Clicked += (s,e) => SlotDelayInteraction(fts.Id,10);
                    slot.Add(min10,1,1);

                    Border slotb = new()
                    {
                        StrokeThickness = 0,
                        BackgroundColor = Colors.Transparent,
                        Content = slot
                    };


                    Slots.Add(slotb);
                    XPlan.Add(slotb,0,XPlan.RowDefinitions.Count - 1);
                    for (int i = 0; i < allLFZ.Count; i++)
                    {
                        // Event Handlers

                        Border cellBorder = new()
                        {
                            Stroke = stroke,
                            StrokeThickness = 1,
                        };

                        DropGestureRecognizer d = new();

                        if (Manager.AvailableIn(allLFZ[i].Id,fts.Id))
                        {
                            TargetStack cellContainer = new()
                            {
                                LFZID = allLFZ[i].Id,
                                SLTID = (byte)fts.Id,
                            };
                            cellBorder.Content = cellContainer;

                            d.AllowDrop = true;
                            d.DragOver += (s,e) => OnHoverNode(s,e,true);
                            d.Drop += (s,e) => OnDropNode(cellBorder,e);

                            containersRow.Add(cellContainer);
                        }
                        else
                        {
                            Label x = new()
                            {
                                Text = "x",
                                TextColor = GSettings.InactiveIcon,
                                FontAttributes = FontAttributes.Bold,
                                HorizontalTextAlignment = TextAlignment.Center,
                                VerticalTextAlignment = TextAlignment.Center,
                                FontSize = 24
                            };

                            d.AllowDrop = false;
                            d.DragOver += (s,e) => OnHoverNode(s,e,false);

                            containersRow.Add([]); //Ghost-Element, damit die Indizes stimmen
                            cellBorder.Content = x;
                        }
                        cellBorder.GestureRecognizers.Add(d);
                        borders.Add(cellBorder);
                        XPlan.Add(cellBorder,i + 1,XPlan.RowDefinitions.Count - 1);
                    }
                    Cells.Add(borders);
                    StackContainers.Add(containersRow);
                }
                else
                {
                    HiddenSlots.Add(fts.Id);
                }

                if (fts.FTime > now)
                {
                    if (!SlotTickLink.TryGetValue(fts.Id,out int ev) || ev == -1) SlotTickLink.TryAdd(fts.Id,TimeServ.Schedule(fts.STime,() => InvokeSlot(fts.Id)));
                    else if (fts.Delay) SlotTickLink[fts.Id] = TimeServ.Reschedule(SlotTickLink[fts.Id],fts.STime);
                }
                else
                {
                    SlotTickLink.TryAdd(fts.Id,-1);
                    FinishSlot(fts.Id,slts);
                }

                /*if (!SlotTickLink.ContainsKey(fts.Id)) SlotTickLink.Add(fts.Id,fts.FTime > DateTime.Now ? TimeServ.Schedule(fts.STime,() => InvokeSlot(fts.Id)) : 0);
                if (DateTime.Now > fts.FTime)*/


            }
            DateDisplay.ForEach(d => XPlan.SetColumnSpan(d,XPlan.ColumnDefinitions.Count - 1));
            XPlanOpen = true;
            ConProc.Log("[XPLAN] XPlan zurückgesetzt",0);
            XPlanRefresh();
            /*}
            catch (Exception e)
            {
                XPlanOpen = false;
                ConProc.Log("[XPLAN] XPlan-Start fehlgeschlagen: " + e.Message.ToString(),2);
            }*/
        }

        private void InvokeSlot(int id)
        {
            //Cell Defaults Dark/Light
            //Neutral: OffBlack (#1f1f1f)  -  idk
            //Active: SecondaryDarkBg (#004152)  -  SecondaryBg (#a0f0f8)
            //Passed: Gray900 (#212121)  -  Gray300 (#ACACAC)

            List<Sheets.Slot> slottable = RData.GetSlotsTable();
            Sheets.Slot? slot = slottable.Find(x => x.Id == id);
            if (slot == null) return;
            DateTime now = DateTime.Now;
            (int, int) coords = GetCellIndex(id,null);
            if (coords.Item1 == -1 || coords.Item1 > slottable.Count - 1) return;
            if (now <= slot.FTime && now >= slot.STime.Subtract(TimeSpan.FromMinutes(USettings.SlotTolerance)))
            {
                //Slot hat bereits begonnen

                List<Sheets.Flt?>? flts = RData.GetWhere<Sheets.Flt>($"slot={id}");
                //Redetermine Status
                if (flts == null) return;
                Manager.DetermineStatus(slot,flts,USettings.OGNStatus);
                //Status aktualisieren
                IEnumerable<int> tgts = RData.GetTargetTable().Where(t => flts.Where(x => x != null).Select(x => x!.Id).Contains(t.Id)).Select(x => x.Id);
                List<XBlock> affectedNodes = [.. NodeLibrary.Select(x => x.Value).Where(x => tgts.Contains(x.TargetID))];
                string messages = "";
                foreach (XBlock node in affectedNodes)
                {
                    if (node.Attribs[2]) messages += $"- {node.Name} ({node.TargetID}) -\r\n";
                    node.DisableAttrib(2);
                }
                if (messages != "") system.modals.ModalPush.Message(Lang.notification,messages + Lang.ticket_notification);
                //Umfärben
                if (coords.Item1 != -1)
                {
                    Cells.ElementAt(coords.Item1).ForEach(x => x.BackgroundColor = GSettings.CellBackgroundActiveColour);
                    Slots.ElementAt(coords.Item1).BackgroundColor = GSettings.CellBackgroundActiveColour;
                }
                //Finish qeuen
                SlotTickLink[id] = TimeServ.Schedule(slot.FTime,() => FinishSlot(id));
            }
            else
            {
                //Fehler qeue muss geprüft werden
                if (coords.Item1 != -1)
                {
                    Cells.ElementAt(coords.Item1).ForEach(x => x.BackgroundColor = GSettings.CellBackgroundNeutralColour);
                    Slots.ElementAt(coords.Item1).BackgroundColor = GSettings.CellBackgroundNeutralColour;
                }
                if (SlotTickLink.TryGetValue(id,out int link))
                {

                    if (now >= slot.FTime)
                    {
                        TimeServ.Unschedule(link);
                        FinishSlot(id);
                    }
                    else if (now < slot.STime)
                    {
                        SlotTickLink[id] = TimeServ.Schedule(slot.STime,() => InvokeSlot(id));
                    }
                    //Neu qeuen
                }
            }


        }

        private void FinishSlot(int id,List<Sheets.Slot>? slt = null)
        {
            List<Sheets.Slot> slottable = slt ?? RData.GetSlotsTable();
            (int, int) coords = GetCellIndex(id,null);
            //Sheets.Slot? slot = slottable.Find(x => x.Id == id);
            if (coords.Item1 == -1 || coords.Item1 > slottable.Count - 1) return;

            if (DateTime.Now >= slottable[coords.Item1].FTime)
            {
                //Slot hat bereits geendet
                //Umfärben


                Cells.ElementAt(coords.Item1).ForEach(x => x.BackgroundColor = GSettings.CellBackgroundPassedColour);
                Slots.ElementAt(coords.Item1).BackgroundColor = GSettings.CellBackgroundPassedColour;
                if (USettings.HidePastSlots)
                {
                    HiddenSlots.Add(id);
                    XPlanRestart();
                }
            }
            else
            {
                //Fehler
                Cells.ElementAt(coords.Item1).ForEach(x => x.BackgroundColor = GSettings.CellBackgroundNeutralColour);
                Slots.ElementAt(coords.Item1).BackgroundColor = GSettings.CellBackgroundNeutralColour;
            }
            SlotTickLink.Remove(id);
        }

        private void SlotDelayInteraction(int id,int minutes)
        {
            Manager.InitDelay(id,minutes);
            XPlanRestart();
        }

        private bool AddNode(Sheets.Slot timeIn,int lfzIn,Sheets.Target tgtIn)
        {
            try
            {
                (int, int) coords = GetCellIndex(timeIn.Id,lfzIn);
                //System.Diagnostics.Debug.WriteLine($"Col Index: {colIndex} of {lfzIn}");
                //System.Diagnostics.Debug.WriteLine($"LFZ Details:\nId: {lfzIn.Id}\nReg: {lfzIn.Reg}\nType: {lfzIn.Type}\nSeats: {lfzIn.Seats}\nInterval: {lfzIn.Interval}\nPriceCat: {lfzIn.PriceCat}\nAuto: {lfzIn.AutoAssign}\nAvail: {string.Join(", ",lfzIn.AvailTimes)}\n---------");
                if (coords.Item1 == -1 || coords.Item2 == -1) return false; //No Cell found

                XBlock NewNode = new(tgtIn,timeIn.Length);
                if (DateTime.Now >= timeIn.STime.Subtract(TimeSpan.FromMinutes(USettings.SlotTolerance))) NewNode.DisableAttrib(2);
                var Drag = new DragGestureRecognizer();
                Drag.DragStarting += NodeDragStartHandler;
                Drag.CanDrag = !tgtIn.Persistent;
                NewNode.GestureRecognizers.Add(Drag);

                TapGestureRecognizer LClick = new()
                {
                    Buttons = ButtonsMask.Primary,
                };
                LClick.Tapped += NodeSelectionHandler;
                NewNode.GestureRecognizers.Add(LClick);

                StackContainers.ElementAt(coords.Item1).ElementAt(coords.Item2).Children.Add(NewNode);
                NodeLibrary.Add(NewNode.Id,NewNode);
                return true;
            }
            catch (Exception ex)
            {
                ConProc.Log($"[XPLAN-RENDERER] Der Knoten konnte nicht erstellt werden:" + ex.Message,2);
                return false;
            }
        }
        private void NodeSelectionHandler(object? sender,EventArgs e)
        {
            if (sender is XBlock selectedNode && selectedNode.Id != focusedID)
            {
                NodeDeselectionHandler(sender,e);
                focusedID = selectedNode.Id;
                selectedNode.Focus();
                if (selectedNode.Content is Border b) b.Stroke = GSettings.NodeForegroundColour;
                //selectedNode.StrokeThickness = 3;
                System.Diagnostics.Debug.WriteLine($"Selected node: {selectedNode.TargetID}");
            }
        }
        private void NodeDeselectionHandler(object? sender,EventArgs e)
        {
            if (!focusedID.ToByteArray().All(x => x == 0) && NodeLibrary.TryGetValue(focusedID,out XBlock? old))
            {
                if (old is not null)
                {
                    if (old.Content is Border b) b.Stroke = old.Attribs[3] ? GSettings.PrimaryColour : GSettings.NodeBackgroundColour;
                    old.Unfocus();
                }
                focusedID = new(new byte[16]);
            }
        }
        private void NodeDragStartHandler(object? sender,DragStartingEventArgs e)
        {
            if (sender is DragGestureRecognizer dragRecognizer && dragRecognizer.Parent is XBlock draggedNode)
            {
                e.Data.Properties["DraggedNode"] = draggedNode;
                e.Data.Text = string.Empty;
                System.Diagnostics.Debug.WriteLine($"Drag started for: {draggedNode.Content!.GetType()}",0);
            }
        }
        private void OnHoverNode(object? sender,DragEventArgs e,bool avail)
        {
            /* TO IMPLEMENT - Farbwiederherstellung nicht funktionstüchtig
            DropGestureRecognizer? d = (DropGestureRecognizer?)sender;
            if(d != null && d.Parent is Border b)
            {
                Brush? prev = b.Stroke;
                b.Stroke = avail ? Colors.Green : Colors.Red;
                _ = new Scheduler(TimeSpan.FromSeconds(3),(s,e) => b.Stroke = prev,false);  
            }*/
        }

        //Fehlender Refresh nach drop von node
        private async void OnDropNode(Border targetCell,DropEventArgs e)
        {
            e.Data.Properties.TryGetValue("DraggedNode",out var draggedNodeObj);
            System.Diagnostics.Debug.WriteLine($"Drop event - Target: {targetCell}, Node: " + draggedNodeObj);
            if (draggedNodeObj is XBlock draggedNode && draggedNode.Parent is TargetStack sourceContainer && targetCell.Content is TargetStack targetContainer)
            {
                (Func<Task<(int, DatabaseAction?)>>, (Sheets.Target, Sheets.Target))? now = await Manager.DatabaseNodeMove(draggedNode,targetContainer,0,false);
                if (now.HasValue)
                {
                    (int, DatabaseAction?) result = await now.Value.Item1.Invoke();
                    now.Value.Item2.Item2.LId = result.Item1;
                    if (result.Item1 != -1)
                    {
                        List<DatabaseAction> a = [];
                        DatabaseAction da = new() { ActionID = 3,CurrentValue = now.Value.Item2.Item2,PreviousValue = now.Value.Item2.Item1,DataType = typeof(Sheets.Target),ObjectID = now.Value.Item2.Item1.Id };
                        List<Sheets.Flt> rmv = Patcher.CleanupFlights();
                        a.Add(da);
                        if (result.Item2 != null) a.Add(result.Item2 with { LinkAction = da.ID });
                        rmv.ForEach(x => a.Add(new() { ActionID = 2,CurrentValue = null,PreviousValue = x,DataType = typeof(Sheets.Flt),ObjectID = x.Id,ForeignKeyName = now.Value.Item2.Item1.LId == x.Id ? "LId" : null,LinkAction = now.Value.Item2.Item1.LId == x.Id ? da.ID : 0 }));
                        AutoAct.PushAction(null,a);
                    }
                    XPlanRefresh();
                }
            }

            /*//Length Check missing
            e.Data.Properties.TryGetValue("DraggedNode",out var draggedNodeObj);
            System.Diagnostics.Debug.WriteLine($"Drop event - Target: {targetCell}, Node: " + draggedNodeObj);
            if (draggedNodeObj is XBlock draggedNode)
            {
                //Persistency Check
                Sheets.Target? TargetNode = RData.Get<Sheets.Target>(draggedNode.TargetID);
                if (!TargetNode?.Persistent ?? false)
                {
                    if (draggedNode.Parent is TargetStack sourceContainer && targetCell.Content is TargetStack targetContainer && !targetContainer.Id.Equals(sourceContainer.Id))
                    {
                        bool transact = true;
                        int lid = -1;
                        Sheets.Lfz? TargetLFZ = RData.Get<Sheets.Lfz>(targetContainer.LFZID);
                        //Avail Check
                        if (TargetLFZ?.AvailTimes?.Contains(targetContainer.SLTID) ?? false)
                        {
                            Sheets.Flt? flt = RData.GetWhere<Sheets.Flt>($"slot = {targetContainer.SLTID}")?.Where(x => x.Lfz == targetContainer.LFZID).FirstOrDefault();
                            if (flt != null)
                            {
                                //Flug vorhanden
                                lid = flt.Id;
                                if (!Manager.FlightFitsWeight(flt.Id,flt.Lfz,TargetNode?.Weight ?? USettings.DefaultTgtWeight))
                                {
                                    system.modals.ModalPush.Message(Lang.warning,Lang.message_to_much_weight);
                                    transact = false;
                                }
                            }
                            else
                            {
                                //Kein Flug vorhanden
                                if (!Manager.AircraftFitsWeight(targetContainer.LFZID,TargetNode?.Weight ?? USettings.DefaultTgtWeight))
                                {
                                    system.modals.ModalPush.Message(Lang.warning,Lang.message_to_much_weight);
                                    transact = false;
                                }
                            }


                            if (GSettings.AskForNodeMove)
                            {
                                await system.modals.ModalPush.Question(Lang.security,Lang.nodemove_question_sub).ContinueWith(x =>
                                {
                                    if (!x.Result) transact = false;
                                });
                            }
                        }

                        if (transact && TargetNode != null && GSettings.nav != null)
                        {
                            int newpc = await Drawer.AskForPriceUpdate(TargetNode.Price,TargetLFZ?.PriceCat);
                            if (newpc == 0) return;

                            System.Diagnostics.Debug.WriteLine("Transacting Node " + draggedNode.TargetID.ToString());
                            if (lid != -1)
                            {
                                sourceContainer.Children.Remove(draggedNode);
                                targetContainer?.Children.Add(draggedNode);

                                //Datenbankaktion
                                if(RData.UpdateProperty<Sheets.Target,int>(draggedNode.TargetID,lid,"LId"))
                                RData.UpdateProperty<Sheets.Target,int>(draggedNode.TargetID,newpc,"Price");
                            }
                            else
                            {
                                bool result = false;
                                await system.modals.ModalPush.Question(Lang.warning,Lang.newflt_warning).ContinueWith(t => result = t.Result);
                                if (result)
                                {
                                    int? Length = RData.Get<Sheets.Slot>(targetContainer.SLTID)?.Length;

                                    string? Adds = null;
                                    byte Status = (byte)(GSettings.Status.Length - 1);
                                    TargetCustomizer tc = new(null,new(),true);
                                    await GSettings.nav.PushModalAsync(tc);
                                    await tc.ShowAndSelect().ContinueWith(r =>
                                    {
                                        if (r.Result.Item2 == null)
                                        {
                                            result = false;
                                            return;
                                        }
                                        Adds = r.Result.Item2.Add;
                                        Status = r.Result.Item2.Status;
                                    });
                                    if (result)
                                    {


                                        //Datenbankaktion
                                        await Builder.CreateFlight(TargetNode.Weight,Adds,Status,Length,TargetNode.QuickTicket,targetContainer.LFZID,[targetContainer.SLTID]).ContinueWith(r =>
                                        {
                                            if (r.Result.Item1 != -1)
                                            {
                                                if(RData.UpdateProperty<Sheets.Target,int>(draggedNode.TargetID,r.Result.Item1,"LId"))
                                                RData.UpdateProperty<Sheets.Target,int>(draggedNode.TargetID,newpc,"Price");
                                                Manager.CleanupFlights();
                                            }
                                        });
                                        //FLIGHT CLEANUP
                                    }
                                }
                            }

                            //Datenbank aktualisieren
                            XPlanRefresh();
                        }
                        //ACTION

                    }
                }
            }*/
        }

        private void OpenAssistant_Click(object sender,EventArgs e)
        {

            //Window secondWindow = new Window(new Assistant());
            //Application.Current?.OpenWindow(secondWindow);
            Shell.Current.GoToAsync("//Assistant");
        }

        private void XConsoleClick(object sender,EventArgs e)
        {
            Window xConsoleWindow = new XConsole();
            xConsoleWindow.Destroying += ConProc.Window_Closed;
            Application.Current?.OpenWindow(xConsoleWindow);
        }




        private async void XPlan_Add_Click(object sender,EventArgs e)
        {
            //STATUS INDEX LETZTER, wenn AUTO
            string Definition = TGT_Name_Entry.Text == null || TGT_Name_Entry.Text.Trim() == String.Empty ? "N/A" : TGT_Name_Entry.Text.Trim();
            int TgtWeight = TGT_Weight_Entry.Text != null && Int32.TryParse(TGT_Weight_Entry.Text.Trim(),out int ParseWeight) ? ParseWeight : USettings.DefaultTgtWeight;
            int FltLength = FLT_Length_Entry.Text != null && Int32.TryParse(FLT_Length_Entry.Text.Trim(),out int ParseLength) ? ParseLength : USettings.DefaultFltLength;
            int TgtPrice = 0;
            if (!TGT_PriceCat_Dropdown_Enable.IsChecked)
            {
                if (TGT_Price_Dropdown.SelectedIndex > 0)
                {
                    TgtPrice = -CurrentPriceCatIds[TGT_Price_Dropdown.SelectedIndex - 1];
                }
                else
                {
                    int format = GSettings.InterpretePrice(TGT_Price_Entry.Text.Trim());
                    TgtPrice = format == -1 ? -USettings.FallbackPriceCat : format;
                }
            }
            //int TgtPrice = TGT_PriceCat_Dropdown_Enable.IsChecked ? 0 : (TGT_Price_Dropdown.SelectedIndex > 0 ? -CurrentPriceCatIds[TGT_Price_Dropdown.SelectedIndex] : (Int32.TryParse(TGT_Price_Entry.Text.Trim(),out int ParsePrice) ? GSettings.FormatPrice(ParsePrice) : -USettings.FallbackPriceCat));
            int FltStatus = FLT_Status_Dropdown_Enable.IsChecked ? 13 : FLT_Status_Dropdown.SelectedIndex;
            int? LfzOverride = TGT_LFZ_Dropdown_Enable.IsChecked ? null : CurrentAircraftIds[TGT_LFZ_Dropdown.SelectedIndex];
            string Adds = "";
            foreach (Entry entry in FLTAddsEntryContainer.OfType<Entry>())
            {
                Adds += entry.Text == null || entry.Text.Trim() == String.Empty ? ';' : (entry.Text.Trim().Replace(";",String.Empty) + ';');
            }
            await Builder.CreateTarget(Definition,TgtWeight,FltLength,TgtPrice,TGT_Quickticket_Enable.IsChecked,TGT_Persistent_Enable.IsChecked,(byte)FltStatus,Adds,LfzOverride,TGT_Autotime_Enable.IsChecked ? null : TGT_Time_Picker.Time);
            XPlanRefresh();
        }


        ////////////////////////////////////////////DEBUG MENU HANDLING/////////////////////////////////////////////
        private void CreateDemoNode_Click(object sender,EventArgs e)
        {
            Sheets.Target demoTGT = new()
            {
                Id = 1,
                Name = "Demo Target",
                Weight = 1,
                Persistent = false,
            };
            if (RData.GetSlotsTable().Count > 0 && RData.GetAircraftTable().Count > 0)
            {
                AddNode(RData.GetSlotsTable()[1],RData.GetAircraftTable()[0].Id,demoTGT);
            }
        }

        private void OpenSelectorModal_Click(object sender,EventArgs e)
        {
            system.modals.ModalPush.Selector("Test",Simulator.content).ContinueWith(t =>
            {
                ConProc.Log("[DEBUG] Selector-Modal test returned: " + t.Result);
                System.Diagnostics.Debug.WriteLine($"Selected Index: {t.Result}");
            });
        }
        private void OpenYesNoModal_Click(object sender,EventArgs e)
        {
            system.modals.ModalPush.Question("Test","Select type shit please").ContinueWith(t =>
            {
                ConProc.Log("[DEBUG] Question-Modal test returned: " + t.Result);
                System.Diagnostics.Debug.WriteLine($"Selected Option: {t.Result}");
            });
        }
        private void OpenMessageModal_Click(object sender,EventArgs e)
        {
            system.modals.ModalPush.Message("Test","Lorem Ipsum type shit. Ladet meine Software runter ig und rest ist baba. Vertrau. Keine Viren, einfach beste wo gibt");
        }

        private void OpenPriceModal_Click(object sender,EventArgs e)
        {
            PriceCustomizer p = new(1500,"Test");
            Navigation.PushModalAsync(p);
        }

        private void AddFLT_Sample_Click(object sender,EventArgs e)
        {
            Presets.WriteSample();
        }

        private void OpenDBPreview_Click(object sender,EventArgs e)
        {
            Window dbPreviewWindow = new DBPreview();
            Application.Current?.OpenWindow(dbPreviewWindow);
        }

        ////////////////////////////////////////////CREATOR BAR HANDLING////////////////////////////////////////////
        private void SelectedPriceCatChanged(object sender,EventArgs e) => TGT_Price_Entry.IsVisible = TGT_Price_Dropdown.SelectedIndex == 0;
        private void PriceCatAutoChanged(object sender,EventArgs e) => TGT_Price_Entry.IsVisible = !TGT_PriceCat_Dropdown_Enable.IsChecked && TGT_Price_Dropdown.SelectedIndex == 0;

        //////////////////////////////////////////INTERACTION BAR HANDLING//////////////////////////////////////////
        private void UndoInterClick(object sender,EventArgs e)
        {
            if (AutoAct.Undo()) XPlanRestart();
            else XPlanRefresh();
        }
        private void RedoInterClick(object sender,EventArgs e)
        {
            if (AutoAct.Redo()) XPlanRestart();
            else XPlanRefresh();
        }
        private void CopyInterClick(object sender,EventArgs e)
        {
            if (!focusedID.ToByteArray().All(x => x == 0))
            {
                copyBuffer = focusedID;
            }
        }
        private async void PasteInterClick(object sender,EventArgs e)
        {
            if (focusedID != copyBuffer && NodeLibrary.TryGetValue(focusedID,out XBlock? target) && NodeLibrary.TryGetValue(copyBuffer,out XBlock? source))
            {
                if (source != null && target != null && source.Parent is TargetStack vsl1 && target.Parent is TargetStack vsl2)
                {
                    System.Diagnostics.Debug.WriteLine($"Node switching - 1: {source.TargetID}, 2: {target.TargetID}");
                    (Func<Task<(int, DatabaseAction?)>>, (Sheets.Target, Sheets.Target))? move12 = await Manager.DatabaseNodeMove(source,vsl2,RData.Get<Sheets.Target>(target.TargetID)?.Weight ?? 0,false);
                    (Func<Task<(int, DatabaseAction?)>>, (Sheets.Target, Sheets.Target))? move21 = await Manager.DatabaseNodeMove(target,vsl1,RData.Get<Sheets.Target>(source.TargetID)?.Weight ?? 0,true);
                    if (move12.HasValue && move21.HasValue)
                    {
                        (int, DatabaseAction?) ra12 = await move12.Value.Item1.Invoke();
                        (int, DatabaseAction?) ra21 = await move21.Value.Item1.Invoke();

                        move12.Value.Item2.Item2.LId = ra12.Item1;
                        move21.Value.Item2.Item2.LId = ra21.Item1;
                        if (move12.Value.Item2.Item2.LId != -1 && move21.Value.Item2.Item2.LId != -1)
                        {
                            List<DatabaseAction> a =
                            [
                                new() { ActionID = 3,CurrentValue = move12.Value.Item2.Item2,PreviousValue = move12.Value.Item2.Item1,DataType = typeof(Sheets.Target),ObjectID = move12.Value.Item2.Item1.Id },
                                new() { ActionID = 3,CurrentValue = move21.Value.Item2.Item2,PreviousValue = move21.Value.Item2.Item1,DataType = typeof(Sheets.Target),ObjectID = move21.Value.Item2.Item1.Id },
                            ];
                            AutoAct.PushAction(null,a);
                            XPlanRefresh();
                        }
                    }
                }
            }


            /*e.Data.Properties.TryGetValue("DraggedNode",out var draggedNodeObj);
            System.Diagnostics.Debug.WriteLine($"Drop event - Target: {targetCell}, Node: " + draggedNodeObj);
            if (draggedNodeObj is XBlock draggedNode && draggedNode.Parent is TargetStack sourceContainer && targetCell.Content is TargetStack targetContainer)
            {
                Action? now = await Manager.DatabaseNodeMove(draggedNode,targetContainer);
                if (now != null)
                {
                    now.Invoke();
                    XPlanRefresh();
                }
            }*/
        }
        private async void EditInterClick(object sender,EventArgs e)
        {
            if (NodeLibrary.TryGetValue(focusedID,out XBlock? target))
            {
                Sheets.Target? t = RData.Get<Sheets.Target>(target.TargetID);
                //System.Diagnostics.Debug.WriteLine("Test1");
                if (t is null) return;
                Sheets.Flt? f = RData.Get<Sheets.Flt>(t.LId);
                //System.Diagnostics.Debug.WriteLine("Test2");
                if (f is null) return;
                //Sheets.Target? tgt = Sheets.Clone(t);
                //Sheets.Flt? flt = Sheets.Clone(f);

                TargetCustomizer tc = new(Sheets.Clone(t),Sheets.Clone(f));
                await Navigation.PushModalAsync(tc);
                await tc.ShowAndSelect().ContinueWith(r =>
                {
                    if (r.Result.Item1 is null || r.Result.Item2 is null) return;
                    RData.Update(r.Result.Item1,typeof(Sheets.Target));
                    RData.Update(r.Result.Item2,typeof(Sheets.Flt));
                    //CURRENT VALUE UND PREVIOUS VALUE SIND IDENTISCH -> REFERENCE PROBLEM -> SHALLOW COPY NICHT VERWENDEN
                    List<DatabaseAction> a =
                    [
                        new() { ActionID = 3,CurrentValue = r.Result.Item1,PreviousValue = t,DataType = typeof(Sheets.Target),ObjectID = target.TargetID },
                        new() { ActionID = 3,CurrentValue = r.Result.Item2,PreviousValue = f,DataType = typeof(Sheets.Flt),ObjectID = t.LId },
                    ];
                    System.Diagnostics.Debug.WriteLine("Prev: " + t.Name + " After:" + r.Result.Item1.Name);
                    AutoAct.PushAction(null,a);
                    //else ConProc.Log("[AUTOACT] Action couldn't be stacked onto the ActionStack",2);
                });
                XPlanRefresh();
            }
        }
        private void FlagInterClick(object sender,EventArgs e)
        {
            if (NodeLibrary.TryGetValue(focusedID,out XBlock? target)) target.UpdateAttrib(3);
        }
        private void NotifyInterClick(object sender,EventArgs e)
        {
            if (NodeLibrary.TryGetValue(focusedID,out XBlock? target)) target.UpdateAttrib(2);
        }
        private void InfoInterClick(object sender,EventArgs e)
        {
            if (NodeLibrary.TryGetValue(focusedID,out XBlock? target))
            {
                // Haken für Quick, Persitent, Notify

                Sheets.Target? t = RData.Get<Sheets.Target>(target.TargetID);
                StringBuilder? result = null;
                if (t != null)
                {
                    Sheets.Flt? f = RData.Get<Sheets.Flt>(t.LId);
                    Sheets.Slot? s = f != null ? RData.Get<Sheets.Slot>(f.Slot) : null;
                    Sheets.Lfz? l = f != null ? RData.Get<Sheets.Lfz>(f.Lfz) : null;
                    result = new($"{Lang.xplan_name}: {t.Name}{Environment.NewLine}");
                    result.AppendLine($"{Lang.time}: {(s != null ? $"{s.STime.ToShortTimeString()} - {s.FTime.ToShortTimeString()} ({s.Length} min)" : "N/A")}");
                    result.AppendLine($"{Lang.xplan_select_lfz}: {(l != null ? $"{l.Reg} ({l.Type})" : "N/A")}");
                    result.AppendLine($"{Lang.fltno}: {(f != null ? $"{(f.EId != null ? f.EId : f.Id)}" : "N/A")}{Environment.NewLine}{Lang.status}: {(f != null ? $"{GSettings.Status[f.Status]}" : "N/A")}");
                    string[] adds = f != null ? f.Add?.Split(';') ?? [] : [];
                    for (int i = 0; i < adds.Length; i++) result.AppendLine(USettings.Additionals.ElementAt(i) + ": " + adds[i]);
                    result.AppendLine($"{Lang.xplan_weight}: {t.Weight}");
                    string price = Manager.FormatPrice(t.Price).ToString();
                    result.AppendLine($"{Lang.xplan_price}: {GSettings.UnformatPrice(t.Price)}");
                }
                Navigation.PushModalAsync(new system.modals.Notification(Lang.info,result?.ToString() ?? "N/A"));
            }
        }
        private async void DeleteInterClick(object sender,EventArgs e)
        {
            if (NodeLibrary.TryGetValue(focusedID,out XBlock? target))
            {
                await system.modals.ModalPush.Question(Lang.warning,Lang.delete_warning).ContinueWith(t =>
                {
                    if (t.Result)
                    {
                        Sheets.Target? dbtarget = RData.Get<Sheets.Target>(target.TargetID);
                        List<DatabaseAction> a = [];
                        DatabaseAction da = new() { ActionID = 2,CurrentValue = null,PreviousValue = dbtarget,DataType = typeof(Sheets.Target),ObjectID = target.TargetID };
                        //PRE FLIGHT CLEANUP

                        RData.Delete(target.TargetID,typeof(Sheets.Target));
                        a.Add(da);
                        List<Sheets.Flt> rmv = Patcher.CleanupFlights();
                        rmv.ForEach(x => a.Add(new() { ActionID = 2,CurrentValue = null,PreviousValue = x,DataType = typeof(Sheets.Flt),ObjectID = x.Id,ForeignKeyName = dbtarget?.LId == x.Id ? "LId" : null,LinkAction = dbtarget?.LId == x.Id ? da.ID : 0 }));
                        AutoAct.PushAction(null,a);
                    }
                });
                XPlanRefresh();
            }
        }

        /////////////////////////////////////////////FILE MENU HANDLING/////////////////////////////////////////////

        //Profiles
        private void ProfileNewClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        private void ProfileOpenClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        private void ProfileSaveClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        private void ProfileSaveAsClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        private void ProfileViewClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        private void ProfileEditorClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        private void ProfileInfoClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");

        //Config
        private void ConfigNewClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        private void ConfigOpenClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        private void ConfigSaveClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        private void ConfigSaveAsClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        private void ConfigViewClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        private void ConfigEditorClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        private void ConfigInfoClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");

        //Filesystem
        private void OpenCacheClick(object sender,EventArgs e) => DskMan.OpenFolder(true);
        private void OpenDataClick(object sender,EventArgs e) => DskMan.OpenFolder(false);

        //Close
        private void CloseClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");

        ////////////////////////////////////////////SYSTEM MENU HANDLING////////////////////////////////////////////

        //XPlan Options
        private void XPlan_Restart_Click(object sender,EventArgs e) => XPlanRestart();
        private void XPlan_Refresh_Click(object sender,EventArgs e) => XPlanRefresh();

        //FLIGHT INFORMATION DISPLAY SYSTEM
        private void XBoardClick(object sender,EventArgs e)
        {
            BoardPage bp = new();
            Window w = new(bp);
            w.Created += (s,e) =>
            {
                bp.WindowWidth = w.Width;
                BoardController.Refresh(bp.BoardIndex);
                BoardController.SyncBoard(bp,null,null,null,null);
            };

            //RESIZE BUG
            w.SizeChanged += (s,e) =>
            {
                bp.WindowWidth = w.Width;
                BoardController.Refresh(bp.BoardIndex);
                BoardController.SyncBoard(bp,null,null,null,null);
            };
            w.Destroying += (s,e) => BoardController.Terminate(bp.BoardIndex);

            Application.Current?.OpenWindow(w);
        }
        private void XBoard_RefreshClick(object sender,EventArgs e)
        {
            if (BoardController.Boards.Count > 0)
            {
                List<Sheets.Flt> flts = RData.GetFlightTable();
                List<Sheets.Target> tgts = RData.GetTargetTable();
                BoardController.SynchronizeWithFlight(flts,tgts);
            }
        }

        private void XBoard_ClockRestartClick(object sender,EventArgs e)
        {
            BoardTimeServ.Pause();
            BoardTimeServ.Init();
        }

        private void XBoard_TerminateAllClick(object sener,EventArgs e)
        {
            for(int i = 0; i < BoardController.Boards.Count; i++)
            {
                BoardController.Terminate(i);
            }
        }

        //Clock Options
        private void SystemClockDisplay_Click(object sender,EventArgs e)
        {
            Navigation.PushModalAsync(new ClockCheck());
        }
        private void SystemClockClear_Click(object sender,EventArgs e) => TimeServ.Clear();
        private void SystemClockRestart_Click(object sender,EventArgs e) => TimeServ.Restart();


        ////////////////////////////////////////////MANAGE MENU HANDLING////////////////////////////////////////////

        //Delay
        private void CustomDelayClick(object sender,EventArgs e)
        {

        }

        //Check DB
        private async void CheckDBClick(object sender,EventArgs e)
        {
            await Patcher.GeneralInspection();
            XPlanRefresh();
        }

        private async void CheckDB_Unlink_Click(object sender,EventArgs e)
        {
            await Patcher.GeneralInspection();
            XPlanRefresh();
        }

        private void CheckDB_Overload_Click(object sender,EventArgs e) => Patcher.TestOverload();
        private void CheckDB_Overlap_Click(object sender,EventArgs e) => Patcher.TestOverlap();
        private void CheckDB_Night_Click(object sender,EventArgs e) => Patcher.TestNight();
        private void CheckDB_Cleanup_Click(object sender,EventArgs e)
        {
            Patcher.CleanupFlights();
            XPlanRefresh();
        }

        //Clear DB
        private async void ClearDB_Click(object sender,EventArgs e)
        {
            await system.modals.ModalPush.Question(Lang.warning,Lang.db_clear_warning).ContinueWith(t =>
            {
                if (t.Result) RData.Reset();
            });
        }



        ////////////////////////////////////////////NETWORK MENU HANDLING///////////////////////////////////////////
        
        //OGN
        private void OGN_RefreshClick(object sender,EventArgs e) => OGN.Sync();

        private void OGN_FetcherClick(object sender,EventArgs e) => Application.Current?.OpenWindow(new OnlineFetch());
        private void OGN_LinkOverwriteClick(object sender,EventArgs e)
        {
            OGN.LinkAddress(false);
            OGN.RelinkAddress(false,true);
        }
        private void OGN_LinkKeepClick(object sender,EventArgs e)
        {
            OGN.LinkAddress(false);
            OGN.RelinkAddress(false,false);
        }
        private void OGN_LinkManualClick(object sender,EventArgs e) => OGN.RelinkAddress(true,true);
        private void OGN_LinkRemainingManualClick(object sender,EventArgs e) => OGN.RelinkAddress(false,false);



        /////////////////////////////////////////////TOOLS MENU HANDLING////////////////////////////////////////////





        /////////////////////////////////////////////VIEW MENU HANDLING/////////////////////////////////////////////
        
        
        /////////////////////////////////////////////ABOUT MENU HANDLING////////////////////////////////////////////
        private void About_Clicked(object sender, EventArgs e)
        {
            system.modals.ModalPush.Message(Lang.info,Lang.about_text);
        }

        private void Docs_Clicked(object sender,EventArgs e)
        {
            system.modals.ModalPush.Message(Lang.info,"WIP");
        }

    }

    /*internal partial class XBorder : Border
    {
        internal Sheets.Target Tgt { get; set; }
        internal Sheets.Lfz Lfz { get; set; }
        internal Sheets.Slot Fts { get; set; }

        ///<summary>quick, pin, notify, flag</summary>
        internal bool[] Attrib = new bool[4];
    }*/

    internal partial class TargetStack : VerticalStackLayout
    {
        internal int LFZID { get; init; }
        internal byte SLTID { get; init; }
    }

    /*internal partial class FBorder : Border
    {
        internal int FltId { get; set; }
    }*/
}
