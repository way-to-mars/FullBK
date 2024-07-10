using FullBK.CustomView;
using FullBK.Services;
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
                fonts.AddFont("RobotoCondensed-Regular.ttf", "RobotoCondensed");
                fonts.AddFont("RobotoCondensed-Bold.ttf", "RobotoCondensedBold");                
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif

        // Used in CustomView/ConnectivityView
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<DomainService>();
        builder.Services.AddSingleton<DailyTaskService>();
        builder.Services.AddSingleton<ContextProvider>();

        builder.Services.AddSingleton<GroupingCollectionViewModel>();
        builder.Services.AddSingleton<GroupingCollectionView>();

        builder.Services.AddSingleton<RewardsViewModel>();
        builder.Services.AddSingleton<RewardsPage>();

        builder.Services.AddSingleton<ContextManagementPage>();
        builder.Services.AddSingleton<ContextManagementViewModel>();

        builder.Services.AddSingleton<DailyCalendarViewModel>();
        builder.Services.AddSingleton<DailyCalendarPage>();

        return builder.Build();
    }
}