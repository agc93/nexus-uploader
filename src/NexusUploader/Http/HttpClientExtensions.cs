using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using NexusUploader.Nexus.Services;

namespace NexusUploader.Nexus.Http
{
    public static class HttpClientExtensions
    {
        public static IHttpClientBuilder AddNexusClient(this ServiceCollection services)
        {
            // services.AddTransient<HttpMessageHandlerBuilder, NexusMessageHandlerBuilder>();
            services.AddSingleton<NexusCookieHandler>();
            return services.AddHttpClient<ManageClient>(client =>
            {
                client.BaseAddress = new System.Uri("https://www.nexusmods.com");
                client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("NexusUploader", "0.1.0"));
            }).ConfigurePrimaryHttpMessageHandler<NexusCookieHandler>();
        }

        public static IHttpClientBuilder AddUploadClient(this IServiceCollection services)
        {
            services.AddSingleton<NexusCookieHandler>();
            return services.AddHttpClient<UploadClient>(client =>
            {
                client.BaseAddress = new System.Uri("https://upload.nexusmods.com");
                client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("NexusUploader", "0.1.0"));
            }).ConfigurePrimaryHttpMessageHandler<NexusCookieHandler>();
        }

        public static IHttpClientBuilder AddNexusApiClient(this IServiceCollection services)
        {
            return services.AddHttpClient<ApiClient>(client =>
            {
                client.BaseAddress = new System.Uri("https://api.nexusmods.com/v1/");
            });
        }
    }
}