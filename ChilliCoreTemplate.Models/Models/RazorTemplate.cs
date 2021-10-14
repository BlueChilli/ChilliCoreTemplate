using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliCoreTemplate.Models
{
    public class RazorTemplate

    {
        private readonly string _subject;


        public RazorTemplate(string templateName) : this(templateName, "")
        {
            
        }
        
        
        public RazorTemplate(string templateName, string subject)
        {
            TemplateName = templateName;
            _subject = subject;
        }

        /// <summary>
        /// name of the view template
        /// </summary>
        /// <example>
        /// TemplateName = "Test"
        /// where template view are located in ~/Views/Test.cshtml
        /// </example>
        /// <remarks>
        ///  View locations are searched:
        ///  ~/Views/.....
        /// ~/View/Shared/.....
        /// ~/Layout/.....
        /// </remarks>
        public string TemplateName { get; }
        public string Subject { get; }
    }
}
