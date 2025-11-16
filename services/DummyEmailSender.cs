using System.Diagnostics;

public class DummyEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Geliştirme aşamasında e-postayı Visual Studio'nun 'Output' penceresine yazdırır.
        Debug.WriteLine("--- YENİ E-POSTA GÖNDERİMİ ---");
        Debug.WriteLine($"Alıcı: {email}");
        Debug.WriteLine($"Konu: {subject}");
        Debug.WriteLine($"İçerik (HTML): {htmlMessage}");
        Debug.WriteLine("---------------------------------");

        return Task.CompletedTask;
    }
}