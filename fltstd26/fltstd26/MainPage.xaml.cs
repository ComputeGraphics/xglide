using CommunityToolkit.Maui.Behaviors;
using fltstd26.core;
using fltstd26.debug;
using fltstd26.etc;
using fltstd26.system;
using fltstd26.XFly;
using Microsoft.Maui.Controls.Shapes;
using System.Xml.Linq;
namespace fltstd26
{
    public partial class MainPage : ContentPage
    {

        List<List<Border>> cells = [];
        List<List<VerticalStackLayout>> containers = [];

        Dictionary<Guid,XBorder> NodeLibrary = [];
        Guid copyBuffer = new(new byte[16]);
        Guid focusedID = new(new byte[16]);

        public MainPage()
        {
            InitializeComponent();
            DskMan.Init();
            RData.Init();
            System.Diagnostics.Debug.WriteLine(Application.Current!.RequestedTheme.ToString());
            //Application.Current!.UserAppTheme = AppTheme.Light;
        }

        public void XPlan_Restart()
        {
            Color stroke = Application.Current!.RequestedTheme == AppTheme.Dark ? Colors.DarkGray : Colors.LightGray;

            TGT_LFZ_Dropdown.Items.Clear();
            List<Types.LFZ> allLFZ = RData.Handler!.GetAll<Types.LFZ>();
            foreach (Types.LFZ lfz in allLFZ)
            {
                TGT_LFZ_Dropdown.Items.Add(lfz.Reg);
            }
            //XPLAN AUFBAUEN
            TapGestureRecognizer Deselector = new TapGestureRecognizer();
            Deselector.Tapped += NodeDeselectionHandler;
            XPlan.GestureRecognizers.Add(Deselector);

            XPlan.ColumnDefinitions.Clear();
            XPlan.RowDefinitions.Clear();
            XPlan.Children.Clear();
            XPlan.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            XPlan.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto,});
            foreach (Types.LFZ lfz in allLFZ)
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
            foreach (Types.FTS fts in RData.Handler!.GetAll<Types.FTS>())
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
                    Text = $"{fts.Start:HH:mm}\n{fts.End:HH:mm}",
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


                    if (Get.AvailableIn(allLFZ[i].Id,fts.Id))
                    {
                        VerticalStackLayout cellContainer = [];
                        cellBorder.Content = cellContainer;
                        cellBorder.GestureRecognizers.Add(new DropGestureRecognizer());

                        if (cellBorder.GestureRecognizers[0] is DropGestureRecognizer dropGesture)
                        {
                            dropGesture.DragOver += (s,e) => OnHoverNode(s,e,true);
                            dropGesture.AllowDrop = true;
                            dropGesture.Drop += (s,e) => OnDropNode(cellBorder,e);
                        }
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
                        cellBorder.Content = x;
                    }

