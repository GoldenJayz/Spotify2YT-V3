using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Spotty.Pages
{
    /// <summary>
    ///    This class handles the authentication for
    ///    Google and then creates a playlist and adds
    ///    the corresponding songs from Spotify to YouTube.
    /// </summary>
    public class RedirectGoogleModel : PageModel
    {

        public static bool Error;
        public User curUser;


        /// <summary>
        ///     Called when the page is loaded
        ///     processes OAuth2.0 and actual
        ///     backend of the application
        /// </summary>
        public async void OnGet()
        {
            UserCredential credential;
            string clientIp = HttpContext.Connection.RemoteIpAddress.ToString();


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
                curUser.Songs.Add("Enter Sandman");
            }
            


            if (curUser.GoogleAuth is null)
            {
                try
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = "",
                        ClientSecret = ""
                    },
                         new[] { YouTubeService.Scope.Youtube },
                         "user",
                         CancellationToken.None
                     );


                    YouTubeService? service = new YouTubeService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Spotify2YT"
                    });


                    Playlist? newPlaylist = new Playlist
                    {
                        Snippet = new PlaylistSnippet
                        {
                            Title = "Test Playlist",
                            Description = "A playlist created with the YouTube API v3"
                        },
                        Status = new PlaylistStatus
                        {
                            PrivacyStatus = "public"
                        }
                    };

                    newPlaylist = await service.Playlists.Insert(newPlaylist, "snippet, status").ExecuteAsync();

                    YouTubeService? youtube = new YouTubeService(new BaseClientService.Initializer()
                    {
                        ApiKey = "",
                        ApplicationName = GetType().ToString()
                    });



                    foreach (string? song in curUser.Songs) // Loops through the songs in the Spotify playlist and searches it on YouTube to get the VideoId to put in the playlist
                    {
                        SearchResource.ListRequest? searchListRequest = youtube.Search.List("snippet");
                        searchListRequest.Q = song;
                        searchListRequest.MaxResults = 1;
                        SearchListResponse? searchListResponse = await searchListRequest.ExecuteAsync();

                        string? videoID = searchListResponse.Items[0].Id.VideoId.ToString();

                        PlaylistItem? newPlaylistItem = new PlaylistItem
                        {
                            Snippet = new PlaylistItemSnippet
                            {
                                PlaylistId = newPlaylist.Id,
                                ResourceId = new ResourceId
                                {
                                    Kind = "youtube#video",
                                    VideoId = videoID
                                }
                            }
                        };

                        newPlaylistItem = await service.PlaylistItems.Insert(newPlaylistItem, "snippet").ExecuteAsync();
                        Thread.Sleep(3000);

                        Console.WriteLine(newPlaylist.Id);
                    }

                    // Cleaning up
                    curUser.Songs.Clear(); // Clears out song array so the next person can do it
                }


                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
            }
        }
    }
}