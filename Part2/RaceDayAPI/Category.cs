using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        [MaxLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        // For age categories (e.g., "Under 20", "Senior")
        public int? AgeGroupMin { get; set; }
        public int? AgeGroupMax { get; set; }

        // For distance categories (e.g., 10km, 21km)
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Distance { get; set; }

        [MaxLength(10)]
        public string? GenderRestriction { get; set; } // "Male", "Female", "Open"

        [Column(TypeName = "decimal(10,2)")]
        public decimal? EntryFee { get; set; }

        public int? MaxParticipants { get; set; }

        public int CurrentParticipants { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("EventID")]
        public Event? Event { get; set; }

        // One category has many entries
        public ICollection<Entry> Entries { get; set; } = new List<Entry>();
    }
}
