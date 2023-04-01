using Music_Player.MusicOrganization;
using Music_Player.Utility;
using System.IO;
using System.Windows;


namespace Music_Player.Windows
{
    public partial class PlaylistCreatorWindow : Window
    {
        #region Fields
        private const string EMPTY_LIST_NAME_ERROR = "Playlist name cannot be empty";
        private const string PLAYLIST_ALREADY_EXISTS = "Playlist already exists";
        private MainWindow MainWindow { get; set; }
        #endregion Fieldss


        #region Methods
        public PlaylistCreatorWindow(MainWindow _mainWindow)
        {
            InitializeComponent();
            MainWindow = _mainWindow;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistName_TextBox.Text == string.Empty)
            {
                Logger.ShowStatusWithMessageBox(EMPTY_LIST_NAME_ERROR, Logger.MessageType.Error);
                return;
            }

            if (File.Exists(Path.Combine(MainWindow.PlaylistsDirectoryPath, PlaylistName_TextBox.Text + ".json")))
            {
                Logger.ShowStatusWithMessageBox(PLAYLIST_ALREADY_EXISTS, Logger.MessageType.Error);
                return;
            }

            foreach (Playlist playlist in PlaylistsManager.playlists)
            {
                if (playlist.playlistJSON.Name.ToLower() != PlaylistName_TextBox.Text.ToLower())
                    continue;

                Logger.ShowStatusWithMessageBox(PLAYLIST_ALREADY_EXISTS, Logger.MessageType.Error);
                return;
            }

            CreatePlaylist();
        }

        private void CreatePlaylist()
        {
            if ((bool)ImportActive_CheckBox.IsChecked)
            {
                if (!File.Exists(ImportLink_TextBox.Text))
                {
                    Logger.ShowStatusWithMessageBox("Nazwa lub ścieżka pliku nieprawidłowa", Logger.MessageType.Error);
                    return;
                }

                PlaylistsManager.AddPlaylist(PlaylistName_TextBox.Text);
                WinampConverter.Convert(ImportLink_TextBox.Text, PlaylistName_TextBox.Text);
            }
            else
            {
                PlaylistsManager.AddPlaylist(PlaylistName_TextBox.Text);
            }

            Close();
        }

        /// <summary>
        /// Gives message to main window that PlaylistCreator window can be opened again
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) =>
            MainWindow.playlistCreator = null;

        /// <summary>
        /// Allows drag and drop for file of type .m3u (winamp exported list extension)
        /// </summary>
        private void Grid_Drop(object sender, DragEventArgs e)
        {
            string fileName = ((string[])e.Data.GetData(DataFormats.FileDrop, true))[0];

            if (Path.GetExtension(fileName) == ".m3u")
            {
                ImportActive_CheckBox.IsChecked = true;
                ImportLink_TextBox.IsEnabled = true;
                ImportLink_TextBox.Text = fileName;
            }
            else
            {
                Logger.ShowStatus("Wrong Winamp extension", Logger.MessageType.Error);
            }
        }

        private void ImportActive_CheckBox_Changed(object sender, RoutedEventArgs e) =>
            ImportLink_TextBox.IsEnabled = (bool)ImportActive_CheckBox.IsChecked;
    }
    #endregion Methods
}