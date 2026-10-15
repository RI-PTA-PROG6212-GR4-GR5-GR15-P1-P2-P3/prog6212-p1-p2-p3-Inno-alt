using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Participant
    {
        [Key]
        public int ParticipantID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(13)]
        public string IDNumber { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(100)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(15)]
        public string? EmergencyContactNumber { get; set; }

        [MaxLength(500)]
        public string? MedicalConditions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserID")]
        public User? User { get; set; }

        // One participant can have many entries
        public ICollection<Entry> Entries { get; set; } = new List<Entry>();
    }
}
