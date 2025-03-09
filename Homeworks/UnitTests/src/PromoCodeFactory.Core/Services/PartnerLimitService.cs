using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.Core.Models.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace PromoCodeFactory.Core.Services
{
    public class PartnerLimitService
    {
        private readonly IRepository<Partner> _partnersRepository;

        public PartnerLimitService(IRepository<Partner> partnersRepository)
        {
            _partnersRepository = partnersRepository;
        }

        public async Task<SetPartnerPromoCodeLimitResult> SetPartnerPromoCodeLimitAsync(Guid id, SetPartnerPromoCodeLimitRequest request)
        {
            var partner = await _partnersRepository.GetByIdAsync(id);

            if (partner == null)
                throw new InvalidOperationException("Партнер не найден");

            if (!partner.IsActive)
                throw new InvalidOperationException("Данный партнер не активен");

            var activeLimit = partner.PartnerLimits.FirstOrDefault(x => !x.CancelDate.HasValue);

            if (activeLimit != null)
            {
                partner.NumberIssuedPromoCodes = 0;
                activeLimit.CancelDate = DateTime.Now;
            }

            if (request.Limit <= 0)
                throw new InvalidOperationException("Лимит должен быть больше 0");

            var newLimit = new PartnerPromoCodeLimit
            {
                Limit = request.Limit,
                Partner = partner,
                PartnerId = partner.Id,
                CreateDate = DateTime.Now,
                EndDate = request.EndDate
            };

            partner.PartnerLimits.Add(newLimit);

            await _partnersRepository.UpdateAsync(partner);

            return new SetPartnerPromoCodeLimitResult
            {
                PartnerId = partner.Id,
                LimitId = newLimit.Id
            };
        }

    }
}
