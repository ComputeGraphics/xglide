using fltstd26.board;
using fltstd26.core;
using fltstd26.etc;
using fltstd26.Resources.Texts;
namespace fltstd26.XFly
{
    internal class Drawer
    {

        internal static async Task<int> AskForPriceUpdate(int Price,int? LFZ_PriceCat,string Name)
        {
            int pcat = 0;
            if (USettings.Instance.AskForNodePriceChange)
            {
                PriceCustomizer pc = new(Price,Name);
                await GSettings.nav!.PushModalAsync(pc);
                await pc.ShowAndSelect().ContinueWith(r =>
                {
                    pcat = r.Result switch
                    {
                        null => 0,
                        0 => -(LFZ_PriceCat ?? USettings.Instance.FallbackPriceCat),
                        _ => r.Result.Value,
                    };
                });
            }
            return pcat;
        }

        internal static void CallTargets(IEnumerable<Sheets.Target> tgts)
        {
            Color bg = GSettings.DarkMode ? GSettings.GetColour("SecondaryDark") : GSettings.GetColour("SecondaryBg");
            Color fg = GSettings.DarkMode ? GSettings.GetColour("White") : GSettings.GetColour("Black");
            string tgt = string.Join(", ",tgts.Select(x => x.Name ?? x.Id.ToString()));
            BoardController.PushNotification(TimeSpan.FromSeconds(30),"notification.png",Lang.call_target,tgt,bg,fg,true);
        }
    }
}
