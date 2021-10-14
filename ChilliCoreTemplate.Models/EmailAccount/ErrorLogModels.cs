using ChilliSource.Core.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.EmailAccount
{
    public class ErrorLogSummaryModel
    {
        public int Id { get; set; }

        [JsonIgnore]
        public DateTime Date { get; set; }

        public string DateDisplay { get { return Date.ToTimezone().ToIsoDateTime(); } }

        public int? UserId { get; set; }

        public string UserEmail { get; set; }

        public string Message { get; set; }

    }

    public class ErrorLogExpandedModel
    {
        public IDictionary<string, object> Properties { get; set; }

        public string Exception { get; set; }

        public List<ErrorLogPropertyModel> FlattenProperties(IDictionary<string, object> source, List<ErrorLogPropertyModel> destination, int level)
        {
            foreach(var key in source.Keys)
            {
                if (source[key] is IDictionary<string, object>)
                {
                    destination.Add(new ErrorLogPropertyModel
                    {
                        Level = level,
                        Property = key
                    });
                    destination = FlattenProperties(source[key] as IDictionary<string, object>, destination, level + 1);
                }
                else if (source[key] != null)
                {
                    destination.Add(new ErrorLogPropertyModel
                    {
                        Level = level,
                        Property = key,
                        Value = source[key].ToString()
                    });
                }
            }
            return destination;
        }

        public List<ErrorLogPropertyModel> GetFlattenProperties()
        {
            return FlattenProperties(Properties, new List<ErrorLogPropertyModel>(), 0);
        }
    }

    public class ErrorLogPropertyModel
    {
        public int Level { get; set; }

        public string Property { get; set; }

        public string Value { get; set; }

    }

    public class ErrorLogAlertEmail
    {
        public List<ErrorLogSummaryModel> Errors { get; set; }

        public string ErrorCount => Errors.Count == 1 ? "1 error" : Errors.Count == 99 ? "100 errors and more" : $"{Errors.Count} errors";
    }
}
