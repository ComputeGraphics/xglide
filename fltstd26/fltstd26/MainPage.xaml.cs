using CommunityToolkit.Maui.Behaviors;
using fltstd26.core;
using fltstd26.debug;
using fltstd26.etc;
using fltstd26.system;
using Microsoft.Maui.Controls.Shapes;
namespace fltstd26
{
    public partial class MainPage : ContentPage
    {
        List<List<Border>> cells = [];
        List<List<VerticalStackLayout>> containers = [];


        public MainPage()
        {
            InitializeComponent();
            DskMan.Init();
            RData.Init();
        }

        public void XPlan_Restart()
        {
            TGT_LFZ_Dropdown.Items.Clear();
            List<Types.LFZ> allLFZ = RData.Handler!.GetAll<Types.LFZ>();
            foreach (Types.LFZ lfz in allLFZ)
            {
                TGT_LFZ_Dropdown.Items.Add(lfz.Reg);
            }
            //XPLAN AUFBAUEN
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
                    Text = lfz.Reg,VerticalOptions = LayoutOptions.Center,HorizontalOptions = LayoutOptions.Center,
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
                    FontSize = 20 ,
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
                    VerticalStackLayout cellContainer = [];

                    Border cellBorder = new()
                    {
                        Content = cellContainer,
                        GestureRecognizers =
                        {
                            new DropGestureRecognizer
                            {
                                AllowDrop = true,
                            }
                        },
                        Stroke = Colors.DarkGray,
                        StrokeThickness = 1,
                    };

                    containersRow.Add(cellContainer);

                    // Attach Drop event handler
                    if (cellBorder.GestureRecognizers[0] is DropGestureRecognizer dropGesture)
                    {
                        dropGesture.Drop += (s,e) => OnDrop(cellBorder,e);
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
            // Find the cell based on time and aircraft
            int rowIndex = RData.Handler!.GetAll<Types.FTS>().FindIndex(fts => fts.Equals(timeIn));
            System.Diagnostics.Debug.WriteLine($"Row Index: {rowIndex}");
            System.Diagnostics.Debug.WriteLine($"LFZ Details:\nId: {lfzIn.Id}\nReg: {lfzIn.Reg}\nType: {lfzIn.Type}\nSeats: {lfzIn.Seats}\nInterval: {lfzIn.Interval}\nPriceCat: {lfzIn.PriceCat}\nAuto: {lfzIn.AutoAssign}\nAvail: {string.Join(", ",lfzIn.AvailTimes)}\n---------");
            int colIndex = RData.Handler!.GetAll<Types.LFZ>().FindIndex(lfz => lfz.Id.Equals(lfzIn.Id));
            System.Diagnostics.Debug.WriteLine($"Col Index: {colIndex}");
            if (rowIndex == -1 || colIndex == -1) return false; //No Cell found
            Color DefCol = GSettings.GetColor("Gray100");
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
                TextColor = DefCol,
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
                TextColor = DefCol,
                Padding = new Thickness(3),
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = 16
            };

            nodegrid.Add(nodename,0,1);
            nodegrid.SetColumnSpan(nodename,2);

            Label nodelength = new()
            {
                Text = timeIn.Length.ToString() + " min",
                TextColor = DefCol,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Padding = new Thickness(20,0),
                FontSize = 16
            };
            nodegrid.Add(nodelength,0,2);

            HorizontalStackLayout hzslicon = new() {
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
                        new IconTintColorBehavior { TintColor = props[i] ? GSettings.GetColor("Primary") : GSettings.GetColor("Gray500") }
                    },
                };
                imgbtn.Clicked += NodeInteractionHandler;
                hzslicon.Add(imgbtn);
            }
            nodegrid.Add(hzslicon,1,2);

            Label nodeoid = new()
            {
                Text = tgtIn.Id.ToString(),
                TextColor = DefCol,
                Padding = new Thickness(3),
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = 16
            };

            nodegrid.Add(nodeoid,0,3);
            nodegrid.SetColumnSpan(nodeoid,2);

            XBorder node = new()
            {
                BackgroundColor = GSettings.GetColor("Background"),
                Stroke = DefCol,
                StrokeThickness = 2,
                Content = nodegrid,
                Tgt = tgtIn,
                Lfz = lfzIn,
                Fts = timeIn,
                Attrib = props
            };

            // Add drag gesture to the node
            var dragGesture = new DragGestureRecognizer();
            dragGesture.DragStarting += OnDragStarting;
           
            node.GestureRecognizers.Add(dragGesture);
            containers.ElementAt(rowIndex).ElementAt(colIndex).Children.Add(node);
            return true;
        }

        private void NodeInteractionHandler(object? sender, EventArgs e)
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
                        invokerCell.Attrib[buttonIndex] = !invokerCell.Attrib[buttonIndex];
                        btn.Behaviors.Add(new IconTintColorBehavior { TintColor = invokerCell.Attrib[buttonIndex] ? GSettings.GetColor("Primary") : GSettings.GetColor("Gray500") });
                    }
                }
            }
        }

        private void NodeSelectionHandler(object? sender,EventArgs e)
        {
            if (sender is XBorder selectedNode)
            {
                System.Diagnostics.Debug.WriteLine($"Selected node: {selectedNode.Tgt.Id}");
            }
        }


        private void OnDragStarting(object? sender,DragStartingEventArgs e)
        {
            if (sender is DragGestureRecognizer dragRecognizer && dragRecognizer.Parent is XBorder draggedNode)
            {
                e.Data.Properties["DraggedNode"] = draggedNode;
                System.Diagnostics.Debug.WriteLine($"Drag started for: {draggedNode.Content!.GetType()}",0);
            }
        }





        private static void OnDrop(Border targetCell,DropEventArgs e)
        {
            e.Data.Properties.TryGetValue("DraggedNode",out var draggedNodeObj);
            System.Diagnostics.Debug.WriteLine($"Drop event - Target: {targetCell}, Node: " + draggedNodeObj);
            if (draggedNodeObj is Border draggedNode)
            {
                // Find the source container (parent of the dragged node)
                if (draggedNode.Parent is VerticalStackLayout sourceContainer)
                {
                    // Remove from source
                    sourceContainer.Children.Remove(draggedNode);

                    // Find the target container (inside the target border)
                    var targetContainer = targetCell.Content as VerticalStackLayout;

                    // Add to target
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

        public void OpenFolder_Click(object sender,EventArgs e)
        {
            try
            {
                string folderPath = GSettings.dbpath;
                if (Directory.Exists(folderPath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = folderPath,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Directory does not exist: {folderPath}",2);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening folder: {ex.Message}",2);
            }
        }
    }

    public partial class XBorder : Border
    {
        public Types.TGT Tgt { get; set; }
        public Types.LFZ Lfz { get; set; }
        public Types.FTS Fts { get; set; }
        public Types.FLT Flt { get; private set; }

        ///<summary>quick, pin, notify, flag</summary>
        public bool[] Attrib = new bool[4];

        public void GenFLT()
        {
            Types.FLT flt = new()
            {
                Aircraft = Lfz,
                TimeSlot = Fts,
                Target = [Tgt],
            };
        }
    }
}
