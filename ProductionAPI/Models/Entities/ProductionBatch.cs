using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionAPI.Models.Entities
{
    public class ProductionBatch
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string BatchNumber { get; set; } = string.Empty;

        [Required]
        public int RecipeId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal PlannedQuantity { get; set; }

        public decimal ActualQuantity { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Planned";

        [MaxLength(100)]
        public string? ResponsiblePerson { get; set; }

        public string? OperatorNotes { get; set; }

        // Фактические данные от цеха (храним как JSON строку)
        public string? ActualParametersJson { get; set; }

        // Навигационные свойства (связи)
        public virtual Recipe? Recipe { get; set; }
        public virtual ICollection<QualityTest> QualityTests { get; set; } = new List<QualityTest>();
        public virtual ICollection<Deviation> Deviations { get; set; } = new List<Deviation>();
    }
}