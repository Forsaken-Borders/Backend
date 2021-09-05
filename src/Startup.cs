using System;
using System.Globalization;
using Kiki.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Kiki
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

            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger, true));
            services.AddRouting();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseAuthorization();
            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}