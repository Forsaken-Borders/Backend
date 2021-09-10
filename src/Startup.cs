using System;
using System.Globalization;
using System.Security.Cryptography;
using ForSakenBorders.Backend.Utilities;
using ForSakenBorders.Backend.Utilities.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ForSakenBorders.Backend
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .WriteTo.Console(theme: LoggerTheme.Lunar, outputTemplate: outputTemplate);

            loggerConfiguration.WriteTo.File($"logs/{DateTime.Now.ToLocalTime().ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss", CultureInfo.InvariantCulture)}.log", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate);
            Log.Logger = loggerConfiguration.CreateLogger();
            services.AddSingleton(SHA512.Create());
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger, true));
            services.AddRouting();
            services.AddControllers();
            Config config = Config.Load().GetAwaiter().GetResult();
            config.Database.Load(services).GetAwaiter().GetResult();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}