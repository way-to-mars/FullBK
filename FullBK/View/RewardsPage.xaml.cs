using FullBK.ViewModel;
using System.Diagnostics;

namespace FullBK.View;

public partial class RewardsPage : ContentPage
{
    RewardsViewModel vm;

    readonly Animation rotation;

    public RewardsPage(RewardsViewModel viewModel)
	{
		InitializeComponent();

        rotation = new Animation(value => 
            RadarImage.Rotation = value,
            0,
            360,
            Easing.Linear
            );

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

        if (isRefreshing)
        {
            // ANIMATE
            rotation.Commit(this, animName, 16, 1000, Easing.Linear,
                (value, flag) => RadarImage.Rotation = 0,
                () => true
                );
        }
        else
        {
            // stop animation
            this.AbortAnimation(animName);
        }
    
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
		Button button = (Button)sender;

		
		var a1 = button.FadeTo(0.25, 100);
        var a2 = button.ScaleTo(1.1, 250);

        await a1;
        var a3 = button.FadeTo(1, 100);

        await a3;
        var a4 = button.ScaleTo(1, 250);

        await a4;
    }
}