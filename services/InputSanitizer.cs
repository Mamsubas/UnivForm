using System.Text.RegularExpressions;

namespace UnivForm.Services
{
    public interface IInputSanitizer
    {
        string Sanitize(string? input);
        string SanitizeHtml(string? input);
    }

    public class InputSanitizer : IInputSanitizer
    {
        /// <summary>
        /// Temel input sanitization - zararlı karakterleri kaldırır
        /// </summary>
        public string Sanitize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            // XSS saldırılarını engelle
            string sanitized = input
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;")
                .Replace("&", "&amp;");

            // Yönetim karakterlerini kaldır
            sanitized = Regex.Replace(sanitized, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", "");

            return sanitized.Trim();
        }

        /// <summary>
        /// HTML içeriğini sanitize eder - bazı etiketleri izin verir
        /// </summary>
        public string SanitizeHtml(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            // İzin verilen etiketler
            var allowedTags = new[] { "p", "br", "strong", "em", "b", "i", "u", "a", "ul", "ol", "li" };

            // Zararlı etiketleri kaldır
            string sanitized = input;

            // Script, iframe, form etiketlerini tamamen kaldır
            sanitized = Regex.Replace(sanitized, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"<iframe[^>]*>.*?</iframe>", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"<form[^>]*>.*?</form>", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"<input[^>]*>", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"<button[^>]*>.*?</button>", "", RegexOptions.IgnoreCase);

            // on* event handlers'ı kaldır
            sanitized = Regex.Replace(sanitized, @"\s+on\w+\s*=\s*[""'][^""']*[""']", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"\s+on\w+\s*=\s*[^\s>]*", "", RegexOptions.IgnoreCase);

            return sanitized.Trim();
        }
    }
}
