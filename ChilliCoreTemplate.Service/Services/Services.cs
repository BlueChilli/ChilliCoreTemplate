using ChilliSource.Cloud.Core;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Service.EmailAccount;
using System;
using System.Security.Principal;
using Microsoft.AspNetCore.Hosting;

namespace ChilliCoreTemplate.Service
{
    public partial class Services : Service<DataContext>
    {
        private readonly IFileStorage _fileStorage;
        private readonly AccountService _accountService;
        private readonly StripeService _stripe;
        private readonly ProjectSettings _config;
        private readonly IWebHostEnvironment _environment;

        public Services(IPrincipal user, DataContext context, IFileStorage fileStorage, AccountService accountService, StripeService stripe, ProjectSettings config, IWebHostEnvironment environment) : base(user, context)
        {
            _fileStorage = fileStorage;
            _accountService = accountService;
            _stripe = stripe;
            _config = config;
            _environment = environment;
        }
    }    
}
