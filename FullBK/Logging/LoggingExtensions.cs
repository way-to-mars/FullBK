using static FullBK.Logging.FileLogger;

namespace FullBK.Logging;

public static class LoggingExtensions
{
    public enum Level {
        INFO = LogLevel.INFO,
        WARNING = LogLevel.WARNING,
        ERROR = LogLevel.ERROR,
        FATAL = LogLevel.FATAL,
    }

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

    public static string LogIt(this string text, Level logLevel = Level.INFO)
    {
        WriteLine(text, (FileLogger.LogLevel)logLevel);
        return text;
    }
}
