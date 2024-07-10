using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FullBK.Services;
using Microsoft.Playwright;
using System.Collections.ObjectModel;

namespace FullBK.ViewModel;

public partial class ContextManagementViewModel : ObservableObject
{
    const int PageClosingDelay = 1000;

    ContextProvider ContextProvider { get; init; }
    public ObservableCollection<string> messages = new();

    [ObservableProperty]
    bool isTankiSuButtonEnabled = true;

    [ObservableProperty]
    bool isCheckButtonEnabled = true;

    [ObservableProperty]
    bool isLoginButtonEnabled = true;

    [ObservableProperty]
    bool isLogoutButtonEnabled = true;

    public ContextManagementViewModel(ContextProvider contextProvider)
    {
        this.ContextProvider = contextProvider;
    }

    private void ShowMessage(string message) => messages.Add(message);
    public static void DisplayAlert(string text, string title = "Уведомление") => _ = Shell.Current.DisplayAlert(title, text, "OK");



    [RelayCommand]
    async Task TankiSuAsync()
    {
        #region Init
        if (!IsTankiSuButtonEnabled) return;
        IsTankiSuButtonEnabled = false;
        var context = await ContextProvider.GetContext();
        if (context is null) return;
        var page = await context.NewPageAsync();
        #endregion

        try
        {
            var response = await page.GotoAsync("https://tanki.su", new Microsoft.Playwright.PageGotoOptions()
            {
                Timeout = 30000,
                WaitUntil = Microsoft.Playwright.WaitUntilState.Load,
            });

            ShowMessage($"https://tanki.su response={response.ToReadable()}");
        }
        catch (Exception ex)
        {
            ShowMessage(ex.ToReadable());
        }

        #region Dispose
        await Task.Delay(PageClosingDelay);
        await page.CloseAsync();
        IsTankiSuButtonEnabled = true;
        #endregion
    }

    [RelayCommand]
    async Task CheckAuthorizationAsync()
    {
        #region Init
        if (!IsCheckButtonEnabled) return;
        IsCheckButtonEnabled = false;
        var context = await ContextProvider.GetContext();
        if (context is null) return;
        var page = await context.NewPageAsync();
        #endregion

        bool IsAuthorized = false;
        string? userName = null;
        try
        {
            await page.GotoAsync("https://tanki.su/ru/community/accounts/show/me/",
                new Microsoft.Playwright.PageGotoOptions()
                {
                    Timeout = 30000,
                    WaitUntil = Microsoft.Playwright.WaitUntilState.Commit,
                });
            string? userMenuDivStyle = await page.Locator(".cm-menu__user").GetAttributeAsync("class");
            if (userMenuDivStyle is not null && !userMenuDivStyle.ToLower().Contains("unauthorized"))
            {
                IsAuthorized = true;
                userName = (await page.Locator("//div[@class='user-name_inner']").TextContentAsync())?.Trim();
            }
        }
        catch (Exception ex)
        {
            ShowMessage(ex.ToReadable());
        }
        string state = IsAuthorized ? $"Yes, Name {userName}" : "No";
        ShowMessage($"Состояние авторизации: {state}");

        #region Dispose
        await Task.Delay(PageClosingDelay);
        await page.CloseAsync();
        IsCheckButtonEnabled = true;
        #endregion
    }

    [RelayCommand]
    async Task LogoutAsync()
    {
        #region Init
        if (!IsLogoutButtonEnabled) return;
        IsLogoutButtonEnabled = false;
        var context = await ContextProvider.GetContext();
        if (context is null) return;
        var page = await context.NewPageAsync();
        #endregion

        try
        {
            const string logoutSelector = ".cm-footer-logout_link";     // links to "Log out" button

            var response = await page.GotoAsync("https://tanki.su/ru/community/accounts/show/me/",
                new Microsoft.Playwright.PageGotoOptions()
                {
                    Timeout = 60000,
                    WaitUntil = Microsoft.Playwright.WaitUntilState.Commit,
                });

            var branch = Task.WaitAny(
                page.WaitForVisibleAsync("#jsc-submit-button-e60e-c2c6-"),  // No authorization
                page.WaitForAttachedAsync(".cm-footer-logout_link")          // Authorized
                );

            if (branch == 0)
            {
                ShowMessage("Игрок не был авторизован");
            }
            else
            {
                await page.Locator(".cm-menu__user").ClickAsync();                     // That should make "Log out" button visible
                await page.Locator(logoutSelector).DispatchEventAsync("click");        // but we press it regardless of its visibility

                await page.WaitForSelectorAsync(logoutSelector, new Microsoft.Playwright.PageWaitForSelectorOptions()
                {
                    State = Microsoft.Playwright.WaitForSelectorState.Detached
                });

                ShowMessage("Игрок теперь разлогинен");
            }
        }
        catch (Exception ex)
        {
            ShowMessage(ex.ToReadable());
        }        

        #region Dispose
        await Task.Delay(PageClosingDelay);
        await page.CloseAsync();
        IsLogoutButtonEnabled = true;
        #endregion
    }

