using SpotifyAPI.Web;
using Google.Apis.Auth.OAuth2;


namespace Spotty.Pages
{
    public class User
    {
        public string Client { get; set; }
        public SpotifyClient SpotifyAuth { get; set; }
        public UserCredential GoogleAuth { get; set; }

        public List<string> Songs = new List<string>();

        public List<SimplePlaylist> Playlists;
        
        public bool isSubmitted = false;

        public string AuthCode;
        public User(string ip)
        {
            Client = ip;
        }
    }
}
