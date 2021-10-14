using ChilliSource.Cloud.Core.Phone;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ChilliCoreTemplate.Models
{
    public class PhoneNumberAttribute : ValidationAttribute
    {
        public PhoneNumberAttribute(string region, params PhoneNumberType[] phoneTypesToCheck)
        {
            this.Region = region;
            this.PhoneTypesToCheck = phoneTypesToCheck?.Length > 0 ? phoneTypesToCheck
                                        : new PhoneNumberType[] { PhoneNumberType.FIXED_LINE_OR_MOBILE, PhoneNumberType.MOBILE, PhoneNumberType.FIXED_LINE };
        }

        public string Region { get; set; }
        public PhoneNumberType[] PhoneTypesToCheck { get; set; }

        public override string FormatErrorMessage(string name)
        {
            return this.ErrorMessage ?? String.Format("The {0} field contains an invalid phone number.", name);
        }

        public override bool IsValid(object value)
        {
            var s = value as string;
            if (String.IsNullOrEmpty(s))
                return true;

            return s.IsValidPhoneNumber(this.Region, this.PhoneTypesToCheck);
        }
    }
}
