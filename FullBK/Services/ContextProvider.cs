using FullBK.VerboseResult;

namespace FullBK.Services;

public class ContextProvider : IDisposable
{
    readonly SemaphoreSlim _slim = new(1, 1);
    readonly object _lock = new();

    global::Microsoft.Playwright.IPlaywright? _playwright = null;
    global::Microsoft.Playwright.IBrowser? _browser = null;
    global::Microsoft.Playwright.IBrowserContext? _context = null;

    public async Task<global::Microsoft.Playwright.IBrowserContext> GetContext()
    {
        await _slim.WaitAsync();
        try
        {
            if (_context is null)
            {
                _playwright = await global::Microsoft.Playwright.Playwright.CreateAsync();
                _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = false });
                _context = await _browser.NewContextAsync();
            }
            return _context;
        }
        finally
        {
            _slim.Release();
        }
    }

    private void LogEvents(global::Microsoft.Playwright.IPage page, Action<string> logger)
    {
        string Dict2str(Dictionary<string, string> dict)
        {
            global::System.Text.StringBuilder sb = new();
            foreach (var (k, v) in dict) sb.Append(k + "=" + v + "; ");
            return sb.ToString();
        }

        page.Close += (s, e) => { logger($"Page Close sender({s?.GetHashCode()}) page({e.GetHashCode()})"); };
        page.Console += (s, e) => { logger($"Page Console sender({s?.GetHashCode()}) console({e.Text})"); };
        page.Crash += (s, e) => { logger($"Page Crash sender({s?.GetHashCode()}) page({e.GetHashCode()})"); };
        page.Dialog += (s, e) => { logger($"Page Dialog sender({s?.GetHashCode()}) dialog({e.Message})"); };
        page.DOMContentLoaded += (s, e) => { logger($"Page DOMContentLoaded sender({s?.GetHashCode()}) page({e.GetHashCode()})"); };
        page.Download += (s, e) => { logger($"Page Download sender({s?.GetHashCode()}) download({e.Url})"); };
        page.FileChooser += (s, e) => { logger($"Page File chooser sender({s?.GetHashCode()}) element({e.Element})"); };
        page.FrameAttached += (s, e) => { logger($"Page FrameAttached sender({s?.GetHashCode()}) frame({e})"); };
        page.FrameDetached += (s, e) => { logger($"Page FrameDetached sender({s?.GetHashCode()}) frame({e})"); };
        page.FrameNavigated += (s, e) => { logger($"Page FrameNavigated sender({s?.GetHashCode()}) frame({e})"); };
        page.Load += (s, e) => { logger($"Page Load sender({s?.GetHashCode()}) page({e.GetHashCode()})"); };
        page.PageError += (s, e) => { logger($"Page Error sender({s?.GetHashCode()}) error({e})"); };
        page.Popup += (s, e) => { logger($"Page Popup sender({s?.GetHashCode()}) page({e.GetHashCode()})"); };
        page.Request += (s, e) => { logger($"Page Request sender({s?.GetHashCode()}) request2({e.Method}, {e.PostData}, {Dict2str(e.Headers)})"); };
        page.RequestFailed += (s, e) => { logger($"Page RequestFailed sender({s?.GetHashCode()}) request2 error({e.Failure})"); };
        page.RequestFinished += (s, e) => { logger($"Page RequestFinished sender({s?.GetHashCode()}) request2({e.PostData})"); };
        page.Response += (s, e) => { logger($"Page Response sender({s?.GetHashCode()}) Response({e.StatusText})"); };
        page.WebSocket += (s, e) => { logger($"Page WebSocket sender({s?.GetHashCode()}) WebSocket({e.GetHashCode()})"); };
        page.Worker += (s, e) => { logger($"Page Worker sender({s?.GetHashCode()}) Worker({e.GetHashCode()})"); };
    }

    /// <summary>
    /// <br>Every sub-page on lesta.ru (tanki.su) contains 'div' with authorization status</br>
    /// <br>Let's locate it by css ".cm-menu__user"</br>
    /// <br></br>
    /// <br>For authorized user it has class="cm-menu cm-menu__user js-cm-menu__user"</br>
    /// <br>For unauthorized visitor class="cm-menu cm-menu__user <b>cm-menu__user-unauthorized</b> js-cm-menu__user" </br>
    /// </summary>
    public static async Task<bool> IsLestaAuthorized(Microsoft.Playwright.IPage page)
    {
        try
        {
            string? userMenuDivStyle = await page.Locator(".cm-menu__user").GetAttributeAsync("class");
            if (userMenuDivStyle is null) return false;
            if (userMenuDivStyle.ToLower().Contains("unauthorized")) return false;
            return true;
        }
        catch { return false; }
    }


    /// <summary>
    /// <br>ErrorCode 1 - wrong login/password</br>
    /// <br>ErrorCode 2 - can't read username</br>
    /// <br>ErrorCode 3 - exception</br>
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<Verbose<string>> LestaAuthorize(string login, string password)
    {
        var context = await GetContext();
        var page = await context.NewPageAsync();

        Verbose<string>.Builder resultBuilder = new();
        try
        {
            await page.GotoAsync("https://tanki.su/ru/community/accounts/show/me/");

            if (await IsLestaAuthorized(page))
            {
                // Log Out
                await page.Locator(".cm-menu__user").ClickAsync();
                await page.Locator(".cm-footer-logout_link").ClickAsync();
                await page.WaitForSelectorAsync(".cm-footer-logout_link",
                  new Microsoft.Playwright.PageWaitForSelectorOptions() { State = Microsoft.Playwright.WaitForSelectorState.Detached });
            }

            await page.GotoAsync("https://lesta.ru/id/signin/", new Microsoft.Playwright.PageGotoOptions() { WaitUntil = Microsoft.Playwright.WaitUntilState.DOMContentLoaded });
            await page.Locator("//input[@id='id_login']").PressSequentiallyAsync(login, new() { Delay = 10 });
            await page.Locator("//input[@id='id_password']").PressSequentiallyAsync(password, new() { Delay = 10 });
            await page.Locator("//button[@type='submit']").ClickAsync();

            var request = Task.WaitAny(
                page.WaitForVisibleAsync(".cm-user-menu-link_cutted-text"),   // #0 - you're logged
                page.WaitForVisibleAsync("#jsc-error-message-block-b1a4-e4ce- > div")   // #1 - wrong login/password
            );

            if (request == 1)
            {
                resultBuilder.Error("Wrong login/password");
                return resultBuilder.BuildFailure(1);
            }

            await page.GotoAsync("https://tanki.su/ru/community/accounts/show/me/");

            string? userName = (await page.Locator("//div[@class='user-name_inner']").TextContentAsync())?.Trim();
            if (userName is null) return resultBuilder.BuildFailure(2);

            resultBuilder.Info($"'{userName}' is logged in");
            return resultBuilder.Build(userName);
        }
        catch (Exception ex)
        {
            resultBuilder.Error(ex.Message);
            return resultBuilder.BuildFailure(2);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    private async Task ExportContext(string file)
    {
        await _slim.WaitAsync();
        try
        {
            if (_context is not null)
            {
                await _context.StorageStateAsync(new global::Microsoft.Playwright.BrowserContextStorageStateOptions()
                {
                    Path = file,
                });
            }
        }
        finally
        {
            _slim.Release();
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _playwright?.Dispose();
            _playwright = null;
            _browser = null;
            _context = null;
        }
    }

}

public static class ContextProviderPageExtensions
{
    public static Task WaitForVisibleAsync(this Microsoft.Playwright.IPage page, string selector) =>
        page.WaitForSelectorAsync(
            selector,
            new Microsoft.Playwright.PageWaitForSelectorOptions() { State = Microsoft.Playwright.WaitForSelectorState.Visible });

    public static Task WaitForAttachedAsync(this Microsoft.Playwright.IPage page, string selector) =>
    page.WaitForSelectorAsync(
        selector,
        new Microsoft.Playwright.PageWaitForSelectorOptions() { State = Microsoft.Playwright.WaitForSelectorState.Attached });
}


