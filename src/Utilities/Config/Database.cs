using System.Globalization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ForSakenBorders.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ForSakenBorders.Utilities.Config
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
            services.AddDbContext<ForSakenBordersContext>(options =>
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

            using IServiceScope scope = services.BuildServiceProvider().CreateScope();
            ForSakenBordersContext database = scope.ServiceProvider.GetService<ForSakenBordersContext>();
            await database.Database.MigrateAsync();
        }
    }
}