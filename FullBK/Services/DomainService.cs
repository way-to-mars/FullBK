using FullBK.Services.DAO;
using FullBK.VerboseResult;
using static FullBK.Logging.LoggingExtensions;

namespace FullBK.Services;


public class DomainService
{
    DailyTaskService dailyTaskService;

    public DomainService(DailyTaskService dailyTaskService)
    {
        this.dailyTaskService = dailyTaskService;
    }

    /// <summary>
    /// <br>Error codes:</br>
    /// <br> 0 - Success. User is logged, daily rewards obtained</br>
    /// <br> 1 - User is not logged, but daily rewards obtained</br>
    /// <br> 2 - Critical error. Exception thrown</br>
    /// </summary>
    /// <returns></returns>
    public async Task<Verbose<List<DailyTaskDAO>>> MonthlyList()
    {
        var daos = await Task.Run(dailyTaskService.MonthlyList);        

        $"DomainService.MonthlyList _notes:\n{daos.NotesToString()}".DebugLn();

        return daos;
    }


}
