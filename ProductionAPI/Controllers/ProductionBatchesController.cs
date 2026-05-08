using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionAPI.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ProductionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductionBatchesController : ControllerBase
    {
        private static List<ProductionBatch> _batches = new List<ProductionBatch>();
        private static List<QualityTest> _qualityTests = new List<QualityTest>();
        private static List<Deviation> _deviations = new List<Deviation>();
        private static int _nextBatchId = 1;
        private static int _nextTestId = 1;
        private static int _nextDeviationId = 1;

        /// <summary>
        /// Получить все партии
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_batches);
        }

        /// <summary>
        /// Получить партию по ID
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var batch = _batches.FirstOrDefault(b => b.Id == id);
            if (batch == null)
                return NotFound(new { message = "Партия не найдена" });
            return Ok(batch);
        }

        /// <summary>
        /// Получить партии по статусу
        /// </summary>
        [HttpGet("status/{status}")]
        public IActionResult GetByStatus(string status)
        {
            var batches = _batches.Where(b => b.Status == status).ToList();
            return Ok(batches);
        }

        /// <summary>
        /// Создать новую производственную партию
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Technologist")]
        public IActionResult Create([FromBody] ProductionBatch batch)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            batch.Id = _nextBatchId++;
            batch.BatchNumber = $"BATCH-{DateTime.Now:yyyyMMdd}-{batch.Id:D4}";
            batch.Status = "Planned";
            batch.StartDate = DateTime.UtcNow;
            _batches.Add(batch);

            return CreatedAtAction(nameof(GetById), new { id = batch.Id }, batch);
        }

        /// <summary>
        /// Ввод фактических данных от цеха
        /// </summary>
        [HttpPatch("{id}/actual-data")]
        [Authorize(Roles = "Admin,Technologist,Operator")]
        public IActionResult InputActualData(int id, [FromBody] Dictionary<string, decimal> actualData)
        {
            var batch = _batches.FirstOrDefault(b => b.Id == id);
            if (batch == null)
                return NotFound(new { message = "Партия не найдена" });

            // Сохраняем фактические данные как JSON
            batch.ActualParametersJson = JsonSerializer.Serialize(actualData);
            batch.Status = "InProgress";

            // Проверяем отклонения
            CheckAndRecordDeviations(batch, actualData);

            return Ok(new { message = "Данные успешно сохранены", batch });
        }

        /// <summary>
        /// Завершить производственную партию
        /// </summary>
        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Admin,Technologist,Operator")]
        public IActionResult CompleteBatch(int id, [FromBody] decimal actualQuantity)
        {
            var batch = _batches.FirstOrDefault(b => b.Id == id);
            if (batch == null)
                return NotFound(new { message = "Партия не найдена" });

            batch.EndDate = DateTime.UtcNow;
            batch.ActualQuantity = actualQuantity;
            batch.Status = "Completed";

            return Ok(new { message = "Партия успешно завершена", batch });
        }

        /// <summary>
        /// Отменить партию
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,Technologist")]
        public IActionResult CancelBatch(int id, [FromBody] string reason)
        {
            var batch = _batches.FirstOrDefault(b => b.Id == id);
            if (batch == null)
                return NotFound(new { message = "Партия не найдена" });

            batch.Status = "Rejected";
            batch.OperatorNotes = reason;

            return Ok(new { message = "Партия отменена", batch });
        }

        /// <summary>
        /// Добавить результат лабораторного испытания
        /// </summary>
        [HttpPost("{id}/quality-test")]
        [Authorize(Roles = "Admin,LabTech")]
        public IActionResult AddQualityTest(int id, [FromBody] QualityTest test)
        {
            var batch = _batches.FirstOrDefault(b => b.Id == id);
            if (batch == null)
                return NotFound(new { message = "Партия не найдена" });

            test.Id = _nextTestId++;
            test.BatchId = id;
            test.TestDate = DateTime.UtcNow;
            test.PerformedBy = User.Identity?.Name;

            _qualityTests.Add(test);

            // Добавляем тест к партии
            if (batch.QualityTests == null)
                batch.QualityTests = new List<QualityTest>();
            batch.QualityTests.Add(test);

            return Ok(new { message = "Лабораторное испытание добавлено", test });
        }

        /// <summary>
        /// Получить все лабораторные испытания для партии
        /// </summary>
        [HttpGet("{id}/quality-tests")]
        public IActionResult GetQualityTests(int id)
        {
            var tests = _qualityTests.Where(t => t.BatchId == id).ToList();
            return Ok(tests);
        }

        /// <summary>
        /// Получить все отклонения для партии
        /// </summary>
        [HttpGet("{id}/deviations")]
        public IActionResult GetDeviations(int id)
        {
            var deviations = _deviations.Where(d => d.BatchId == id).ToList();
            return Ok(deviations);
        }

        /// <summary>
        /// Устранить отклонение
        /// </summary>
        [HttpPatch("deviations/{deviationId}/resolve")]
        [Authorize(Roles = "Admin,Technologist")]
        public IActionResult ResolveDeviation(int deviationId, [FromBody] string resolutionNotes)
        {
            var deviation = _deviations.FirstOrDefault(d => d.Id == deviationId);
            if (deviation == null)
                return NotFound(new { message = "Отклонение не найдено" });

            deviation.IsResolved = true;
            deviation.ResolutionNotes = resolutionNotes;

            return Ok(new { message = "Отклонение устранено", deviation });
        }

        /// <summary>
        /// Получить статистику по партиям
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetStatistics()
        {
            var stats = new
            {
                TotalBatches = _batches.Count,
                Planned = _batches.Count(b => b.Status == "Planned"),
                InProgress = _batches.Count(b => b.Status == "InProgress"),
                Completed = _batches.Count(b => b.Status == "Completed"),
                Rejected = _batches.Count(b => b.Status == "Rejected"),
                TotalDeviations = _deviations.Count,
                ResolvedDeviations = _deviations.Count(d => d.IsResolved),
                TotalQualityTests = _qualityTests.Count,
                PassingTests = _qualityTests.Count(t => t.IsPassed)
            };

            return Ok(stats);
        }

        // Приватный метод для проверки и записи отклонений
        private void CheckAndRecordDeviations(ProductionBatch batch, Dictionary<string, decimal> actualData)
        {
            // Здесь можно реализовать логику проверки отклонений
            // Например, сравнение с ожидаемыми значениями из рецептуры

            foreach (var param in actualData)
            {
                // Пример: если температура выходит за пределы нормы
                if (param.Key == "temperature" && (param.Value < 160 || param.Value > 200))
                {
                    var deviation = new Deviation
                    {
                        Id = _nextDeviationId++,
                        BatchId = batch.Id,
                        ParameterName = param.Key,
                        ExpectedValue = 180, // Ожидаемая температура
                        ActualValue = param.Value,
                        DeviationValue = param.Value - 180,
                        Severity = param.Value < 160 || param.Value > 200 ? "Critical" : "Warning",
                        Description = $"Температура {param.Value}°C выходит за пределы нормы (160-200°C)",
                        DetectedAt = DateTime.UtcNow,
                        DetectedBy = User.Identity?.Name,
                        IsResolved = false
                    };
                    _deviations.Add(deviation);

                    if (batch.Deviations == null)
                        batch.Deviations = new List<Deviation>();
                    batch.Deviations.Add(deviation);
                }
            }
        }
    }
}