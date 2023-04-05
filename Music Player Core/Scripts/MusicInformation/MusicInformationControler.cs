using Music_Player.MusicList;
using Music_Player.MusicPanel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace Music_Player.MusicInformation
{
    public class MusicInformationControler
    {
        #region Constants
        private const string DUPLICATED_INSTANCE_ERROR_MESSAGE = "Instance of MusicInformationControler can be only one";
        private const string WRONG_TYPE_OF_GRID_CHILDREN_ERROR_MESSAGE = "Grid MusicInformation_Grid.Children music być typu Grid";
        private const string WRONG_NUMBER_OF_CHILDREN_ERROR_MESSAGE =
            "Grid Title Children Count has to be the same as Grid Info Children Count\n" +
            "Grid Title Children Count: {0}\n" +
            "Grid Info Children Count: {1}";
        #endregion Constants

        #region References
        public static MusicInformationControler Instance { get; set; }
        private MainWindow MainWindow { get; }
        private Grid MusicInformation_Grid { get; }
        private TextBlock _fullName;
        private TextBlock _groups;
        private TextBlock _tags;
        private TextBlock _rating;
        private TextBlock _musicLength;
        private TextBlock _creationDate;
        private readonly IDictionary<string, TextBlock> _informationsTextBlocks = new Dictionary<string, TextBlock>();
        #endregion References


        #region Methods
        public MusicInformationControler(Grid _musicInfoGrid, MainWindow _mainWindow)
        {
            if (Instance != null)
                throw new Exception(DUPLICATED_INSTANCE_ERROR_MESSAGE);

            Instance = this;
            MainWindow = _mainWindow;
            MusicInformation_Grid = _musicInfoGrid;
            Grid firstGrid = (MusicInformation_Grid.Children[1] as Grid).Children[1] as Grid;
            Grid secondGrid = (MusicInformation_Grid.Children[2] as Grid).Children[1] as Grid;

            _fullName = firstGrid.Children[0] as TextBlock;
            // firstGrid.Children 1-3 currently removed
            _groups = firstGrid.Children[4] as TextBlock;
            _tags = firstGrid.Children[5] as TextBlock;

            _rating = secondGrid.Children[0] as TextBlock;
            _musicLength = secondGrid.Children[1] as TextBlock;
            _creationDate = secondGrid.Children[2] as TextBlock;

            GetInformationsTextBlocks();
        }

        private void GetInformationsTextBlocks()
        {
            try
            {
                for (int i = 1; i < MusicInformation_Grid.Children.Count; i++)
                {
                    Grid mainGrid = MusicInformation_Grid.Children[i] as Grid;

                    Grid titleGrid = mainGrid.Children[0] as Grid;
                    Grid infoGrid = mainGrid.Children[1] as Grid;

                    if (titleGrid.Children.Count != infoGrid.Children.Count)
                        throw new Exception(string.Format(WRONG_NUMBER_OF_CHILDREN_ERROR_MESSAGE, titleGrid.Children.Count, infoGrid.Children.Count));

                    for (int j = 0; j < titleGrid.Children.Count; j++)
                        _informationsTextBlocks.Add((titleGrid.Children[j] as TextBlock).Text, infoGrid.Children[j] as TextBlock);
                }
            }
            catch (InvalidCastException e)
            {
                throw new Exception(WRONG_TYPE_OF_GRID_CHILDREN_ERROR_MESSAGE + Environment.NewLine + "Error Message: \"" + e.Message + "\"");
            }
        }

        public void ShowInformations()
        {
            MusicInformation_Grid.Visibility =
            MainWindow.CurrentDuration_TextBlock.Visibility =
            MainWindow.MaxDuration_TextBlock.Visibility = Visibility.Visible;
        }

        public void UpdateInformations(MusicElement musicElement)
        {
            // Row 1
            MainWindow.CurrentDuration_TextBlock.Text = "0:00";
            UpdateAlbumCover(MediaElementController.Instance.PlaylistControler.currentlyPlaying.MusicPath);
            _fullName.ToolTip = _fullName.Text = musicElement.MusicName;
            _groups.ToolTip = _groups.Text = musicElement.Groups;
            _tags.ToolTip = _tags.Text = musicElement.Tags;

            // Row 2
            _rating.Text = musicElement.Rating;
            _musicLength.Text = musicElement.Duration;
            _creationDate.Text = musicElement.CreationDate;
        }

        private void UpdateAlbumCover(string filePath)
        {
            // Zabezpieczenie przed usuniętym plikiem
            try
            {
                TagLib.File tagLibFile = TagLib.File.Create(filePath);

                byte[] bytes = tagLibFile.Tag.Pictures[0].Data.Data;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(bytes);
                bitmapImage.EndInit();

                MainWindow.AlbumCover_Image.Source = bitmapImage;
            }
            catch
            {
                MainWindow.AlbumCover_Image.Source = null;
            }
        }

        public void HideInformations() => MusicInformation_Grid.Visibility = Visibility.Hidden;
        #endregion Methods
    }
}