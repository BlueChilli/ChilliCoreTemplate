using ChilliSource.Cloud.Core.GooglePlaceDetailsInternal;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using NetTopologySuite.Algorithm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Admin.Migration
{
    public class MigrationImportModel
    {
        public MigrationImportType Type { get; set; }

        [Required]
        [HttpPostedFileExtensions(allowedExtensions: "csv")]
        public IFormFile CsvFile { get; set; }

    }

    public enum MigrationImportType
    {
        Migration1
    }
}
