using System;
using System.Runtime;
using PromoCodeFactory.Core.Domain;
using PromoCodeFactory.Core.Domain.Administration;

namespace PromoCodeFactory.Core.Domain.PromoCodeManagement
{
    public class PromoCode
        : BaseEntity
    {
        public string Code { get; set; }

        public string ServiceInfo { get; set; }

        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public string PartnerName { get; set; }

        public Employee PartnerManager { get; set; }

        // Связь с Customer (One-to-Many)
        public Guid CustomerId { get; set; }

        public Customer Customer { get; set; }

        // Связь с Preference (One-to-Many)
        public Guid PreferenceId { get; set; }

        public Preference Preference { get; set; }
    }
}