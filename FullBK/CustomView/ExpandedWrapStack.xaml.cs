using FullBK.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace FullBK.CustomView;

public partial class ExpandedWrapStack : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(ExpandedWrapStack), string.Empty, propertyChanged: OnTextPropertyChanged);

    public static readonly BindableProperty IsExpandedProperty =
        BindableProperty.Create(nameof(Title), typeof(bool), typeof(ExpandedWrapStack), true, propertyChanged: OnIsExpandedPropertyChanged);

    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public bool IsExpanded
    {
        get { return (bool)GetValue(IsExpandedProperty); }
        set
        {
            SetValue(IsExpandedProperty, value);
            SetIcons(value);
        }
    }

    public ExpandedWrapStack()
	{
		InitializeComponent();

        // Provide a tapping recognizer for the title bar
        TapGestureRecognizer tapGestureRecognizer = new();
        tapGestureRecognizer.Tapped += OnExpanderTapped;
        HeaderGrid.GestureRecognizers.Add(tapGestureRecognizer);

        // Implement initial state
        SetIcons(IsExpanded);

        // auto adding and then removing items (buttons)
        // this.Loaded += ExpandedWrapStack_Loaded;
    }

    private void ExpandedWrapStack_Loaded(object? sender, EventArgs e)
    {
        if (sender is not ExpandedWrapStack) return;

        "Start async adding".DebugLn();
        ExpandedWrapStack expandedWrapStack = (ExpandedWrapStack)sender;

        Task.Run(async () => {
            Random random = new Random();

            int count = 0;
            while (count < 7)
            {
                await Task.Delay(TimeSpan.FromSeconds(random.Next(1, 7)));
                count++;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Button button = new Button()
                    {
                        Text = $"NEW-{count}",
                        FontSize = 32.0,
                        Padding = new Thickness(10, 80),
                        Margin = new Thickness(10),
                        WidthRequest = 200,
                    };
                    button.Clicked += Button_Clicked;

                    // expandedWrapStack.ItemsHolder.Add(button);
                    expandedWrapStack.AddChild(button);
                });
            }

            while (count > 3)
            {
                await Task.Delay(TimeSpan.FromSeconds(random.Next(1, 7)));
                count--;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    int index = random.Next(0, ItemsHolder.Count);
                    expandedWrapStack.RemoveChildAt(index);
                });
            }

        });
    }

    private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        $"{bindable}: oldValue={oldValue} newValue={newValue}".DebugLn();
    }

    private static async void OnIsExpandedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        $"{bindable}: oldValue={oldValue} newValue={newValue}".DebugLn();

        if (bindable is ExpandedWrapStack expandedWrapStack && newValue is bool state)
        {
            await expandedWrapStack.ExpandingAnimation(state, 250);
        }
    }

    private void SetIcons(bool state)
    {
        if (state)
        {
            RightIcon.SetAppTheme<ImageSource>(Microsoft.Maui.Controls.Image.SourceProperty, "arrow_down_black.png", "arrow_down_white.png");
            LeftIcon.SetAppTheme<ImageSource>(Microsoft.Maui.Controls.Image.SourceProperty, "arrow_down_black.png", "arrow_down_white.png");
        }
        else
        {
            RightIcon.SetAppTheme<ImageSource>(Microsoft.Maui.Controls.Image.SourceProperty, "arrow_left_black.png", "arrow_left_white.png");
            LeftIcon.SetAppTheme<ImageSource>(Microsoft.Maui.Controls.Image.SourceProperty, "arrow_right_black.png", "arrow_right_white.png");
        }

    }

    private async Task ExpandingAnimation(bool state, uint duration)
    {
        var it = ItemsHolder;
        it.CancelAnimations();
        it.AnchorY = 0;
        if (state)
        {
            it.ScaleY = 0;
            it.Opacity = 0;
            it.IsVisible = true;
            await Task.WhenAll(
                it.ScaleYTo(1f, duration, Easing.SinOut),
                it.FadeTo(1f, duration, Easing.SinOut)
            );
        }
        else
        {
            await Task.WhenAll(
                it.ScaleYTo(0f, duration, Easing.SinOut),
                it.FadeTo(0f, duration, Easing.SinOut)
            );
            it.IsVisible = false;
            it.ScaleY = 1;  // restore
            it.Opacity = 1;  // restore
        }
    }

    void OnExpanderTapped(object? sender, TappedEventArgs args)
    {
        // Handle the tap
        "Expander Tapped".DebugLn();
        IsExpanded = !IsExpanded;
    }

    private async void Button_Clicked(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var a1 = button.FadeTo(0.25, 100);
            var a2 = button.ScaleTo(1.1, 250);

            await a1;
            var a3 = button.FadeTo(1, 100);

            await a3;
            var a4 = button.ScaleTo(1, 250);

            await a4;
        }
    }


    /// <summary>
    /// Adds a child view using animation to the end of the container.
    /// </summary>
    /// <param name="child">
    /// The child view to add.
    /// </param>
    public async void AddChild(VisualElement child)
    {
        uint duration = 250;

        child.Scale = 0;
        child.Opacity = 0;
        child.IsVisible = true;

        ItemsHolder.Add(child);

        await Task.WhenAll(
            child.ScaleTo(1f, duration, Easing.SinOut),
            child.FadeTo(1f, duration, Easing.SinOut)
        );
    }


    private async Task RemovingAnimation(VisualElement child, uint duration = 250)
    {
        child.CancelAnimations();
        await Task.WhenAll(
            child.ScaleTo(0f, duration, Easing.SinOut),
            child.FadeTo(0f, duration, Easing.SinOut)
        );
    }

    public async void RemoveChild(VisualElement child)
    {
        await RemovingAnimation(child, 250);

        if (ItemsHolder.Remove(child)) return;

        // restore item
        child.Scale = 1;
        child.Opacity = 1;
        child.IsVisible = true;
    }

    public async void RemoveChildAt(int index)
    {
        try
        {
            VisualElement[] children = ItemsHolder.Children.Select(view => (VisualElement)view).ToArray();

            if (children.Length > 0 && index >= 0 && index < children.Length)
            {
                var child = children[index];
                await RemovingAnimation(child, 250);
                ItemsHolder.RemoveAt(index);

                // restore item ???
                child.Scale = 1;
                child.Opacity = 1;
                child.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ex.Message.DisplayAlert(title: "Ошибка удаления элемента RemoveChildAt");
        }
    }
}