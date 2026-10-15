using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        public int OrganiserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string EventName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public DateTime RegistrationDeadline { get; set; }

        [Required]
        [MaxLength(200)]
        public string Venue { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Province { get; set; } = string.Empty;

        // NEW for Part 2
        [MaxLength(20)]
        public string? EventType { get; set; } // "Run", "Walk", or "Cycle"

        [Column(TypeName = "decimal(10,2)")]
      
        public decimal? Distance { get; set; } // in km

        public int? MaxParticipants { get; set; }

        public int CurrentParticipants { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? EntryFee { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("OrganiserID")]
        public Organiser? Organiser { get; set; }

        // One event has many categories
        public ICollection<Category> Categories { get; set; } = new List<Category>();

        // One event has many entries
        public ICollection<Entry> Entries { get; set; } = new List<Entry>();
    }
}
