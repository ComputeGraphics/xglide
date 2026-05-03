using CommunityToolkit.Maui.Behaviors;
using fltstd26.core;
using fltstd26.etc;
using fltstd26.Resources.Texts;
using fltstd26.system;

namespace fltstd26.XFly;

public partial class XBlock : ContentView
{
    public int TargetID = 0;
    public bool[] Attribs = new bool[4];
    public XBlock(Sheets.Target t,int Length)
    {
        InitializeComponent();
        TargetID = t.Id;
        NodeID.Text = "TGTID: " + t.Id.ToString();
        NodeName.Text = t.Name;
        NodeWeight.Text = t.Weight.ToString() + " " + Lang.xplan_weight;
        NodeLength.Text = Length == -1 ? "N/A" : (Length.ToString() + " min");
        Attribs = [t.QuickTicket,t.Persistent,false,false];

        //Image Buttons populaten ig
        for (int i = 0; i < Attribs.Length; i++)
        {
            ImageButton imgbtn = new()
            {
                BackgroundColor = Colors.Transparent,
                Source = GSettings.TargetAttribIcons[i],
                Aspect = Aspect.AspectFit,
                Behaviors =
                    {
                        new IconTintColorBehavior { TintColor = Attribs[i] ? GSettings.PrimaryColour : GSettings.InactiveIcon }
                    },
            };
            imgbtn.Clicked += NodeInteractionHandler;
            NodeIconStack.Add(imgbtn);
        }
    }

    internal void NodeInteractionHandler(object? sender,EventArgs e)
    {
        if (sender is ImageButton interaction)
        {
            int AttribIndex = NodeIconStack.Children.IndexOf(interaction);
            if (AttribIndex != -1)
            {
                ConProc.Log("[XBLOCK] Attributes of target" + TargetID.ToString() + " updated");
                Attribs[AttribIndex] = !Attribs[AttribIndex];
                interaction.Behaviors.Clear();
                interaction.Behaviors.Add(new IconTintColorBehavior { TintColor = Attribs[AttribIndex] ? GSettings.PrimaryColour : GSettings.InactiveIcon });
            }
        }
    }
}