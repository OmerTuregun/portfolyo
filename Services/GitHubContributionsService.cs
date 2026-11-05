using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace My_Portfolyo.Services
{
    public sealed class GitHubContributionDay
    {
        [JsonPropertyName("d")]
        public string D { get; set; } = "";

        [JsonPropertyName("c")]
        public int C { get; set; }

        [JsonPropertyName("l")]
        public int L { get; set; }
    }

    public sealed class GitHubContributionsResult
    {
        public int Total { get; set; }
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public List<List<GitHubContributionDay>> Weeks { get; set; } = new();
    }

    public sealed class GitHubContributionsService
    {
        private const string CacheKey = "github:contributions";
        private const string FallbackKey = "github:contributions:last";
        private const string GraphQlQuery = """
            query($login: String!) {
              user(login: $login) {
                contributionsCollection {
                  contributionCalendar {
                    totalContributions
                    weeks {
                      contributionDays {
                        date
                        contributionCount
                        contributionLevel
                      }
                    }
                  }
                }
              }
            }
            """;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GitHubContributionsService> _logger;

        public GitHubContributionsService(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<GitHubContributionsService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<GitHubContributionsResult?> GetAsync(CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(CacheKey, out GitHubContributionsResult? cached) && cached != null)
            {
                return cached;
            }

            var fresh = await FetchFromGitHubAsync(cancellationToken);
            if (fresh != null)
            {
                _cache.Set(CacheKey, fresh, TimeSpan.FromHours(6));
                _cache.Set(FallbackKey, fresh);
                return fresh;
            }

            if (_cache.TryGetValue(FallbackKey, out GitHubContributionsResult? fallback) && fallback != null)
            {
                return fallback;
            }

            return null;
        }

        private async Task<GitHubContributionsResult?> FetchFromGitHubAsync(CancellationToken cancellationToken)
        {
            var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogInformation("GITHUB_TOKEN yok; katkı verisi alınamadı.");
                return null;
            }

            var username = _configuration["GitHub:Username"];
            if (string.IsNullOrWhiteSpace(username))
            {
                username = "OmerTuregun";
            }

            try
            {
                var client = _httpClientFactory.CreateClient("github");
                using var request = new HttpRequestMessage(HttpMethod.Post, "graphql");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());

                var payload = new
                {
                    query = GraphQlQuery,
                    variables = new { login = username }
                };
                request.Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                using var response = await client.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GitHub GraphQL yanıtı başarısız: {StatusCode}", (int)response.StatusCode);
                    return null;
                }

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

                if (doc.RootElement.TryGetProperty("errors", out _))
                {
                    _logger.LogWarning("GitHub GraphQL hatalı yanıt döndü.");
                    return null;
                }

                if (!doc.RootElement.TryGetProperty("data", out var data) ||
                    !data.TryGetProperty("user", out var user) ||
                    user.ValueKind == JsonValueKind.Null ||
                    !user.TryGetProperty("contributionsCollection", out var collection) ||
                    !collection.TryGetProperty("contributionCalendar", out var calendar) ||
                    !calendar.TryGetProperty("weeks", out var weeksEl))
                {
                    _logger.LogWarning("GitHub katkı takvimi beklenen biçimde değil.");
                    return null;
                }

                var allWeeks = new List<List<GitHubContributionDay>>();
                foreach (var weekEl in weeksEl.EnumerateArray())
                {
                    if (!weekEl.TryGetProperty("contributionDays", out var daysEl))
                    {
                        continue;
                    }

                    var days = new List<GitHubContributionDay>(7);
                    foreach (var dayEl in daysEl.EnumerateArray())
                    {
                        var date = dayEl.TryGetProperty("date", out var dateEl) ? dateEl.GetString() ?? "" : "";
                        var count = dayEl.TryGetProperty("contributionCount", out var countEl) ? countEl.GetInt32() : 0;
                        var levelRaw = dayEl.TryGetProperty("contributionLevel", out var levelEl)
                            ? levelEl.GetString()
                            : null;
                        days.Add(new GitHubContributionDay
                        {
                            D = date,
                            C = count,
                            L = MapLevel(levelRaw)
                        });
                    }

                    while (days.Count < 7)
                    {
                        days.Add(new GitHubContributionDay());
                    }

                    if (days.Count > 7)
                    {
                        days = days.Take(7).ToList();
                    }

                    allWeeks.Add(days);
                }

                if (allWeeks.Count == 0)
                {
                    return null;
                }

                var last30 = allWeeks.Count <= 30
                    ? allWeeks
                    : allWeeks.Skip(allWeeks.Count - 30).ToList();

                var total = 0;
                string? from = null;
                string? to = null;
                foreach (var week in last30)
                {
                    foreach (var day in week)
                    {
                        if (string.IsNullOrEmpty(day.D)) continue;
                        total += day.C;
                        if (from == null || string.CompareOrdinal(day.D, from) < 0) from = day.D;
                        if (to == null || string.CompareOrdinal(day.D, to) > 0) to = day.D;
                    }
                }

                return new GitHubContributionsResult
                {
                    Total = total,
                    From = from ?? "",
                    To = to ?? "",
                    Weeks = last30
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GitHub katkıları alınamadı.");
                return null;
            }
        }

        private static int MapLevel(string? level)
        {
            return level switch
            {
                "NONE" => 0,
                "FIRST_QUARTILE" => 1,
                "SECOND_QUARTILE" => 2,
                "THIRD_QUARTILE" => 3,
                "FOURTH_QUARTILE" => 4,
                _ => 0
            };
        }
    }
}
