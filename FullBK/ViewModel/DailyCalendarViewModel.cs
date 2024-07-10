using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FullBK.Logging;
using FullBK.Model;
using FullBK.Services;
using FullBK.Services.DAO;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FullBK.ViewModel;

public partial class DailyCalendarViewModel : ObservableObject
{
    [ObservableProperty]
    bool isRefreshing;

    DomainService domainService;

    public ObservableCollection<DailyTaskSimpleModel> DailyTasks { get; } = new();

    public DailyCalendarViewModel(DomainService domainService)
    {
        this.domainService = domainService;
    }

    [RelayCommand]
    async Task RefreshAsync()
    {
        IsRefreshing = true;
        Debug.WriteLine("Refresh ICommand");

        var response = await domainService.MonthlyList();

        if (response.HasResult)
        {
            if (response.ErrorCode == 1) "Для получения информации о статусе полученных наград необходимо авторизоваться".DisplayAlert("Вы не авторизованы");

            var newTasks = response.Result;

            DailyTasks.Clear();
            foreach (var task in newTasks)
            {
                await Task.Delay(100);
                DailyTasks.Add(DailyTaskSimpleModel.From(task));
            }
        }
        else
        {
            response.NotesToString().DisplayAlert("Ошибка обновления списка");
        }

        IsRefreshing = false;
    }
}
