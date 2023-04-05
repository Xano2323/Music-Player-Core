using Music_Player.MusicOrganization;
using Music_Player.MusicPanel;
using System;
using System.Linq;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System.Windows;
using System.Windows.Controls;
using Music_Player.Utility;
using System.IO;
using Newtonsoft.Json;
using Microsoft.WindowsAPICodePack.COMNative.Shell;


namespace Music_Player.MusicList
{
    /// <summary>
    /// List that is currently loaded into list of musics on frontend side (not currently playing list)
    /// </summary>
    public static class DisplayedList
    {
        #region Fields
        public static Playlist Playlist { get; set; }
        private static ScrollViewer Music_ScrollViewer { get; set; }
        public static StackPanel Musics_StackPanel { get; set; }
        private static bool _allowDrop = false;
        #endregion Fields


        #region Methods
        public static void Initialize()
        {
            Music_ScrollViewer = MainWindow.Instance.MusicList_ScrollViewer;
            Music_ScrollViewer.DragEnter += MusicList_ScrollViewer_DragEnter;
            Music_ScrollViewer.DragLeave += MusicList_ScrollViewer_DragLeave;
            Music_ScrollViewer.Drop += MusicList_ScrollViewer_Drop;

            if (Music_ScrollViewer.Content is not StackPanel musicsStackPanel)
                throw new InvalidCastException($"{nameof(Music_ScrollViewer.Content)} has to be of type {typeof(StackPanel)}" );

            Musics_StackPanel = musicsStackPanel;
        }

        /// <summary>
        /// Adds UI Element to musics StackPanel
        /// </summary>
        /// <param name="border">UI Element of one music from playlist</param>
        public static void AddElement(Border border) => Musics_StackPanel.Children.Add(border);

        /// <summary>
        /// Removes previous UI Elements from list and creates new
        /// </summary>
        /// <param name="playlist">New loaded displayed playlist</param>
        public static void LoadPlaylist(Playlist playlist)
        {
            Playlist = playlist;

            Musics_StackPanel?.Children.Clear();

            SelectionHandler.Clear();
            UIMusicElement.LoadMusicsFromPlaylist(playlist);
        }

        public static void SavePlaylist()
        {
            File.WriteAllText(Playlist.playlistJSON.FilePath, JsonConvert.SerializeObject(Playlist.listOfMusicElements, Formatting.Indented));
        }

        private static void MusicList_ScrollViewer_DragEnter(object sender, DragEventArgs e) => _allowDrop = true;
        private static void MusicList_ScrollViewer_DragLeave(object sender, DragEventArgs e) => _allowDrop = false;

        /// <summary>
        /// Drop and check integrity of files dropped on the list of musics
        /// </summary>
        private static void MusicList_ScrollViewer_Drop(object sender, DragEventArgs e)
        {
            if (!_allowDrop)
                return;


            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, true);

            if (fileNames == null)
                return;

            AddMusicsFromStringPathsToDisplayedPlaylist(fileNames);
        }

        public static void AddMusicsFromStringPathsToDisplayedPlaylist(string[] fileNames)
        {
            var collection =
                from fileName
                in fileNames
                where MediaElementController.Instance.PlaylistControler.IsMusicType(fileName)
                select fileName;

            foreach (string fileName in collection)
                ValidMusicName(fileName);

            SavePlaylist();
        }

        /// <summary>
        /// Validates if music format is correct
        /// </summary>
        public static void ValidMusicName(string fileName)
        {
            MusicElement playlistElement = null;
            int index = Playlist.listOfMusicElements.Count;

            using (ShellObject shell = ShellObject.FromParsingName(fileName))
            {
                IShellProperty shellProperty = shell.Properties.System.Media.Duration;

                try
                {
                    ulong time = (ulong)shellProperty.ValueAsObject;
                    TimeSpan normalizedTime = TimeSpan.FromTicks(Convert.ToInt64(time));

                    playlistElement = new MusicElement()
                    {
                        Initiated = true,
                        Index = index,
                        MusicPath = fileName,
                        MusicName = Path.GetFileNameWithoutExtension(fileName),
                        Duration = DateAndTime.GetMusicLength(normalizedTime),
                        DurationNumber = time,
                        DurationTimeSpan = normalizedTime,

                        // New
                        Rating = "",
                        CreationDate = DateAndTime.GetNowDateAndTime(),
                        ListenTimes = 0,
                        ListenLength = "0:00",
                        Groups = "",
                        Tags = ""
                    };
                }
                finally
                {
                    shell.Dispose();
                }
            }

            if (playlistElement != null)
            {
                Playlist.isModified = true;
                Playlist.listOfMusicElements.Add(playlistElement);
                UIMusicElement.CreateMusicElement(Playlist, playlistElement, index);
            }
        }
    }
    #endregion Methods
}