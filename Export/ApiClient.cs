using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TownOfUsStatsExporter.Models;

namespace TownOfUsStatsExporter.Export;

/// <summary>
/// HTTP client for sending data to API.
/// </summary>
public static class ApiClient
{
    private static readonly HttpClient httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30),
    };

    /// <summary>
    /// Sends game stats data to the API endpoint.
    /// </summary>
    /// <param name="data">The game stats data to send.</param>
    /// <param name="endpoint">The API endpoint URL.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SendToApiAsync(GameStatsData data, string endpoint)
    {
        try
        {
            // Ensure endpoint ends with /among-data
            var apiUrl = endpoint.TrimEnd('/');
            if (!apiUrl.EndsWith("/among-data", StringComparison.OrdinalIgnoreCase))
            {
                apiUrl += "/among-data";
            }

            TownOfUsStatsPlugin.Logger.LogInfo($"Sending data to API: {apiUrl}");

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            };

            var jsonData = JsonSerializer.Serialize(data, jsonOptions);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add(
                "User-Agent",
                $"TownOfUs-StatsExporter/{TownOfUsStatsPlugin.PluginVersion}");

            var response = await httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                TownOfUsStatsPlugin.Logger.LogInfo($"API response: {responseContent}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TownOfUsStatsPlugin.Logger.LogError($"API returned error: {response.StatusCode} - {errorContent}");
            }
        }
        catch (HttpRequestException httpEx)
        {
            TownOfUsStatsPlugin.Logger.LogError($"HTTP error sending to API: {httpEx.Message}");
        }
        catch (TaskCanceledException)
        {
            TownOfUsStatsPlugin.Logger.LogError("API request timeout (30 seconds exceeded)");
        }
        catch (Exception ex)
        {
            TownOfUsStatsPlugin.Logger.LogError($"Unexpected error sending to API: {ex.Message}");
        }
    }
}
