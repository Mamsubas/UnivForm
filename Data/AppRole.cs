using Microsoft.AspNetCore.Identity;

namespace UnivForm.Data
{
    public class AppRole : IdentityRole<int>
    {
        public string Description { get; set; } = "";
    }
}