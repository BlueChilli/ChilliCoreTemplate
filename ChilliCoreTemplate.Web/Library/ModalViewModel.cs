using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChilliCoreTemplate.Web
{
    /// <summary>
    /// Encapsulates modal view.
    /// </summary>
    public class ModalViewModel
    {
        public ModalViewModel()
        {
            Size = ModalSize.Medium;
        }


        /// <summary>
        /// Menu item.
        /// </summary>
        public IMvcActionDefinition Menu { get; set; }

        /// <summary>
        /// Modal title, Outputs default modal header with title if present otherwise no modal header is generated.
        /// </summary>
        public string Title { get; set; }   //Optional. Outputs default modal header with title if present otherwise no modal header is generated.

        /// <summary>
        /// Change the size of the modal from default to either modal-lg or modal-sm
        /// </summary>
        public ModalSize Size { get; set; }
    }

    public enum ModalSize
    {
        [Data("Css", "sm")]
        Small = 1,
        [Data("Css", "md")]
        Medium,
        [Data("Css", "lg")]
        Large,
        [Data("Css", "xl")]
        ExtraLarge
    }
}