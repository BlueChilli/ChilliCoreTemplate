using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Api
{
    public class ApiLogEntry
    {
        public long Id { get; set; }

        [StringLength(100)]
        public string User { get; set; }                    // The user that made the request.

        [StringLength(100)]
        public string Machine { get; set; }                 // The machine that made the request.

        [StringLength(100)]
        public string RequestIpAddress { get; set; }        // The IP address that made the request.

        [StringLength(100)]
        public string RequestContentType { get; set; }      // The request content type.
        public string RequestContentBody { get; set; }      // The request content body.
        public string RequestUri { get; set; }              // The request URI.

        [StringLength(100)]
        public string RequestMethod { get; set; }           // The request method (GET, POST, etc).
        
        public string RequestHeaders { get; set; }          // The request headers.
        
        public DateTime RequestTimestamp { get; set; }     // The request timestamp.

        [StringLength(100)]
        public string ResponseContentType { get; set; }     // The response content type.
        public string ResponseContentBody { get; set; }     // The response content body.
        public int? ResponseStatusCode { get; set; }        // The response status code.
        public string ResponseHeaders { get; set; }         // The response headers.
        public DateTime? ResponseTimestamp { get; set; }    // The response timestamp.
    }
}
