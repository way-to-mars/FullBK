using CommunityToolkit.Mvvm.ComponentModel;

namespace FullBK.CustomView.ViewModel;

public partial class ConnectivityViewModel : ObservableObject
{
    IConnectivity connectivity;

    [ObservableProperty]
    string connectivityName;

    public ConnectivityViewModel()
    {
        this.connectivity = Application.Current?.Handler.MauiContext?.Services.GetService<IConnectivity>() ?? Connectivity.Current;
        this.connectivity.ConnectivityChanged += (s, e) => ConnectivityName = GetConnectivityName(e.NetworkAccess);

        // Initialize with the current state
        ConnectivityName = GetConnectivityName(connectivity.NetworkAccess);
    }

    private string GetConnectivityName(NetworkAccess networkAccess) => networkAccess.ToString();
}
