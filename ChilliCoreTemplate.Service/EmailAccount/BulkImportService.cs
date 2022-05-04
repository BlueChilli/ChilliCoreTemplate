using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using AutoMapper;
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public class BulkImportService
    {

        internal static void AutoMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<BulkImport, BulkImportViewModel>()
                .ForMember(dest => dest.StartedOn, opt => opt.MapFrom((src, dest, destMember, ctx) => src.StartedOn == null ? null : src.StartedOn.Value.ToTimezone((string)ctx.Items["Timezone"]).ToNullable<DateTime>()))
                .ForMember(dest => dest.FinishedOn, opt => opt.MapFrom((src, dest, destMember, ctx) => src.FinishedOn == null ? null : src.FinishedOn.Value.ToTimezone((string)ctx.Items["Timezone"]).ToNullable<DateTime>()));
        }


        public static List<BulkImportViewModel> BulkImport_List(DataContext context, BulkImportType type, string timezone = Constants.DefaultTimezone)
        {
            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
            return Mapper.Map<List<BulkImportViewModel>>(context.BulkImports
                .Where(x => x.Type == type && (x.StartedOn == null || x.StartedOn > oneWeekAgo))
                .OrderByDescending(x => x.StartedOn == null ? DateTime.MaxValue : x.StartedOn)
                .ToList(), opt => opt.Items["Timezone"] = timezone);
        }

    }
}
