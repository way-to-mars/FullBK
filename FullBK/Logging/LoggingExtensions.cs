namespace FullBK.Logging;

public static class LoggingExtensions
{
    public static string DebugLn(this string text) {
#if DEBUG
        System.Diagnostics.Debug.WriteLine(text);  
#endif
        return text;
    }

    public static string DisplayAlert(this string text, string title = "Уведомление")
    {
        _ = Shell.Current.DisplayAlert(title, text, "OK");
        return text;
    }

}
