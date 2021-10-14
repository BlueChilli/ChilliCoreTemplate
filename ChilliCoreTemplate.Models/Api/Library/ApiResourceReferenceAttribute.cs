using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Api
{
    public class ApiResourceReferenceAttribute : Attribute
    {
        public ApiResourceReferenceAttribute(string version, string resourceName)
        {
            this.Version = version;
            this.ResourceName = resourceName;
        }

        public string ResourceName { get; set; }

        public string Version { get; set; }
    }
}
