using System.Collections.Generic;

namespace PromoCodeFactory.Core.Domain.PromoCodeManagement
{
    public class Preference
        : BaseEntity
    {
        public string Name { get; set; }

        // Связь с Customer (Many-to-Many через CustomerPreference)
        public ICollection<CustomerPreference> CustomerPreferences { get; set; }
    }
}