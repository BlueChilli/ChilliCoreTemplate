using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Api
{
    public class ErrorResult
    {
        public ErrorResult()
        {
            this.FieldErrors = new Dictionary<string, List<string>>();
            this.GlobalErrors = new List<ErrorResultItem>();
        }

        public string SchemaVersion => "1.1";

        public Dictionary<string, List<string>> FieldErrors { get; }

        public List<ErrorResultItem> GlobalErrors { get; set; }

        public static ErrorResult Create(params string[] errors)
        {
            return Create((IEnumerable<string>)errors);
        }

        public static ErrorResult Create(IEnumerable<string> errors)
        {
            var result = new ErrorResult();
            if (errors != null)
            {
                var values = errors.Where(s => !string.IsNullOrEmpty(s))
                                .Select(s => new ErrorResultItem() { Message = s }).ToList();
                if (values.Count > 0)
                {
                    result.GlobalErrors.AddRange(values);
                }
            }

            return result;
        }

        public static ErrorResult Create(ModelStateDictionary modelState)
        {
            var result = new ErrorResult();

            foreach (var kvp in modelState)
            {
                var key = kvp.Key;
                var errors = kvp.Value.Errors;
                if (errors != null && errors.Count > 0)
                {
                    var errorMessages = errors.Select(error => error.ErrorMessage).Where(s => !String.IsNullOrEmpty(s)).ToList();

                    if (errorMessages.Count > 0)
                    {
                        if (String.IsNullOrEmpty(key))
                        {
                            result.GlobalErrors.AddRange(errorMessages.Select(s => new ErrorResultItem() { Message = s }).ToList());
                        }
                        else
                        {
                            result.FieldErrors.Add(key, errorMessages);
                        }
                    }
                }
            }

            return result;
        }

        public static ErrorResult Create<T>(ServiceResult<T> serviceResult)
        {
            var result = new ErrorResult();
            if (!serviceResult.Success) result.GlobalErrors.Add(ErrorResultItem.Create(serviceResult));
            return result;
        }

    }

    public class ErrorResultItem
    {
        public ErrorResultItem() { }

        public string ErrorCode { get; set; }
        public string ErrorKey { get; set; }
        public string Message { get; set; }

        public static ErrorResultItem Create<T>(ServiceResult<T> result)
        {
            return new ErrorResultItem()
            {
                ErrorCode = ((int)result.StatusCode).ToString(),
                ErrorKey = result.Key,
                Message = result.Error,
            };
        }
    }
}
