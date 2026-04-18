using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.InputModels.Auth;
using ThuHaiDuong.Application.Payloads.Responses;

namespace ThuHaiDuong.Application.ImplementService;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
 
    public GoogleAuthService(IConfiguration config, HttpClient httpClient)
    {
        _config     = config;
        _httpClient = httpClient;
    }
 
    public async Task<GoogleUserInfo> VerifyIdTokenAsync(string idToken)
    {
        // Google tokeninfo endpoint — verify idToken server-side
        // Không dùng Google SDK để giữ dependency tối giản
        var url      = $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}";
        var response = await _httpClient.GetAsync(url);
 
        if (!response.IsSuccessStatusCode)
            throw new ResponseErrorObject(
                "Invalid Google token.", StatusCodes.Status401Unauthorized);
 
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
 
        // Kiểm tra token đúng audience (clientId của app)
        var clientId = _config["Google:ClientId"];
        var aud      = json.GetProperty("aud").GetString();
 
        if (aud != clientId)
            throw new ResponseErrorObject(
                "Google token audience mismatch.", StatusCodes.Status401Unauthorized);
 
        return new GoogleUserInfo
        {
            GoogleId = json.GetProperty("sub").GetString()!,
            Email    = json.GetProperty("email").GetString()!,
            Name     = json.TryGetProperty("name",    out var name)    ? name.GetString()    : null,
            Picture  = json.TryGetProperty("picture", out var picture) ? picture.GetString() : null,
        };
    }
}