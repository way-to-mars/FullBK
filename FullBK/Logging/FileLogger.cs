using System.Collections.Concurrent;
using System.Text;

namespace FullBK.Logging;

public static class FileLogger
{
    public static readonly bool SwitchOn = true;  // FileLogger becomes dummy when false
    private static readonly FileHolder? fHolder = null;
    private static readonly BlockingCollection<string>? LogQueue = null;
    private static readonly Thread? WritingThread = null;
    private static bool Disposed;
    private static string Header => $"[{DateTime.Now:G}]";

    public enum LogLevel {
        INFO = 0,
        WARNING = 1,
        ERROR = 2,
        FATAL = 3,
    }

    /// <summary>
    /// Writes to log file. This methos is static. You don't need to initialize logger.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="level"></param>
    public static void WriteLine(string message, LogLevel level = LogLevel.INFO)
    {
        if (!SwitchOn) return;
        if (Disposed) return;
        if (LogQueue is null) return;
        if (LogQueue.IsCompleted) return;
        try
        {
            string lvl = level switch {
                LogLevel.INFO => nameof(LogLevel.INFO),
                LogLevel.WARNING => nameof(LogLevel.WARNING),
                LogLevel.ERROR => nameof(LogLevel.ERROR),
                LogLevel.FATAL => nameof(LogLevel.FATAL),
                _ => "?"
            };

            LogQueue?.Add($"{Header} [{lvl}] {message}\n");
        }
        catch (Exception) { }
    }

    static FileLogger()
    {
        try
        {
            Disposed = false;
            fHolder = new();
            LogQueue = new BlockingCollection<string>(100);

            WritingThread = new Thread(fHolder.InfiniteWriter)
            {
                Name = "Infinite Log Writer",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            WritingThread.Start();
        }
        catch (Exception)
        {
            Dispose();
        }
    }


    public static void Dispose()
    {
        Disposed = true;
        try
        {
            LogQueue?.CompleteAdding();
        }
        catch (ObjectDisposedException) { /* ignore */ }
        try
        {
            WritingThread?.Join();  // wait until it stops
        }
        catch (Exception){ /* ignore */ }
        fHolder?.Dispose();
    }

    internal class FileHolder : IDisposable
    {
        private FileStream? _stream = null;
        private static readonly object __lock = new();

        public FileStream? Stream
        {
            get
            {
                lock (__lock)
                {
                    _stream ??= CreateLogFileStream();
                }
                return _stream;
            }
        }

        private static FileStream? CreateLogFileStream()
        {
            static string TempName(string prefix)
            {
                Int32 UnixTimestamp = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                string name = $"{prefix}{UnixTimestamp:X}.txt";

                return Path.Combine(Path.GetTempPath(), name);
            }

            Int32 UnixTimestamp = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            Random rnd = new(UnixTimestamp % 65535);

            string startsWith = "log-";
            for (int i = 0; i < 10; i++)  // tries 10 times to create a log file using different names
            {
                try
                {
                    FileStream stream = new(TempName(startsWith), FileMode.CreateNew, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous);
                    $"Log file created: {stream.Name}".DebugLn();
                    return stream;
                }
                catch (IOException) { /* Ignore */ }
                catch (Exception) { /* Ignore */  }
                Thread.Sleep(1);
                char a = (char)rnd.Next('a', 'z');
                char b = (char)rnd.Next('a', 'z');
                char c = (char)rnd.Next('a', 'z');
                startsWith = $"{startsWith}{a}{b}{c}-";
            }
            return null;
        }

        private void Write(string message)
        {
            if (Stream is null) return;
            byte[] encodedText = Encoding.Unicode.GetBytes(message);
            try
            {
                Stream.Write(encodedText, 0, encodedText.Length);
                Stream.Seek(0, SeekOrigin.End);
            }
            catch { /* Do nothing */ }
        }

        public void InfiniteWriter()
        {
            try
            {
                while (LogQueue is not null)
                    Write(LogQueue.Take());
            }
            catch (Exception)
            {
                Write($"{Header} FileLogger is stopped");
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}
