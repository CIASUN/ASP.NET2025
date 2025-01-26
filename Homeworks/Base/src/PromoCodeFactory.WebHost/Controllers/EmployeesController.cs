using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.WebHost.Models;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Сотрудники
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Role> _roleRepository;
        public EmployeesController(IRepository<Employee> employeeRepository, IRepository<Role> rolesRepository)
        {
            _employeeRepository = employeeRepository;
            _roleRepository = rolesRepository;
        }

        /// <summary>
        /// Получить данные всех сотрудников
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<EmployeeShortResponse>> GetEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();

            var employeesModelList = employees.Select(x =>
                new EmployeeShortResponse()
                {
                    Id = x.Id,
                    Email = x.Email,
                    FullName = x.FullName,
                }).ToList();

            return employeesModelList;
        }

        /// <summary>
        /// Получить данные сотрудника по Id
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EmployeeResponse>> GetEmployeeByIdAsync(Guid id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);

            if (employee == null)
                return NotFound();

            var employeeModel = new EmployeeResponse()
            {
                Id = employee.Id,
                Email = employee.Email,
                Roles = employee.Roles.Select(x => new RoleItemResponse()
                {
                    Name = x.Name,
                    Description = x.Description
                }).ToList(),
                FullName = employee.FullName,
                AppliedPromocodesCount = employee.AppliedPromocodesCount
            };

            return employeeModel;
        }

        /// <summary>
        /// Создать нового сотрудника.
        /// </summary>
        /// <param name="request">Данные для создания сотрудника.</param>
        /// <returns>Созданный сотрудник с присвоенным ID.</returns>
        /// <response code="201">Сотрудник успешно создан.</response>
        /// <response code="400">Ошибка валидации данных сотрудника.</response>
        [HttpPost]
        public async Task<ActionResult<EmployeeResponse>> CreateEmployeeAsync(EmployeeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Фамилия, имя и email обязательны.");

            var newEmployee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };

            if (request.RoleIds != null)
            {
                var roles = await _roleRepository.GetByIdsAsync(request.RoleIds);
                newEmployee.Roles = roles.ToList();
            }

            await _employeeRepository.AddAsync(newEmployee);

            var employeeResponse = new EmployeeResponse
            {
                Id = newEmployee.Id,
                FullName = newEmployee.FullName,
                Email = newEmployee.Email,
                Roles = newEmployee.Roles.Select(r => new RoleItemResponse
                { 
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description
                }).ToList()
            };

            return Ok(employeeResponse);
        }

        /// <summary>
        /// Обновить данные сотрудника.
        /// </summary>
        /// <param name="id">Идентификатор сотрудника.</param>
        /// <param name="request">Новые данные для обновления.</param>
        /// <returns>Обновлённый объект сотрудника.</returns>
        /// <response code="200">Данные сотрудника успешно обновлены.</response>
        /// <response code="404">Сотрудник с указанным ID не найден.</response>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult> UpdateEmployeeAsync(Guid id, EmployeeRequest request)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);

            if (employee == null)
                return NotFound($"Сотрудник с ID {id} не найден.");

            employee.FirstName = request.FirstName ?? employee.FirstName;
            employee.LastName = request.LastName ?? employee.LastName;
            employee.Email = request.Email ?? employee.Email;

            if (request.RoleIds != null)
            {
                var roles = await _roleRepository.GetByIdsAsync(request.RoleIds);
                employee.Roles = roles.ToList();
            }

            await _employeeRepository.UpdateAsync(employee);

            var employeeResponse = new EmployeeResponse
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Email = employee.Email,
                Roles = employee.Roles.Select(r => new RoleItemResponse
                {
                    Name = r.Name,
                    Description = r.Description
                }).ToList()
            };

            return Ok(employeeResponse);
        }

        /// <summary>
        /// Удалить сотрудника.
        /// </summary>
        /// <param name="id">Идентификатор сотрудника.</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <response code="204">Сотрудник успешно удалён.</response>
        /// <response code="404">Сотрудник с указанным ID не найден.</response>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteEmployeeAsync(Guid id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);

            if (employee == null)
                return NotFound($"Сотрудник с ID {id} не найден.");

            await _employeeRepository.DeleteAsync(employee);

            return NoContent();
        }
    }
}