using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ChilliCoreTemplate.Data.EmailAccount
{
    public class ErrorLog
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }

        public string Message { get; set; }

        public string MessageTemplate { get; set; }

        [MaxLength(128)]
        public string Level { get; set; }

        public DateTime TimeStamp { get; set; }

        public string ExceptionMessage { get; set; }

        public string Exception { get; set; }

        public string LogEvent { get; set; }

    }
}
