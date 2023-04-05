using System;
using System.Windows;
using System.Windows.Media;
using Music_Player.Windows;
using Music_Player.MusicPanel;
using Music_Player.MusicPanel.Sliders;
using Music_Player.MusicInformation;
using Music_Player.Utility;
using Music_Player.MusicOrganization;
using Music_Player.MusicList;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Music_Player
{
    public partial class MainWindow : Window
    {
        #region Constants
        private const string RANDOM_OFF = "pack://application:,,,/Resources/Random Off.png";
        private const string RANDOM_ON = "pack://application:,,,/Resources/Next Random.png";
        private const string RANDOM_STYLE_OFF = "pack://application:,,,/Resources/Next Weighted.png";
        private const string RANDOM_STYLE_WEIGHTED = "pack://application:,,,/Resources/Next Normal.png";
        private const string RANDOM_STYLE_RATED = "pack://application:,,,/Resources/Random Rated.png";
        private const double SECONDS_WINDUP = 5d;
        #endregion Constants

        #region Components
        public enum RandomStyle { Off, Weighted, Rated }
        #endregion Components

        #region References
        private static MainWindow instance;
        internal static MainWindow Instance => instance;
        public PlaylistCreatorWindow playlistCreator;
        private ImageViewer imageViewer;
        #endregion References

        #region Fields
        public bool IsMusicLooped { get; set; } = false;
        public bool IsRandomOn { get; set; } = false;
        public static string LoggerPath { get; set; }
        public static string DirectoryPath { get; set; }
        public static string PlaylistsDirectoryPath { get; set; }
        public static string PlaylistsJSONPath { get; set; }
        public RandomStyle randomStyle = RandomStyle.Off;
        #endregion Fields


        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch { }
            instance = this;
        }

        /// <summary>
        /// Instantiate dependant objects
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App.mainWindow = this;
            CreateListsIfDoesntExists();
            Logger.InitializeLogger(Logger_Border);
            PlaylistsManager.Initialize();
            DisplayedList.Initialize();
            SearchEngine.Initialize();

            VolumeControler volumeControler = new VolumeControler(Volume_Slider, VolumeDisplay_TextBlock, Music_MediaElement);
            _ = new StoryboardAnimator(this, MusicName_TextBlock, GridMusicPlayerColumnWidth);
            _ = new PlaybackSpeedControler(PlaybackSpeed_Slider, PlaybackSpeedDisplay_TextBlock, Music_MediaElement);
            _ = new MusicLengthSlider(MusicTimer_Slider, CurrentDuration_TextBlock);
            _ = new MusicLengthPreviewSlider(PreviewMusicTimer_Slider, MusicTimer_Slider, CurrentDuration_TextBlock, Music_MediaElement);
            _ = new MuteUnmute(MuteUnmute_Button, volumeControler);
            _ = new MediaElementController(Music_MediaElement, MediaElementHolder_Border, CurrentDuration_TextBlock, MaxDuration_TextBlock, MediaButtons_Grid);
            _ = new MusicInformationControler(MusicInformation_Grid, this);
            imageViewer = new ImageViewer();
            App.currentThumbBrush = Application.Current.Resources["GrayWhiteGradient"] as LinearGradientBrush;
        }

        /// <summary>
        /// Creates Music Player directory in local appdata with Playlists folder and file Playlists.json if it doesn't exist
        /// </summary>
        private void CreateListsIfDoesntExists()
        {
            DirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Music Player");
            PlaylistsDirectoryPath = Path.Combine(DirectoryPath, "Playlists");
            PlaylistsJSONPath = Path.Combine(DirectoryPath, "Playlists.json");
            LoggerPath = Path.Combine(DirectoryPath, "Logger.txt");

            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);

            if (!Directory.Exists(PlaylistsDirectoryPath))
                Directory.CreateDirectory(PlaylistsDirectoryPath);

            if (!File.Exists(PlaylistsJSONPath))
                File.Create(PlaylistsJSONPath).Close();

            if (!File.Exists(LoggerPath))
                File.Create(LoggerPath).Close();
        }

        /// <summary>
        /// Activates window for new playlist creation
        /// </summary>
        private void AddNewPlaylist_Button_Click(object sender, RoutedEventArgs e)
        {
            if (playlistCreator != null)
                playlistCreator.Activate();
            else
            {
                playlistCreator = new PlaylistCreatorWindow(this);
                playlistCreator.Show();
            }
        }

        /// <summary>
        /// Action for clicking on album cover
        /// </summary>
        private void AlbumCover_Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.LeftButton == MouseButtonState.Pressed)
            {
                if (AlbumCover_Image == null)
                    return;

                string path = PlaylistControler.Instance.currentlyPlaying.MusicPath;

                TagLib.File tagLibFile = TagLib.File.Create(path);

                byte[] bytes = tagLibFile.Tag.Pictures[0].Data.Data;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(bytes);
                bitmapImage.EndInit();

                imageViewer.Image.Source = bitmapImage;

                imageViewer.ShowWindow();
            }
        }

        /// <summary>
        /// Closes dependant windowses before closing main window
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            imageViewer.CanClose = true;
            imageViewer.Close();
            if (playlistCreator is not null)
                playlistCreator.Close();
            Logger.SaveToLoggerFile("Program has been closed", Logger.MessageType.Success);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    SelectionHandler.PlaySelected();
                    break;
                case Key.LeftCtrl:
                    SelectionHandler.IsHoldingCtrl = true;
                    break;
                case Key.LeftShift:
                    SelectionHandler.IsHoldingShift = true;
                    break;
                case Key.F:
                    if (SelectionHandler.IsHoldingCtrl)
                        SearchEngine.ShowSearchPanel();
                    return;
                case Key.A:
                    if (SelectionHandler.IsHoldingCtrl)
                        SelectionHandler.SelectAll();
                    return;
                case Key.F2:
                    if (SelectionHandler.currentSingleSelection is not null)
                        UIMusicElement.EditProperty((SelectionHandler.currentSingleSelection.referencedBorder.Child as Grid).Children[1] as Border, SelectionHandler.currentSingleSelection.referencedIndex, DisplayedList.Playlist, "MusicName");
                    return;
                case Key.F3:
                    if (SelectionHandler.currentSingleSelection is not null)
                        UIMusicElement.EditProperty((SelectionHandler.currentSingleSelection.referencedBorder.Child as Grid).Children[3] as Border, SelectionHandler.currentSingleSelection.referencedIndex, DisplayedList.Playlist, "Rating");
                    return;

                default:
                    return;
            }

            e.Handled = true;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftCtrl:
                    SelectionHandler.IsHoldingCtrl = false;
                    e.Handled = true;
                    return;
                case Key.LeftShift:
                    SelectionHandler.IsHoldingShift = false;
                    SelectionHandler.FirstSelectedShift = -1;
                    e.Handled = true;
                    return;
            }

            e.Handled = true;
        }

        private void VolumeUp() => Volume_Slider.Value = Mathf.Clamp(Volume_Slider.Value + (Volume_Slider.Value % 2 == 0 ? 2d : 1d), 0d, 100d);
        private void VolumeDown() => Volume_Slider.Value = Mathf.Clamp(Volume_Slider.Value - (Volume_Slider.Value % 2 == 0 ? 2d : 1d), 0d, 100d);

        private void RandomSwitchOnOff_Button_Click(object sender, RoutedEventArgs e) => RandomOptionChange();
        private void RandomOptionChange() => Random_Image.Source = new BitmapImage(new Uri((IsRandomOn = !IsRandomOn) ? RANDOM_ON : RANDOM_OFF));
        private void RandomStyle_Button_Click(object sender, RoutedEventArgs e) => ChangeRandomStyle();

        public void ChangeRandomStyle()
        {
            if (randomStyle == RandomStyle.Off)
            {
                randomStyle = RandomStyle.Weighted;
                RandomStyle_Image.Source = new BitmapImage(new Uri(RANDOM_STYLE_WEIGHTED));
            }
            else if (randomStyle == RandomStyle.Weighted)
            {
                randomStyle = RandomStyle.Rated;
                RandomStyle_Image.Source = new BitmapImage(new Uri(RANDOM_STYLE_RATED));
            }
            else
            {
                randomStyle = RandomStyle.Off;
                RandomStyle_Image.Source = new BitmapImage(new Uri(RANDOM_STYLE_OFF));
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (SearchInputField_TextBox.IsFocused)
                return;

            switch (e.Key)
            {
                case Key.Right:
                    SkipForwardSeconds();
                    e.Handled = true;
                    break;
                case Key.Left:
                    SkipBackwardSeconds();
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Rewinds 5s forwards
        /// </summary>
        private void SkipForwardSeconds()
        {
            if (PlaylistControler.Instance.currentlyPlaying == null)
                return;

            MusicLengthSlider.Instance.SetValue((Convert.ToDouble(Music_MediaElement.Position.Ticks) + SECONDS_WINDUP * TimeSpan.TicksPerSecond)
                / Convert.ToDouble(PlaylistControler.Instance.currentlyPlaying.DurationTimeSpan.Ticks), false);

            MediaElementController.Instance.SetTime();
        }

        /// <summary>
        /// Rewinds 5s backwards
        /// </summary>
        private void SkipBackwardSeconds()
        {
            if (PlaylistControler.Instance.currentlyPlaying == null)
                return;

            MusicLengthSlider.Instance.SetValue((Convert.ToDouble(Music_MediaElement.Position.Ticks) - SECONDS_WINDUP * TimeSpan.TicksPerSecond)
                / Convert.ToDouble(PlaylistControler.Instance.currentlyPlaying.DurationTimeSpan.Ticks), false);

            MediaElementController.Instance.SetTime();
        }

        private void JumpToMusic_Button_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistControler.currentPlaylist?.listOfMusicElements.Count == 0
                || PlaylistControler.Instance.currentlyPlaying is null)
                return;

            if (DisplayedList.Playlist != PlaylistControler.currentPlaylist)
                DisplayedList.LoadPlaylist(PlaylistControler.currentPlaylist);

            foreach (object border in DisplayedList.Musics_StackPanel.Children)
            {
                TextBlock focusedElement = (((border as Border).Child as Grid).Children[1] as Border).Child as TextBlock;

                if (focusedElement.Text != PlaylistControler.Instance.currentlyPlaying.MusicName)
                    continue;

                focusedElement.BringIntoView();
                SelectionHandler.SelectElements(new SelectedMusicElement(border as Border, PlaylistControler.Instance.currentlyPlaying.Index, PlaylistControler.currentPlaylist));
            }

        }

        private void Loop_Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button.Content is not Image image)
                return;

            LoopMusic(image);
        }

        public void LoopMusic(Image image)
        {
            const string LOOPED_IMAGE_PATH = "pack://application:,,,/Resources/Loop.png";
            const string NOT_LOOPED_IMAGE_PATH = "pack://application:,,,/Resources/Loop Not.png";

            IsMusicLooped = !IsMusicLooped;

            image.BeginInit();
            image.Source = new BitmapImage(new Uri(IsMusicLooped ? LOOPED_IMAGE_PATH : NOT_LOOPED_IMAGE_PATH));
            image.EndInit();
        }

        private void SkipLooped()
        {
            if (IsMusicLooped is false)
                return;

            LoopMusic(Loop_Image);
            PlaylistControler.Instance.PlayNext();
        }

        /// <summary>
        /// Music List ScrollViewer - not implemented yet - don't remove it has assigned controller
        /// </summary>
        private void MusicList_ScrollViewer_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        /// <summary>
        /// Music List ScrollViewer - not implemented yet - don't remove it has assigned controller
        /// </summary>
        private void MusicList_ScrollViewer_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        /// <summary>
        /// Button for adding files via FileDialog
        /// </summary>
        private void AddMusicsFromDirectoryDialog_Button_Click(object sender, RoutedEventArgs e)
        {
            MediaElementController.BlockDragAndDrop = true;

            FileDialog fileDialog = new OpenFileDialog()
            {
                Multiselect = true
            };

            fileDialog.ShowDialog();

            string[] fileNames = fileDialog.FileNames;

            if (DisplayedList.Playlist is null)
            {
                MediaElementController.Instance.AddMusicsFromFileDialog(fileNames);
            }
            else
            {
                DisplayedList.AddMusicsFromStringPathsToDisplayedPlaylist(fileNames);
            }

            MediaElementController.BlockDragAndDrop = false;
        }
    }
}