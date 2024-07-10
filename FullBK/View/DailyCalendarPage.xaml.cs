using FullBK.ViewModel;

namespace FullBK.View;

public partial class DailyCalendarPage : ContentPage
{
    DailyCalendarViewModel vm;

    readonly Animation rotation;
    public DailyCalendarPage(DailyCalendarViewModel viewModel)
	{
		InitializeComponent();

        rotation = new Animation()
            {
                { 0, 1, new Animation((x) => RadarImage.Rotation = x, 0, 360, easing: Easing.SinOut) },
            };

        vm = viewModel;
        BindingContext = viewModel;
        vm.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(vm.IsRefreshing))
            SetRefreshingAnimation(vm.IsRefreshing);
    }

    private void SetRefreshingAnimation(bool isRefreshing)
    {
        string animName = "rotate";

        var initialBgColor = RefreshIt.BackgroundColor;

        if (isRefreshing)
        {
            rotation.Commit(this, animName, 16, 1000, Easing.Linear,
                (value, flag) =>
                {
                    RadarImage.Rotation = 0;
                    RefreshIt.BackgroundColor = initialBgColor;
                },
                () => true
                );
        }
        else
        {
            this.AbortAnimation(animName);
        }

    }

}