using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public static class MenuHtmlHelper
    {
        public static MenuElement GetCurrentMenu(this IHtmlHelper htmlHelper)
        {
            return MenuConfigByRole.GetCurrentMenu(htmlHelper.ViewContext.HttpContext);
        }
    }
}
