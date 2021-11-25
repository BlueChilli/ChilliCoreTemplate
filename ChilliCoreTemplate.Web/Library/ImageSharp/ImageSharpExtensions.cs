using ChilliCoreTemplate.Service;
using ChilliSource.Cloud.ImageSharp;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Providers;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public static class ImageSharpExtensions
    {
        public static void ConfigureImageSharp(IServiceCollection services)
        {
            //See https://github.com/SixLabors/ImageSharp.Web for more options
            services.AddOptions<ImageSharpMiddlewareOptions>()
                .Configure<IHttpContextAccessor>((options, httpContextAccessor) =>
                {
                    options.Configuration.ImageFormatsManager.SetEncoder(JpegFormat.Instance, new JpegEncoder { Quality = 90 });
                    options.Configuration.ImageFormatsManager.SetEncoder(PngFormat.Instance, new PngEncoder { CompressionLevel = PngCompressionLevel.DefaultCompression });

                    options.OnParseCommandsAsync = ImageSharpExtensions.EnhanceParsedCommand;
                });

            services.AddImageSharp()
                .SetRequestParser<QueryCollectionRequestParser>()
                .SetCache<PhysicalFileSystemCache>()
                .SetCacheHash<CacheHash>()
                //image providers
                .RemoveProvider<PhysicalFileSystemProvider>()
                .AddProvider<CloudStorageImageProvider>() //IRemoteStorage adapter (must be tried first otherwise never called)
                .AddProvider<PhysicalFileSystemProvider>()
                //image processors
                .AddProcessor<AutoOrientImageSharpProcessor>()
                .AddProcessor<RotateImageSharpProcessor>();
        }

        //https://docs.sixlabors.com/articles/imagesharp.web/processingcommands.html
        public static Task EnhanceParsedCommand(ImageCommandContext commandContext)
        {
            MutateCommand(commandContext, "w", "width");
            MutateCommand(commandContext, "h", "height");
            MutateCommand(commandContext, "mode", "rmode");
            MutateCommand(commandContext, "q", "quality");

            return Task.CompletedTask;
        }

        private static void MutateCommand(ImageCommandContext commandContext, string commandA, string commandB)
        {
            if (commandContext.Context.Request.Query.ContainsKey(commandA) && !commandContext.Commands.ContainsKey(commandB))
            {
                commandContext.Commands[commandB] = commandContext.Context.Request.Query[commandA];
            }
        }

        public static ImageSharpHelper Resizer(this IHtmlHelper helper)
        {
            var storagePath = helper.ViewContext.HttpContext.RequestServices.GetRequiredService<FileStoragePath>();
            return storagePath.CreateImageSharpHelper();
        }
    }
}