using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Api
{
    public interface IInterceptApiModel
    {
        [JsonIgnore]
        List<string> PopulatedMembers { get; set; }
    }    
}
