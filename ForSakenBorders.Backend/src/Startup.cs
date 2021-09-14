using System.Globalization;
using System.Security.Cryptography;
using ForSakenBorders.Backend.Api.Auth;
using ForSakenBorders.Backend.Database;
using ForSakenBorders.Backend.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using Serilog.Events;

namespace ForSakenBorders.Backend
{
    public class Startup
    {
        public static IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (!Configuration.GetValue<bool>("logging:disabled"))
            {
                LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                    .Enrich.WithThreadId()
                    .MinimumLevel.Is(Configuration.GetValue<LogEventLevel>("logging:level"))
                    .WriteTo.Console(theme: LoggerTheme.Lunar, outputTemplate: Configuration.GetValue("logging:format", "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));

                foreach (IConfigurationSection logOverride in Configuration.GetSection("logging:overrides").GetChildren())
                {
                    loggerConfiguration.MinimumLevel.Override(logOverride.Key, Enum.Parse<LogEventLevel>(logOverride.Value));
                }
                loggerConfiguration.WriteTo.File($"logs/{DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd' 'HH'_'mm'_'ss", CultureInfo.InvariantCulture)}.log", rollingInterval: Configuration.GetValue<RollingInterval>("logging:rolling_interval"), outputTemplate: Configuration.GetValue("logging:format", "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));
                Serilog.Log.Logger = loggerConfiguration.CreateLogger();
                services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Serilog.Log.Logger, true));
            }

            services.AddDbContext<BackendContext>(options =>
            {
                if (Configuration.GetValue<bool>("dev"))
                {
                    // Not using an in-memory database. See https://github.com/dotnet/efcore/issues/16103 and https://github.com/dotnet/efcore/issues/4922
                    options.UseSqlite("Data Source=./dev.db");
                }
                else
                {
                    NpgsqlConnectionStringBuilder connectionBuilder = new();
                    connectionBuilder.ApplicationName = Configuration.GetValue<string>("database:application_name");
                    connectionBuilder.Database = Configuration.GetValue<string>("database:database_name");
                    connectionBuilder.Host = Configuration.GetValue<string>("database:host");
                    connectionBuilder.Username = Configuration.GetValue<string>("database:username");
                    connectionBuilder.Port = Configuration.GetValue<int>("database:port");
                    connectionBuilder.Password = Configuration.GetValue<string>("database:password");
                    options.UseNpgsql(connectionBuilder.ToString(), options => options.EnableRetryOnFailure());
                    options.UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture);
                }
            }, ServiceLifetime.Transient);

            services.AddSingleton(SHA512.Create());
            services.AddRouting();
            services.AddControllers();
            services.AddAuthentication("TokenAuthentication").AddScheme<AuthenticationSchemeOptions, TokenAuth>("TokenAuthentication", "Token Authentication", null);
            services.AddAuthorization(options => options.AddPolicy("TokenAuthentication", policy => policy.RequireAuthenticatedUser()));
        }

        public void Configure(IApplicationBuilder app)
        {
            BackendContext database = app.ApplicationServices.CreateScope().ServiceProvider.GetService<BackendContext>();
            if (Configuration.GetValue<bool>("dev"))
            {
                // Wipe the database if it exists.
                database.Database.EnsureDeleted();
            }
            database.Database.EnsureCreated();

            if (database.Database.GetMigrations().Any())
            {
                try
                {
                    database.Database.Migrate();
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

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