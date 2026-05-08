using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionAPI.Models.Entities
{
    public class RecipeIngredient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RecipeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string IngredientCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string IngredientName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        [MaxLength(10)]
        public string Unit { get; set; } = "kg";

        public decimal? ToleranceMin { get; set; }

        public decimal? ToleranceMax { get; set; }

        // Навигационное свойство (будет связано с Recipe)
        public virtual Recipe? Recipe { get; set; }
    }
}