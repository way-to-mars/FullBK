using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace FullBK.ViewModel;

public partial class RewardsViewModel : ObservableObject
{
    [ObservableProperty]
    bool isRefreshing;


    public RewardsViewModel()
    {
    
    }


    [RelayCommand]
    async Task RefreshAsync() {
        IsRefreshing = true;

        Debug.WriteLine("Refresh ICommand");
        await Task.Delay(5000);

        IsRefreshing = false;
    }

}
