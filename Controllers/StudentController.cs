using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Encodings.Web;
using UnivForm.Data;
using UnivForm.Models.ViewModels;

namespace UnivForm.Controllers
{
    public class StudentController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<StudentController> _logger;
        private readonly AppDbContext _context;

        public StudentController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailSender emailSender,
            ILogger<StudentController> logger,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _context = context;
        }

        // --- STUDENT REGISTER (GET) ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // --- STUDENT REGISTER (POST) ---
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(StudentRegisterViewModel model)
        {
            // Öğrenci tipine göre validasyon
            if (model.StudentTypeSelect == "HighSchool")
            {
                if (string.IsNullOrEmpty(model.HighSchoolName))
                    ModelState.AddModelError("HighSchoolName", "Lise adı zorunludur.");
                if (!model.ExamScore.HasValue)
                    ModelState.AddModelError("ExamScore", "Sınav puanı zorunludur.");
            }
            else if (model.StudentTypeSelect == "University")
            {
                if (string.IsNullOrEmpty(model.University))
                    ModelState.AddModelError("University", "Üniversite adı zorunludur.");
                if (string.IsNullOrEmpty(model.Department))
                    ModelState.AddModelError("Department", "Bölüm adı zorunludur.");
                if (!model.Grade.HasValue)
                    ModelState.AddModelError("Grade", "Sınıf zorunludur.");
            }

            if (ModelState.IsValid)
            {
                // AppUser oluştur
                var user = new AppUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    StudentType = model.StudentTypeSelect == "HighSchool" ?
                        StudentType.HighSchool : StudentType.University
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Öğrenci tipine göre kayıt yap
                    if (model.StudentTypeSelect == "HighSchool")
                    {
                        var highSchoolStudent = new HighSchoolStudent
                        {
                            TCKimlikNo = model.TCKimlikNo,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Email = model.Email,
                            PhoneNumber = model.PhoneNumber,
                            BirthDate = model.BirthDate,
                            BirthPlace = model.BirthPlace,
                            Gender = model.Gender,
                            HighSchoolName = model.HighSchoolName ?? "",
                            ExamScore = model.ExamScore ?? 0,
                            Address = model.Address ?? "",
                            RegistrationDate = DateTime.Now,
                            AppUserId = user.Id
                        };

                        _context.HighSchoolStudents.Add(highSchoolStudent);
                        user.HighSchoolStudent = highSchoolStudent;
                    }
                    else if (model.StudentTypeSelect == "University")
                    {
                        var universityStudent = new UniversityStudent
                        {
                            TCKimlikNo = model.TCKimlikNo,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Email = model.Email,
                            PhoneNumber = model.PhoneNumber,
                            BirthDate = model.BirthDate,
                            BirthPlace = model.BirthPlace,
                            Gender = model.Gender,
                            University = model.University ?? "",
                            Department = model.Department ?? "",
                            Grade = model.Grade ?? 1,
                            StudentId = model.StudentId ?? "",
                            Address = model.Address ?? "",
                            RegistrationDate = DateTime.Now,
                            AppUserId = user.Id
                        };

                        _context.UniversityStudents.Add(universityStudent);
                        user.UniversityStudent = universityStudent;
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Yeni öğrenci ({Type}) oluşturuldu: {Email}", model.StudentTypeSelect, user.Email);

                    // Rol atama
                    await _userManager.AddToRoleAsync(user, "Student");

                    // E-posta onay kodu
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                    var callbackUrl = Url.Action(
                        action: nameof(ConfirmEmail),
                        controller: "Student",
                        values: new { userId = user.Id, token = token },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(model.Email,
                        "Lütfen E-posta Adresinizi Doğrulayın",
                        $"E-posta adresinizi doğrulamak için lütfen <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? "")}'>buraya tıklayın</a>.");

                    // Geliştirme ortamında linki ekranda göster
                    TempData["ConfirmationLink"] = callbackUrl;

                    return RedirectToAction(nameof(RegistrationConfirmation));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // --- REGISTRATION CONFIRMATION (GET) ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegistrationConfirmation()
        {
            return View();
        }

        // --- CONFIRM EMAIL (GET) ---
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
                _logger.LogError(ex, "Geçersiz e-posta onay token'ı");
                ViewBag.Message = "Token geçersiz veya bozuk.";
                return View("ConfirmEmailResult");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                _logger.LogInformation("Öğrenci (ID: {UserId}) e-postasını başarıyla doğruladı.", user.Id);
                ViewBag.Message = "E-postanız başarıyla doğrulandı. Artık giriş yapabilirsiniz.";
            }
            else
            {
                _logger.LogWarning("Öğrenci (ID: {UserId}) e-posta doğrulaması başarısız oldu.", user.Id);
                ViewBag.Message = "E-posta doğrulaması başarısız oldu.";
            }

            return View("ConfirmEmailResult");
        }

        // --- STUDENT LOGIN (GET) ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // --- STUDENT LOGIN (POST) ---
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

                // Kullanıcının Student rolü olup olmadığını kontrol et
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Student"))
                {
                    ModelState.AddModelError(string.Empty, "Bu hesap öğrenci hesabı değildir.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Öğrenci {UserName} giriş yaptı.", user.UserName);
                    user.LastLogin = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Dashboard");
                    }
                }

                if (result.IsNotAllowed)
                {
                    _logger.LogWarning("Öğrenci {UserName} e-posta onayı olmadan giriş yapmaya çalıştı.", model.UsernameOrEmail);
                    ModelState.AddModelError(string.Empty,
                        "Giriş yapamazsınız. Lütfen önce e-posta adresinizi doğrulayın.");
                    return View(model);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Kilitlenen öğrenci hesabı {UserName} için giriş denemesi.", model.UsernameOrEmail);
                    ModelState.AddModelError(string.Empty, "Hesap kilitlendi, lütfen daha sonra tekrar deneyin.");
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            }

            return View(model);
        }

        // --- STUDENT DASHBOARD (GET) ---
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            var student = _context.Students.FirstOrDefault(s => s.AppUserId == user.Id);
            if (student == null)
            {
                return NotFound("Öğrenci kaydı bulunamadı.");
            }

            return View(student);
        }

        // --- LOGOUT ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Öğrenci çıkış yaptı.");
            return RedirectToAction("Index", "Home");
        }
    }
}
