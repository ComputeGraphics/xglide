using fltstd26.etc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.system.modals
{
    internal static class ModalPush
    {

        internal async static Task<int> Selector(string title, List<(string, string, string)> content)
        {
            if (GSettings.nav == null) return -1;
            Selector selector = new(title,content);
            await GSettings.nav!.PushModalAsync(selector);
            return await selector.ShowAndSelect();
        }

        internal async static Task<bool> Question(string Title, string Subtitle)
        {
            if (GSettings.nav == null) return false;
            YesNo question = new(Title, Subtitle);
            await GSettings.nav!.PushModalAsync(question);
            return await question.ShowAndSelect();
        }

        internal async static void Message(string Title, string Subtitle)
        {
            if(GSettings.nav == null) return;
            await GSettings.nav!.PushModalAsync(new Notification(Title,Subtitle));
            return;
        }
    }
}
