
namespace PromoCodeFactory.Core.Models.Models
{
    public class SetPartnerPromoCodeLimitRequest
    {
        public DateTime EndDate { get; set; }
        public int Limit { get; set; }
    }
}