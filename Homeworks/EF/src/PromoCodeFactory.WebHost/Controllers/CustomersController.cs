using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.DataAccess.Repositories;
using PromoCodeFactory.WebHost.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Клиенты
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CustomersController
        : ControllerBase
    {
        private readonly EfRepository<Customer> _customerRepository;
        private readonly EfRepository<PromoCode> _promoCodeRepository;

        public CustomersController(EfRepository<Customer> customerRepository, EfRepository<PromoCode> promoCodeRepository)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _promoCodeRepository = promoCodeRepository ?? throw new ArgumentNullException(nameof(promoCodeRepository));
        }

        /// <summary>
        /// Получить список всех клиентов.
        /// </summary>
        /// <returns>Список клиентов.</returns>
        [HttpGet]
        public async Task<IActionResult> GetCustomersAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            var response = customers.Select(c => new CustomerResponse
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Preferences = c.CustomerPreferences.Select(cp => new PreferenceResponse
                {
                    Id = cp.Preference.Id,
                    Name = cp.Preference.Name
                }).ToList()
            });

            return Ok(response);
        }

        /// <summary>
        /// Получить клиента по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор клиента.</param>
        /// <returns>Клиент.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerAsync(Guid id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var response = new CustomerResponse
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Preferences = customer.CustomerPreferences.Select(cp => new PreferenceResponse
                {
                    Id = cp.Preference.Id,
                    Name = cp.Preference.Name
                }).ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Создать нового клиента.
        /// </summary>
        /// <param name="request">Данные для создания клиента.</param>
        /// <returns>Созданный клиент.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCustomerAsync([FromBody] CustomerRequest request)
        {
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };

            await _customerRepository.AddAsync(customer);
            return Ok(customer);
        }

        /// <summary>
        /// Обновить данные клиента.
        /// </summary>
        /// <param name="id">Идентификатор клиента.</param>
        /// <param name="request">Данные для обновления.</param>
        /// <returns>Обновленный клиент.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomerAsync(Guid id, [FromBody] CustomerRequest request)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.Email = request.Email;

            await _customerRepository.UpdateAsync(customer);
            return Ok(customer);
        }

        /// <summary>
        /// Удалить клиента по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор клиента.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomerAsync(Guid id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            // Удаление связанных промокодов
            var promoCodes = await _promoCodeRepository.GetAllAsync();
            var customerPromoCodes = promoCodes.Where(p => p.CustomerId == id).ToList();
            foreach (var promoCode in customerPromoCodes)
            {
                await _promoCodeRepository.DeleteAsync(promoCode.Id);
            }

            await _customerRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}