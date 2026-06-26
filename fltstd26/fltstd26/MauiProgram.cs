using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace fltstd26
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf","OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf","OpenSansSemibold");
                    fonts.AddFont("Archive.otf","Archive");
                    fonts.AddFont("square_sans_serif_7.ttf","SquareSans");
                    fonts.AddFont("ZenDots-Regular.ttf","ZenDots");
                })
                .UseMauiCommunityToolkit();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
