using System;
using System.ComponentModel.DataAnnotations;

namespace ProductionAPI.Models.Entities
{
    public class QualityTest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BatchId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TestName { get; set; } = string.Empty;

        public string? TestResult { get; set; }

        public decimal? NumericResult { get; set; }

        [MaxLength(20)]
        public string? Unit { get; set; }

        public bool IsPassed { get; set; }

        public DateTime TestDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? PerformedBy { get; set; }

        public string? Comments { get; set; }

        // Навигационное свойство
        public virtual ProductionBatch? Batch { get; set; }
    }
}