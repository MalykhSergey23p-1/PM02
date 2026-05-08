using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ProductionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReferenceDataController : ControllerBase
    {
        /// <summary>
        /// Получить список статусов партий
        /// </summary>
        [HttpGet("batch-statuses")]
        [AllowAnonymous]
        public IActionResult GetBatchStatuses()
        {
            var statuses = new[]
            {
                new { Code = "Planned", Name = "Запланирована", Description = "Партия создана, ожидает запуска" },
                new { Code = "InProgress", Name = "В процессе", Description = "Партия в производстве" },
                new { Code = "Completed", Name = "Завершена", Description = "Партия успешно завершена" },
                new { Code = "Rejected", Name = "Отменена", Description = "Партия отменена или забракована" }
            };
            return Ok(statuses);
        }

        /// <summary>
        /// Получить список типов продукции
        /// </summary>
        [HttpGet("product-types")]
        [AllowAnonymous]
        public IActionResult GetProductTypes()
        {
            var types = new[]
            {
                new { Code = "BAKERY", Name = "Хлебобулочные изделия" },
                new { Code = "DAIRY", Name = "Молочная продукция" },
                new { Code = "MEAT", Name = "Мясная продукция" },
                new { Code = "BEVERAGE", Name = "Напитки" },
                new { Code = "OTHER", Name = "Прочее" }
            };
            return Ok(types);
        }

        /// <summary>
        /// Получить список ролей пользователей
        /// </summary>
        [HttpGet("user-roles")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetUserRoles()
        {
            var roles = new[]
            {
                new { Name = "Admin", Description = "Полный доступ ко всем функциям" },
                new { Name = "Technologist", Description = "Управление рецептурами и партиями" },
                new { Name = "Operator", Description = "Ввод данных с производства" },
                new { Name = "LabTech", Description = "Регистрация лабораторных испытаний" }
            };
            return Ok(roles);
        }

        /// <summary>
        /// Получить единицы измерения
        /// </summary>
        [HttpGet("units")]
        [AllowAnonymous]
        public IActionResult GetUnits()
        {
            var units = new[]
            {
                new { Code = "kg", Name = "Килограмм", Category = "Weight" },
                new { Code = "g", Name = "Грамм", Category = "Weight" },
                new { Code = "L", Name = "Литр", Category = "Volume" },
                new { Code = "ml", Name = "Миллилитр", Category = "Volume" },
                new { Code = "pcs", Name = "Штуки", Category = "Count" }
            };
            return Ok(units);
        }
    }
}