using System.Security.Claims;
using System.Text.Json;
using AuthFlowLab.WebUI.Dtos.LoginDtos;
using AuthFlowLab.WebUI.Dtos.RegisterDtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthFlowLab.WebUI.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectAfterLogin(returnUrl);

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginDto());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            var client = _httpClientFactory.CreateClient("AnonymousApi");
            using var response = await client.PostAsJsonAsync(
                "api/auth/login", new { dto.Email, dto.Password }, HttpContext.RequestAborted);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "E-posta veya şifre hatalı.");
                return View(dto);
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(
                cancellationToken: HttpContext.RequestAborted);

            if (result == null || !result.Succeeded ||
                string.IsNullOrWhiteSpace(result.AccessToken) ||
                string.IsNullOrWhiteSpace(result.UserId) ||
                result.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            {
                ModelState.AddModelError("", "API geçerli bir giriş cevabı göndermedi.");
                return View(dto);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.UserId),
                new(ClaimTypes.Name, result.NameSurname),
                new(ClaimTypes.Email, result.Email)
            };

            claims.AddRange(result.Roles.Select(role =>
                new Claim(ClaimTypes.Role, role)));

            var scheme = CookieAuthenticationDefaults.AuthenticationScheme;

            var identity = new ClaimsIdentity(claims, scheme, ClaimTypes.Name, ClaimTypes.Role);

            var properties = new AuthenticationProperties
            {
                IsPersistent = dto.RememberMe,
                ExpiresUtc = result.ExpiresAtUtc,
                AllowRefresh = false
            };

            properties.StoreTokens(new[]
            {
                new AuthenticationToken
                {
                    Name = "access_token",
                    Value = result.AccessToken
                }
            });

            await HttpContext.SignInAsync(scheme, new ClaimsPrincipal(identity), properties);

            return RedirectAfterLogin(returnUrl);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "API'ye ulaşılamadı. API projesinin çalıştığını kontrol edin.");
        }
        catch (TaskCanceledException) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            ModelState.AddModelError("", "Giriş isteği zaman aşımına uğradı.");
        }
        catch (JsonException)
        {
            ModelState.AddModelError("", "Giriş servisi beklenen cevabı göndermedi.");
        }

        return View(dto);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "User");

        return View(new RegisterDto());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "User");

        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            var client = _httpClientFactory.CreateClient("AnonymousApi");
            using var response = await client.PostAsJsonAsync(
                "api/auth/register", dto, HttpContext.RequestAborted);

            var result = await response.Content.ReadFromJsonAsync<RegisterResponseDto>(
                cancellationToken: HttpContext.RequestAborted);

            if (!response.IsSuccessStatusCode || result == null || !result.Succeeded)
            {
                ModelState.AddModelError("", result?.Message ?? "Kayıt oluşturulamadı.");
                foreach (var error in result?.Errors ?? Array.Empty<string>())
                    ModelState.AddModelError("", error);
                return View(dto);
            }

            TempData["SuccessMessage"] = "Hesabınız oluşturuldu. Şimdi giriş yapabilirsiniz.";
            return RedirectToAction(nameof(Login));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "API'ye ulaşılamadı. API projesinin çalıştığını kontrol edin.");
        }
        catch (TaskCanceledException) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            ModelState.AddModelError("", "Kayıt isteği zaman aşımına uğradı.");
        }
        catch (JsonException)
        {
            ModelState.AddModelError("", "Kayıt servisi beklenen cevabı göndermedi.");
        }

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    private IActionResult RedirectAfterLogin(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "User");
    }
}
