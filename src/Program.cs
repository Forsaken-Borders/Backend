using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
namespace Kiki
{
    public class Program
    {
        // TODO: Command line arguments
        // TODO: Read from config file
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}