using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service
{
    public class FileStoragePath
    {
        IServiceProvider _serviceProvider;
        FileStorageHelper _storageHelper;
        IUrlHelper _urlHelper;

        public FileStoragePath(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _storageHelper = serviceProvider.GetRequiredService<FileStorageHelper>();
            _urlHelper = serviceProvider.GetRequiredService<IUrlHelper>();
        }

        private T GetContextScopedService<T>()
        {
            var context = _serviceProvider.GetRequiredService<IHttpContextAccessor>()?.HttpContext;
            if (context == null)
                throw new ApplicationException("HttpContext is null");

            return context.RequestServices.GetRequiredService<T>();
        }

        public string GetImagePath(string filename)
        {
            return GetImagePath(filename, false);
        }

        public string GetImagePath(string filename, bool fullPath)
        {
            if (String.IsNullOrEmpty(filename))
                return filename;

            return CreateImageSharpHelper().ImageUrl(filename, fullPath: fullPath);
        }

        public ImageSharpHelper CreateImageSharpHelper()
        {
            return new ImageSharpHelper(_urlHelper, _storageHelper.GetImagePrefix());
        }

        public string GetDefaultPath(bool fullPath = false)
        {
            return GetImagePath(" ", fullPath);
        }

        public string GetPreSignedUrl(string filename)
        {
            return GetPreSignedUrl(filename, TimeSpan.FromHours(1));
        }

        public string GetPreSignedUrl(string filename, TimeSpan expiresIn)
        {
            var storage = _storageHelper.CreateFileStorage();
            var url = storage.GetPreSignedUrl(filename, expiresIn);

            if (_storageHelper.CreateRemoteStorage() is ChilliSource.Cloud.Core.LocalStorageProvider)
            {
                return _urlHelper.Content(Path.Combine(_storageHelper.GetImagePrefix(), url));
            }

            return url;
        }
    }
}
