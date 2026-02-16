using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GetCastSchedule
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string url = "https://www.oideyasukyoto2.com/cast/schedule-json/";
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                url = args[0];
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string targetDir = Path.Combine(baseDir, "CastSchedule");
            string filePath = Path.Combine(targetDir, "cast_list.txt");

            try
            {
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                using HttpClient client = new HttpClient();
                // User-Agent setting for robustness
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

                string json = await client.GetStringAsync(url);

                using JsonDocument doc = JsonDocument.Parse(json);
                List<string> castNames = new List<string>();

                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement element in doc.RootElement.EnumerateArray())
                    {
                        if (element.TryGetProperty("castName", out JsonElement nameElement))
                        {
                            string? name = nameElement.GetString();
                            if (!string.IsNullOrEmpty(name))
                            {
                                castNames.Add(name);
                            }
                        }
                    }
                }

                await File.WriteAllLinesAsync(filePath, castNames);
                Console.WriteLine($"Successfully saved {castNames.Count} cast names to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
