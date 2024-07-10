
using FullBK.Services.DAO;

namespace FullBK.Model;

public class DailyTaskSimpleModel
{
    public string ImageBackground { get; set; } = string.Empty;

    public string ImagePrize { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public enum TaskStates
    {
        COMPLETE = 0,       // Получено ранее
        NEW_ABLE = 1,       // Доступно к получению
        NEW_COMPLETE = 2,   // Только что получено
        UNDEFINED = 3,      // Недоступно к получению (будет доступно позже или требуется аутентификация)
    }
    public TaskStates State { get; set; } = TaskStates.UNDEFINED;

    public static DailyTaskSimpleModel From(DailyTaskDAO dailyTaskDAO) => new DailyTaskSimpleModel()
    {
        ImageBackground = dailyTaskDAO.ImageBackground,
        ImagePrize = dailyTaskDAO.ImagePrize,
        Description = dailyTaskDAO.Description,
        State = dailyTaskDAO.TaskState switch
        {
            DailyTaskDAO.State.COMPLETE => TaskStates.COMPLETE,
            DailyTaskDAO.State.NEW_ABLE => TaskStates.NEW_ABLE,
            DailyTaskDAO.State.NEW_COMPLETE => TaskStates.NEW_COMPLETE,
            _ => TaskStates.UNDEFINED,
        }
    };
}
