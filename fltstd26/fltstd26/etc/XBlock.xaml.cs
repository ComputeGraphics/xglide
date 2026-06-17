using CommunityToolkit.Maui.Behaviors;
using fltstd26.core;
using fltstd26.etc;
using fltstd26.Resources.Texts;
using fltstd26.system;

namespace fltstd26.XFly;

public partial class XBlock : ContentView
{
    public int TargetID;
    public int Length;
    public bool[] Attribs;
    private Scheduler? notifier = null;
    public XBlock(Sheets.Target t,int l)
    {
        InitializeComponent();
        TargetID = t.Id;
        Length = l;
        //NodeID.Text = "TGTID: " + t.Id.ToString();
        NodeName.Text = t.Name;
        NodeWeight.Text = t.Weight.ToString() + " " + Lang.xplan_weight;
        NodeLength.Text = l == -1 ? "N/A" : (l.ToString() + " min");
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

    private void NodeInteractionHandler(object? sender,EventArgs e)
    {
        if (sender is ImageButton interaction)
        {
            int AttribIndex = NodeIconStack.Children.IndexOf(interaction);
            if (AttribIndex != -1) UpdateAttrib(AttribIndex);
        }
    }

    internal void UpdateAttrib(int id)
    {
        if (id >= 0 && id < Attribs.Length && NodeIconStack.Children[id] is ImageButton interaction && interaction.Behaviors[0] is IconTintColorBehavior tint)
        {
            ConProc.Log("[XBLOCK] Attributes of target " + TargetID.ToString() + " updated");
            AttribAction(id);
            tint.TintColor = Attribs[id] ? GSettings.PrimaryColour : GSettings.InactiveIcon;
        }
    }

    internal void AttribAction(int id)
    {
        if (!RData.Active()) return;
        try
        {
            switch (id)
            {
                case 0:
                    RData.UpdateProperty<Sheets.Target,bool>(TargetID,!Attribs[id],"QuickTicket");
                    break;
                case 1:
                    RData.UpdateProperty<Sheets.Target,bool>(TargetID,!Attribs[id],"Persistent");
                    if(GestureRecognizers[0] is DragGestureRecognizer d) d.CanDrag = Attribs[id];
                    break;
                case 2:
                    //Notify
                    if(!notifier?.IsRunning ?? true)
                    {
                        notifier = new(TimeSpan.FromSeconds(5),(s,e) =>
                        {
                            system.modals.ModalPush.Message("Test","Notifier Test");
                        },false);
                    }
                    else
                    {
                        notifier?.Terminate();
                    }
                    break;
                case 3:
                    //Flag
                    NodeFrame.Stroke = Attribs[3] ? GSettings.NodeBackgroundColour : GSettings.PrimaryColour;
                    break;
            }
            Attribs[id] = !Attribs[id];
        }
        catch (Exception ex)
        {
            ConProc.Log("[XBLOCK] Attribute update of  " + TargetID.ToString() + " failed: " + ex.Message,2);
        }
    }
}