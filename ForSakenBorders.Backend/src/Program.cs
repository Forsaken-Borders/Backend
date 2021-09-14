namespace ForSakenBorders.Backend
{
    public class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureLogging(logging => logging.ClearProviders())
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.Sources.Clear();
                config.AddJsonFile("res/config.jsonc.prod", optional: true, reloadOnChange: true);
                config.AddCommandLine(args);
                config.AddEnvironmentVariables("FORSAKENBORDERS_");
                config.Build();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}