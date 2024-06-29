using FullBK.CustomView;
using FullBK.View;
using FullBK.ViewModel;
using Microsoft.Extensions.Logging;

namespace FullBK;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Better-VCR.ttf", "BetterVCR");
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif

        // Used in CustomView/ConnectivityView
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<GroupingCollectionViewModel>();
        builder.Services.AddSingleton<GroupingCollectionView>();

        builder.Services.AddSingleton<RewardsViewModel>();
        builder.Services.AddSingleton<RewardsPage>();


        return builder.Build();
    }
}