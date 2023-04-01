using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Music_Player.MusicList;
using Music_Player.MusicPanel;


namespace Music_Player.MusicOrganization
{
    public static class PlaylistsManager
    {
        #region Info
        // Pobieranie playlist z json
        // Zapisywanie zmodyfikowanych playlist do jsona danej playlisty
        // Zapisywanie nowo dodanych playlist do jsona
        // Tworzenie elementu playlisty z JSON'a jako TextBlock pod kontrolką UI Playlists
        // Dodanie ContextMenu do TextBlocku odpowiadających za daną playlistę:
        //   Rename
        //   Delete
        //   Load ???


        // JSON:
        //   Nazwy playlist (w tym ścieżka)
        //   Index playlisty

        // JSON danej playlisty:227u 
        //   Tablica elementów muzyki:
        //      Nazwa muzyki, autor, długość trwania itp.
        #endregion Info

        public static List<Playlist> playlists = new List<Playlist>();

        #region Methods
        public static void Initialize()
        {
            LoadPlaylists();
        }

        /// <summary>
        /// Wynik wypełnienia formularza z poprzedniego okna (przycisk Add - w przyszłości + button czy coś takiego)
        /// </summary>
        /// <param name="name">Name of playlist</param>
        public static void AddPlaylist(string name)
        {
            string path = Path.Combine(MainWindow.PlaylistsDirectoryPath, name + ".json");

            if (!Directory.Exists(MainWindow.PlaylistsDirectoryPath))
                Directory.CreateDirectory(MainWindow.PlaylistsDirectoryPath);

            File.Create(path).Close();

            Playlist newPlaylist = new Playlist()
            {
                Index = playlists.Count,

                playlistJSON = new PlaylistJSON()
                {
                    Name = name,
                    FilePath = path
                }
            };

            newPlaylist.listOfMusicElements = JsonConvert.DeserializeObject<List<MusicElement>>(
                File.ReadAllText(newPlaylist.playlistJSON.FilePath),
                new JsonSerializerSettings() { Formatting = Formatting.Indented });

            if (newPlaylist.listOfMusicElements == null)
                newPlaylist.listOfMusicElements = new List<MusicElement>();

            playlists.Add(newPlaylist);

            newPlaylist.label = UIPlaylistElement.CreatePlaylistElement(newPlaylist);

            SavePlaylists();
        }

        /// <summary>
        /// Creates playlists on UI side and List of playlists for further operations (delete, rename)
        /// </summary>
        private static void CreatePlaylistsElements(List<PlaylistJSON> playlistJSONs)
        {
            foreach (PlaylistJSON playlistJSON in playlistJSONs)
            {
                Playlist newPlaylist = new Playlist()
                {
                    Index = playlists.Count,
                    playlistJSON = playlistJSON,
                };

                List<MusicElement> musicElements = JsonConvert.DeserializeObject<List<MusicElement>>(
                    File.ReadAllText(newPlaylist.playlistJSON.FilePath),
                    new JsonSerializerSettings() { Formatting = Formatting.Indented });

                if (musicElements == null)
                    newPlaylist.listOfMusicElements = new List<MusicElement>();
                else
                    newPlaylist.listOfMusicElements = musicElements;

                newPlaylist.label = UIPlaylistElement.CreatePlaylistElement(newPlaylist);

                playlists.Add(newPlaylist);
            }
        }

        /// <summary>
        /// Loads playlists from JSON file
        /// </summary>
        public static void LoadPlaylists()
        {
            if (!File.Exists(MainWindow.PlaylistsJSONPath))
                File.Create(MainWindow.PlaylistsJSONPath).Close();

            List<PlaylistJSON> playlistsJson = JsonConvert.DeserializeObject<List<PlaylistJSON>>(File.ReadAllText(MainWindow.PlaylistsJSONPath),
                new JsonSerializerSettings() { Formatting = Formatting.Indented });

            if (playlistsJson == null)
                playlists = new List<Playlist>();
            else
                CreatePlaylistsElements(playlistsJson);
        }

        /// <summary>
        /// Saves playlists to JSON file
        /// </summary>
        public static void SavePlaylists()
        {
            if (!File.Exists(MainWindow.PlaylistsJSONPath))
                File.Create(MainWindow.PlaylistsJSONPath).Close();

            List<PlaylistJSON> playlistJSONs = new List<PlaylistJSON>();

            foreach (Playlist playlist in playlists)
                playlistJSONs.Add(playlist.playlistJSON);

            File.WriteAllText(MainWindow.PlaylistsJSONPath, JsonConvert.SerializeObject(playlistJSONs, Formatting.Indented));
        }
    }
    #endregion Methods
}