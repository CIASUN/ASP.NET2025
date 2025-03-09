using System;
using System.Collections.Generic;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.Core.Models.Models;

namespace PromoCodefactory.UnitTests.Builders
{
    public static class PartnerTestDataBuilder
    {
        public static Partner CreatePartner(bool isActive = true, int numberIssuedPromoCodes = 0)
        {
            return new Partner
            {
                Id = Guid.NewGuid(),
                IsActive = isActive,
                NumberIssuedPromoCodes = numberIssuedPromoCodes,
                PartnerLimits = new List<PartnerPromoCodeLimit>()
            };
        }

        public static SetPartnerPromoCodeLimitRequest CreateRequest(int limit = 10, DateTime? endDate = null)
        {
            return new SetPartnerPromoCodeLimitRequest
            {
                Limit = limit,
                EndDate = endDate ?? DateTime.Now.AddDays(30)
            };
        }
    }
}
