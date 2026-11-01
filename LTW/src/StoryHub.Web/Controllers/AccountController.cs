using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Common.Options;
using StoryHub.Application.Interfaces;
using StoryHub.Domain.Entities;
using StoryHub.Web.ViewModels;

namespace StoryHub.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStoryService _storyService;
    private readonly ICaptchaService _captchaService;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly IDataProtector _dataProtector;
    private readonly IDataProtector _authSetupProtector;
    private readonly IDataProtector _passwordResetProtector;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IStoryService storyService,
        ICaptchaService captchaService,
        IEmailService emailService,
        IOptions<EmailSettings> emailOptions,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _storyService = storyService;
        _captchaService = captchaService;
        _emailService = emailService;
        _emailSettings = emailOptions.Value;
        _dataProtector = dataProtectionProvider.CreateProtector("TruyenVietSub.Account.Captcha");
        _authSetupProtector = dataProtectionProvider.CreateProtector("TruyenVietSub.Account.AuthenticatorSetup");
        _passwordResetProtector = dataProtectionProvider.CreateProtector("TruyenVietSub.Account.PasswordReset");
        _logger = logger;
    }

    [HttpGet("dang-nhap")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("dang-nhap")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ ban quản trị.");
            return View(model);
        }

        if (!user.EmailConfirmed)
        {
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                await _userManager.AccessFailedAsync(user);
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            var hasAuthenticatorKey = !string.IsNullOrEmpty(await _userManager.GetAuthenticatorKeyAsync(user));
            if (hasAuthenticatorKey)
            {
                var setupPayload = $"{user.Id}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                var setupToken = _authSetupProtector.Protect(setupPayload);
                TempData["ErrorMessage"] = "Tài khoản của bạn chưa hoàn tất xác minh Google Authenticator. Vui lòng quét mã bên dưới để kích hoạt tài khoản.";
                return RedirectToAction(nameof(VerifyAuthenticator), new { token = setupToken });
            }

            if (_emailSettings.Enabled)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn chưa được xác thực email. Vui lòng kiểm tra hộp thư để kích hoạt tài khoản.");
                ViewData["UnconfirmedEmail"] = user.Email;
                return View(model);
            }
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded || result.RequiresTwoFactor)
        {
            if (result.RequiresTwoFactor)
            {
                // In TruyenVietSub, Google Authenticator TOTP is used for account activation & password recovery.
                // Standard login uses email & password. Sign in directly and ensure TwoFactorEnabled is synced.
                await _signInManager.SignInAsync(user, model.RememberMe);
                if (user.TwoFactorEnabled)
                {
                    user.TwoFactorEnabled = false;
                    await _userManager.UpdateAsync(user);
                }
            }

            user.LastLoginAt = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Người dùng {Email} đã đăng nhập thành công.", model.Email);
            return RedirectToLocal(returnUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Tài khoản {Email} đã bị khóa do đăng nhập sai nhiều lần.", model.Email);
            ModelState.AddModelError(string.Empty, "Tài khoản đã bị tạm khóa 15 phút do nhập sai mật khẩu nhiều lần.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
        return View(model);
    }

    [HttpGet("dang-ky")]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new RegisterViewModel());
    }

    [HttpGet("dang-ky/captcha")]
    [HttpGet("quen-mat-khau/captcha")]
    [EnableRateLimiting("captcha")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult CaptchaImage()
    {
        var code = _captchaService.GenerateRandomCode(6);

        // Store in Session
        HttpContext.Session.SetString("RegisterCaptcha", code);

        // Also protect in encrypted HttpOnly cookie as fallback
        var token = _dataProtector.Protect($"{code}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
        Response.Cookies.Append("Captcha_Token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(10)
        });

        var svgBytes = _captchaService.GenerateCaptchaSvg(code);
        return File(svgBytes, "image/svg+xml");
    }

    [HttpPost("dang-ky")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // 1. Validate Captcha
        if (!ValidateCaptcha(model.CaptchaCode))
        {
            ModelState.AddModelError(nameof(model.CaptchaCode), "Mã xác nhận không chính xác hoặc đã hết hạn.");
        }

        // 2. Validate Google account email
        if (!GoogleEmailValidator.IsValidGoogleEmail(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), GoogleEmailValidator.DefaultErrorMessage);
        }

        if (!ModelState.IsValid)
        {
            model.CaptchaCode = string.Empty;
            return View(model);
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(model.Email), "Địa chỉ Email này đã được đăng ký.");
            model.CaptchaCode = string.Empty;
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            DisplayName = model.DisplayName.Trim(),
            EmailConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            // Default role is User
            var roleManager = HttpContext.RequestServices.GetService<RoleManager<IdentityRole>>();
            if (roleManager != null && !await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }
            await _userManager.AddToRoleAsync(user, "User");

            // Generate Google Authenticator secret key for this user
            await _userManager.ResetAuthenticatorKeyAsync(user);

            var setupPayload = $"{user.Id}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var setupToken = _authSetupProtector.Protect(setupPayload);

            _logger.LogInformation("Người dùng mới đã đăng ký (chờ xác thực Google Authenticator): {Email}", normalizedEmail);
            return RedirectToAction(nameof(VerifyAuthenticator), new { token = setupToken });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        model.CaptchaCode = string.Empty;
        return View(model);
    }

    [HttpGet("dang-ky/xac-nhan")]
    public IActionResult RegisterConfirmation(string? email)
    {
        ViewData["Email"] = email ?? string.Empty;
        return View();
    }

    [HttpGet("xac-thuc-email")]
    public async Task<IActionResult> ConfirmEmail(string? userId, string? token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            ViewData["ErrorMessage"] = "Liên kết xác thực email không hợp lệ hoặc đã bị lỗi.";
            return View(false);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            ViewData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản người dùng tương ứng.";
            return View(false);
        }

        if (user.EmailConfirmed)
        {
            ViewData["AlreadyConfirmed"] = true;
            return View(true);
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            _logger.LogInformation("Tài khoản {Email} đã xác thực email thành công.", user.Email);
            return View(true);
        }

        ViewData["ErrorMessage"] = "Liên kết xác nhận đã hết hạn hoặc không hợp lệ. Vui lòng yêu cầu gửi lại email xác nhận mới.";
        return View(false);
    }

    [HttpPost("dang-ky/gui-lai-xac-thuc")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResendConfirmationEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["ErrorMessage"] = "Vui lòng cung cấp địa chỉ email.";
            return RedirectToAction(nameof(Login));
        }

        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user != null && !user.EmailConfirmed)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { userId = user.Id, token = token },
                protocol: Request.Scheme);

            await _emailService.SendConfirmationEmailAsync(user.Email!, user.DisplayName, confirmationLink!);
            _logger.LogInformation("Đã gửi lại email xác thực cho {Email}.", user.Email);
        }

        TempData["SuccessMessage"] = $"Nếu email {email.Trim()} đã đăng ký và chưa được kích hoạt, hệ thống đã gửi một liên kết xác nhận mới đến hộp thư của bạn.";
        return RedirectToAction(nameof(RegisterConfirmation), new { email = email.Trim() });
    }

    [HttpGet("dang-ky/xac-thuc-authenticator")]
    public async Task<IActionResult> VerifyAuthenticator(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["ErrorMessage"] = "Liên kết xác thực Google Authenticator không hợp lệ.";
            return RedirectToAction(nameof(Register));
        }

        var user = await ValidateSetupTokenAsync(token);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Phiên xác thực đã hết hạn (tối đa 15 phút) hoặc không hợp lệ. Vui lòng đăng nhập để tiếp tục.";
            return RedirectToAction(nameof(Login));
        }

        if (user.EmailConfirmed)
        {
            TempData["SuccessMessage"] = "Tài khoản của bạn đã được xác thực trước đó. Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var model = new VerifyAuthenticatorViewModel
        {
            SetupToken = token,
            Email = user.Email ?? string.Empty,
            SharedKey = unformattedKey!,
            FormattedKey = FormatKey(unformattedKey!),
            AuthenticatorUri = GenerateQrCodeUri(user.Email ?? "user", unformattedKey!)
        };

        return View(model);
    }

    [HttpPost("dang-ky/xac-thuc-authenticator")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> VerifyAuthenticator(VerifyAuthenticatorViewModel model)
    {
        var user = await ValidateSetupTokenAsync(model.SetupToken);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Phiên xác thực đã hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại để nhận mã xác thực mới.";
            return RedirectToAction(nameof(Login));
        }

        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        model.Email = user.Email ?? string.Empty;
        model.SharedKey = unformattedKey!;
        model.FormattedKey = FormatKey(unformattedKey!);
        model.AuthenticatorUri = GenerateQrCodeUri(user.Email ?? "user", unformattedKey!);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var cleanCode = model.Code.Replace(" ", "").Replace("-", "").Trim();
        var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, cleanCode);

        if (!is2faTokenValid)
        {
            await _userManager.AccessFailedAsync(user);
            var isLocked = await _userManager.IsLockedOutAsync(user);
            if (isLocked)
            {
                ModelState.AddModelError(string.Empty, "Bạn đã nhập sai mã xác thực quá nhiều lần. Tài khoản tạm thời bị khóa 15 phút.");
            }
            else
            {
                ModelState.AddModelError(nameof(model.Code), "Mã xác thực Google Authenticator không chính xác hoặc đã hết hạn. Vui lòng kiểm tra lại thiết bị.");
            }
            return View(model);
        }

        // Activate account
        user.EmailConfirmed = true;
        user.TwoFactorEnabled = false;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);
        await _userManager.ResetAccessFailedCountAsync(user);

        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation("Người dùng {Email} đã hoàn tất kích hoạt tài khoản bằng Google Authenticator.", user.Email);
        TempData["SuccessMessage"] = "Xác thực tài khoản qua Google Authenticator thành công! Chào mừng bạn đến với TruyenVietSub.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("quen-mat-khau")]
    public IActionResult ForgotPassword()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost("quen-mat-khau")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ValidateCaptcha(model.CaptchaCode))
        {
            ModelState.AddModelError(nameof(model.CaptchaCode), "Mã xác nhận không chính xác hoặc đã hết hạn.");
        }

        if (!GoogleEmailValidator.IsValidGoogleEmail(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), GoogleEmailValidator.DefaultErrorMessage);
        }

        if (!ModelState.IsValid)
        {
            model.CaptchaCode = string.Empty;
            return View(model);
        }

        var email = model.Email.Trim().ToLowerInvariant();

        // Anti-enumeration: Always redirect to ResetPassword with the email
        return RedirectToAction(nameof(ResetPassword), new { email = email });
    }

    [HttpGet("dat-lai-mat-khau")]
    public IActionResult ResetPassword(string? email)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new ResetPasswordViewModel
        {
            Email = email ?? string.Empty
        });
    }

    [HttpPost("dat-lai-mat-khau")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!GoogleEmailValidator.IsValidGoogleEmail(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), GoogleEmailValidator.DefaultErrorMessage);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);

        // Anti-enumeration: generic error message if user not found or inactive
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Thông tin xác thực không chính xác hoặc mã Google Authenticator không hợp lệ.");
            return View(model);
        }

        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            ModelState.AddModelError(string.Empty, "Tài khoản chưa thiết lập Google Authenticator. Vui lòng liên hệ ban quản trị để được hỗ trợ.");
            return View(model);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            _logger.LogWarning("Tài khoản {Email} đang bị tạm khóa nhưng cố gắng đặt lại mật khẩu.", email);
            ModelState.AddModelError(string.Empty, "Tài khoản đang tạm thời bị khóa do nhập sai nhiều lần. Vui lòng thử lại sau 15 phút.");
            return View(model);
        }

        var cleanCode = model.AuthenticatorCode.Replace(" ", "").Replace("-", "").Trim();
        var is2faValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, cleanCode);

        if (!is2faValid)
        {
            await _userManager.AccessFailedAsync(user);
            var isLocked = await _userManager.IsLockedOutAsync(user);
            if (isLocked)
            {
                ModelState.AddModelError(string.Empty, "Bạn đã nhập sai mã xác thực quá nhiều lần. Tài khoản đã bị tạm khóa 15 phút.");
            }
            else
            {
                ModelState.AddModelError(nameof(model.AuthenticatorCode), "Mã xác thực 6 số của TruyenVietSub không chính xác hoặc đã hết hạn. Vui lòng kiểm tra lại thiết bị.");
            }
            return View(model);
        }

        // TOTP verification succeeded!
        await _userManager.ResetAccessFailedCountAsync(user);

        // Generate Identity password reset token
        var identityResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Protect token with 15-minute expiration timestamp
        var payload = $"{user.Id}\t{identityResetToken}\t{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var protectedToken = _passwordResetProtector.Protect(payload);

        _logger.LogInformation("Người dùng {Email} đã xác thực mã Google Authenticator thành công, chuyển sang bước nhập mật khẩu mới.", user.Email);
        return RedirectToAction(nameof(SetNewPassword), new { token = protectedToken });
    }

    [HttpGet("dat-lai-mat-khau/mat-khau-moi")]
    public async Task<IActionResult> SetNewPassword(string? token)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["ErrorMessage"] = "Liên kết đặt lại mật khẩu không hợp lệ.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        var validated = await ValidatePasswordResetTokenAsync(token);
        if (validated == null)
        {
            TempData["ErrorMessage"] = "Phiên đặt lại mật khẩu đã hết hạn (tối đa 15 phút) hoặc không hợp lệ. Vui lòng xác thực lại mã Google Authenticator.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        return View(new SetNewPasswordViewModel
        {
            ResetToken = token,
            Email = validated.Value.User.Email ?? string.Empty
        });
    }

    [HttpPost("dat-lai-mat-khau/mat-khau-moi")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> SetNewPassword(SetNewPasswordViewModel model)
    {
        var validated = await ValidatePasswordResetTokenAsync(model.ResetToken);
        if (validated == null)
        {
            TempData["ErrorMessage"] = "Phiên đặt lại mật khẩu đã hết hạn hoặc không hợp lệ. Vui lòng xác thực lại.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        model.Email = validated.Value.User.Email ?? string.Empty;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = validated.Value.User;
        var identityResetToken = validated.Value.IdentityResetToken;

        var result = await _userManager.ResetPasswordAsync(user, identityResetToken, model.NewPassword);
        if (result.Succeeded)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, null);

            _logger.LogInformation("Người dùng {Email} đã đặt lại mật khẩu thành công bằng Google Authenticator.", user.Email);
            TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập bằng mật khẩu mới.";
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    private bool ValidateCaptcha(string? inputCode)
    {
        if (string.IsNullOrWhiteSpace(inputCode)) return false;

        string? validCode = HttpContext.Session.GetString("RegisterCaptcha");

        if (string.IsNullOrEmpty(validCode) && Request.Cookies.TryGetValue("Captcha_Token", out var token) && !string.IsNullOrEmpty(token))
        {
            try
            {
                var decrypted = _dataProtector.Unprotect(token);
                var parts = decrypted.Split(':');
                if (parts.Length == 2 && long.TryParse(parts[1], out var timestamp))
                {
                    // 10 minutes expiry
                    if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp <= 600)
                    {
                        validCode = parts[0];
                    }
                }
            }
            catch
            {
                // Ignore tampered or invalid token
            }
        }

        if (string.IsNullOrEmpty(validCode)) return false;

        var isValid = _captchaService.Validate(inputCode, validCode);
        if (isValid)
        {
            // Clear used captcha to prevent replay attacks
            HttpContext.Session.Remove("RegisterCaptcha");
            Response.Cookies.Delete("Captcha_Token");
        }

        return isValid;
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("tai-khoan/ho-so")]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = new ProfileViewModel
        {
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty,
            AvatarUrl = user.AvatarUrl,
            CommentBlockedUntil = user.CommentBlockedUntil,
            CreatedAt = user.CreatedAt
        };

        return View(model);
    }

    [HttpPost("tai-khoan/ho-so")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (string.IsNullOrWhiteSpace(model.DisplayName) || model.DisplayName.Trim().Length < 2 || model.DisplayName.Trim().Length > 15)
        {
            ModelState.AddModelError(nameof(model.DisplayName), "Tên hiển thị từ 2 đến 15 ký tự.");
            return View(model);
        }

        user.DisplayName = model.DisplayName.Trim();
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Cập nhật thông tin hồ sơ thành công!";
            return RedirectToAction(nameof(Profile));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet("tai-khoan/theo-doi")]
    [Authorize]
    public async Task<IActionResult> Followed(int pageIndex = 1, CancellationToken cancellationToken = default)
    {
        var stories = await _storyService.GetFollowedStoriesAsync(pageIndex, 20, cancellationToken);
        return View(stories);
    }

    [HttpGet("tai-khoan/lich-su-doc")]
    [Authorize]
    public async Task<IActionResult> History(int pageIndex = 1, CancellationToken cancellationToken = default)
    {
        var histories = await _storyService.GetReadingHistoriesAsync(pageIndex, 20, cancellationToken);
        return View(histories);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new System.Text.StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }
        return result.ToString().ToUpperInvariant();
    }

    private static string GenerateQrCodeUri(string email, string unformattedKey)
    {
        const string issuer = "TruyenVietSub";
        return $"otpauth://totp/{issuer}:{Uri.EscapeDataString(email)}?secret={unformattedKey}&issuer={issuer}&digits=6";
    }

    private async Task<ApplicationUser?> ValidateSetupTokenAsync(string token)
    {
        try
        {
            var decrypted = _authSetupProtector.Unprotect(token);
            var parts = decrypted.Split(':');
            if (parts.Length == 2 && long.TryParse(parts[1], out var timestamp))
            {
                // 15 minutes validity window
                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp <= 900)
                {
                    var userId = parts[0];
                    return await _userManager.FindByIdAsync(userId);
                }
            }
        }
        catch
        {
            // Invalid, tampered, or expired token
        }
        return null;
    }

    private async Task<(ApplicationUser User, string IdentityResetToken)?> ValidatePasswordResetTokenAsync(string token)
    {
        try
        {
            var decrypted = _passwordResetProtector.Unprotect(token);
            var parts = decrypted.Split('\t');
            if (parts.Length == 3 && long.TryParse(parts[2], out var timestamp))
            {
                // 15 minutes validity window
                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp <= 900)
                {
                    var userId = parts[0];
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null && user.IsActive)
                    {
                        return (user, parts[1]);
                    }
                }
            }
        }
        catch
        {
            // Invalid, tampered, or expired token
        }
        return null;
    }
}
