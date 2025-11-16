using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UnivForm.Data; // Kendi AppUser ve AppDbContext yolunuz
using UnivForm.Models.ViewModels;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Authorization; // [AllowAnonymous] için
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web; // HtmlEncoder için

namespace UnivForm.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        // --- DEĞİŞİKLİK: Gerekli servisler eklendi ---
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;

        // --- CONSTRUCTOR GÜNCELLENDİ ---
        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        // --- KAYIT OL (Register) (TAMAMEN GÜNCELLENDİ) ---

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Yeni kullanıcı (onay bekliyor) oluşturuldu.");

                    await _userManager.AddToRoleAsync(user, "User");

                    // --- E-POSTA ONAY KODU ---
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                    var callbackUrl = Url.Action(
                        action: nameof(ConfirmEmail),
                        controller: "Account",
                        values: new { userId = user.Id, token = token },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(model.Email,
                        "Lütfen Hesabınızı Doğrulayın",
                        $"Hesabınızı doğrulamak için lütfen <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? "")}'>buraya tıklayın</a>.");

                    // OTOMATİK GİRİŞ KODU SİLİNDİ

                    return RedirectToAction(nameof(RegistrationConfirmation));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // --- YENİ EKLENDİ: Kayıt Onay Sayfası (GET) ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegistrationConfirmation()
        {
            return View();
        }

        // --- YENİ EKLENDİ: E-posta Onaylama (GET) ---
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(int userId, string token)
        {
            if (userId == 0 || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning("Geçersiz e-posta onay denemesi: Kullanıcı bulunamadı (ID: {UserId})", userId);
                return NotFound($"Kullanıcı bulunamadı.");
            }

            try
            {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geçersiz e-posta onay token'ı: {Token}", token);
                ViewBag.Message = "Token geçersiz veya bozuk.";
                return View("ConfirmEmailResult");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                _logger.LogInformation("Kullanıcı (ID: {UserId}) e-postasını başarıyla doğruladı.", user.Id);
                ViewBag.Message = "E-postanız başarıyla doğrulandı. Artık giriş yapabilirsiniz.";
            }
            else
            {
                _logger.LogWarning("Kullanıcı (ID: {UserId}) e-posta doğrulaması başarısız oldu.", user.Id);
                ViewBag.Message = "E-posta doğrulaması başarısız oldu.";
            }

            return View("ConfirmEmailResult");
        }


        // --- GİRİŞ YAP (Login) (TAMAMEN GÜNCELLENDİ) ---

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UsernameOrEmail)
                           ?? await _userManager.FindByEmailAsync(model.UsernameOrEmail);

                if (user == null || !user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi veya hesap aktif değil.");
                    return View(model);
                }

                // Use CheckPasswordSignInAsync so we can handle IsNotAllowed specially for Admins
                var check = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

                if (check.Succeeded)
                {
                    await _signInManager.SignInAsync(user, model.RememberMe);
                    _logger.LogInformation("Kullanıcı {UserName} giriş yaptı.", user.UserName);
                    user.LastLogin = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                if (check.IsNotAllowed)
                {
                    // If user is admin, allow login even if email not confirmed
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        await _signInManager.SignInAsync(user, model.RememberMe);
                        _logger.LogInformation("Admin kullanıcı {UserName} (email onayı olmadan) giriş yaptı.", user.UserName);
                        user.LastLogin = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }

                    _logger.LogWarning("Kullanıcı {UserName} e-posta onayı olmadan giriş yapmaya çalıştı.", model.UsernameOrEmail);
                    ModelState.AddModelError(string.Empty,
                        "Giriş yapamazsınız. Lütfen önce e-posta adresinizi doğrulayın.");
                    return View(model);
                }

                if (check.IsLockedOut)
                {
                    _logger.LogWarning("Kilitlenen hesap {UserName} için giriş denemesi.", model.UsernameOrEmail);
                    ModelState.AddModelError(string.Empty, "Hesap kilitlendi, lütfen daha sonra tekrar deneyin.");
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            }
            return View(model);
        }

        // --- ÇIKIŞ YAP (Logout) ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Kullanıcı çıkış yaptı.");
            return RedirectToAction("Index", "Home");
        }
    }
}