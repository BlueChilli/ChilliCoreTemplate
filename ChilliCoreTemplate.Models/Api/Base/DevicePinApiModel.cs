using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Api
{
    public class PersistDevicePinApiModel
    {
        [Required, MinLength(4), MaxLength(4)]
        public string Pin { get; set; }

        [Required, StringLength(100)]
        public string DeviceId { get; set; }
    }

    public class DevicePinResponseApiModel
    {
        public string PinToken { get; set; }
    }

    public class PinLoginPinApiModel
    {
        [Required, StringLength(200)]
        public string PinToken { get; set; }

        [Required, MinLength(4), MaxLength(4)]
        public string Pin { get; set; }

        [Required, StringLength(100)]
        public string DeviceId { get; set; }
    }
}
