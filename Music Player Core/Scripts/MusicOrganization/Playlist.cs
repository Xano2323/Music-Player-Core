using Music_Player.MusicList;
using System.Collections.Generic;
using System.Windows.Controls;


namespace Music_Player.MusicOrganization
{
    public class Playlist
    {
        public bool isModified;

        public Label label;
        public int Index { get; set; } = -1;
        public List<MusicElement> listOfMusicElements = new List<MusicElement>();
        public PlaylistJSON playlistJSON;
    }
}