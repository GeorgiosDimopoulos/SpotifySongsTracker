using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace SpotifySongsTracker.Services;

public class SpotifyAuth
{
    private static EmbedIOAuthServer? _server;

    public async Task AuthenticateUserAsync()
    {
        // Step 1: Set the server
        Console.WriteLine("Starting authorization server...");
        _server = new EmbedIOAuthServer(new Uri(AppResources.RedirectUri), int.Parse(AppResources.Port));

        // Step 2: Register event handlers
        _server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
        _server.ErrorReceived += OnErrorReceived;

        // Step 3: Start the server
        await _server.Start();
        Console.WriteLine("Server started at: " + _server.BaseUri);

        // Step 4: Start the authentication process (e.g., open browser)
        var loginRequest = new LoginRequest(_server.BaseUri, AppResources.ClientId, LoginRequest.ResponseType.Code)
        {
            Scope = new List<string> { Scopes.UserReadEmail }
        };
        BrowserUtil.Open(loginRequest.ToUri());

        Console.WriteLine("Waiting for authorization...");
    }

    private static async Task OnErrorReceived(object sender, string error, string? state)
    {
        Console.WriteLine($"Aborting authorization, error received: {error}");
        await _server!.Stop();
    }

    private static async Task OnAuthorizationCodeReceived(object arg1, AuthorizationCodeResponse response)
    {
        await _server!.Stop();

        var config = SpotifyClientConfig.CreateDefault();
        var authResponse = await new OAuthClient(config).RequestToken(
            new AuthorizationCodeTokenRequest(AppResources.ClientId, AppResources.ClientSecret, response.Code, new Uri(AppResources.RedirectUri)));

        var spotify = new SpotifyClient(authResponse.AccessToken);
        Console.WriteLine($"Access Token: {authResponse.AccessToken}");
        Console.WriteLine("Authorization successful! You can now make API calls.");

        // do calls with Spotify and save token?
    }
}
