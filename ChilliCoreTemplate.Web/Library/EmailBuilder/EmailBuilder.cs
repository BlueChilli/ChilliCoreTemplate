

using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ChilliCoreTemplate.Web
{
    /// <summary>
    /// Email builder factory
    /// </summary>
    public static class EmailBuilder
    {
        /// <summary>
        /// Creates a default email builder.
        /// </summary>
        /// <param name="htmlHelper">A htmlHelper instance.</param>
        /// <param name="options">(Optional) An instance of DefaultEmailBuilderOptions</param>
        /// <returns>An email builder helper.</returns>
        public static DefaultEmailBuilder CreateDefault(IHtmlHelper htmlHelper, DefaultEmailBuilderOptions options = null)
        {            
            options = options ?? new DefaultEmailBuilderOptions();

            return new DefaultEmailBuilder(htmlHelper, options);
        }     
    }
}
