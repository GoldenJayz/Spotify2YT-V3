using Microsoft.AspNetCore.Mvc.RazorPages;
using SpotifyAPI.Web;


namespace Spotty.Pages
{

    public class IndexModel : PageModel
    {
        public User curUser;
        private readonly ILogger<IndexModel> _logger;


        /// <summary>
        ///     Initializes variables in Model
        /// </summary>
        /// <param name="logger">Sets up for logging</param>
        /// <param name="config">Loading the secrets stored in the stash</param>
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }


        /// <summary>
        ///     Checks the query string if there is a code
        /// </summary>
        /// 

        public void getCurUser()
        {
            string clientIp = HttpContext.Connection.RemoteIpAddress.ToString();


            if (IndexModelHelpers.users.Count == 0)
            {
                curUser = new User(clientIp);

                Console.WriteLine(curUser.Client);
                IndexModelHelpers.users.Add(curUser);
            }

            else
            {
                bool anyIp = false;


                foreach (var user in IndexModelHelpers.users)
                {
                    if (user.Client == clientIp || clientIp == "::1")
                    {
                        anyIp = true;
                        curUser = user;
                        break;
                    }
                }


                if (anyIp == false)
                {
                    curUser = new User(clientIp);
                }
            }
        }

        public async Task OnGet()
        {
            // Checks if there is a current user registered in static ussers array
            getCurUser();


            if (Request.QueryString.ToString().Length > 0)
            {
                curUser.AuthCode = Request.QueryString.ToString();
            }

            if (curUser.AuthCode is not null)
            {
                curUser.AuthCode = curUser.AuthCode.Remove(0, 6);
                await GetCallback(curUser.AuthCode);
            }
        }


        /// <summary>
        ///     Awaits the callback for the code
        ///     for the Spotify API
        /// </summary>
        /// <param name="code">Code passed in from the query</param>
        public async Task GetCallback(string code)
        {
            if (curUser.SpotifyAuth is null)
            {
                AuthorizationCodeTokenResponse? response = await new OAuthClient().RequestToken(
                new AuthorizationCodeTokenRequest("", "", code, new Uri(""))
                );

                curUser.SpotifyAuth = new SpotifyClient(response.AccessToken);
            }

            #pragma warning disable CS8602 // Dereference of a possibly null reference.
            PrivateUser? profile = await curUser.SpotifyAuth.UserProfile.Current();
            #pragma warning restore CS8602 // Dereference of a possibly null reference.

            if (profile != null)
            {
                string id = profile.Id;
                Paging<SimplePlaylist> getPlaylists = await curUser.SpotifyAuth.Playlists.GetUsers(id);
                #pragma warning disable CS8601 // Possible null reference assignment.
                curUser.Playlists = getPlaylists.Items;
                #pragma warning restore CS8601 // Possible null reference assignment.


                if (curUser.Playlists.Count == 0)
                {
                    return;
                }
            }
        }


        /// <summary>
        ///     When the submit button is clicked
        ///     it grabs the songs from the Spotify
        ///     playlist then adds it to an array so
        ///     Google api can interact with it
        /// </summary>
        public async void OnPostPlaylist()
        {
            getCurUser();
            string playlistName = Request.Form["chicken"];
            curUser.isSubmitted = true;

            foreach (SimplePlaylist? item in curUser.Playlists)
            {
                if (item.Name == playlistName)
                {
                    SimplePlaylist[]? playlists = curUser.Playlists.ToArray();
                    string[] names = makeArrayOfNames(playlists);
                    int index = Array.IndexOf(names, playlistName);
                    SimplePlaylist? playlist = playlists[index];
                    PlaylistGetItemsRequest? playlistGetItemsRequest = new PlaylistGetItemsRequest();
                    playlistGetItemsRequest.Fields.Add("items(track(name,type))");
                    Paging<PlaylistTrack<IPlayableItem>>? getTracks = await curUser.SpotifyAuth.Playlists.GetItems(playlist.Id, playlistGetItemsRequest);


                    foreach (PlaylistTrack<IPlayableItem> song in getTracks.Items)
                    {
                        if (song.Track is FullTrack track)
                        {
                            curUser.Songs.Add(track.Name);
                        }
                    }
                }
            }

        }


        /// <summary>
        ///     Creates an array of the song names
        ///     instead of having an object array
        /// </summary>
        /// <param name="array">Takes in the array of objects</param>
        /// <returns>Returns the array of stringed names</returns>
        public string[] makeArrayOfNames(SimplePlaylist[] array)
        {
            List<string> retval = new List<string>();

            foreach (SimplePlaylist? item in array)
            {
                retval.Add(item.Name);
            }

            return retval.ToArray();
        }
    }
}