using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTW.Data;
using LTW.Models;
using System.Text.Json;
using System.Security.Claims;

namespace LTW.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectBasedOnRole();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find user by Email or Username
            var user = await _userManager.FindByEmailAsync(model.UsernameOrEmail) 
                       ?? await _userManager.FindByNameAsync(model.UsernameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc tên đăng nhập không tồn tại.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị tạm khóa. Vui lòng liên hệ Admin.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return await RedirectBasedOnRoleAsync(user);
            }

            ModelState.AddModelError(string.Empty, "Mật khẩu không chính xác. Vui lòng thử lại.");
            return View(model);
        }

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider = "Google", string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                TempData["ErrorMessage"] = $"Lỗi từ dịch vụ đăng nhập ngoài: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["ErrorMessage"] = "Không thể lấy thông tin đăng nhập Google.";
                return RedirectToAction(nameof(Login));
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user != null)
                {
                    TempData["SuccessMessage"] = $"Đăng nhập Google thành công! Chào mừng {user.FullName}.";
                    return await RedirectBasedOnRoleAsync(user);
                }
                return RedirectToAction("Dashboard", "Student");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? "Google Student";

            if (!string.IsNullOrEmpty(email))
            {
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    await _userManager.AddLoginAsync(existingUser, info);
                    await _signInManager.SignInAsync(existingUser, isPersistent: false);
                    TempData["SuccessMessage"] = $"Đăng nhập thành công với tài khoản {existingUser.FullName}!";
                    return await RedirectBasedOnRoleAsync(existingUser);
                }

                var newUser = new ApplicationUser
                {
                    UserName = email.Split('@')[0] + "_" + Random.Shared.Next(100, 999),
                    Email = email,
                    FullName = name,
                    RoleName = "Student",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    AvatarUrl = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=250&q=80"
                };

                var createResult = await _userManager.CreateAsync(newUser);
                if (createResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, "Student");
                    await _userManager.AddLoginAsync(newUser, info);

                    var notif = new Notification
                    {
                        UserId = newUser.Id,
                        Title = "Chào mừng bạn đến với LUMORA English Center!",
                        Content = "Tài khoản của bạn đã được đăng ký thành công qua Google. Khám phá các khóa học và làm bài kiểm tra năng lực ngay hôm nay!",
                        Type = "System",
                        LinkUrl = "/Test/PlacementTest",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.Notifications.Add(notif);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(newUser, isPersistent: false);
                    TempData["SuccessMessage"] = "Đăng ký và đăng nhập Google thành công!";
                    return RedirectToAction("Dashboard", "Student");
                }
            }

            TempData["ErrorMessage"] = "Không thể đăng nhập bằng tài khoản Google này.";
            return RedirectToAction(nameof(Login));
        }

        // POST: /Account/QuickGoogleLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> QuickGoogleLogin()
        {
            var demoEmail = "student@lumora.edu.vn";
            var user = await _userManager.FindByEmailAsync(demoEmail);
            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["SuccessMessage"] = $"Đăng nhập Google nhanh với tư cách {user.FullName} thành công!";
                return await RedirectBasedOnRoleAsync(user);
            }
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectBasedOnRole();
            }
            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if email or username already taken
            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Địa chỉ email này đã được đăng ký.");
                return View(model);
            }

            var existingUsername = await _userManager.FindByNameAsync(model.Username);
            if (existingUsername != null)
            {
                ModelState.AddModelError(nameof(model.Username), "Tên đăng nhập này đã được sử dụng.");
                return View(model);
            }

            // Generate 6-digit OTP
            var otpCode = Random.Shared.Next(100000, 999999).ToString();

            // Expire previous OTPs for this email
            var oldOtps = await _context.OtpRecords
                .Where(o => o.Email == model.Email && o.Purpose == "Register" && !o.IsUsed)
                .ToListAsync();
            foreach (var o in oldOtps)
            {
                o.IsUsed = true;
            }

            var otpRecord = new OtpRecord
            {
                Email = model.Email,
                OtpCode = otpCode,
                Purpose = "Register",
                PayloadJson = JsonSerializer.Serialize(model),
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(15),
                IsUsed = false
            };

            _context.OtpRecords.Add(otpRecord);
            await _context.SaveChangesAsync();

            // Simulation of email OTP send
            TempData["SuccessMessage"] = $"Mã OTP xác thực đã được gửi tới email {model.Email}. (Mã thử nghiệm hệ thống: {otpCode})";
            return RedirectToAction(nameof(VerifyOtp), new { email = model.Email, purpose = "Register" });
        }

        // GET: /Account/VerifyOtp
        [HttpGet]
        public IActionResult VerifyOtp(string email, string purpose = "Register")
        {
            var model = new VerifyOtpViewModel
            {
                Email = email,
                Purpose = purpose
            };
            return View(model);
        }

        // POST: /Account/VerifyOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = await _context.OtpRecords
                .Where(o => o.Email == model.Email && o.Purpose == model.Purpose && !o.IsUsed && o.ExpiresAt >= DateTime.Now)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (record == null || record.OtpCode != model.OtpCode.Trim())
            {
                ModelState.AddModelError(nameof(model.OtpCode), "Mã OTP không chính xác hoặc đã hết hạn. Vui lòng nhập lại.");
                return View(model);
            }

            record.IsUsed = true;
            await _context.SaveChangesAsync();

            if (model.Purpose == "Register")
            {
                if (string.IsNullOrEmpty(record.PayloadJson))
                {
                    ModelState.AddModelError(string.Empty, "Không tìm thấy dữ liệu đăng ký. Vui lòng thực hiện lại.");
                    return View(model);
                }

                var registerData = JsonSerializer.Deserialize<RegisterViewModel>(record.PayloadJson);
                if (registerData == null)
                {
                    ModelState.AddModelError(string.Empty, "Dữ liệu đăng ký không hợp lệ.");
                    return View(model);
                }

                var newUser = new ApplicationUser
                {
                    UserName = registerData.Username,
                    Email = registerData.Email,
                    FullName = registerData.FullName,
                    RoleName = "Student",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    AvatarUrl = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=250&q=80"
                };

                var createResult = await _userManager.CreateAsync(newUser, registerData.Password);
                if (createResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, "Student");

                    // Welcome notification
                    var notif = new Notification
                    {
                        UserId = newUser.Id,
                        Title = "Chào mừng bạn đến với LUMORA English Center!",
                        Content = "Tài khoản của bạn đã được kích hoạt thành công qua mã OTP. Hãy làm bài Placement Test hoặc trải nghiệm Example Test miễn phí ngay!",
                        Type = "System",
                        LinkUrl = "/Test/PlacementTest",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.Notifications.Add(notif);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(newUser, isPersistent: false);
                    TempData["SuccessMessage"] = "Chúc mừng bạn đã đăng ký và xác thực tài khoản thành công!";
                    return RedirectToAction("Dashboard", "Student");
                }

                foreach (var err in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                return View(model);
            }

            if (model.Purpose == "ForgotPassword")
            {
                return RedirectToAction(nameof(ResetPassword), new { email = model.Email, code = model.OtpCode });
            }

            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(nameof(model.Email), "Địa chỉ email chưa từng được đăng ký trong hệ thống.");
                return View(model);
            }

            var otpCode = Random.Shared.Next(100000, 999999).ToString();
            var otpRecord = new OtpRecord
            {
                Email = model.Email,
                OtpCode = otpCode,
                Purpose = "ForgotPassword",
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(15),
                IsUsed = false
            };

            _context.OtpRecords.Add(otpRecord);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Mã OTP khôi phục mật khẩu đã được gửi đến {model.Email}. (Mã thử nghiệm hệ thống: {otpCode})";
            return RedirectToAction(nameof(VerifyForgotOtp), new { email = model.Email });
        }

        // GET: /Account/VerifyForgotOtp
        [HttpGet]
        public IActionResult VerifyForgotOtp(string email)
        {
            return View(new VerifyOtpViewModel { Email = email, Purpose = "ForgotPassword" });
        }

        // POST: /Account/VerifyForgotOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyForgotOtp(VerifyOtpViewModel model)
        {
            model.Purpose = "ForgotPassword";
            return await VerifyOtp(model);
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string email, string code)
        {
            var model = new ResetPasswordViewModel
            {
                Email = email,
                OtpCode = code
            };
            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Người dùng không tồn tại.");
                return View(model);
            }

            // Reset password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập bằng mật khẩu mới.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "Bạn đã đăng xuất khỏi hệ thống thành công.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? "",
                Username = user.UserName ?? "",
                PhoneNumber = user.PhoneNumber,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                RoleName = user.RoleName
            };

            return View(model);
        }

        // POST: /Account/Profile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.Email = user.Email ?? "";
                model.Username = user.UserName ?? "";
                model.RoleName = user.RoleName;
                return View(model);
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Bio = model.Bio;
            if (!string.IsNullOrEmpty(model.AvatarUrl))
            {
                user.AvatarUrl = model.AvatarUrl;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var err in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                return View(model);
            }

            // Change password if requested
            if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                var passResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!passResult.Succeeded)
                {
                    foreach (var err in passResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, err.Description);
                    }
                    return View(model);
                }
                TempData["SuccessMessage"] = "Cập nhật hồ sơ và đổi mật khẩu thành công!";
            }
            else
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin hồ sơ thành công!";
            }

            return RedirectToAction(nameof(Profile));
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task<IActionResult> RedirectBasedOnRoleAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            if (roles.Contains("Teacher"))
            {
                return RedirectToAction("Dashboard", "Teacher");
            }
            return RedirectToAction("Dashboard", "Student");
        }

        private IActionResult RedirectBasedOnRole()
        {
            if (User.IsInRole("Admin")) return RedirectToAction("Dashboard", "Admin");
            if (User.IsInRole("Teacher")) return RedirectToAction("Dashboard", "Teacher");
            return RedirectToAction("Dashboard", "Student");
        }
    }
}

