using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliCoreTemplate.Models
{
    public interface IMvcActionDefinition
    {
        IMvcActionDefinition AddRouteValues(object values);
        IMvcActionDefinition AddRouteId(int id);
        IReadOnlyDictionary<string, object> GetRouteValueDictionary();
    }
}
