using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace HappyNotes.Services;

internal static class OAuth1Helper
{
    private const string OAuthVersion = "1.0";
    private const string OAuthSignatureMethod = "HMAC-SHA1";
    private static readonly string[] OAuthReservedCharacters = { "!", "*", "'", "(", ")" };

    public static string GenerateSignature(
        string url,
        string method,
        List<KeyValuePair<string, string>> parameters,
        string consumerSecret,
        string tokenSecret)
    {
        // Sort parameters
        var sortedParams = parameters.OrderBy(p => p.Key).ThenBy(p => p.Value).ToList();

        // Build parameter string
        var parameterString = string.Join("&", sortedParams.Select(p =>
            $"{UrlEncode(p.Key)}={UrlEncode(p.Value)}"));

        // Build signature base string
        var signatureBaseString = $"{method.ToUpper()}&{UrlEncode(url)}&{UrlEncode(parameterString)}";

        // Build signing key
        var signingKey = $"{UrlEncode(consumerSecret)}&{UrlEncode(tokenSecret)}";

        // Generate signature
        using var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));
        var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString));
        return Convert.ToBase64String(hash);
    }

    public static string UrlEncode(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var encoded = new StringBuilder();
        foreach (char c in value)
        {
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
            {
                encoded.Append(c);
            }
            else if (OAuthReservedCharacters.Contains(c.ToString()))
            {
                encoded.Append(c);
            }
            else
            {
                encoded.Append('%' + $"{(int)c:X2}");
            }
        }
        return encoded.ToString();
    }

    public static Dictionary<string, string> GenerateOAuthParameters(
        string consumerKey,
        string token,
        string callbackUrl = "oob")
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)).Replace("=", "").Replace("+", "").Replace("/", "");

        return new Dictionary<string, string>
        {
            { "oauth_callback", callbackUrl },
            { "oauth_consumer_key", consumerKey },
            { "oauth_nonce", nonce },
            { "oauth_signature_method", OAuthSignatureMethod },
            { "oauth_timestamp", timestamp },
            { "oauth_token", token },
            { "oauth_version", OAuthVersion }
        };
    }

    public static Dictionary<string, string> GenerateOAuthParametersForRequestToken(
        string consumerKey,
        string callbackUrl = "oob")
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)).Replace("=", "").Replace("+", "").Replace("/", "");

        return new Dictionary<string, string>
        {
            { "oauth_callback", callbackUrl },
            { "oauth_consumer_key", consumerKey },
            { "oauth_nonce", nonce },
            { "oauth_signature_method", OAuthSignatureMethod },
            { "oauth_timestamp", timestamp },
            { "oauth_version", OAuthVersion }
        };
    }
}

