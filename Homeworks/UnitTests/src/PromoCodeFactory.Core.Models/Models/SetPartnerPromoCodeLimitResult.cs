using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromoCodeFactory.Core.Models.Models
{
    public class SetPartnerPromoCodeLimitResult
    {
        public Guid PartnerId { get; set; }
        public Guid LimitId { get; set; }
    }
}
