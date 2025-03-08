using PromoCodeFactory.Core.Domain;
using System;
using System.Collections.Generic;

namespace PromoCodeFactory.Core.Domain.PromoCodeManagement
{
    public class Customer
        : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string Email { get; set; }

        // Связь с Preference (Many-to-Many через CustomerPreference)
        public ICollection<CustomerPreference> CustomerPreferences { get; set; }

        // Связь с PromoCode (One-to-Many)
        public ICollection<PromoCode> PromoCodes { get; set; }
    }
}