public class FanfouStatusService(
    IHttpClientFactory httpClientFactory,
    ILogger<FanfouStatusService> logger) : IFanfouStatusService
{
    private const string FanfouApiBaseUrl = "https://api.fanfou.com";

    public async Task<string> SendStatusAsync(
        string consumerKey,
        string consumerSecret,
        string accessToken,
        string accessTokenSecret,
        string content,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending status to Fanfou. ContentLength: {ContentLength}", content.Length);

        try
        {
            var url = $"{FanfouApiBaseUrl}/statuses/update.json";
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("status", content)
            };

            var response = await SendOAuthRequestAsync(
                url,
                "POST",
                consumerKey,
                consumerSecret,
                accessToken,
                accessTokenSecret,
                parameters,
                cancellationToken);

            var json = JObject.Parse(response);
            var statusId = json["id"]?.ToString() ?? throw new Exception("Failed to get status ID from response");

            logger.LogInformation("Successfully sent status to Fanfou. StatusId: {StatusId}", statusId);
            return statusId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send status to Fanfou");
            throw;
        }
    }

    public async Task<string> SendStatusWithPhotoAsync(
        string consumerKey,
        string consumerSecret,
        string accessToken,
        string accessTokenSecret,
        byte[] photoData,
        string? statusText = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending photo status to Fanfou. PhotoSize: {PhotoSize} bytes", photoData.Length);

        try
        {
            var url = $"{FanfouApiBaseUrl}/photos/upload.json";

            // For multipart/form-data, we need a different approach
            using var client = httpClientFactory.CreateClient();
            using var content = new MultipartFormDataContent();

            // Add photo
            content.Add(new ByteArrayContent(photoData), "photo", "photo.jpg");

            // Add status text if provided
            if (!string.IsNullOrEmpty(statusText))
            {
                content.Add(new StringContent(statusText), "status");
            }

            // Generate OAuth Authorization header for multipart request
            var authHeader = GenerateAuthorizationHeader(
                url,
                "POST",
                consumerKey,
                consumerSecret,
                accessToken,
                accessTokenSecret,
                []);

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", authHeader);

            var response = await client.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var json = JObject.Parse(responseContent);
            var statusId = json["id"]?.ToString() ?? throw new Exception("Failed to get status ID from response");

            logger.LogInformation("Successfully sent photo status to Fanfou. StatusId: {StatusId}", statusId);
            return statusId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send photo status to Fanfou");
            throw;
        }
    }

    public async Task DeleteStatusAsync(
        string consumerKey,
        string consumerSecret,
        string accessToken,
        string accessTokenSecret,
        string statusId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting status from Fanfou. StatusId: {StatusId}", statusId);

        try
        {
            var url = $"{FanfouApiBaseUrl}/statuses/destroy/{statusId}.json";

            await SendOAuthRequestAsync(
                url,
                "POST",
                consumerKey,
                consumerSecret,
                accessToken,
                accessTokenSecret,
                [],
                cancellationToken);

            logger.LogInformation("Successfully deleted status {StatusId} from Fanfou", statusId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete status {StatusId} from Fanfou", statusId);
            throw;
        }
    }

    private async Task<string> SendOAuthRequestAsync(
        string url,
        string method,
        string consumerKey,
        string consumerSecret,
        string token,
        string tokenSecret,
        List<KeyValuePair<string, string>> parameters,
        CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient();

        // Generate OAuth parameters
        var oauthParams = OAuth1Helper.GenerateOAuthParameters(consumerKey, token);

        // Build signature base string with all parameters
        var allParams = new List<KeyValuePair<string, string>>(parameters);
        foreach (var param in oauthParams)
        {
            allParams.Add(new KeyValuePair<string, string>(param.Key, param.Value));
        }

        // Generate signature
        var signature = OAuth1Helper.GenerateSignature(
            url,
            method,
            allParams,
            consumerSecret,
            tokenSecret);

        oauthParams["oauth_signature"] = signature;

        // Build Authorization header
        var authHeader = "OAuth " + string.Join(", ", oauthParams
            .OrderBy(p => p.Key)
            .Select(p => $"{OAuth1Helper.UrlEncode(p.Key)}=\"{OAuth1Helper.UrlEncode(p.Value)}\""));

        // Send request
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", authHeader);

        HttpResponseMessage response;

        if (method.ToUpper() == "GET")
        {
            var queryString = string.Join("&", parameters.Select(p =>
                $"{HttpUtility.UrlEncode(p.Key)}={HttpUtility.UrlEncode(p.Value)}"));
            var fullUrl = string.IsNullOrEmpty(queryString) ? url : $"{url}?{queryString}";
            response = await client.GetAsync(fullUrl, cancellationToken);
        }
        else
        {
            response = await client.PostAsync(url, new FormUrlEncodedContent(parameters), cancellationToken);
        }

        // Check for rate limiting
        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == (HttpStatusCode)429)
        {
            logger.LogWarning("Fanfou rate limit exceeded");
            throw new Exception("Fanfou rate limit exceeded. Please try again later.");
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static string GenerateAuthorizationHeader(
        string url,
        string method,
        string consumerKey,
        string consumerSecret,
        string token,
        string tokenSecret,
        List<KeyValuePair<string, string>> parameters)
    {
        var oauthParams = OAuth1Helper.GenerateOAuthParameters(consumerKey, token);

        var allParams = new List<KeyValuePair<string, string>>(parameters);
        foreach (var param in oauthParams)
        {
            allParams.Add(new KeyValuePair<string, string>(param.Key, param.Value));
        }

        var signature = OAuth1Helper.GenerateSignature(url, method, allParams, consumerSecret, tokenSecret);
        oauthParams["oauth_signature"] = signature;

        return "OAuth " + string.Join(", ", oauthParams
            .OrderBy(p => p.Key)
            .Select(p => $"{OAuth1Helper.UrlEncode(p.Key)}=\"{OAuth1Helper.UrlEncode(p.Value)}\""));
    }
}
