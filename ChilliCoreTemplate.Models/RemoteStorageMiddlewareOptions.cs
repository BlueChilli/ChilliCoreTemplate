using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliCoreTemplate.Models
{
    public class RemoteStorageMiddlewareOptions
    {
        public RemoteStorageMiddlewareOptions()
        {
            this.AllowedExtensions = new List<string>();
        }

        public string UrlPrefix { get; set; }

        public IList<string> AllowedExtensions { get; private set; }

        public string DefaultCacheControl { get; set; }
    }
}
