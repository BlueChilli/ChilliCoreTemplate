using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Hosting;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models;
using System.Security.Principal;

namespace ChilliCoreTemplate.Service
{
    public abstract class BaseService : Service<DataContext>
    {
        protected readonly IFileStorage _fileStorage;
        protected readonly ProjectSettings _config;
        protected readonly IWebHostEnvironment _environment;

        protected int? UserId { get { return User.UserData() == null ? null : (int?)User.UserData().UserId; } }

        protected int? CompanyId { get { return User.UserData() == null ? null : (int?)User.UserData().CompanyId; } }

        protected bool IsInRole(Role role) => User.UserData() != null && User.UserData().IsInRole(role);

        public BaseService(IPrincipal user, DataContext context, ProjectSettings config, IFileStorage fileStorage, IWebHostEnvironment environment) : base(user, context)
        {
            _config = config;
            _fileStorage = fileStorage;
            _environment = environment;
        }
    }

}
