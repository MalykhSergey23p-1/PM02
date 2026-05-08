using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionAPI.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecipesController : ControllerBase
    {
        private static List<Recipe> _recipes = new List<Recipe>();
        private static List<RecipeIngredient> _ingredients = new List<RecipeIngredient>();
        private static int _nextRecipeId = 1;
        private static int _nextIngredientId = 1;

        /// <summary>
        /// Получить все рецептуры
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_recipes);
        }

        /// <summary>
        /// Получить активные рецептуры
        /// </summary>
        [HttpGet("active")]
        public IActionResult GetActive()
        {
            var activeRecipes = _recipes.Where(r => r.IsActive).ToList();
            return Ok(activeRecipes);
        }

        /// <summary>
        /// Получить рецептуру по ID
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var recipe = _recipes.FirstOrDefault(r => r.Id == id);
            if (recipe == null)
                return NotFound(new { message = "Рецептура не найдена" });

            // Загружаем ингредиенты
            recipe.Ingredients = _ingredients.Where(i => i.RecipeId == id).ToList();
            return Ok(recipe);
        }

        /// <summary>
        /// Создать новую рецептуру
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Technologist")]
        public IActionResult Create([FromBody] Recipe recipe)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            recipe.Id = _nextRecipeId++;
            recipe.CreatedAt = DateTime.UtcNow;
            recipe.CreatedBy = User.Identity?.Name;
            recipe.Version = 1;

            _recipes.Add(recipe);

            // Сохраняем ингредиенты
            if (recipe.Ingredients != null)
            {
                foreach (var ingredient in recipe.Ingredients)
                {
                    ingredient.Id = _nextIngredientId++;
                    ingredient.RecipeId = recipe.Id;
                    _ingredients.Add(ingredient);
                }
            }

            return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, recipe);
        }

        /// <summary>
        /// Обновить рецептуру
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Technologist")]
        public IActionResult Update(int id, [FromBody] Recipe updatedRecipe)
        {
            var recipe = _recipes.FirstOrDefault(r => r.Id == id);
            if (recipe == null)
                return NotFound(new { message = "Рецептура не найдена" });

            recipe.Name = updatedRecipe.Name;
            recipe.Description = updatedRecipe.Description;
            recipe.BatchSize = updatedRecipe.BatchSize;
            recipe.IsActive = updatedRecipe.IsActive;
            recipe.Version++;

            return Ok(recipe);
        }

        /// <summary>
        /// Добавить ингредиент в рецептуру
        /// </summary>
        [HttpPost("{id}/ingredients")]
        [Authorize(Roles = "Admin,Technologist")]
        public IActionResult AddIngredient(int id, [FromBody] RecipeIngredient ingredient)
        {
            var recipe = _recipes.FirstOrDefault(r => r.Id == id);
            if (recipe == null)
                return NotFound(new { message = "Рецептура не найдена" });

            ingredient.Id = _nextIngredientId++;
            ingredient.RecipeId = id;
            _ingredients.Add(ingredient);

            return Ok(ingredient);
        }

        /// <summary>
        /// Удалить рецептуру
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var recipe = _recipes.FirstOrDefault(r => r.Id == id);
            if (recipe == null)
                return NotFound(new { message = "Рецептура не найдена" });

            // Удаляем связанные ингредиенты
            var ingredients = _ingredients.Where(i => i.RecipeId == id).ToList();
            foreach (var ing in ingredients)
            {
                _ingredients.Remove(ing);
            }

            _recipes.Remove(recipe);
            return NoContent();
        }
    }
}