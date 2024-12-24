using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpotifySongsTracker.Services;

public class SpotifyAuth
{
    private static EmbedIOAuthServer _server;

    public SpotifyAuth()
    {
        _server = new EmbedIOAuthServer(new Uri(AppResources.RedirectUri), int.Parse(AppResources.Port));
        _server.Start();

        _server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
        _server.ErrorReceived += OnErrorReceived;

        var request = new LoginRequest(_server.BaseUri, AppResources.ClientId, LoginRequest.ResponseType.Code)
        {
            Scope = new List<string> { Scopes.UserReadEmail }
        };
        BrowserUtil.Open(request.ToUri());
    }

    private static async Task OnErrorReceived(object sender, string error, string? state)
    {
        Console.WriteLine($"Aborting authorization, error received: {error}");
        await _server.Stop();
    }

    private static async Task OnAuthorizationCodeReceived(object arg1, AuthorizationCodeResponse response)
    {
        await _server.Stop();

        var config = SpotifyClientConfig.CreateDefault();
        var authResponse = await new OAuthClient(config).RequestToken(
            new AuthorizationCodeTokenRequest(AppResources.ClientId, AppResources.ClientSecret, response.Code, new Uri(AppResources.RedirectUri)));

        var spotify = new SpotifyClient(authResponse.AccessToken);
        // do calls with Spotify and save token?
    }
}
