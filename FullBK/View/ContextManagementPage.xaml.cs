using FullBK.ViewModel;

namespace FullBK.View;

public partial class ContextManagementPage : ContentPage
{
	public ContextManagementPage(ContextManagementViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;

        viewModel.messages.CollectionChanged += Messages_CollectionChanged;
	}

    private void Messages_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems is not null)
            foreach (var message in e.NewItems)
            {
                var editor = new Editor()
                {
                    Text = (string)message,
                    IsReadOnly = true
                };
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MsgHolder.Add(new Frame()
                    {
                        Padding = new Thickness(10),
                        Content = editor
                    });
                });
            }
    }
}