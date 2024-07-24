using AutoMapper;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Hosting;
using System.Security.Principal;

namespace ChilliCoreTemplate.Service.Api
{
    public abstract class BaseApiService : Service<DataContext>
    {
        protected readonly ProjectSettings _config;
        protected readonly IFileStorage _fileStorage;
        protected readonly IWebHostEnvironment _environment;
        protected readonly IMapper _mapper;

        protected bool IsInRole(Role role) => User.UserData() != null && User.UserData().IsInRole(role);

        public BaseApiService(IPrincipal user, DataContext context, ProjectSettings config, IFileStorage fileStorage, IWebHostEnvironment environment, IMapper mapper) : base(user, context)
        {
            _config = config;
            _fileStorage = fileStorage;
            _environment = environment;
            _mapper = mapper;
        }
    }
}
