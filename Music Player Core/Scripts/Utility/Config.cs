using Music_Player.MusicOrganization;
using Music_Player.MusicPanel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Music_Player.Scripts.Utility
{
    public class Config
    {
        private static string ConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Music Player", "config.config");

        public static Config ConfigFile { get; set; }
        public int MusicIndex { get; set; }
        public int Volume { get; set; }
        public bool IsRandomOn { get; set; }
        public MainWindow.RandomStyle RandomStyle { get; set; }
        public float PlaybackSpeed { get; set; }
        public bool IsMuted { get; set; }
        public string PlaylistName { get; set; } // done
#pragma warning disable
        [JsonIgnore]
        private bool playlistIsFound = false;
#pragma warning restore


        public static void LoadConfiguration()
        {
            if (!File.Exists(ConfigPath))
            {
                File.Create(ConfigPath).Close();
                return;
            }

            ConfigFile = JsonConvert.DeserializeObject<Config>(
                File.ReadAllText(ConfigPath),
                new JsonSerializerSettings() { Formatting = Formatting.Indented });

            foreach (Playlist playlist in from element in PlaylistsManager.playlists where element.playlistJSON.Name == ConfigFile.PlaylistName select element)
            {
                PlaylistControler.LoadPlaylist(playlist);
                ConfigFile.playlistIsFound = true;
                break;
            }
        }

        public static void SaveConfiguration()
        {
            if (!File.Exists(ConfigPath))
                File.Create(ConfigPath).Close();

            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(ConfigFile, Formatting.Indented));
        }
    }
}