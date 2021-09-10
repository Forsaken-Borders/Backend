using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ForSakenBorders.Backend.Utilities.Config
{
    public class Config
    {
        [JsonPropertyName("database")]
        public Database Database { get; set; }

        public static async Task<Config> Load()
        {
            // Setup Config
            // Look for Environment variables for Docker. If the variable is set, but doesn't exist, assume it was improper configuration and exit.
            string tokenFile = Environment.GetEnvironmentVariable("CONFIG_FILE");
            if (tokenFile != null && !File.Exists(tokenFile))
            {
                Console.WriteLine($"The config file \"{tokenFile}\" does not exist. Consider removing the $CONFIG_FILE environment variable or making sure the file exists.");
                Environment.Exit(1);
            }
            else if (File.Exists("res/config.jsonc.prod"))
            {
                // Look for production file first. Contributers are expected not to fill out res/config.jsonc, but res/config.jsonc.prod instead.
                tokenFile = "res/config.jsonc.prod";
            }
            else if (File.Exists("res/config.jsonc"))
            {
                tokenFile = "res/config.jsonc";
            }
            else
            {
                throw new FileNotFoundException("No config file found. Please create a config file in res/config.jsonc or res/config.jsonc.prod.");
            }

            // Prefer JsonSerializer.DeserializeAsync over JsonSerializer.Deserialize due to being able to send the stream directly.
            return await JsonSerializer.DeserializeAsync<Config>(File.OpenRead(tokenFile), new() { IncludeFields = true, AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip, PropertyNameCaseInsensitive = true });
        }
    }
}