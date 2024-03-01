using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.EntityFramework;
using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Data.EmailAccount
{
    /// <summary>
    /// Track progress of a Bulk Import operation
    /// </summary>
    public class BulkImport
    {
        public int Id { get; set; }

        public int? CompanyId { get; set; }
        public Company Company { get; set; }

        public BulkImportType Type { get; set; }

        [MaxLength(200)]
        public string Parameters { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        public string FilesJson { get; set; }   //JSON array of file paths

        public IReadOnlyList<BulkImportFileModel> Files() => FilesJson == null ? new List<BulkImportFileModel>() : FilesJson.FromJson<List<BulkImportFileModel>>();

        public int Records { get; set; }

        public int Processed { get; set; }

        public string Errors { get; set; }

        [DateTimeKind]
        public DateTime? StartedOn { get; set; }

        [DateTimeKind]
        public DateTime? FinishedOn { get; set; }

        public async Task FatalErrorAsync<T>(DataContext context, string error)
        {
            Errors = error;
            FinishedOn = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }
}
