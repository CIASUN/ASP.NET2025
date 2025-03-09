using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;
using PromoCodeFactory.Core.Models.Models;
using PromoCodeFactory.Core.Services;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Партнеры
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PartnersController
        : ControllerBase
    {
        private readonly IRepository<Partner> _partnersRepository;
        private readonly PartnerLimitService _partnerLimitService;

        public PartnersController(IRepository<Partner> partnersRepository, PartnerLimitService partnerLimitService)
        {
            _partnersRepository = partnersRepository;
            _partnerLimitService = partnerLimitService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PartnerResponse>>> GetPartnersAsync()
        {
            var partners = await _partnersRepository.GetAllAsync();

            var response = partners.Select(x => new PartnerResponse()
            {
                Id = x.Id,
                Name = x.Name,
                NumberIssuedPromoCodes = x.NumberIssuedPromoCodes,
                IsActive = true,
                PartnerLimits = x.PartnerLimits
                    .Select(y => new PartnerPromoCodeLimitResponse()
                    {
                        Id = y.Id,
                        PartnerId = y.PartnerId,
                        Limit = y.Limit,
                        CreateDate = y.CreateDate.ToString("dd.MM.yyyy hh:mm:ss"),
                        EndDate = y.EndDate.ToString("dd.MM.yyyy hh:mm:ss"),
                        CancelDate = y.CancelDate?.ToString("dd.MM.yyyy hh:mm:ss"),
                    }).ToList()
            });

            return Ok(response);
        }
        
        [HttpGet("{id}/limits/{limitId}")]
        public async Task<ActionResult<PartnerPromoCodeLimit>> GetPartnerLimitAsync(Guid id, Guid limitId)
        {
            var partner = await _partnersRepository.GetByIdAsync(id);

            if (partner == null)
                return NotFound();
            
            var limit = partner.PartnerLimits
                .FirstOrDefault(x => x.Id == limitId);

            var response = new PartnerPromoCodeLimitResponse()
            {
                Id = limit.Id,
                PartnerId = limit.PartnerId,
                Limit = limit.Limit,
                CreateDate = limit.CreateDate.ToString("dd.MM.yyyy hh:mm:ss"),
                EndDate = limit.EndDate.ToString("dd.MM.yyyy hh:mm:ss"),
                CancelDate = limit.CancelDate?.ToString("dd.MM.yyyy hh:mm:ss"),
            };
            
            return Ok(response);
        }
        
        [HttpPost("{id}/limits")]
        public async Task<IActionResult> SetPartnerPromoCodeLimitAsync(Guid id, SetPartnerPromoCodeLimitRequest request)
        {
            try
            {
                var result = await _partnerLimitService.SetPartnerPromoCodeLimitAsync(id, request);
                return CreatedAtAction(
                    nameof(GetPartnerLimitAsync),
                    new { id = result.PartnerId, limitId = result.LimitId },
                    null);
            }
            catch (InvalidOperationException ex) when (ex.Message == "Партнер не найден")
            {
                return NotFound(ex.Message); // Возвращаем 404, если партнер не найден
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // Возвращаем 400 для других ошибок
            }
        }
        
        [HttpPost("{id}/canceledLimits")]
        public async Task<IActionResult> CancelPartnerPromoCodeLimitAsync(Guid id)
        {
            var partner = await _partnersRepository.GetByIdAsync(id);
            
            if (partner == null)
                return NotFound();
            
            //Если партнер заблокирован, то нужно выдать исключение
            if (!partner.IsActive)
                return BadRequest("Данный партнер не активен");
            
            //Отключение лимита
            var activeLimit = partner.PartnerLimits.FirstOrDefault(x => 
                !x.CancelDate.HasValue);
            
            if (activeLimit != null)
            {
                activeLimit.CancelDate = DateTime.Now;
            }

            await _partnersRepository.UpdateAsync(partner);

            return NoContent();
        }
    }
}