                    borders.Add(cellBorder);
                    XPlan.Add(cellBorder,i + 1,XPlan.RowDefinitions.Count - 1);
                }
                containers.Add(containersRow);

                cells.Add(borders);
            }
        }

        private bool AddNode(Types.FTS timeIn,Types.LFZ lfzIn,Types.TGT tgtIn,bool auto = false)
        {
            int rowIndex = RData.Handler!.GetAll<Types.FTS>().FindIndex(fts => fts.Equals(timeIn));
            System.Diagnostics.Debug.WriteLine($"Row Index: {rowIndex}");
            System.Diagnostics.Debug.WriteLine($"LFZ Details:\nId: {lfzIn.Id}\nReg: {lfzIn.Reg}\nType: {lfzIn.Type}\nSeats: {lfzIn.Seats}\nInterval: {lfzIn.Interval}\nPriceCat: {lfzIn.PriceCat}\nAuto: {lfzIn.AutoAssign}\nAvail: {string.Join(", ",lfzIn.AvailTimes)}\n---------");
            int colIndex = RData.Handler!.GetAll<Types.LFZ>().FindIndex(lfz => lfz.Id.Equals(lfzIn.Id));
            System.Diagnostics.Debug.WriteLine($"Col Index: {colIndex}");
            if (rowIndex == -1 || colIndex == -1) return false; //No Cell found

            Grid nodegrid = new()
            {
                ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition {Width = GridLength.Star}
                    },
                RowDefinitions =
                    {
                        new RowDefinition(),
                        new RowDefinition(),
                        new RowDefinition(),
                        new RowDefinition()
                    }
            };

            Label nodeid = new()
            {
                Text = tgtIn.Id.ToString(),
                TextColor = GSettings.NodeColour,
                Padding = new Thickness(3),
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = 18
            };

            nodegrid.Add(nodeid);
            nodegrid.SetColumnSpan(nodeid,2);

            Label nodename = new()
            {
                Text = tgtIn.Name,
                TextColor = GSettings.NodeColour,
                Padding = new Thickness(3),
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = 16
            };

            nodegrid.Add(nodename,0,1);
            nodegrid.SetColumnSpan(nodename,2);

            Label nodelength = new()
            {
                Text = timeIn.Length.ToString() + " min",
                TextColor = GSettings.NodeColour,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Padding = new Thickness(20,0),
                FontSize = 16
            };
            nodegrid.Add(nodelength,0,2);

            HorizontalStackLayout hzslicon = new()
            {
                HorizontalOptions = LayoutOptions.End,
                Padding = new Thickness(0,0,20,0),
            };
            bool[] props = [tgtIn.QuickTicket,tgtIn.Persistent,false,false];
            ImageSource[] sources = ["quick.png","pin.png","notify.png","flag.png"];
            string[] xname = ["QuickBtn","PinBtn","NotifyBtn","FlagBtn"];

            for (int i = 0; i < props.Length; i++)
            {
                ImageButton imgbtn = new()
                {
                    BackgroundColor = Colors.Transparent,
                    Source = sources[i],
                    Aspect = Aspect.AspectFit,
                    Behaviors =
                    {
                        new IconTintColorBehavior { TintColor = props[i] ? GSettings.PrimaryColour : GSettings.InactiveIcon }
                    },
                };
                imgbtn.Clicked += NodeInteractionHandler;
                hzslicon.Add(imgbtn);
            }
            nodegrid.Add(hzslicon,1,2);

            Label nodeoid = new()
            {
                Text = tgtIn.Id.ToString(),
                TextColor = GSettings.NodeColour,
                Padding = new Thickness(3),
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = 16
            };

            nodegrid.Add(nodeoid,0,3);
            nodegrid.SetColumnSpan(nodeoid,2);

            XBorder node = new()
            {
                BackgroundColor = Application.Current!.RequestedTheme == AppTheme.Dark ? GSettings.GetColour("Gray950") : GSettings.GetColour("Gray100"),
                Stroke = GSettings.NodeColour,
                StrokeThickness = 0,
                Content = nodegrid,
                Tgt = tgtIn,
                Lfz = lfzIn,
                Fts = timeIn,
                Attrib = props
            };

            // Gesture Control
            var Drag = new DragGestureRecognizer();
            Drag.DragStarting += NodeDragStartHandler;
            node.GestureRecognizers.Add(Drag);

            TapGestureRecognizer LClick = new TapGestureRecognizer
            {
                Buttons = ButtonsMask.Primary,
            };
            LClick.Tapped += NodeSelectionHandler;
            node.GestureRecognizers.Add(LClick);

            containers.ElementAt(rowIndex).ElementAt(colIndex).Children.Add(node);
            NodeLibrary.Add(node.Id,node);
            return true;
        }

        private void NodeInteractionHandler(object? sender,EventArgs e)
        {
            if (sender is ImageButton interaction)
            {
                if (interaction.Parent.Parent.Parent is XBorder invokerCell)
                {
                    System.Diagnostics.Debug.WriteLine("Invoked by " + invokerCell.Tgt.Id);
                    if (interaction.Parent is HorizontalStackLayout hzsl && invokerCell.Content is Grid)
                    {
                        int buttonIndex = hzsl.Children.IndexOf(interaction);
                        var btn = hzsl.Children.OfType<ImageButton>().ToList().ElementAt(buttonIndex);
                        if (btn.Behaviors.OfType<IconTintColorBehavior>().FirstOrDefault() != null) btn.Behaviors.Clear();
                        //Invoke button specific action here
                        invokerCell.Attrib[buttonIndex] = !invokerCell.Attrib[buttonIndex];
                        btn.Behaviors.Add(new IconTintColorBehavior { TintColor = invokerCell.Attrib[buttonIndex] ? GSettings.PrimaryColour : GSettings.InactiveIcon });
                    }
                }
            }
        }

        private void NodeSelectionHandler(object? sender,EventArgs e)
        {
            if (sender is XBorder selectedNode && selectedNode.Id != focusedID)
            {
                NodeDeselectionHandler(sender,e);
                focusedID = selectedNode.Id;
                selectedNode.Focus();
                selectedNode.StrokeThickness = 3;
                System.Diagnostics.Debug.WriteLine($"Selected node: {selectedNode.Tgt.Id}");
            }
        }

        private void NodeDeselectionHandler(object? sender,EventArgs e)
        {
            if (!focusedID.ToByteArray().All(x => x == 0) && NodeLibrary.TryGetValue(focusedID,out XBorder? old))
            {
                if (old is not null)
                {
                    old.StrokeThickness = 0;
                    old.Unfocus();
                }
                focusedID = new(new byte[16]);
            }
        }

        private void NodeDragStartHandler(object? sender,DragStartingEventArgs e)
        {
            if (sender is DragGestureRecognizer dragRecognizer && dragRecognizer.Parent is XBorder draggedNode)
            {
                e.Data.Properties["DraggedNode"] = draggedNode;
                e.Data.Text = string.Empty;
                System.Diagnostics.Debug.WriteLine($"Drag started for: {draggedNode.Content!.GetType()}",0);
            }
        }

        private void OnHoverNode(object? sender,DragEventArgs e,bool avail)
        {
            e.AcceptedOperation = avail ? e.AcceptedOperation = DataPackageOperation.Copy : DataPackageOperation.None;
        }
        private static void OnDropNode(Border targetCell,DropEventArgs e)
        {
            e.Data.Properties.TryGetValue("DraggedNode",out var draggedNodeObj);
            System.Diagnostics.Debug.WriteLine($"Drop event - Target: {targetCell}, Node: " + draggedNodeObj);
            if (draggedNodeObj is XBorder draggedNode)
            {
                if (draggedNode.Parent is VerticalStackLayout sourceContainer)
                {
                    sourceContainer.Children.Remove(draggedNode);
                    var targetContainer = targetCell.Content as VerticalStackLayout;
                    targetContainer?.Children.Add(draggedNode);
                }
            }
        }

        public void OpenAssistant_Click(object sender,EventArgs e)
        {
            //Window secondWindow = new Window(new Assistant());
            //Application.Current?.OpenWindow(secondWindow);
            Shell.Current.GoToAsync("//Assistant");
        }

        public void XConsoleClick(object sender,EventArgs e)
        {
            Window xConsoleWindow = new XConsole();
            Application.Current?.OpenWindow(xConsoleWindow);
        }

        public void XPlan_Restart_Click(object sender,EventArgs e)
        {
            XPlan_Restart();
        }

        public void XPlan_Add_Click(object sender,EventArgs e)
        {

        }

        public void AutoAircraft_Toggled(object sender,CheckedChangedEventArgs e)
        {
            TGT_LFZ_Dropdown.IsEnabled = !e.Value;
        }

        public void AutoTime_Toggled(object sender,CheckedChangedEventArgs e)
        {
            TGT_Time_Picker.IsEnabled = !e.Value;
        }

        public void AutoStatus_Toggled(object sender,CheckedChangedEventArgs e)
        {
            FLT_Status_Dropdown.IsEnabled = !e.Value;
        }
        public void CreateDemoNode_Click(object sender,EventArgs e)
        {
            Types.TGT demoTGT = new()
            {
                Id = 1,
                Name = "Demo Target",
                Weight = 1,
                Persistent = false,
            };
            if (RData.Handler!.GetAll<Types.FTS>().Count > 0 && RData.Handler!.GetAll<Types.LFZ>().Count > 0)
            {
                AddNode(RData.Handler!.GetAll<Types.FTS>()[1],RData.Handler!.GetAll<Types.LFZ>()[0],demoTGT);
            }
        }

        public void AddFLT_Sample_Click(object sender,EventArgs e)
        {
            Presets.WriteSample();
        }

        public void OpenDBPreview_Click(object sender,EventArgs e)
        {
            Window dbPreviewWindow = new DBPreview();
            Application.Current?.OpenWindow(dbPreviewWindow);
        }


        //////////////////////////////////////////INTERACTION BAR HANDLING//////////////////////////////////////////
        public void UndoInterClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Undo Click");
        public void RedoInterClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Redo Click");
        public void CopyInterClick(object sender,EventArgs e)
        {
            if(!focusedID.ToByteArray().All(x => x == 0))
            {
                System.Diagnostics.Debug.WriteLine("Copied");
                copyBuffer = focusedID;
            }
        }
        public void PasteInterClick(object sender,EventArgs e)
        {
            if(focusedID != copyBuffer && NodeLibrary.ContainsKey(focusedID) && NodeLibrary.ContainsKey(copyBuffer))
            {
                XBorder source = NodeLibrary[copyBuffer];
                XBorder target = NodeLibrary[focusedID];
                if (source.Parent is VerticalStackLayout vsl1 && target.Parent is VerticalStackLayout vsl2)
                {
                    vsl1.Children.Remove(source);
                    vsl2.Children.Remove(target);
                    vsl1.Children.Add(target);
                    vsl2.Children.Add(source);
                }
            }
        }
        public void EditInterClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Edit Click");
        public void FlagInterClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Flag Click");
        public void NotifyInterClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Notify Click");
        public void InfoInterClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Info Click");
        public void DeleteInterClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Delete Click");

        /////////////////////////////////////////////FILE MENU HANDLING/////////////////////////////////////////////

        //Profiles
        public void ProfileNewClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        public void ProfileOpenClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        public void ProfileSaveClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        public void ProfileSaveAsClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        public void ProfileViewClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        public void ProfileEditorClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        public void ProfileInfoClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");

        //Config
        public void ConfigNewClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        public void ConfigOpenClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        public void ConfigSaveClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        public void ConfigSaveAsClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        public void ConfigViewClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        public void ConfigEditorClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
        public void ConfigInfoClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");

        //Filesystem
        public void OpenCacheClick(object sender,EventArgs e) => DskMan.OpenFolder(true);
        public void OpenDataClick(object sender,EventArgs e) => DskMan.OpenFolder(false);

        //Close
        public void CloseClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
    }

    public partial class XBorder : Border
    {
        public Types.TGT Tgt { get; set; }
        public Types.LFZ Lfz { get; set; }
        public Types.FTS Fts { get; set; }

        ///<summary>quick, pin, notify, flag</summary>
        public bool[] Attrib = new bool[4];
    }
}
