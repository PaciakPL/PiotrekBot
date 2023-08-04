using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Principal;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace PiotrekApp
{
    public class OpenAIClient
    {
        private readonly HttpClient httpClient;
        private OpenAIConfiguration openAiConfiguration;

        public OpenAIClient(IConfiguration configuration)
        {
            openAiConfiguration = configuration.GetSection("OpenAI").Get<OpenAIConfiguration>();

            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(openAiConfiguration.Url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiConfiguration.Token);
        }

        public async Task<string> Ask(string message)
        {
            var request = new
            {
                model = openAiConfiguration.Model,
                max_tokens = openAiConfiguration.MaxTokens,
                temperature = openAiConfiguration.Temperature,
                messages = new[]
                {
                    new { role = "user", content = message}
                }
            };

            var response = await httpClient.PostAsJsonAsync("chat/completions", request);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadFromJsonAsync<CompletionResponse>();

            return responseContent?.Choices[0]?.Message?.Content ?? "Sory ale chatgpt się wypiął";
        }
    }

    public class CompletionResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public int Created { get; set; }
        public List<CompletionChoice> Choices { get; set; }
        public CompletionUsage Usage { get; set; }
    }

    public class CompletionChoice
    {
        public int Index { get; set; }
        public CompletionChoiceMessage Message { get; set; }
        public string FinishReason { get; set; }
    }

    public class CompletionChoiceMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    public class CompletionUsage
    {
        [JsonProperty(PropertyName = "prompt_tokens")]
        public int PromptTokens { get; set; }
        [JsonProperty(PropertyName = "completion_tokens")]
        public int CompletionTokens { get; set; }
        [JsonProperty(PropertyName = "total_tokens")]
        public int TotalTokens { get; set; }
    }

    record OpenAIConfiguration
    {
        public string Url { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int MaxTokens { get; set; }
        public double Temperature { get; set; }
    }
}
