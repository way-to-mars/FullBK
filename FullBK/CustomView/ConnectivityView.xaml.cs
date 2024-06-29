using System.Diagnostics;

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
        var logger = (string str) => { Debug.WriteLine($"[ConnectivityView]({this.GetHashCode()}) {str}"); };

        InitializeComponent();

        LogEvents(this, logger);

        this.Loaded += (sender, args) =>
        {
            //await Task.Delay(1000);    // Additional delay. Waiting for MAUI and Connectivity to be finally initialized
            logger("Lazy Init asked");
            _ = SetupConnectivityAsync();
            logger("Lazy Init finished");
        };
    }



    private static void LogEvents(ContentView view, Action<string> logger)
    {
        view.BindingContextChanged += (sender, e) => { logger($"BindingContextChanged. {e.GetType}; {e}"); };
        view.ChildAdded += (sender, e) => { logger($"ChildAdded. {e.GetType}; {e}"); };
        view.DescendantAdded += (sender, e) => { logger($"DescendantAdded. {e.GetType}; {e}"); };
        view.Focused += (sender, e) => { logger($"Focused. {e.GetType}; {e}"); };
        view.HandlerChanged += (sender, e) => { logger($"HandlerChanged. {e.GetType}; {e}"); };
        view.LayoutChanged += (sender, e) => { logger($"LayoutChanged. {e.GetType}; {e}"); };
        view.Loaded += (sender, e) => { logger($"Loaded. {e.GetType}; {e}"); };
        view.PropertyChanged += (sender, e) => { logger($"PropertyChanged. {e.GetType}; {e}"); };
        view.SizeChanged += (sender, e) => { logger($"SizeChanged. {e.GetType}; {e}"); };
        view.Unfocused += (sender, e) => { logger($"Unfocused. {e.GetType}; {e}"); };
        view.Unloaded += (sender, e) => { logger($"Unloaded. {e.GetType}; {e}"); };
    } 

    private async Task SetupConnectivityAsync()
    {
        IConnectivity? conn = null;
        while (true)
        {
            try
            {
                var cur = Application.Current;
                if (cur is null) continue;

                var context = cur.Handler.MauiContext;
                if (context is null) continue;

                conn = context.Services.GetService<IConnectivity>();
                if (conn != null) break;
            }
            catch
            {
                Debug.WriteLine("!");
                await Task.Delay(100);
            }
        }
        // Initialize with the current state
        await MainThread.InvokeOnMainThreadAsync(() => { ApplyConnectivityName(conn.NetworkAccess); });

        conn.ConnectivityChanged += (s, e) => ApplyConnectivityName(e.NetworkAccess);
    }

    private void ApplyConnectivityName(NetworkAccess networkAccess) => Description.Text = networkAccess.ToString();
}