using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FullBK.Services;
using System.Diagnostics;

namespace FullBK.ViewModel;

public partial class RewardsViewModel : ObservableObject
{
    [ObservableProperty]
    bool isRefreshing;

    DomainService domainService;

    public RewardsViewModel(DomainService domainService)
    {
        this.domainService = domainService;
    }


    [RelayCommand]
    async Task RefreshAsync() {
        IsRefreshing = true;
        Debug.WriteLine("Refresh ICommand");
        await Task.Delay(5000);
        IsRefreshing = false;
    }

}
