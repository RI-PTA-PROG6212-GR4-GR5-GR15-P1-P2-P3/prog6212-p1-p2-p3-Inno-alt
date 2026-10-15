using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public int EntryID { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? Position { get; set; }

        public TimeSpan? GunTime { get; set; }

        public TimeSpan? ChipTime { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Pace { get; set; } // minutes per km

        [MaxLength(20)]
        public string? Status { get; set; } // Finished, DNS, DNF, Disqualified

        public int? OverallPosition { get; set; }

        public int? AgeGroupPosition { get; set; }

        public int? GenderPosition { get; set; }

        [MaxLength(500)]
        public string? CertificateURL { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("EntryID")]
        public Entry? Entry { get; set; }
    }
}
