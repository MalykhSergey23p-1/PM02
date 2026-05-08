using System;
using System.ComponentModel.DataAnnotations;

namespace ProductionAPI.Models.Entities
{
    public class Deviation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BatchId { get; set; }

        [MaxLength(100)]
        public string ParameterName { get; set; } = string.Empty;

        public decimal ExpectedValue { get; set; }

        public decimal ActualValue { get; set; }

        public decimal DeviationValue { get; set; }

        [MaxLength(20)]
        public string Severity { get; set; } = "Warning";

        public string? Description { get; set; }

        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? DetectedBy { get; set; }

        public bool IsResolved { get; set; }

        public string? ResolutionNotes { get; set; }

        // Навигационное свойство
        public virtual ProductionBatch? Batch { get; set; }
    }
}