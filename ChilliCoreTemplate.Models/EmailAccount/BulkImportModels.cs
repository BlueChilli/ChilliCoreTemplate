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
    public class BulkImportListModel
    {
        [EmptyItem("Any status")]
        public BulkImportStatus? Status { get; set; }

        public List<BulkImportViewModel> Imports { get; set; }
    }

    [Serializable]
    public class BulkImportViewModel
    {
        public int Id { get; set; }

        public BulkImportType Type { get; set; }

        public string CompanyName { get; set; }

        public string CompanyTimezone { get; set; }

        public string Description { get; set; }

        public int Records { get; set; }

        public int Processed { get; set; }

        public BulkImportStatus Status => StartedOn == null ? BulkImportStatus.Queued : FinishedOn == null ? BulkImportStatus.Started : Errors == null ? BulkImportStatus.Finished : BulkImportStatus.Failed;

        public string Errors { get; set; }

        public DateTime QueuedOn { get; set; }

        public DateTime? StartedOn { get; set; }
        public string StartedOnAgo => StartedOn == null ? null : StartedOn.Humanize();

        public DateTime? FinishedOn { get; set; }
        public string FinishedOnAgo => FinishedOn == null ? null : FinishedOn.Value.Humanize();

        public bool CanDownload { get; set; }
    }

    [Serializable]
    public class BaseImportModel
    {
        public int BulkImportId { get; set; }

        public List<BulkImportViewModel> Imports { get; set; }

    }

    public class BulkImportFileModel
    {
        public string FileName { get; set; }

        public string FilePath { get; set; }
    }

    public enum BulkImportType
    {
        EmailUser = 1
    }

    public enum BulkImportStatus
    {
        [Data("LabelType", LabelType.Warning)]
        Queued,
        [Data("LabelType", LabelType.Info)]
        Started,
        [Data("LabelType", LabelType.Success)]
        Finished,
        [Data("LabelType", LabelType.Danger)]
        Failed
    }
}
