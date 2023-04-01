using Music_Player.MusicList;
using Music_Player.MusicPanel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;
using Music_Player.Utility;
using System.Windows.Media.Imaging;


namespace Music_Player.MusicOrganization
{
    /// <summary>
    /// Class for creating playlists read from 
    /// </summary>
    public static class UIPlaylistElement
    {
        private const string REMOVING_PLAYLIST_WARNING = "Do you really want to remove permanently playlist \"{0}\" from your Program?";


        #region Methods
        public static Label CreatePlaylistElement(Playlist playlist)
        {
            Label label = CreatePlaylistLabel(playlist);
            TextBlock textBlock = CreatePlaylistTextBlock(playlist.playlistJSON.Name);
            Button button = CreateLoadButton(playlist);

            Grid grid = new Grid();
            ColumnDefinition[] columnDefinitions = new ColumnDefinition[]
            {
                new ColumnDefinition() { Width = new GridLength(150, GridUnitType.Pixel) },
                new ColumnDefinition() { Width = new GridLength(50 , GridUnitType.Pixel) }
            };
            grid.ColumnDefinitions.Add(columnDefinitions[0]);
            grid.ColumnDefinitions.Add(columnDefinitions[1]);

            Grid.SetColumnSpan(textBlock, 2);

            grid.Children.Add(textBlock);
            grid.Children.Add(button);

            label.Content = grid;

            label.MouseEnter += (sender, e) => button.Visibility = Visibility.Visible;
            label.MouseLeave += (sender, e) => button.Visibility = Visibility.Hidden;

            MainWindow.Instance.Playlists_StackPanel.Children.Add(label);

            return label;
        }

        private static Label CreatePlaylistLabel(Playlist playlist)
        {
            Label label = new Label()
            {
                Height = 40,
                Foreground = Application.Current.Resources["DefaultForeground"] as SolidColorBrush
            };

            ContextMenu contextMenu = new ContextMenu();
            MenuItem[] menuItems = new MenuItem[]
            {
                new MenuItem() { Header = "Show" },
                new MenuItem() { Header = "Load" },
                new MenuItem() { Header = "Change Name" },
                new MenuItem() { Header = "Delete" },
            };

            menuItems[0].Click += (sender, e) => DisplayedList.LoadPlaylist(playlist);
            menuItems[1].Click += (sender, e) => LoadPlaylistAndPlay(playlist);
            menuItems[2].Click += (sender, e) => RenamePlaylist(playlist);
            menuItems[3].Click += (sender, e) => RemovePlaylist(playlist);

            foreach (MenuItem menuItem in menuItems)
                contextMenu.Items.Add(menuItem);

            label.ContextMenu = contextMenu;

            label.MouseEnter += (sender, e) => { label.Background = Application.Current.Resources["PlaylistGradient_OnHover"] as LinearGradientBrush; };
            label.MouseLeave += (sender, e) => { label.Background = new SolidColorBrush(Colors.Transparent); };
            label.MouseDown += (sender, e) =>
            {
                if (e.LeftButton != MouseButtonState.Pressed)
                    return;

                if (e.ClickCount == 1)
                {
                    DisplayedList.LoadPlaylist(playlist);
                    return;
                }

                if (e.ClickCount == 2)
                {
                    LoadPlaylistAndPlay(playlist);
                    return;
                }
            };

            return label;
        }

        private static Button CreateLoadButton(Playlist playlist)
        {
            Button button = new Button()
            {
                Margin = new Thickness(0, 0, 20, 0),
                BorderThickness = new Thickness(0),
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush(Colors.Transparent),
                Visibility = Visibility.Hidden
            };

            Image image = new Image()
            {
                Width = 30,
                Height = 30
            };

            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

            image.BeginInit();
            image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Playlist play.png"));
            image.EndInit();

            button.Content = image;
            Grid.SetColumn(button, 1);

            button.Click += async (sender, e) =>
            {
                LoadPlaylistAndPlay(playlist);
                await Task.Delay(1);
                DisplayedList.LoadPlaylist(playlist);
            };

            return button;
        }

