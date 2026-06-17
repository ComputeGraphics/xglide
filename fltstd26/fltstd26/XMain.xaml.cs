using CommunityToolkit.Maui.Behaviors;
using fltstd26.board;
using fltstd26.core;
using fltstd26.debug;
using fltstd26.etc;
using fltstd26.etc.online;
using fltstd26.Resources.Texts;
using fltstd26.system;
using fltstd26.XFly;
using Microsoft.Maui.Platform;
using System.Text;
namespace fltstd26
{
    public partial class XMain : ContentPage
    {
        //CLEANUP FLIGHTS REVERSABLE MACHEN!!!!!!
        //List<List<Border>> cells = [];
        private readonly List<List<TargetStack>> StackContainers = [];
        private readonly List<int> CurrentPriceCatIds = [];
        private readonly List<int> CurrentAircraftIds = [];

        private readonly Dictionary<Guid,XBlock> NodeLibrary = [];
        private Guid copyBuffer = new(new byte[16]);
        private Guid focusedID = new(new byte[16]);
        internal bool XPlanOpen = false;

        public XMain()
        {
            InitializeComponent();
            GSettings.nav = Navigation;
            DskMan.Init();
            RData.Init();
            OGN.Sync();
            DskMan.SaveConfig("Test");
            System.Diagnostics.Debug.WriteLine(Application.Current!.RequestedTheme.ToString());
            //System.Diagnostics.Debug.WriteLine(RData.Insert<Sheets.Target>(new() { Name = "Test" }));
            //System.Diagnostics.Debug.WriteLine(RData.GetWhere<Sheets.Slot>($"id=2").First().Length);
            //Application.Current!.UserAppTheme = AppTheme.Light;

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
                StackContainers.Clear();
                XPlanOpen = false;
            }
        }
        private void XPlanRefresh()
        {
            if (XPlanOpen)
            {
                List<Sheets.Flt> allFLT = RData.GetFlightTable();
                List<Sheets.Target> allTGT = RData.GetTargetTable();

                //Alle Nodes löschen
                XPlanClear(false);

                //Datenbank Nodes einfügen
                if (allFLT.Count != 0 && allTGT.Count != 0)
                {
                    AddFlightNo(allFLT);
                    foreach (var t in allTGT)
                    {
                        Sheets.Flt? f = allFLT.Find(x => x.Id == t.LId);
                        if (f == null)
                        {
                            ConProc.Log("[XPLAN-SYNC] Target " + t.Id.ToString() + " konnte kein Flug zugeordnet werden",2);
                            continue;
                        }
                        Sheets.Slot? s = RData.Get<Sheets.Slot>(f.Slot);
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
        private void AddFlightNo(List<Sheets.Flt> allFLT)
        {
            foreach (Sheets.Flt flt in allFLT)
            {
                short rowIndex = (short)RData.GetSlotsTable().FindIndex(fts => fts.Id.Equals(flt.Slot));
                System.Diagnostics.Debug.WriteLine($"Row Index: {rowIndex}");
                short colIndex = (short)RData.GetAircraftTable().FindIndex(lfz => lfz.Id.Equals(flt.Lfz));
                System.Diagnostics.Debug.WriteLine($"Col Index: {colIndex}");
                if (rowIndex == -1 || colIndex == -1)
                {
                    ConProc.Log($"[XPLAN-RENDERER] Dem Flug {flt.Id} konnte keine Zelle zugeordnet werden",2);
                    continue;
                }
                StackContainers.ElementAt(rowIndex).ElementAt(colIndex).Children.Add(Drawer.CreateFltCollector(flt.Id,flt.EId));
            }
        }
        internal void XPlanRestart()
        {
            try
            {
                XPlanClear(true);
                Color stroke = GSettings.DarkMode ? Colors.DarkGray : Colors.LightGray;
                List<Sheets.Lfz> allLFZ = RData.GetAircraftTable();
                SidebarRefresh();

                //XPLAN AUFBAUEN
                TapGestureRecognizer Deselector = new();
                Deselector.Tapped += NodeDeselectionHandler;
                XPlan.GestureRecognizers.Add(Deselector);
                XPlan.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                XPlan.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto,});
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
                    Label lbl = new()
                    {
                        Text = lfz.Reg,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 24,
                    };
                    XPlan.Add(lbl,XPlan.ColumnDefinitions.Count - 1,0);
                }
                foreach (Sheets.Slot fts in RData.GetSlotsTable())
                {
                    List<Border> borders = [];
                    List<TargetStack> containersRow = [];
                    XPlan.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    Grid slot = new()
                    {
                        Margin = new Thickness(0,0,10,0),
                        ColumnDefinitions =
                    {
                        new ColumnDefinition(),
                        new ColumnDefinition()
                    },
                        RowDefinitions =
                    {
                        new RowDefinition(),
                        new RowDefinition()
                    }
                    };

                    Label lbl = new()
                    {
                        Text = $"{fts.STime:HH:mm}\n{fts.FTime:HH:mm}",
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.End,
                        HorizontalOptions = LayoutOptions.Center,
                        FontSize = 20,
                    };
                    slot.Add(lbl,0,0);
                    slot.SetColumnSpan(lbl,2);

                    Button min5 = new()
                    {
                        Text = "+5",
                        CornerRadius = 0,
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.Start,
                    };
                    slot.Add(min5,0,1);
                    Button min15 = new()
                    {
                        Text = "+15",
                        CornerRadius = 0,
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.Start,
                    };
                    slot.Add(min15,1,1);

                    XPlan.Add(slot,0,XPlan.RowDefinitions.Count - 1);
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
                    StackContainers.Add(containersRow);
                    //cells.Add(borders);
                }
                XPlanOpen = true;
                ConProc.Log("[XPLAN] XPlan reset",0);
                XPlanRefresh();
            }
            catch (Exception e)
            {
                XPlanOpen = false;
                ConProc.Log("[XPLAN] XPlan restart failed" + e.Message.ToString(),2);
            }
        }
        private bool AddNode(Sheets.Slot timeIn,int lfzIn,Sheets.Target tgtIn)
        {
            try
            {
                int rowIndex = RData.GetSlotsTable().FindIndex(fts => fts.Id.Equals(timeIn.Id));
                //System.Diagnostics.Debug.WriteLine($"Row Index: {rowIndex}");
                int colIndex = RData.GetAircraftTable().FindIndex(lfz => lfz.Id.Equals(lfzIn));
                //System.Diagnostics.Debug.WriteLine($"Col Index: {colIndex} of {lfzIn}");
                //System.Diagnostics.Debug.WriteLine($"LFZ Details:\nId: {lfzIn.Id}\nReg: {lfzIn.Reg}\nType: {lfzIn.Type}\nSeats: {lfzIn.Seats}\nInterval: {lfzIn.Interval}\nPriceCat: {lfzIn.PriceCat}\nAuto: {lfzIn.AutoAssign}\nAvail: {string.Join(", ",lfzIn.AvailTimes)}\n---------");
                if (rowIndex == -1 || colIndex == -1) return false; //No Cell found

                XBlock NewNode = new(tgtIn,timeIn.Length);

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

                StackContainers.ElementAt(rowIndex).ElementAt(colIndex).Children.Add(NewNode);
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
                (Func<Task>, (Sheets.Target, Sheets.Target))? now = await Manager.DatabaseNodeMove(draggedNode,targetContainer,0,false);
                if (now.HasValue)
                {
                    await now.Value.Item1.Invoke();

                    Stack<DatabaseAction> a = new();
                    a.Push(new() { ActionID = 3,CurrentValue = now.Value.Item2.Item2,PreviousValue = now.Value.Item2.Item1,DataType = typeof(Sheets.Target),ObjectID = now.Value.Item2.Item1.Id });

                    List<Sheets.Flt> rmv = Patcher.CleanupFlights();
                    rmv.ForEach(x => a.Push(new() { ActionID = 2,CurrentValue = null,PreviousValue = x,DataType = typeof(Sheets.Flt),ObjectID = x.Id }));
                    AutoAct.PushAction(null,a);
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
            Application.Current?.OpenWindow(xConsoleWindow);
        }
        private void XBoardClick(object sender,EventArgs e)
        {
            Application.Current?.OpenWindow(new Window(new BoardPage()));
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
                    TgtPrice = -CurrentPriceCatIds[TGT_Price_Dropdown.SelectedIndex];
                }
                else
                {
                    int format = GSettings.InterpretePrice(TGT_Price_Entry.Text.Trim());
                    TgtPrice = format == -1 ? -USettings.FallbackPriceCat : format;
                }
            }
            //int TgtPrice = TGT_PriceCat_Dropdown_Enable.IsChecked ? 0 : (TGT_Price_Dropdown.SelectedIndex > 0 ? -CurrentPriceCatIds[TGT_Price_Dropdown.SelectedIndex] : (Int32.TryParse(TGT_Price_Entry.Text.Trim(),out int ParsePrice) ? GSettings.FormatPrice(ParsePrice) : -USettings.FallbackPriceCat));
            int FltStatus = FLT_Status_Dropdown_Enable.IsChecked ? GSettings.Status.Length - 1 : FLT_Status_Dropdown.SelectedIndex;
            int? LfzOverride = TGT_LFZ_Dropdown_Enable.IsChecked ? null : CurrentAircraftIds[TGT_LFZ_Dropdown.SelectedIndex];
            string Adds = "";
            foreach (IView AddEntry in FLTAddsEntryContainer)
            {
                if (AddEntry is Entry entry) Adds += entry.Text == null || entry.Text.Trim() == String.Empty ? ';' : entry.Text.Trim().Replace(";",String.Empty) + ';';
            }
            await Builder.CreateTarget(Definition,TgtWeight,FltLength,TgtPrice,TGT_Quickticket_Enable.IsChecked,TGT_Persistent_Enable.IsChecked,(byte)FltStatus,"",LfzOverride,TGT_Autotime_Enable.IsChecked ? null : TGT_Time_Picker.Time);
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

        internal void RefreshActionButtons(bool u,bool r)
        {
            //XPlan_UndoButton.IsEnabled = u;
            //XPlan_RedoButton.IsEnabled = r;
        }
        private void UndoInterClick(object sender,EventArgs e)
        {
            AutoAct.Undo();
            XPlanRefresh();
        }
        private void RedoInterClick(object sender,EventArgs e)
        {
            AutoAct.Redo();
            XPlanRefresh();
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
                    (Func<Task>, (Sheets.Target, Sheets.Target))? move12 = await Manager.DatabaseNodeMove(source,vsl2,RData.Get<Sheets.Target>(target.TargetID)?.Weight ?? 0,false);
                    (Func<Task>, (Sheets.Target, Sheets.Target))? move21 = await Manager.DatabaseNodeMove(target,vsl1,RData.Get<Sheets.Target>(source.TargetID)?.Weight ?? 0,true);
                    if (move12.HasValue && move21.HasValue)
                    {
                        await move12.Value.Item1.Invoke();
                        await move21.Value.Item1.Invoke();
                        Stack<DatabaseAction> a = new();
                        a.Push(new() { ActionID = 3,CurrentValue = move12.Value.Item2.Item2,PreviousValue = move12.Value.Item2.Item1,DataType = typeof(Sheets.Target),ObjectID = move12.Value.Item2.Item1.Id });
                        a.Push(new() { ActionID = 3,CurrentValue = move21.Value.Item2.Item2,PreviousValue = move21.Value.Item2.Item1,DataType = typeof(Sheets.Target),ObjectID = move21.Value.Item2.Item1.Id });
                        AutoAct.PushAction(null,a);
                        XPlanRefresh();
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
                    Stack<DatabaseAction> a = new();
                    System.Diagnostics.Debug.WriteLine("Prev: " + t.Name + " After:" + r.Result.Item1.Name);
                    a.Push(new() { ActionID = 3,CurrentValue = r.Result.Item1,PreviousValue = t,DataType = typeof(Sheets.Target),ObjectID = target.TargetID });
                    a.Push(new() { ActionID = 3,CurrentValue = r.Result.Item2,PreviousValue = f,DataType = typeof(Sheets.Flt),ObjectID = t.LId });
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
                        Stack<DatabaseAction> a = new();
                        a.Push(new() { ActionID = 2,CurrentValue = null,PreviousValue = RData.Get<Sheets.Target>(target.TargetID),DataType = typeof(Sheets.Target),ObjectID = target.TargetID });
                        //PRE FLIGHT CLEANUP

                        RData.Delete(target.TargetID,typeof(Sheets.Target));
                        List<Sheets.Flt> rmv = Patcher.CleanupFlights();
                        rmv.ForEach(x => a.Push(new() { ActionID = 2,CurrentValue = null,PreviousValue = x,DataType = typeof(Sheets.Flt),ObjectID = x.Id }));
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


        ////////////////////////////////////////////MANAGE MENU HANDLING////////////////////////////////////////////


        private async void CheckDBClick(object sender,EventArgs e)
        {
            await Patcher.GeneralInspection();
            XPlanRestart();
        }


        /////////////////////////////////////////////TOOLS MENU HANDLING////////////////////////////////////////////

        private void OGN_RefreshClick(object sender,EventArgs e) => OGN.Sync();

        private void OGN_FetcherClick(object sender,EventArgs e) => Application.Current?.OpenWindow(new OnlineFetch());

        /////////////////////////////////////////////VIEW MENU HANDLING/////////////////////////////////////////////

        //XPlan Options
        private void XPlan_Restart_Click(object sender,EventArgs e) => XPlanRestart();
        private void XPlan_Refresh_Click(object sender,EventArgs e) => XPlanRefresh();
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

    internal partial class FBorder : Border
    {
        internal int FltId { get; set; }
    }
}
