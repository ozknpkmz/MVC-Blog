using System.ComponentModel.DataAnnotations;

namespace FineBlog.Models.ViewModels
{
    public class ResetPasswordVM
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        [Required]
        public string? NewPassword { get; set; }
        [Compare(nameof(NewPassword))]
        [Required]
        public string? ConfirmPasswor { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
