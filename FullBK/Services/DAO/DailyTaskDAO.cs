namespace FullBK.Services.DAO;

public record class DailyTaskDAO
{
    public string ImageBackground { get; init; }

    public string ImagePrize { get; init; }

    public string Description { get; init; }

    public State TaskState { get; init; }

    public enum State {
        COMPLETE = 0,       // Получено ранее
        NEW_ABLE = 1,       // Доступно к получению
        NEW_COMPLETE = 2,   // Только что получено
        UNAVAILABLE = 3,    // Недоступно (можно получить позже)
    };

    public DailyTaskDAO(string imageBackground, string imagePrize, string description, State taskState)
    {
        ImageBackground = imageBackground;
        ImagePrize = imagePrize;
        Description = description;
        TaskState = taskState;
    }

}
