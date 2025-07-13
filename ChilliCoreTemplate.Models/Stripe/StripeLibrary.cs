using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Stripe;

public static class StripeLibrary
{
    public static bool IsCharge(string chargeId) => chargeId != null && (chargeId.StartsWith("py_") || chargeId.StartsWith("ch_"));

    public static bool IsTransfer(string transerId) => transerId != null && transerId.StartsWith("tr_");

}