        /// <summary>
        /// Loads playlist as list of musics and plays music based on randomization
        /// </summary>
        private static void LoadPlaylistAndPlay(Playlist playlist)
        {
            PlaylistControler.LoadPlaylist(playlist);

            if (!MainWindow.Instance.IsRandomOn)
            {
                PlaylistControler.Instance.Play(0);
                return;
            }

            int randomNumber;

            if (MainWindow.Instance.randomStyle == MainWindow.RandomStyle.Off)
            {
                Random r = new Random();
                randomNumber = r.Next(0, playlist.listOfMusicElements.Count);
            }
            else if (MainWindow.Instance.randomStyle == MainWindow.RandomStyle.Weighted)
            {
                randomNumber = WeightedRandomization.RandomizeWeighted(playlist, true);
            }
            else
            {
                randomNumber = RatedRandomization.RandomizeRated(playlist, true);
            }

            PlaylistControler.Instance.Play(randomNumber);
        }

        /// <summary>
        /// Shows TextBox user input
        /// </summary>
        private static void RenamePlaylist(Playlist playlist)
        {
            TextBlock textBlock = playlist.label.Content as TextBlock;
            TextBox textBox = new TextBox()
            {
                Style = Application.Current.Resources["Modifier_TextBox"] as Style,
                Text = textBlock.Text,
                Width = 180,
                VerticalAlignment = VerticalAlignment.Center
            };

            textBlock.Visibility = Visibility.Hidden;
            playlist.label.Content = textBox;

            textBox.SelectAll();
            textBox.Focus();

            textBox.LostFocus += (sender, e) => RenamingCancelled(playlist, textBlock, textBox);
            textBox.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Enter)
                    RenamingConfirmed(playlist, textBlock, textBox);
                else if (e.Key == Key.Escape)
                    RenamingCancelled(playlist, textBlock, textBox);
            };
        }

        /// <summary>
        /// Confirms renaming playlist saving it to JSON file and closes TextBox user input
        /// </summary>
        private static void RenamingConfirmed(Playlist playlist, TextBlock textBlock, TextBox textBox)
        {
            if (File.Exists(Path.Combine(MainWindow.PlaylistsDirectoryPath, $"{textBox.Text}.json")))
            {
                Logger.ShowStatusWithMessageBox($"Plik {textBox.Text}.json już istnieje", Logger.MessageType.Warning);
                textBox.SelectAll();
                textBox.Focus();
                return;
            }

            string newPath = Path.Combine(MainWindow.PlaylistsDirectoryPath, textBox.Text + ".json");
            File.Move(playlist.playlistJSON.FilePath, newPath);

            textBlock.Text = playlist.playlistJSON.Name = textBox.Text;
            playlist.playlistJSON.FilePath = newPath;
            PlaylistsManager.SavePlaylists();

            RestoreTextBlock(playlist.label, textBlock, textBox);
        }

        /// <summary>
        /// Closes TextBox user input without saving to to Label Text
        /// </summary>
        private static void RenamingCancelled(Playlist playlist, TextBlock textBlock, TextBox textBox) =>
            RestoreTextBlock(playlist.label, textBlock, textBox);

        private static void RestoreTextBlock(Label label, TextBlock textBlock, TextBox textBox)
        {
            textBlock.Visibility = Visibility.Visible;
            label.Content = textBlock;
        }

        /// <summary>
        /// Deletes choosen playlist and saves it to JSON file
        /// </summary>
        private static void RemovePlaylist(Playlist playlist)
        {
            if (MessageBox.Show(string.Format(REMOVING_PLAYLIST_WARNING, playlist.playlistJSON.Name), "Warning!", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                PlaylistsManager.playlists.Remove(playlist);
                MainWindow.Instance.Playlists_StackPanel.Children.Remove(playlist.label);
                playlist.label = null;
                File.Delete(playlist.playlistJSON.FilePath);
                PlaylistsManager.SavePlaylists();
            }
        }

        /// <summary>
        /// User input for change name of playlist
        /// </summary>
        private static TextBlock CreatePlaylistTextBlock(string playlistName)
        {
            return new TextBlock()
            {
                Margin = new Thickness(3, 0, 3, 0),
                Style = Application.Current.Resources["NormalTextBlock"] as Style,
                Text = Path.GetFileNameWithoutExtension(playlistName)
            };
        }
    }
    #endregion Methods
}