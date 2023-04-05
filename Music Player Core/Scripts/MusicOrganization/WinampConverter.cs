using Music_Player.MusicList;
using Music_Player.MusicPanel;
using Music_Player.Utility;
using System;
using System.IO;
using System.Linq;


namespace Music_Player.MusicOrganization
{
    /// <summary>
    /// Class that creates playlist based on exported playlist from Winamp
    /// </summary>
    public static class WinampConverter
    {
        public static void Convert(string winampPath, string jsonPath)
        {
            string[] lines = File.ReadAllLines(winampPath);

            try
            {
                DisplayedList.LoadPlaylist(PlaylistsManager.playlists[^1]);
            }
            catch (Exception e)
            {
                Logger.ShowStatusWithMessageBox(e.Message, Logger.MessageType.Error);
            }

            var collection = 
                from line 
                in lines 
                where !line.StartsWith("#EXT") && File.Exists(line) 
                select line;

            foreach (string line in collection)
                if (MediaElementController.Instance.PlaylistControler.IsMusicType(line))
                    DisplayedList.ValidMusicName(line);

            DisplayedList.SavePlaylist();
            PlaylistsManager.SavePlaylists();
        }
    }
}