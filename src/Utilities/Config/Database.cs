using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ForSakenBorders.Backend.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ForSakenBorders.Backend.Utilities.Config
{
    public class Database
    {
        [JsonPropertyName("application_name")]
        public string ApplicationName { get; set; }

        [JsonPropertyName("database_name")]
        public string DatabaseName { get; set; }

        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        public async Task Load(IServiceCollection services)
        {
            services.AddDbContext<BackendContext>(options =>
            {
                NpgsqlConnectionStringBuilder connectionBuilder = new();
                connectionBuilder.ApplicationName = ApplicationName;
                connectionBuilder.Database = DatabaseName;
                connectionBuilder.Host = Host;
                connectionBuilder.Username = Username;
                connectionBuilder.Port = Port;
                connectionBuilder.Password = Password;
                options.UseNpgsql(connectionBuilder.ToString(), options => options.EnableRetryOnFailure());
                options.UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture);
#if DEBUG
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
#endif
            }, ServiceLifetime.Transient);

            // Apparently getting the service from the service provider is an anti-pattern? Not sure what to do with it...
            using IServiceScope scope = services.BuildServiceProvider().CreateScope();
            BackendContext database = scope.ServiceProvider.GetService<BackendContext>();
            if ((await database.Database.GetPendingMigrationsAsync()).Any())
            {
                await database.Database.MigrateAsync();
            }
        }
    }
}