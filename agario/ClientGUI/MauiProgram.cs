using FileLogger;
using Microsoft.Extensions.Logging;

namespace ClientGUI;

public static class MauiProgram {
    public static MauiApp CreateMauiApp() {
        var builder = MauiApp.CreateBuilder();
        builder
           .UseMauiApp<App>()
           .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
           .Services.AddLogging(configure => {
                configure.AddDebug();
                configure.AddProvider(new CustomFileLogProvider());
                configure.SetMinimumLevel(LogLevel.Debug);
            }).AddTransient<MainPage>();
        return builder.Build();
    }
}