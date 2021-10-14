using ChilliCoreTemplate.Models;
using ChilliSource.Cloud.Core;
using SixLabors.ImageSharp.Web.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service
{
    /// <summary>
    /// <para>Helper to communicate with amazon S3 or Azure servers.</para>  
    /// </summary>
    public class FileStorageHelper
    {
        ProjectSettings _config;
        public FileStorageHelper(ProjectSettings config)
        {
            _config = config;
        }

        public ChilliSource.Cloud.Azure.AzureStorageConfiguration CreateAzureStorageConfiguration(AzureStorageElement azureElement = null)
        {
            azureElement = azureElement ?? _config.FileStorage.Azure;
            var config = new ChilliSource.Cloud.Azure.AzureStorageConfiguration()
            {
                AccountName = azureElement.AccountName,
                AccountKey = azureElement.AccountKey,
                Container = azureElement.Container
            };

            return config;
        }

        /// <summary>
        /// <para>It uses the azurestorage element configuration in the ProjectConfigurationSection (Web.config).
        ///     AccountName, AccountKey - secret keys.
        ///     container - name of the container.
        ///     </para>
        /// </summary>
        /// <returns>Azure storage helper</returns>
        public IRemoteStorage Azure(AzureStorageElement azureConfig = null)
        {
            var config = CreateAzureStorageConfiguration(azureConfig);
            return new ChilliSource.Cloud.Azure.AzureRemoteStorage(config);
        }

        /// <summary>
        /// <para>it uses the s3 element configuration in the ProjectConfigurationSection (Web.config).
        ///     AccessKeyId, SecretAccessKey - secret keys.
        ///     Bucket - name of the bucket.
        ///     Host - Server URL. Defaults to "s3.amazonaws.com". For buckets in Sydney use host="s3-ap-southeast-2.amazonaws.com". The complete list of s3 host addresses is available at http://docs.aws.amazon.com/general/latest/gr/rande.html#s3_region.
        ///     </para>        
        /// </summary>
        /// <returns>S3 helper</returns>
        public IRemoteStorage S3(S3Element s3Config = null)
        {
            var config = CreateS3StorageConfiguration(s3Config);
            return new ChilliSource.Cloud.AWS.S3RemoteStorage(config);
        }

        public ChilliSource.Cloud.AWS.S3StorageConfiguration CreateS3StorageConfiguration(S3Element s3Element = null)
        {
            s3Element = s3Element ?? _config.FileStorage.S3;

            var config = new ChilliSource.Cloud.AWS.S3StorageConfiguration()
            {
                AccessKeyId = s3Element.AccessKeyId,
                SecretAccessKey = s3Element.SecretAccessKey,
                Bucket = s3Element.Bucket,
                Host = s3Element.Host
            };

            return config;
        }


        public IRemoteStorage LocalStorage(LocalStorageElement element = null)
        {
            var config = CreateLocalStorageConfiguration(element);
            return new LocalStorageProvider(config);
        }

        public LocalStorageConfiguration CreateLocalStorageConfiguration(LocalStorageElement element = null)
        {
            element = element ?? _config.FileStorage.Local;
            var config = new LocalStorageConfiguration()
            {
                BasePath = element.BasePath               
            };

            return config;
        }


        public IRemoteStorage CreateRemoteStorage()
        {
            var fileStorage = _config.FileStorage;
            if (fileStorage == null)
                throw new ApplicationException("Trying to use FileStorage without setting it up in appsettings");

            switch (fileStorage.DefaultProvider)
            {
                case FileStorageProvider.S3:
                    return S3();
                case FileStorageProvider.Azure:
                    return Azure();
                case FileStorageProvider.Local:
                    return LocalStorage();
                default:
                    throw new ApplicationException($"Unknown File Storage Provider: {fileStorage.DefaultProvider}");
            }
        }

        public IFileStorage CreateFileStorage()
        {
            return FileStorageFactory.Create(CreateRemoteStorage());
        }

        public string GetImagePrefix()
        {
            var fileStorage = _config.FileStorage;
            if (fileStorage.DefaultProvider == 0)
                throw new ArgumentNullException("FileStorage element is not setup");

            switch (fileStorage.DefaultProvider)
            {
                case FileStorageProvider.S3:
                    return fileStorage.S3.ImagePrefix;
                case FileStorageProvider.Azure:
                    return fileStorage.Azure.ImagePrefix;
                case FileStorageProvider.Local:
                    return fileStorage.Local.ImagePrefix;
                default:
                    throw new ApplicationException($"Unknown File Storage Provider: {fileStorage.DefaultProvider}");
            }
        }
    }
}
