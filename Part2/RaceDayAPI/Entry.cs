using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Entry
    {
        [Key]
        public int EntryID { get; set; }

        [Required]
        public int ParticipantID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled

        public int? BibNumber { get; set; }

        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Refunded

        public DateTime? PaymentDate { get; set; }

        [MaxLength(50)]
        public string? PaymentReference { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? EntryFeePaid { get; set; }

        [MaxLength(10)]
        public string? TShirtSize { get; set; }

        [MaxLength(200)]
        public string? DietaryRequirements { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ParticipantID")]
        public Participant? Participant { get; set; }

        [ForeignKey("EventID")]
        public Event? Event { get; set; }

        [ForeignKey("CategoryID")]
        public Category? Category { get; set; }

        // One entry has one result
        public Result? Result { get; set; }
    }
}
