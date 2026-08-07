using fltstd26.core;
using fltstd26.etc;
using fltstd26.Resources.Texts;
using fltstd26.system;
using fltstd26.system.modals;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

namespace fltstd26.assistant;

public partial class FileManager : ContentPage
{
    List<IFile> files = [];
    int SelectedFile = -1;

    private WindowLock? wl = null;
    private Window? editor = null;

    private readonly bool Config;
    private readonly bool PopOnCont;
    public FileManager(bool c, bool poponcont)
    {
        PopOnCont = poponcont;
        Config = c;
        if(c && !RData.Active()) RData.Init();
        InitializeComponent();
        WindowTitle.Text = c ? Lang.config_manager : Lang.profile_manager;
        WindowSubtitle.Text = c ? Lang.config_select : Lang.profile_select;
        Refresh();
    }

    private void Refresh()
    {
        FileView.Clear();
        SelectedFile = -1;
        RemoteButton.IsVisible = !Config;
        RemoteButton.IsEnabled = USettings.Instance.AllowNEXUS;
        RemoteButton.TextColor = USettings.Instance.AllowNEXUS ? Colors.Black : GSettings.GetColour("Gray600");

        files = DskMan.GetFolder(Config);
        foreach (IFile file in files)
        {

            void view() { if (!RData.Locked) Application.Current?.OpenWindow(new debug.DBPreview(file.Location)); }

            Action modify = Config ?
                () => 
                {
                    if (editor == null && !RData.Locked)
                    {
                        editor = new(new config.ConfigSettings(file.Location,file.Name));
                        Application.Current?.OpenWindow(editor);
                        LockWindow();
                    }
                }
            : () =>
            {
                if (editor == null && !RData.Locked)
                {
                    editor = new(new profiles.ProfileCreator(file.Location));
                    Application.Current?.OpenWindow(editor);
                    LockWindow();
                }
            };
            void share()
            {
                Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = file.Name,
                    File = new ShareFile(file.Location)
                });
            }
            void delete()
            {
                if (!RData.Locked) DskMan.Delete(file.Name,Config);
                Refresh();
            }
            ListTile lt = new(true,Config ? "file.png" : "db.png",Config ? file.Name.Replace(".xml",string.Empty) : file.Name.Replace(".sqlite",string.Empty),file.Context,[null,Config ? null : view,modify,share,delete]);
            lt.Checked.CheckedChanged += CheckedChanged;
            FileView.Add(lt);
        }

    }

    private void CheckedChanged(object? sender, EventArgs e)
    {
        if(sender != null && sender is CheckBox cb)
        {
            List<ListTile> children = [..FileView.Children.OfType<ListTile>()];
            int newindex = children.FindIndex(x => x.Checked.Id == cb.Id);
            if(newindex != -1)
            {
                if (SelectedFile != -1 && SelectedFile < children.Count)
                {  
                    if (!cb.IsChecked)
                    {
                        SelectedFile = -1;
                        return;
                    }
                    else children[SelectedFile].Checked.IsChecked = false;
                }
                SelectedFile = newindex;
            }

        }
    }

    private void OpenClick(object sender,EventArgs e) => DskMan.OpenFolder(false,DskMan.IAppDataFolders[Config ? 1 : 0]);
    private void RemoteClick(object sender,EventArgs e) => System.Diagnostics.Debug.WriteLine("Not implemented");
    private void RefreshClick(object sender,EventArgs e) => Refresh();
    private async void PlusClick(object sender,EventArgs e)
    {
        if(Config)
        {

        }
        else
        {
            string? res = "";
            await ModalPush.Entry(Lang.new_db,Lang.new_db_name,Lang.xplan_name,null).ContinueWith(x => res = x.Result);
            if (res != null && editor == null && !RData.Locked)
            {
                editor = new(new profiles.ProfileCreator(Path.Combine(GSettings.Paths["Database"],res + ".sqlite")));
                Application.Current?.OpenWindow(editor);
                LockWindow();
            }
        }
    }

    private void ContinueClick(object sender, EventArgs e)
    {
        if(SelectedFile != -1 && SelectedFile < files.Count)
        {
            System.Diagnostics.Debug.WriteLine("Selected File: " + SelectedFile.ToString());
            if (Config)
            {
                USettings.ConfigName = files[SelectedFile].Location;
                DskMan.LoadConfig(files[SelectedFile].Location,true);
                Navigation.PopToRootAsync(true);
            }
            else
            {
                RData.Close();
                RData.DatabaseFilename = files[SelectedFile].Name;
                if (PopOnCont)
                {
                    EditorClosed(null,null);
                    Navigation.PopAsync(true);
                }
                Navigation.PushAsync(new FileManager(true,false));
            }
        }
    }

    private void LockWindow()
    {
        wl = new(null,null);
        if(editor != null) editor.Destroying += EditorClosed;
        GSettings.nav?.PushModalAsync(wl);
    }

    private void EditorClosed(object? sender,EventArgs? e)
    {
        if(!Config) RData.Close();
        editor = null;
        wl?.ReleaseLock();
    }
}