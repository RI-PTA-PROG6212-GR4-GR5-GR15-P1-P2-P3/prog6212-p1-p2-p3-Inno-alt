using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs
{
    public class RegisterDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // "Participant" or "Organiser"

        // Participant fields
        public string? FullName { get; set; }
        public string? IDNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }

        // Organiser fields
        public string? OrganisationName { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? ContactPerson { get; set; }
        public string? Website { get; set; }
    }
}