    [RelayCommand]
    async Task LoginAsync()
    {
        #region Init
        if (!IsLoginButtonEnabled) return;
        IsLoginButtonEnabled = false;
        var context = await ContextProvider.GetContext();
        if (context is null) return;
        var page = await context.NewPageAsync();
        #endregion

        try
        {
            await page.GotoAsync("https://lesta.ru/id/signin/",
                new Microsoft.Playwright.PageGotoOptions()
                {
                    Timeout = 60000,
                    WaitUntil = Microsoft.Playwright.WaitUntilState.Commit,
                });

            var submit = page.Locator("#jsc-submit-button-e60e-c2c6-").IsEnabledAsync();
            var logout = page.Locator(".cm-footer-logout_link").IsEnabledAsync();

            var branch1 = Task.WaitAny(submit, logout);

            if (branch1 == 1)
            {
                ShowMessage($"Пользователь уже авторизован. Имя игрока: {await page.LestaUserNameAsync()}");
            }
            else
            {
                var (login, password) = await LoadAuth(@"d:\tmp.txt");
                if (login == null || password == null)
                {
                    ShowMessage("Unable to read login/password");
                    throw new ArgumentNullException("login/password");
                }

                await page.Locator("//input[@id='id_login']").PressSequentiallyAsync(login, new() { Delay = 10 });
                await page.Locator("//input[@id='id_password']").PressSequentiallyAsync(password, new() { Delay = 10 });

                var clickResponse = await page.RunAndWaitForResponseAsync(
                    action: async () =>
                    {
                        await page.Locator("//button[@type='submit']").ClickAsync();
                    },
                    urlOrPredicate: (IResponse r) => r.Url.ToLower().Contains("lesta.ru/id/signin/status")
                    );

                int submitHttpStatus = clickResponse.Status;

                ShowMessage($"Http Status: {submitHttpStatus}");

                if (submitHttpStatus < 200 && submitHttpStatus > 299)
                {
                    ShowMessage($"Unable to login with provided login/password. Http Status: {submitHttpStatus}");
                    throw new ArgumentException("login/password");
                }

                ShowMessage($"Успешная авторизация на сайте игры. Имя игрока: {await page.LestaUserNameAsync()}");
            }
        }
        catch (Exception ex)
        {
            ShowMessage(ex.ToReadable());
        }

        #region Dispose
        await Task.Delay(PageClosingDelay);
        await page.CloseAsync();
        IsLoginButtonEnabled = true;
        #endregion
    }

    private static async Task<(string?, string?)> LoadAuth(string file)
    {
        string? line1 = null;
        string? line2 = null;
        if (File.Exists(file))
        {
            await Task.Run(() =>
            {
                using StreamReader textFile = new(file);
                try
                {
                    line1 = textFile.ReadLine();
                    line2 = textFile.ReadLine();
                }
                catch { /* ignore */ }
            });
        }
        return (line1, line2);
    }

}



public static class ContextManagerExtensions
{
    public static string ToReadable(this IResponse? response)
    {
        if (response is null) return "Response is null";
        return $"[Response: {response.Status} '{response.StatusText}']. Headers\n{string.Join("\n", response.Headers.Select(pair => $"[{pair.Key}]: [{pair.Value}]"))}";
    }

    public static string ToReadable(this Exception ex)
    {
        return $"[Exception: {ex.HResult}. Type of the instance'{ex.GetType}']\n" +
               $"Source: {ex.Source}\n" +
               $"Message: {ex.Message}\n" +
               $"InnerException: {ex.InnerException}";
    }


    /// <summary>
    /// <br>Получить имя игрока из правого верхнего угла страницы. Предполагается, что игрок в настоящее время авторизован.</br>
    /// <br>В случае неудачи возвращает null</br>
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public static async Task<string?> LestaUserNameAsync(this IPage page)
    {
        try
        {
            return (await page.Locator(".cm-user-menu-link_cutted-text").TextContentAsync())?.Trim();
        }
        catch { return null; }
    }
}
