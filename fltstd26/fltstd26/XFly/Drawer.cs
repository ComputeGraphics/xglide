using fltstd26.core;
using fltstd26.etc;
using static SQLite.SQLite3;
using static System.Net.WebRequestMethods;
namespace fltstd26.XFly
{
    internal class Drawer
    {

        internal static async Task<int> AskForPriceUpdate(int Price,int? LFZ_PriceCat,string Name)
        {
            int pcat = 0;
            if (USettings.AskForNodePriceChange)
            {
                PriceCustomizer pc = new(Price,Name);
                await GSettings.nav!.PushModalAsync(pc);
                await pc.ShowAndSelect().ContinueWith(r =>
                {
                    pcat = r.Result switch
                    {
                        null => 0,
                        0 => -(LFZ_PriceCat ?? USettings.FallbackPriceCat),
                        _ => r.Result.Value,
                    };
                });
            }
            return pcat;
        }

    }
}
