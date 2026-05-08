using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductionAPI.Models.Entities
{
    public class Recipe
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ProductType { get; set; } // Тип продукции

        public decimal BatchSize { get; set; } // Размер партии (кг)

        public string Unit { get; set; } = "kg";

        public int Version { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedBy { get; set; }

        // Связи с другими таблицами
        public virtual ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
        public virtual ICollection<ProductionBatch> Batches { get; set; } = new List<ProductionBatch>();
    }
}