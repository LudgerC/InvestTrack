using System.ComponentModel.DataAnnotations;

namespace InvestTrack.Web.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Naam is verplicht")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mailadres is verplicht")]
        [EmailAddress(ErrorMessage = "Geldig e-mailadres vereist")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wachtwoord is verplicht")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Het wachtwoord moet minstens 6 tekens bevatten.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "De wachtwoorden komen niet overeen.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
