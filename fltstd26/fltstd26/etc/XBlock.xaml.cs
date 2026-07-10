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
    public string Name;
    public XBlock(Sheets.Target t,int l)
    {
        InitializeComponent();
        TargetID = t.Id;
        Length = l;
        //NodeID.Text = "TGTID: " + t.Id.ToString();
        NodeName.Text = t.Name ?? "N/A";
        Name = t.Name ?? "";
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
            ConProc.Log($"[XBLOCK] Attribute {id} of target {TargetID} updated");
            AttribAction(id);
            tint.TintColor = Attribs[id] ? GSettings.PrimaryColour : GSettings.InactiveIcon;
        }
    }

    //Wenn eine Slot STime erreicht wird werden alle Targets bei Attrib 2 disabled!!
    internal void DisableAttrib(int id)
    {
        if (id >= 0 && id < Attribs.Length && NodeIconStack.Children[id] is ImageButton interaction && interaction.Behaviors[0] is IconTintColorBehavior tint)
        {
            ConProc.Log($"[XBLOCK] Attribute {id} of target {TargetID} disabled");
            interaction.IsEnabled = false;
            tint.TintColor = Colors.Transparent;
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
                    RData.UpdateProperty<bool>(TargetID,!Attribs[id],"QuickTicket",typeof(Sheets.Target));
                    break;
                case 1:
                    RData.UpdateProperty<bool>(TargetID,!Attribs[id],"Persistent",typeof(Sheets.Target));
                    if (GestureRecognizers[0] is DragGestureRecognizer d) d.CanDrag = Attribs[id];
                    break;
                case 2:
                    //Notify
                    /*if (Attribs[id]) TimeServ.Unschedule(NotifierID);
                    else TimeServ.Schedule(DateTime.Now.Add(TimeSpan.FromSeconds(5)),() =>
                    {
                        system.modals.ModalPush.Message(Lang.notification,$"- {NodeName.Text} ({TargetID}) -\r\n{Lang.ticket_notification}");
                        DisableAttrib(2);
                    });*/
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