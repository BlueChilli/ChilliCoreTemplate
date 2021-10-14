using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Api
{
    public class ApiMessageResult
    {
        public string Title { get; set; }
        public string Message { get; set; }

        public static ApiMessageResult OK
        {
            get
            {
                return new ApiMessageResult() { Title = "OK", Message = "OK" };
            }
        }
    }
}
