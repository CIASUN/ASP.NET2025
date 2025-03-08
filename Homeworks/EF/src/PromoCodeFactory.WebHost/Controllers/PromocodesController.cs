using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PromoCodeFactory.WebHost.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromoCodesController : ControllerBase
    {
        private readonly IRepository<PromoCode> _promoCodeRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Preference> _preferenceRepository;

        public PromoCodesController(
            IRepository<PromoCode> promoCodeRepository,
            IRepository<Customer> customerRepository,
            IRepository<Preference> preferenceRepository)
        {
            _promoCodeRepository = promoCodeRepository ?? throw new ArgumentNullException(nameof(promoCodeRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _preferenceRepository = preferenceRepository ?? throw new ArgumentNullException(nameof(preferenceRepository));
        }

        /// <summary>
        /// Создает новый промокод и добавляет его клиентам с указанным предпочтением.
        /// </summary>
        /// <param name="preferenceName">Название предпочтения.</param>
        /// <param name="promoCodeRequest">Данные для создания промокода.</param>
        /// <returns>Список клиентов, которым был добавлен промокод.</returns>
        [HttpPost]
        public async Task<IActionResult> GivePromocodesToCustomersWithPreferenceAsync(
            [FromQuery] string preferenceName,
            [FromBody] PromoCodeRequest promoCodeRequest)
        {
            // Создаем новый промокод
            var promoCode = new PromoCode
            {
                Id = Guid.NewGuid(),
                Code = promoCodeRequest.Code,
                BeginDate = DateTime.UtcNow, // Начало действия промокода
                EndDate = DateTime.UtcNow.AddDays(promoCodeRequest.DurationInDays), // Конец действия промокода
                PreferenceId = (await _preferenceRepository.GetAllAsync())
                    .FirstOrDefault(p => p.Name == preferenceName).Id
            };

            if (promoCode.PreferenceId == null)
            {
                return NotFound($"Preference with name '{preferenceName}' not found.");
            }

            // Сохраняем промокод в базе данных
            await _promoCodeRepository.AddAsync(promoCode);

            // Находим клиентов с указанным предпочтением
            var customersWithPreference = (await _customerRepository.GetAllAsync())
                .Where(c => c.CustomerPreferences.Any(cp => cp.Preference.Name == preferenceName))
                .ToList();

            // Добавляем промокод каждому клиенту
            foreach (var customer in customersWithPreference)
            {
                customer.PromoCodes.Add(promoCode);
                await _customerRepository.UpdateAsync(customer);
            }

            return Ok(customersWithPreference.Select(c => new
            {
                c.Id,
                c.FirstName,
                c.LastName,
                c.Email
            }));
        }

        /// <summary>
        /// Получает список промокодов в указанном диапазоне дат.
        /// </summary>
        /// <param name="beginDateStr">Дата начала в формате строки (например, "2023-10-01").</param>
        /// <param name="endDateStr">Дата окончания в формате строки (например, "2023-10-31").</param>
        /// <returns>Список промокодов.</returns>
        [HttpGet]
        public async Task<IActionResult> GetPromocodesAsync(
            [FromQuery] string beginDateStr,
            [FromQuery] string endDateStr)
        {
            // Преобразуем строки дат в DateTime
            if (!DateTime.TryParse(beginDateStr, out var beginDate) ||
                !DateTime.TryParse(endDateStr, out var endDate))
            {
                return BadRequest("Invalid date format. Please use 'yyyy-MM-dd'.");
            }

            // Получаем промокоды из базы данных
            var promoCodes = (await _promoCodeRepository.GetAllAsync())
                .Where(p => p.BeginDate >= beginDate && p.EndDate <= endDate)
                .Select(p => new
                {
                    p.Id,
                    p.Code,
                    BeginDate = p.BeginDate.ToString("yyyy-MM-dd"),
                    EndDate = p.EndDate.ToString("yyyy-MM-dd"),
                    PreferenceName = p.Preference?.Name
                })
                .ToList();

            return Ok(promoCodes);
        }
    }
}