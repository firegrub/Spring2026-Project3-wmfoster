using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Spring2026_Project3_wmfoster.Services
{
    public class AiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly string _deployment;

        public AiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _endpoint = configuration["DeepSeek:Endpoint"] ?? "";
            _apiKey = configuration["DeepSeek:ApiKey"] ?? "";
            _deployment = configuration["DeepSeek:Deployment"] ?? "";
        }

        public async Task<List<string>> GenerateMovieReviewsAsync(string movieTitle, string genre, int year)
        {
            string prompt =
                $"Write exactly 5 short movie reviews for \"{movieTitle}\" ({year}), genre {genre}. " +
                $"Return only the 5 reviews, one per line. No bullets, no numbering, no analysis, no <think> tags.";

            return await SendPromptForLinesAsync(prompt, 5);
        }

        public async Task<List<string>> GenerateActorTweetsAsync(string actorName)
        {
            string prompt =
                $"Write exactly 10 short fictional social media comments about \"{actorName}\". " +
                $"Return only the 10 comments, one per line. No bullets, no numbering, no analysis, no <think> tags.";

            return await SendPromptForLinesAsync(prompt, 10);
        }

        private async Task<List<string>> SendPromptForLinesAsync(string prompt, int expectedCount)
        {
            var url = $"{_endpoint.TrimEnd('/')}/openai/deployments/{_deployment}/chat/completions?api-version=2024-10-21";

            var body = new
            {
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = "Return only the final answer. Do not include reasoning, analysis, or <think> tags."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.7,
                max_tokens = 800
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("api-key", _apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"DeepSeek request failed: {response.StatusCode} - {responseText}");
            }

            using var doc = JsonDocument.Parse(responseText);

            string content =
                doc.RootElement
                   .GetProperty("choices")[0]
                   .GetProperty("message")
                   .GetProperty("content")
                   .GetString() ?? "";

            content = Regex.Replace(content, "<think>.*?</think>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase).Trim();

            var lines = content
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Select(x => x.TrimStart('-', '*', '•'))
                .Select(x => Regex.Replace(x, @"^\d+[\).\-\:]\s*", ""))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !x.StartsWith("<think>", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.StartsWith("</think>", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.Contains("Let me", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.Contains("I need to", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.Contains("user wants", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.Contains("return only", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (lines.Count > expectedCount)
            {
                lines = lines.Take(expectedCount).ToList();
            }

            while (lines.Count < expectedCount)
            {
                lines.Add("Neutral response.");
            }

            return lines;
        }
    }
}