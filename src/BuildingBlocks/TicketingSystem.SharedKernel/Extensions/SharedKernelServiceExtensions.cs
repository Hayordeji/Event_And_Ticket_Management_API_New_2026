using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel.Configurations;
using TicketingSystem.SharedKernel.Services;

namespace TicketingSystem.SharedKernel.Extensions
{
    public static class SharedKernelServiceExtensions
    {
        public static IServiceCollection AddSharedKernel(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            services.AddOptions<QrEncryptionConfig>()
           .BindConfiguration(QrEncryptionConfig.SectionName)
           .ValidateOnStart();
                //var key = configuration.GetSection(QrEncryptionConfig.SectionName);
                //services.Configure<QrEncryptionConfig>(key);

            services.AddSingleton<IValidateOptions<QrEncryptionConfig>,
            QrEncryptionConfigValidator>();

            services.AddSingleton<IQrCodeEncryptionService, AesQrCodeEncryptionService>();

            return services;
        }
    }
}
