using fltstd26.core;
using fltstd26.etc;
using fltstd26.Resources.Texts;
using fltstd26.system;
using fltstd26.system.modals;
using System.Threading.Tasks;

namespace fltstd26.assistant.profiles;

public partial class ProfileCreator : ContentPage
{
    private int Step = 0;
    private readonly string DB;
    private List<Sheets.Slot> slots = [];
    private List<Sheets.PriceCat> prccs = [];

    public ProfileCreator(string db)
    {
        InitializeComponent();
        DB = db;
        RData.Init(db);
        Refresh();
    }



    private void Refresh()
    {
        FlexContainer.Clear();
        slots = RData.GetSlotsTable();
        prccs = RData.GetPriceTable();
        List<EntityView> entities = [];
        switch (Step)
        {
            case 0:
                //Price Cats
                ProfileTitle.Text = Lang.pricecustomizer;
                foreach (Sheets.PriceCat pc in prccs)
                {
                    void delete0()
                    {
                        RData.Delete(pc.Id,typeof(Sheets.PriceCat));
                    }
                    async void modify0()
                    {
                        PriceCatCreator pcc = new(pc);
                        await Navigation.PushModalAsync(pcc);
                        await pcc.ShowAndSelect().ContinueWith(pc =>
                        {
                            if (pc.Result != null)
                            {
                                RData.Update(pc.Result,typeof(Sheets.PriceCat));
                            }
                        });
                    }
                    EntityView ev = new("price.png",pc.Name ?? "N/A",GSettings.UnformatPrice(pc.Price),null,(delete0, modify0));
                    entities.Add(ev);
                }
                break;
            case 1:
                //Slots
                ProfileTitle.Text = Lang.slotcustomizer;
                foreach (Sheets.Slot fts in slots)
                {
                    void delete1()
                    {
                        RData.Delete(fts.Id,typeof(Sheets.Slot));
                    }
                    async void modify1()
                    {
                        SlotCreator sc = new(fts);
                        await Navigation.PushModalAsync(sc);
                        await sc.ShowAndSelect().ContinueWith(sc =>
                        {
                            if (sc.Result != null)
                            {
                                //System.Diagnostics.Debug.WriteLine(sc.Result.STime.ToString("G"));
                                RData.Update(sc.Result,typeof(Sheets.Slot));
                            }
                        });
                    }
                    EntityView ev = new("slot.png",$"{fts.STime:G}\r\n-{fts.FTime:G}" ?? "N/A",$"{fts.Length} min",null,(delete1, modify1));
                    entities.Add(ev);
                }
                break;
            case 2:
                ProfileTitle.Text = Lang.accustomizer;
                List<Sheets.Lfz> lfzs = RData.GetAircraftTable();
                foreach (Sheets.Lfz lfz in lfzs)
                {
                    void delete2()
                    {
                        RData.Delete(lfz.Id,typeof(Sheets.Lfz));
                    }
                    async void modify2()
                    {
                        AircraftCreator acc = new(slots,prccs,lfz);
                        await Navigation.PushModalAsync(acc);
                        await acc.ShowAndSelect().ContinueWith(ac =>
                        {
                            if (ac.Result != null)
                            {
                                //Id ist bullshit
                                
                                System.Diagnostics.Debug.WriteLine("Changed ID: " + ac.Id.ToString());
                                RData.Update(ac.Result,typeof(Sheets.Lfz));
                            }
                        });
                    }
                    EntityView ev = new("plane.png",lfz.Reg ?? "N/A",lfz.Type ?? "N/A",$"{Lang.weight_doubledot} {lfz.Seats}\r\n{Lang.flight_interval_doubledot} {lfz.Interval}\r\n{Lang.price_doubledot} {lfz.PriceCat}\r\n{Lang.autoassign}: {lfz.AutoAssign}\r\n{lfz.OGN ?? "OGN AUTO"}",(delete2, modify2));
                    entities.Add(ev);
                }
                break;



        }
        entities.ForEach(FlexContainer.Add);
    }

    private async void AddClicked(object sender,EventArgs e)
    {
        switch (Step)
        {
            case 0:
                PriceCatCreator pcc = new(null);
                await Navigation.PushModalAsync(pcc);
                Sheets.PriceCat? returnpc = null;
                await pcc.ShowAndSelect().ContinueWith(pc => returnpc = pc.Result);
                if (returnpc != null)
                {
                    RData.Insert(returnpc,typeof(Sheets.PriceCat));
                    Refresh();
                }
                break;
            case 1:
                if (FlexContainer.Children.Count > 254) break;
                SlotCreator sc = new(null);
                await Navigation.PushModalAsync(sc);
                Sheets.Slot? returnsc = null;
                await sc.ShowAndSelect().ContinueWith(sc => returnsc = sc.Result);
                if (returnsc != null)
                {
                    RData.Insert(returnsc,typeof(Sheets.Slot));
                    Refresh();
                }
                break;
            case 2:
                AircraftCreator acc = new(slots,prccs,null);
                await Navigation.PushModalAsync(acc);
                Sheets.Lfz? returnac = null;
                await acc.ShowAndSelect().ContinueWith(ac => returnac = ac.Result);
                if (returnac != null)
                {
                    RData.Insert(returnac,typeof(Sheets.Lfz));
                    Refresh();
                }
                break;
        }


    }

    private async void RequestDBClick(object sender, EventArgs e)
    {
        RData.Close();
        List<IFile> profiles = DskMan.GetFolder(false);
        List<(string, string, string)> items = [..profiles.Select(file => ("db.png", file.Name.Replace(".sqlite",string.Empty), file.Context))];
        int index = -1;
        //await ModalPush.Selector(Lang.profile_choose,items).ContinueWith(t => index = t.Result);
        Selector slc = new(Lang.profile_choose,items);
        await Navigation.PushModalAsync(slc);
        await slc.ShowAndSelect().ContinueWith(t => index = t.Result);

        if (index != -1)
        {
            RData.Init(profiles[index].Location);
            switch (Step)
            {
                case 0:
                    List<Sheets.PriceCat> pcs = RData.GetPriceTable();
                    RData.Close();
                    RData.Init(DB);
                    RData.InsertRange(pcs);
                    break;
                case 1:
                    List<Sheets.Slot> sts = RData.GetSlotsTable();
                    RData.Close();
                    RData.Init(DB);
                    RData.InsertRange(sts);
                    break;
                case 2:
                    List<Sheets.Lfz> acs = RData.GetAircraftTable();
                    foreach(Sheets.Lfz ac in acs)
                    {
                        ac.AvailTimes = [];
                        ac.PriceCat = 0;
                    }
                    RData.Close();
                    RData.Init(DB);
                    RData.InsertRange(acs);
                    break;
            }
        }
    }

    private void BackwardClick(object sender,EventArgs e)
    {
        if (Step != 0)
        {
            Step--;
            Refresh();
        }
    }

    private void ForwardClick(object sender,EventArgs e)
    {
        //1 weniger als steps
        if(Step < 2)
        {
            Step++;
            Refresh();
        }
        else
        {
            Application.Current?.CloseWindow(Window);
            ModalPush.Message(Lang.notification,Lang.profile_ready);
        }
    }

    private void RefreshClick(object sender,EventArgs e) => Refresh();
}