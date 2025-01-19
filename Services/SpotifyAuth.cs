using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace SpotifySongsTracker.Services;

public class SpotifyAuth
{
    private static EmbedIOAuthServer? _server;
    private static SpotifyClient _spotifyClient = default!;

    public async Task<SpotifyClient> AuthenticateUserAsync()
    {
        Console.WriteLine("Starting authorization server...");
        _server = new EmbedIOAuthServer(new Uri(AppResources.RedirectUri), int.Parse(AppResources.Port));

        _server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
        _server.ErrorReceived += OnErrorReceived;

        await _server.Start();
        Console.WriteLine("Server started at: " + _server.BaseUri);

        var loginRequest = new LoginRequest(_server.BaseUri, AppResources.ClientId, LoginRequest.ResponseType.Code)
        {
            Scope =
            [
                Scopes.UserReadEmail,
                Scopes.UserTopRead,
                Scopes.PlaylistReadPrivate,
                Scopes.PlaylistReadCollaborative,
            ],
        };

        BrowserUtil.Open(loginRequest.ToUri());
        Console.WriteLine($"Waiting for authorization..."); // {loginRequest.ToUri()}
        await Task.Delay(10000);

        while (_spotifyClient == null)
            await Task.Delay(500);

        if (_spotifyClient == null)
        {
            Console.WriteLine("Failed to authenticate with Spotify.");
            return null!;
        }

        Console.WriteLine("Authentication complete.");
        return _spotifyClient;
    }

    public async Task<SpotifyClient> InitializeSpotifyClient()
    {
        var config = SpotifyClientConfig.CreateDefault();
        var clientRequest = new ClientCredentialsRequest(AppResources.ClientId, AppResources.ClientSecret);
        var clientResponse = await new OAuthClient(config).RequestToken(clientRequest);

        if (clientResponse != null)
        {
            _spotifyClient = new SpotifyClient(clientResponse.AccessToken);
            Console.WriteLine("Spotify client initialized!");

            return _spotifyClient;
        }
        else
        {
            Console.WriteLine("Failed to authenticate with Spotify.");
            return null!;
        }
    }

    public async Task GetCallback(string code)
    {
        var response = await new OAuthClient().RequestToken(
          new AuthorizationCodeTokenRequest(AppResources.ClientId, AppResources.ClientSecret, code, new Uri(AppResources.RedirectUri))
        );

        var spotify = new SpotifyClient(response.AccessToken);
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
        var tokenRequest = new AuthorizationCodeTokenRequest(
            AppResources.ClientId,
            AppResources.ClientSecret,
            response.Code, new Uri(AppResources.RedirectUri));

        var tokenResponse = await new OAuthClient(config).RequestToken(tokenRequest);

        _spotifyClient = new SpotifyClient(tokenResponse.AccessToken);

        Console.WriteLine($"Authorization successful!");
    }
}
