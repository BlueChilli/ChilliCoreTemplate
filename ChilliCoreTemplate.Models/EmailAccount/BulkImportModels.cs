using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using Humanizer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChilliCoreTemplate.Models.EmailAccount
{
    [Serializable]
    public class BulkImportViewModel
    {
        public int Id { get; set; }

        public BulkImportType Type { get; set; }

        public string Description { get; set; }

        public int Records { get; set; }

        public int Processed { get; set; }

        public string Errors { get; set; }

        public DateTime? StartedOn { get; set; }
        public string StartedOnAgo => StartedOn == null ? null : StartedOn.Humanize();

        public DateTime? FinishedOn { get; set; }
        public string FinishedOnAgo => FinishedOn == null ? null : FinishedOn.Value.Humanize();

    }

    [Serializable]
    public class BaseImportModel
    {
        public byte[] Data { get; set; }

        public int BulkImportId { get; set; }

        public List<BulkImportViewModel> Imports { get; set; }

    }

    public enum BulkImportType
    {
        MyFirstBulkImport = 1
    }

}
