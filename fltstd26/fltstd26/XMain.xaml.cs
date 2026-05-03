using CommunityToolkit.Maui.Behaviors;
using fltstd26.board;
using fltstd26.core;
using fltstd26.debug;
using fltstd26.etc;
using fltstd26.Resources.Texts;
using fltstd26.system;
using fltstd26.XFly;
using System.Xml.Linq;
namespace fltstd26
{
    public partial class XMain : ContentPage
    {
        List<List<Border>> cells = [];
        List<List<VerticalStackLayout>> StackContainers = [];

        Dictionary<Guid,XBlock> NodeLibrary = [];
        Guid copyBuffer = new(new byte[16]);
        Guid focusedID = new(new byte[16]);

        public XMain()
        {
            InitializeComponent();
            USettings.nav = Navigation;
            DskMan.Init();
            RData.Init();
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
            RData.GetAircraftTable().ForEach(x => TGT_LFZ_Dropdown.Items.Add(x.Reg));
            TGT_Price_Dropdown.Items.Add(Lang.custom);
            RData.GetPriceTable().ForEach(x => TGT_Price_Dropdown.Items.Add($"{x.Name} ({GSettings.FormatPrice(x.Price)})"));
        }

        private void XPlanRefresh()
        {
            List<Sheets.Flt> allFLT = RData.GetFlightTable();
            List<Sheets.Target> allTGT = RData.GetTargetTable();

            //Alle Nodes löschen
            StackContainers.ForEach(c => c.ForEach(x => x.Clear()));
            NodeLibrary.Clear();
            copyBuffer = new(new byte[16]);
            focusedID = new(new byte[16]);

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
                    };
                    Sheets.Slot? s = RData.Get<Sheets.Slot>(f.Slot);
                    if (s == null)
                    {
                        ConProc.Log("[XPLAN-SYNC] Target " + t.Id.ToString() + " konnte kein Slot zugeordnet werden",2);
                        continue;
                    };
                    AddNode(s,f.Lfz,t);
                }
            }
        }

        private void AddFlightNo(List<Sheets.Flt> allFLT)
        {
            Drawer drawer = new();
            foreach (Sheets.Flt flt in allFLT)
            {
                int rowIndex = RData.GetSlotsTable().FindIndex(fts => fts.Id.Equals(flt.Slot));
                System.Diagnostics.Debug.WriteLine($"Row Index: {rowIndex}");
                int colIndex = RData.GetAircraftTable().FindIndex(lfz => lfz.Id.Equals(flt.Lfz));
                System.Diagnostics.Debug.WriteLine($"Col Index: {colIndex}");
                if (rowIndex == -1 || colIndex == -1)
                {
                    ConProc.Log($"[XPLAN-RENDERER] Dem Flug {flt.Id} konnte keine Zelle zugeordnet werden",2);
                    continue;
                }
                StackContainers.ElementAt(rowIndex).ElementAt(colIndex).Children.Add(drawer.CreateFltCollector(flt.Id,flt.EId));
            }

        }

        internal void XPlan_Restart()
        {
            Color stroke = GSettings.DarkMode ? Colors.DarkGray : Colors.LightGray;
            List<Sheets.Lfz> allLFZ = RData.GetAircraftTable();
            SidebarRefresh();
            //XPLAN AUFBAUEN
            TapGestureRecognizer Deselector = new();
            Deselector.Tapped += NodeDeselectionHandler;
            XPlan.GestureRecognizers.Add(Deselector);

            XPlan.ColumnDefinitions.Clear();
            XPlan.RowDefinitions.Clear();
            XPlan.Children.Clear();
            XPlan.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            XPlan.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto,});
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
                List<VerticalStackLayout> containersRow = [];
                XPlan.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
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
                        VerticalStackLayout cellContainer = [];
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

                cells.Add(borders);
            }
            XPlanRefresh();
        }

        private bool AddNode(Sheets.Slot timeIn,int lfzIn,Sheets.Target tgtIn)
        {
            int rowIndex = RData.GetSlotsTable().FindIndex(fts => fts.Id.Equals(timeIn.Id));
            System.Diagnostics.Debug.WriteLine($"Row Index: {rowIndex}");
            int colIndex = RData.GetAircraftTable().FindIndex(lfz => lfz.Id.Equals(lfzIn));
            System.Diagnostics.Debug.WriteLine($"Col Index: {colIndex} of {lfzIn.ToString()}");
            //System.Diagnostics.Debug.WriteLine($"LFZ Details:\nId: {lfzIn.Id}\nReg: {lfzIn.Reg}\nType: {lfzIn.Type}\nSeats: {lfzIn.Seats}\nInterval: {lfzIn.Interval}\nPriceCat: {lfzIn.PriceCat}\nAuto: {lfzIn.AutoAssign}\nAvail: {string.Join(", ",lfzIn.AvailTimes)}\n---------");
            if (rowIndex == -1 || colIndex == -1) return false; //No Cell found

            XBlock NewNode = new(tgtIn,timeIn.Length);

            var Drag = new DragGestureRecognizer();
            Drag.DragStarting += NodeDragStartHandler;
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

        private void NodeSelectionHandler(object? sender,EventArgs e)
        {
            if (sender is XBlock selectedNode && selectedNode.Id != focusedID)
            {
                NodeDeselectionHandler(sender,e);
                focusedID = selectedNode.Id;
                selectedNode.Focus();
                if (selectedNode.Content is Border b) b.StrokeThickness = 3;
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
                    if (old.Content is Border b) b.StrokeThickness = 0;
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
        private static void OnDropNode(Border targetCell,DropEventArgs e)
        {
            e.Data.Properties.TryGetValue("DraggedNode",out var draggedNodeObj);
            System.Diagnostics.Debug.WriteLine($"Drop event - Target: {targetCell}, Node: " + draggedNodeObj);
            if (draggedNodeObj is XBlock draggedNode)
            {
                if (draggedNode.Parent is VerticalStackLayout sourceContainer)
                {
                    sourceContainer.Children.Remove(draggedNode);
                    var targetContainer = targetCell.Content as VerticalStackLayout;
                    targetContainer?.Children.Add(draggedNode);
                }
            }
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

        private void XPlan_Restart_Click(object sender,EventArgs e)
        {
            XPlan_Restart();
        }

        private void XPlan_Add_Click(object sender,EventArgs e)
        {
            Builder.CreateTarget(TGT_Name_Entry.Text,TGT_Weight_Entry.Text,FLT_Length_Entry.Text,TGT_Price_Entry.Text,TGT_Quickticket_Enable.IsChecked,TGT_Persistent_Enable.IsChecked,0,"");
        }

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
            List<(string, string, string)> content = new()
            {
                ("plane.png","Option 1","Description for option 1"),
                ("control.png","Option 2","Description for option 2"),
                ("copy.png","Option 3","Description for option 3")
            };
            system.modals.ModalPush.Selector("Test",content).ContinueWith(t =>
            {
                System.Diagnostics.Debug.WriteLine($"Selected Index: {t.Result}");
            });



            /*system.modals.Selector selector = new("Test", content);
            await Navigation.PushModalAsync(selector);
            await selector.ShowAndSelectAsync().ContinueWith(t =>
            {
                int selectedIndex = t.Result;
                
            });*/
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

        private void OGNFetcher_Click(object sender,EventArgs e) => Application.Current?.OpenWindow(new OnlineFetch());

        ////////////////////////////////////////////CREATOR BAR HANDLING////////////////////////////////////////////
        private void SelectedPriceCatChanged(object sender,EventArgs e) => TGT_Price_Entry.IsVisible = TGT_Price_Dropdown.SelectedIndex == 0;
        private void PriceCatAutoChanged(object sender,EventArgs e) => TGT_Price_Entry.IsVisible = !TGT_PriceCat_Dropdown_Enable.IsChecked && TGT_Price_Dropdown.SelectedIndex == 0;



        //////////////////////////////////////////INTERACTION BAR HANDLING//////////////////////////////////////////
        private void UndoInterClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Undo Click");
        private void RedoInterClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Redo Click");
        private void CopyInterClick(object sender,EventArgs e)
        {
            if (!focusedID.ToByteArray().All(x => x == 0))
            {
                copyBuffer = focusedID;
            }
        }
        private void PasteInterClick(object sender,EventArgs e)
        {
            if (focusedID != copyBuffer && NodeLibrary.TryGetValue(focusedID,out XBlock? target) && NodeLibrary.TryGetValue(copyBuffer,out XBlock? source))
            {
                if (source.Parent is VerticalStackLayout vsl1 && target.Parent is VerticalStackLayout vsl2)
                {
                    vsl1.Children.Remove(source);
                    vsl2.Children.Remove(target);
                    vsl1.Children.Add(target);
                    vsl2.Children.Add(source);
                }
            }
        }
        private void EditInterClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Edit Click");
        private void FlagInterClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Flag Click");
        private void NotifyInterClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Notify Click");
        private void InfoInterClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Info Click");
        private void DeleteInterClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Delete Click");

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
    }

    /*internal partial class XBorder : Border
    {
        internal Sheets.Target Tgt { get; set; }
        internal Sheets.Lfz Lfz { get; set; }
        internal Sheets.Slot Fts { get; set; }

        ///<summary>quick, pin, notify, flag</summary>
        internal bool[] Attrib = new bool[4];
    }*/

    internal partial class FBorder : Border
    {
        internal int FltId { get; set; }
    }
}
