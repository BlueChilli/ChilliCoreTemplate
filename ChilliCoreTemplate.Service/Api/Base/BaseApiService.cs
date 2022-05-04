using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.Api
{
    public abstract class BaseApiService : Service<DataContext>
    {
        protected readonly ProjectSettings _config;
        protected readonly IFileStorage _fileStorage;
        protected readonly IWebHostEnvironment _environment;

        protected int? UserId { get { return User.UserData() == null ? null : (int?)User.UserData().UserId; } }

        protected int? CompanyId { get { return User.UserData() == null ? null : (int?)User.UserData().CompanyId; } }

        protected bool IsInRole(Role role) => User.UserData() != null && User.UserData().IsInRole(role);

        public BaseApiService(IPrincipal user, DataContext context, ProjectSettings config, IFileStorage fileStorage, IWebHostEnvironment environment) : base(user, context)
        {
            _config = config;
            _fileStorage = fileStorage;
            _environment = environment;
        }
    }
}
