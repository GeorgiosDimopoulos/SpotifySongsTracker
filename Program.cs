using SpotifyAPI.Web;

Console.WriteLine("Hello! Lets see whats new in your spotify account");

// Creates default config for the Spotify client.
var config = SpotifyClientConfig.CreateDefault();

// Prepares the credentials request.
var request = new ClientCredentialsRequest("YourClientId", "YourClientSecret");

// Sends the request to the Spotify API.
var response = await new OAuthClient(config).RequestToken(request);

// Creates a new Spotify client with the access token.
var spotify = new SpotifyClient(response.AccessToken); // "YourAccessToken"
Console.WriteLine("Connected to Spotify!");

// Gets the track with the given ID.
var track = await spotify.Tracks.Get("1s6ux0lNiTziSrd7iUAADH");
Console.WriteLine(track.Name);