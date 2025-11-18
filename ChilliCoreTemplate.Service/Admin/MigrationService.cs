using AutoMapper;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Admin.Migration;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.Admin;

public class MigrationServiceAutoMapperConfig : Profile
{
    public MigrationServiceAutoMapperConfig()
    {
    }
}

//For temporary code to run adhoc data migrations, not suited to be in data seed
public class MigrationService : Service<DataContext>
{
    private readonly IHostEnvironment _environment;
    private readonly IFileStorage _fileStorage;
    private readonly StripeService _stripe;
    private readonly AccountService _accountService;
    private readonly ProjectSettings _config;
    private readonly IMapper _mapper;

    public MigrationService(IPrincipal user, DataContext context, IHostEnvironment environment, IFileStorage fileStorage, StripeService stripe, AccountService accountService, ProjectSettings projectSettings, IMapper mapper) : base(user, context)
    {
        _environment = environment;
        _fileStorage = fileStorage;
        _stripe = stripe;
        _accountService = accountService;
        _config = projectSettings;
        _mapper = mapper;
    }

    public ServiceResult Migration(MigrationImportModel model)
    {
        try
        {
            var csvConfig = new CsvConfiguration(CultureInfo.GetCultureInfo("en-AU")) { HasHeaderRecord = true, TrimOptions = TrimOptions.Trim };

            using (var reader = new CsvReader(new StreamReader(model.CsvFile.OpenReadStream()), csvConfig))
            {
                reader.Context.TypeConverterOptionsCache.GetOptions<string>().NullValues.Add("");
                reader.Context.TypeConverterOptionsCache.GetOptions<string>().NullValues.Add("NULL");
                reader.Context.TypeConverterOptionsCache.GetOptions<int?>().NullValues.Add("NULL");
                reader.Context.TypeConverterOptionsCache.GetOptions<double?>().NullValues.Add("NULL");
                reader.Context.TypeConverterOptionsCache.GetOptions<DateTime?>().NullValues.Add("NULL");
                reader.Context.TypeConverterOptionsCache.GetOptions<bool?>().NullValues.Add("NULL");

                if (model.Type == MigrationImportType.Migration1)
                {
                    //reader.Context.RegisterClassMap<Migration1ClassMap>();
                    //return Migration1(reader.GetRecords<Migration1Model>().ToList());
                }
            }
        }
        catch (Exception ex)
        {
            ex.LogException();
            return ServiceResult.AsError(ex.Message);
        }
        return ServiceResult.AsSuccess();
    }

    internal async Task Run(BulkImport bulkImport, ITaskExecutionInfo executionInfo)
    {
        Context.BulkImports.Attach(bulkImport);

        switch (bulkImport.Parameters)
        {
            case "MyFirstAdhocMigration":
                await MyFirstAdhocMigration(bulkImport, executionInfo);
                break;
        }

        bulkImport.FinishedOn = DateTime.UtcNow;
        await Context.SaveChangesAsync();
    }

    private async Task MyFirstAdhocMigration(BulkImport bulkImport, ITaskExecutionInfo executionInfo)
    {
        await bulkImport.FatalErrorAsync(Context, "Not implemented");
    }
}
