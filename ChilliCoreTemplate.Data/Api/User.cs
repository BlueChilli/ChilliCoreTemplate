using ChilliCoreTemplate.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChilliCoreTemplate.Data.EmailAccount
{
    // This is the EF model
    public partial class User
    {
        [StringLength(100)]
        public string PhoneVerificationCode { get; set; }

        public Guid? PhoneVerificationToken { get; set; }

        public DateTime? PhoneVerificationExpiry { get; set; }
        public int PhoneVerificationRetries { get; set; }
    }
}

