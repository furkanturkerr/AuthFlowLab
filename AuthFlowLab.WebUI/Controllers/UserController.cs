using System.Net;
using System.Text.Json;
using AuthFlowLab.WebUI.Dtos.UserDtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthFlowLab.WebUI.Controllers;

[Authorize]

public class UserController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public UserController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AuthenticatedApi");

            using var response = await client.GetAsync("api/users", HttpContext.RequestAborted);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["SessionMessage"] = "Oturumunuz doğrulanamadı. Lütfen yeniden giriş yapın.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index), "User") });
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToAction("AccessDenied", "Account");

            if (!response.IsSuccessStatusCode)
                return ListError("API kullanıcı listesini getiremedi.");

            var users = await response.Content.ReadFromJsonAsync<List<UserListDto>>(
                cancellationToken: HttpContext.RequestAborted)
                ?? throw new JsonException("Kullanıcı listesi boş bir cevap içeriyor.");

            return View(users);
        }
        catch (HttpRequestException)
        {
            return ListError("API'ye ulaşılamadı. API projesini çalıştırıp tekrar deneyin.");
        }
        catch (TaskCanceledException) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            return ListError("Kullanıcı listesi isteği zaman aşımına uğradı.");
        }
        catch (JsonException)
        {
            return ListError("API kullanıcı listesi için beklenen cevabı göndermedi.");
        }
    }

    private IActionResult ListError(string message)
    {
        Response.StatusCode = StatusCodes.Status502BadGateway;
        ViewData["ApiError"] = message;
        return View("Index", new List<UserListDto>());
    }
}
