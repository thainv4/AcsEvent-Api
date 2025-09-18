using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AcsEvent.DTOs.ThietBi;

namespace AcsEvent.Services;

public class HikvisionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HikvisionService> _logger;

    public HikvisionService(HttpClient httpClient, ILogger<HikvisionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                // Chỉ ignore SSL cho IP nội bộ
                var uri = message.RequestUri;
                if (uri != null)
                {
                    var host = uri.Host;
                    // Chỉ accept self-signed cert cho private IP ranges
                    return IsPrivateIP(host) || sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
                }

                return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
            }
        };
        _httpClient = new HttpClient(handler);
    }

    private bool IsPrivateIP(string host)
    {
        if (System.Net.IPAddress.TryParse(host, out var ip))
        {
            var bytes = ip.GetAddressBytes();
            return bytes[0] == 10 ||
                   (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                   (bytes[0] == 192 && bytes[1] == 168);
        }

        return false;
    }

    public async Task<string> CallAcsEventApiAsync(ThietBiAuthorDto authInfo, object requestBody)
    {
        try
        {
            var url = $"http://{authInfo.IP}/ISAPI/AccessControl/AcsEvent?format=json";
            var jsonContent = JsonSerializer.Serialize(requestBody);

            _logger.LogInformation($"Calling Hikvision API: {url}");
            _logger.LogDebug($"Request body: {jsonContent}");

            // Bước 1: Gọi API đầu tiên để lấy digest challenge
            var initialRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            var initialResponse = await _httpClient.SendAsync(initialRequest);

            // Bước 2: Nếu trả về 401 Unauthorized, parse digest challenge
            if (initialResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var wwwAuthHeader = initialResponse.Headers.WwwAuthenticate.FirstOrDefault();

                if (wwwAuthHeader != null && wwwAuthHeader.Scheme.Equals("Digest", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug($"Received digest challenge: {wwwAuthHeader.Parameter}");

                    // Bước 3: Tạo digest authorization header
                    var authHeader = CreateDigestAuthHeader(
                        authInfo.username,
                        authInfo.password,
                        "POST",
                        "/ISAPI/AccessControl/AcsEvent?format=json",
                        wwwAuthHeader.Parameter);

                    // Bước 4: Gọi lại API với digest authentication
                    var authenticatedRequest = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                    };
                    authenticatedRequest.Headers.Authorization = new AuthenticationHeaderValue("Digest", authHeader);

                    var authenticatedResponse = await _httpClient.SendAsync(authenticatedRequest);

                    if (authenticatedResponse.IsSuccessStatusCode)
                    {
                        var result = await authenticatedResponse.Content.ReadAsStringAsync();
                        _logger.LogInformation("Successfully retrieved ACS events");
                        return result;
                    }
                    else
                    {
                        var errorContent = await authenticatedResponse.Content.ReadAsStringAsync();
                        _logger.LogError(
                            $"API call failed with status: {authenticatedResponse.StatusCode}, Content: {errorContent}");
                        throw new Exception($"API call failed: {authenticatedResponse.StatusCode} - {errorContent}");
                    }
                }

                throw new Exception("Server requires Digest authentication but no Digest challenge was received");
            }

            if (initialResponse.IsSuccessStatusCode)
            {
                // Trường hợp không cần authentication (hiếm)
                var result = await initialResponse.Content.ReadAsStringAsync();
                return result;
            }

            {
                var errorContent = await initialResponse.Content.ReadAsStringAsync();
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Network error calling Hikvision API at {authInfo.IP}");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, $"Timeout calling Hikvision API at {authInfo.IP}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error calling Hikvision API at {authInfo.IP}");
            return null;
            
        }
    }

// Các methods khác (CreateDigestAuthHeader, ParseDigestChallenge, etc.) giữ nguyên
    private string CreateDigestAuthHeader(string username, string password, string method, string uri,
        string challengeHeader)
    {
        try
        {
            var challenge = ParseDigestChallenge(challengeHeader);

            // Validate required fields
            if (!challenge.ContainsKey("realm") || !challenge.ContainsKey("nonce"))
            {
                throw new Exception("Invalid digest challenge - missing realm or nonce");
            }

            // Calculate digest response
            var ha1 = ComputeMd5Hash($"{username}:{challenge["realm"]}:{password}");
            var ha2 = ComputeMd5Hash($"{method}:{uri}");
            var response = ComputeMd5Hash($"{ha1}:{challenge["nonce"]}:{ha2}");

            // Build authorization header
            var authHeader = $"username=\"{username}\", " +
                             $"realm=\"{challenge["realm"]}\", " +
                             $"nonce=\"{challenge["nonce"]}\", " +
                             $"uri=\"{uri}\", " +
                             $"response=\"{response}\"";

            // Add optional fields if present
            if (challenge.ContainsKey("qop"))
            {
                var nc = "00000001";
                var cnonce = GenerateClientNonce();
                var responseWithQop =
                    ComputeMd5Hash($"{ha1}:{challenge["nonce"]}:{nc}:{cnonce}:{challenge["qop"]}:{ha2}");

                authHeader = $"username=\"{username}\", " +
                             $"realm=\"{challenge["realm"]}\", " +
                             $"nonce=\"{challenge["nonce"]}\", " +
                             $"uri=\"{uri}\", " +
                             $"qop={challenge["qop"]}, " +
                             $"nc={nc}, " +
                             $"cnonce=\"{cnonce}\", " +
                             $"response=\"{responseWithQop}\"";
            }

            if (challenge.ContainsKey("opaque"))
            {
                authHeader += $", opaque=\"{challenge["opaque"]}\"";
            }

            _logger.LogDebug($"Generated digest auth header: {authHeader}");
            return authHeader;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating digest auth header");
            throw new Exception($"Failed to create digest authentication: {ex.Message}");
        }
    }

    private Dictionary<string, string> ParseDigestChallenge(string challenge)
    {
        var result = new Dictionary<string, string>();

        // Split by comma but handle quoted values
        var parts = new List<string>();
        var currentPart = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < challenge.Length; i++)
        {
            var c = challenge[i];
            if (c == '"' && (i == 0 || challenge[i - 1] != '\\'))
            {
                inQuotes = !inQuotes;
                currentPart.Append(c);
            }
            else if (c == ',' && !inQuotes)
            {
                parts.Add(currentPart.ToString().Trim());
                currentPart.Clear();
            }
            else
            {
                currentPart.Append(c);
            }
        }

        if (currentPart.Length > 0)
        {
            parts.Add(currentPart.ToString().Trim());
        }

        // Parse key-value pairs
        foreach (var part in parts)
        {
            var equalIndex = part.IndexOf('=');
            if (equalIndex > 0)
            {
                var key = part[..equalIndex].Trim();
                var value = part[(equalIndex + 1)..].Trim();

                // Remove quotes from value
                if (value.StartsWith('"') && value.EndsWith('"') && value.Length >= 2)
                {
                    value = value[1..^1];
                }

                result[key] = value;
            }
        }

        return result;
    }

    private string ComputeMd5Hash(string input)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private string GenerateClientNonce()
    {
        var bytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}