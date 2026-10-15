using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Organiser
    {
        [Key]
        public int OrganiserID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string OrganisationName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? RegistrationNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Website { get; set; }

        public bool IsVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserID")]
        public User? User { get; set; }

        // One organiser creates many events
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}