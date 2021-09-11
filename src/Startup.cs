using System;
using System.Globalization;
using System.Security.Cryptography;
using ForSakenBorders.Backend.Api.Auth;
using ForSakenBorders.Backend.Utilities;
using ForSakenBorders.Backend.Utilities.Config;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace ForSakenBorders.Backend
{
    public class Startup
    {
        public const string OutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .WriteTo.Console(theme: LoggerTheme.Lunar, outputTemplate: OutputTemplate);

            loggerConfiguration.WriteTo.File($"logs/{DateTime.Now.ToLocalTime().ToString("yyyy'-'MM'-'dd' 'HH'_'mm'_'ss", CultureInfo.InvariantCulture)}.log", rollingInterval: RollingInterval.Day, outputTemplate: OutputTemplate);
            Log.Logger = loggerConfiguration.CreateLogger();
            services.AddSingleton(SHA512.Create());
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger, true));
            services.AddRouting();
            services.AddControllers();
            services.AddAuthentication("TokenAuthentication").AddScheme<AuthenticationSchemeOptions, TokenAuth>("TokenAuthentication", "Token Authentication", null);
            services.AddAuthorization(options => options.AddPolicy("TokenAuthentication", policy => policy.RequireAuthenticatedUser()));
            Config config = Config.Load().GetAwaiter().GetResult();
            config.Database.Load(services).GetAwaiter().GetResult();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}