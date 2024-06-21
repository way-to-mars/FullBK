using FullBK.CustomView.ViewModel;

namespace FullBK.CustomView;

public partial class ConnectivityView : ContentView
{
    public double LabelFontSize { set => Description.FontSize = value; }

    public double IconSize
    {
        set
        {
            Icon.WidthRequest = value;
            Icon.HeightRequest = value;
        }
    }

    public ConnectivityView()
    {
        InitializeComponent();
        BindingContext = Application.Current?.Handler.MauiContext?.Services.GetService<ConnectivityViewModel>() ?? new ConnectivityViewModel();
    }
}