using Music_Player.MusicOrganization;
using System.Windows.Controls;


namespace Music_Player.MusicList
{
    /// <summary>
    /// Class storing informations about selected music elements for mass operations such as "Play selected musics", "Delete selected musics"
    /// </summary>
    public class SelectedMusicElement
    {
        public Border referencedBorder;
        public int referencedIndex;
        public bool isSelected;
        public Playlist referencedPlaylist;


        public SelectedMusicElement(Border _referencedBorder, int index, Playlist _referencedPlaylist)
        {
            referencedBorder = _referencedBorder;
            referencedIndex = index;
            referencedPlaylist = _referencedPlaylist;
        }
    }
}