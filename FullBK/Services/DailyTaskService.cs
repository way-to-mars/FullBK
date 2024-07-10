namespace FullBK.Services;

using FullBK.Logging;
using FullBK.Services.DAO;
using FullBK.VerboseResult;

public class DailyTaskService
{
    ContextProvider ContextProvider { get; init; }

    public DailyTaskService(ContextProvider contextProvider)
    {
        this.ContextProvider = contextProvider;
    }

    /// <summary>
    /// <br>Get a list of daily rewards from https://tanki.su/ru/daily-check-in/</br>
    /// <br>Error codes:</br>
    /// <br> 0 - Success. User is logged, daily rewards obtained</br>
    /// <br> 1 - User is not logged, but daily rewards obtained</br>
    /// <br> 2 - Selector is not working</br>
    /// <br> 99 - Critical error. Exception thrown</br>
    /// </summary>
    /// <returns></returns>
    public async Task<Verbose<List<DailyTaskDAO>>> MonthlyList()
    {
        List<DailyTaskDAO> result = new();
        Verbose<List<DailyTaskDAO>>.Builder resultBuilder = new(list =>
                $"(size={list.Count}) [{string.Join(", ", list.Select(dao => dao.Description[..5].Trim()))}]"
        );

        var context = await ContextProvider.GetContext();
        var page = await context.NewPageAsync();

        try
        {
            string selectorItemsContainer = "//div[@class='c_items']";  // a Table containing daily rewards

            #region Force log out and then log in
            // ***************************
            var (log, pas) = LoadAuth(@"d:\tmp.txt");
            string? username = null;
            if (log != null && pas != null)
            {
                var authResult = await ContextProvider.LestaAuthorize(log, pas);
                username = authResult.ResultOrDefault;
                resultBuilder.ImportNotes(authResult);
            }
            // ***************************
            #endregion

            await page.GotoAsync("https://tanki.su/ru/daily-check-in/");
            var itemsContainer = page.Locator(selectorItemsContainer);  
            bool isAuthorized = await ContextProvider.IsLestaAuthorized(page);

            var allItems = await itemsContainer.Locator(".c_item").AllAsync();
            foreach (var item in allItems)
            {
                string? imgBg = await item.Locator(".c_item__bg > img").GetAttributeAsync("src");
                string? imgBody = await item.Locator(".c_item__body > img").GetAttributeAsync("src");
                string? description = (await item.Locator("span").TextContentAsync())?.Trim();
                string? divClass = await item.GetAttributeAsync("class");

                DailyTaskDAO.State taskState = MatchState(divClass);
                if (taskState == DailyTaskDAO.State.NEW_ABLE && isAuthorized)
                {
                    if (await ObtainReward(page, item))
                    {
                        taskState = DailyTaskDAO.State.NEW_COMPLETE;
                        resultBuilder.Info("New reward is spotted and received");
                    }
                    else resultBuilder.Info("New reward is spotted but not received");
                }

                result.Add(new DailyTaskDAO(
                    imageBackground: imgBg ?? "empty_box.png",
                    imagePrize: imgBody ?? "empty_prize.png",
                    description: description ?? "",
                    taskState
                    ));
            }
            return resultBuilder.Build(result, isAuthorized ? 0 : 1);   // Provide error code "1" if user is not logged on "tanki.su"
        }
        catch (Exception ex)
        {
            resultBuilder.Error(ex.Message);
            return resultBuilder.BuildFailure(99);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// <br>Состояние награды определяется классом элемента</br>
    /// <br>Новая: class='c_item c_default'</br>
    /// <br>Получена: class='c_item c_comlete' (не com<b>p</b>lete)</br>
    /// <br>Недоступна: class='c_item c_desable' (не d<b>i</b>sable) - дефолтный результат</br>
    /// </summary>
    /// <param name="divClass"></param>
    /// <returns></returns>
    private static DailyTaskDAO.State MatchState(string? divClass)
    {
        if (divClass == null) return DailyTaskDAO.State.UNAVAILABLE;

        var lowerCase = divClass.ToLower();
        if (lowerCase.Contains("c_default")) return DailyTaskDAO.State.NEW_ABLE;
        if (lowerCase.Contains("c_comlete")) return DailyTaskDAO.State.COMPLETE;

        return DailyTaskDAO.State.UNAVAILABLE;
    }

    private async Task<bool> ObtainReward(Microsoft.Playwright.IPage page, Microsoft.Playwright.ILocator locator)
    {
        try
        {
            "#1".DebugLn();
            await locator.ClickAsync();
            "#2".DebugLn();
            await page.WaitForSelectorAsync(".c_task__body.c_task__comlete");
            "#3".DebugLn();
            return true;
        }
        catch {
            "#4".DebugLn();
        }
        try
        {
            "#5".DebugLn();
            await page.ReloadAsync();
            "#6".DebugLn();
            var locatorClass = await locator.GetAttributeAsync("class");
            "#7".DebugLn();
            var newState = MatchState(locatorClass);
            "#8".DebugLn();
            return newState == DailyTaskDAO.State.COMPLETE;
        }
        catch
        {
            "#9".DebugLn();
            return false;
        }
    }

    private static (string?, string?) LoadAuth(string file)
    {
        string? line1 = null;
        string? line2 = null;
        if (File.Exists(file))
        {
            using StreamReader textFile = new(file);
            try
            {
                line1 = textFile.ReadLine();
                line2 = textFile.ReadLine();
            }
            catch { /* ignore */ }
        }
        return (line1, line2);
    }

    private bool WaitAppearance(Microsoft.Playwright.IPage page, float timeout = 30000, params string[] selectors)
    {
        try
        {
            var waiters = selectors
                .Select(selector => page.Locator(selector))
                .Select(locator => locator.WaitForAsync(
                    new Microsoft.Playwright.LocatorWaitForOptions()
                    {
                        Timeout = timeout,
                        State = Microsoft.Playwright.WaitForSelectorState.Visible
                    }))
                .ToArray();
            Task.WaitAll(waiters);
            return true;
        }
        catch(Exception)
        {
            return false;
        }        
    }
}