namespace FullBK.VerboseResult;

/// <summary>
/// <br>Объект, который либо хранит объект типа T (далее "результат"), либо хранит информацию о его отсутствии.</br>
/// <br>Кроме того хранит лог (_notes) и код возврата (internalCode)</br>
/// <br></br>
/// <br>Булево свойство HasResult сообщает о наличии "результата"</br>
/// <br>Свойство Result возвращает "результат" при его наличии, либо выбрасывает исключение InvalidCastException</br>
/// <br>Свойство ResultOrDefault возвращает "результат" при его наличии, либо default(T) - null для ссылочных типов</br>
/// <br></br>
/// <br>Создание Verbose объекта возможно только с помощью билдера, у которого есть два фабричных метода</br>
/// <br> * Build - для создания объекта Verbose с вложенным "результатом"</br>
/// <br> * BuildFailure - для создания объекта Verbose без "результата"</br>
/// </summary>
/// <typeparam name="T"></typeparam>
public class Verbose<T>
{
    private static readonly object NoResult = new();    // this singleton object stands for "Nothing, No result"

    private object? _result;
    private readonly IReadOnlyList<Note> _notes;
    private readonly int internalCode;

    public bool HasResult => !Object.ReferenceEquals(_result, NoResult);
    public int ErrorCode => internalCode;
    /// <summary>
    /// Throws InvalidCastException if the result is not present
    /// </summary>
    public T Result
    {
        get
        {
            if (Object.ReferenceEquals(_result, NoResult)) throw new InvalidCastException();
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
#pragma warning disable CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.
            return (T)_result;
#pragma warning restore CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        }
    }
    public IReadOnlyList<string> Notes => _notes.Select(note => note.ToString()).ToList();

    /// <summary>
    /// Returns default(T) if the result is not present
    /// </summary>
    public T? ResultOrDefault => _result is T resultAsT ? resultAsT : default;
    /// <summary>
    /// Creates "Failure" instance without result. Remember to provide a documentation with error codes
    /// </summary>
    /// <param name="notes"></param>
    private Verbose(List<Note> notes, int errorCode) { _result = NoResult; _notes = notes; internalCode = errorCode; }
    private Verbose(T Result, List<Note> notes, int errorCode) { _result = Result; _notes = notes; internalCode = errorCode; }

    /// <summary>
    /// Returns the text property of the last Error occurred or null if there's none of them
    /// </summary>
    /// <returns></returns>    
    public string? GetLastError() =>
        this._notes.LastOrDefault(note =>
            note.Type == Note.NoteTypes.ERROR
        )?.Text ?? null;

    public override string ToString()
    {
        if (_result is T resultAsT) return resultAsT?.ToString() ?? string.Empty;
        return string.Empty;
    }
    public string NotesToString() => string.Join("\n", _notes);

    private record class Note
    {
        public enum NoteTypes { INFO, WARNING, ERROR }
        public NoteTypes Type { get; init; }
        public string Text { get; init; }
        public long TimeStamp { get; init; }
        public Note(NoteTypes type, string text) { Type = type; Text = text; TimeStamp = GetUnixTime(); }
        private Note(NoteTypes type, string text, long timeStamp) { Type = type; Text = text; TimeStamp = timeStamp; }
        public override string ToString() => $"[{FromUnixTime(TimeStamp):yyyy-MM-dd HH:mm:ss}] [{Type}] {Text}";
        private static long GetUnixTime()
        {
            System.DateTime localDateTime = System.DateTime.Now; // Локальное время
            System.DateTime utcDateTime = localDateTime.ToUniversalTime(); // Преобразование в UTC
            System.DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

            long unixTimestamp = (long)(utcDateTime - epoch).TotalSeconds;
            return unixTimestamp;
        }
        private static System.DateTime FromUnixTime(long unixTimestamp)
        {
            System.DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            System.DateTime utcDateTime = epoch.AddSeconds(unixTimestamp);

            return utcDateTime.ToLocalTime();
        }
        public Verbose<E>.Note Copy<E>()
        {
            return new Verbose<E>.Note((Verbose<E>.Note.NoteTypes)Type, Text, TimeStamp);
        }
    }

    public class Builder(Func<T, string>? stringConverter = null)
    {
        private readonly List<Note> Notes = new();
        private Func<T, string> ResultToString = (stringConverter is null) ? (T) => $"{T}" : stringConverter;
        public void Info(string text) => Notes.Add(new Note(Note.NoteTypes.INFO, text));
        public void Warning(string text) => Notes.Add(new Note(Note.NoteTypes.WARNING, text));
        public void Error(string text) => Notes.Add(new Note(Note.NoteTypes.ERROR, text));
        public void ImportNotes<E>(Verbose<E> smart)
        {
            foreach (var note in smart._notes)
                Notes.Add(note.Copy<T>());
        }
        public Verbose<T> Build(T Result, int errorCode = 0)
        {
            string typedResult = $"({typeof(T)}){ResultToString(Result)}";
            switch (errorCode)
            {
                case 0: Info($"Result = {typedResult}"); break;
                default: Info($"Result = {typedResult}; Additional code={errorCode}"); break;
            }
            return new(Result, Notes, errorCode);
        }
        public Verbose<T> BuildFailure(int errorCode)
        {
            Info($"Return '{nameof(NoResult)}' with error code = {errorCode}");
            return new(Notes, errorCode);
        }
    }
}