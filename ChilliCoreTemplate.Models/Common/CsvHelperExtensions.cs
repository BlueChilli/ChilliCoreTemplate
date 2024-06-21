using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChilliSource.Cloud.Web;

namespace ChilliCoreTemplate.Models
{
    public static class CsvHelperExtensions
    {
        public static bool ValidateEmailField(this ValidateArgs field, int maxLength, bool isRequired = true)
        {
            ValidateField(field, maxLength, isRequired);
            if (!new EmailAddressWebAttribute().IsValid(field.Field)) throw new FieldValidationException(field.Row.Context, field.Row.HeaderRecord[field.Row.CurrentIndex], "not a valid email address");
            return true;
        }

        public static bool ValidateField(this ValidateArgs field, int maxLength, bool isRequired = true)
        {
            if (isRequired && String.IsNullOrEmpty(field.Field)) throw new FieldValidationException(field.Row.Context, field.Row.HeaderRecord[field.Row.CurrentIndex], "empty. This is a required field");

            if (field.Field != null && field.Field.Length > maxLength) throw new FieldValidationException(field.Row.Context, field.Row.HeaderRecord[field.Row.CurrentIndex], $"too long. This field is limited to {maxLength} characters");

            return true;
        }

    }
}
