using Music_Player.MusicOrganization;
using Music_Player.MusicPanel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using Music_Player.Utility;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System;


namespace Music_Player.MusicList
{
    /// <summary>
    /// Creates UI for music elements on the list of musics loaded from playlist or dropped music files
    /// </summary>
    public static class UIMusicElement
    {
        private static readonly GridLength[] COLUMN_GRID_LENGTHS = new GridLength[]
        {
            // ID
            new GridLength(45, GridUnitType.Pixel),

            // Name
            new GridLength(300, GridUnitType.Pixel),

            // Duration
            new GridLength(70, GridUnitType.Pixel),

            // Rating
            new GridLength(60, GridUnitType.Pixel),

            // Creation Time
            new GridLength(160, GridUnitType.Pixel),

            // Video Link
            new GridLength(300, GridUnitType.Pixel),
        };


        /// <summary>
        /// Creates music elements from a playlist
        /// </summary>
        /// <param name="playlist">Playlist to create list of musics from</param>
        public static void LoadMusicsFromPlaylist(Playlist playlist)
        {
            int i = 0;
            foreach (MusicElement element in playlist.listOfMusicElements)
                CreateMusicElement(playlist, element, i++);
        }

        /// <summary>
        /// Creates UI element of a music and adds it to musics StackPanel
        /// </summary>
        public static void CreateMusicElement(Playlist playlist, MusicElement element, int index)
        {
            Border mainBorder = CreateMainBorder(index, playlist);
            Grid grid = CreateGrid();

            Border idBorder = CreateContentBorder(column: 0);
            Border nameBorder = CreateContentBorder(column: 1);
            Border durationBorder = CreateContentBorder(column: 2);
            Border ratingBorder = CreateContentBorder(column: 3);
            Border creationTimeBorder = CreateContentBorder(column: 4);

            // Index
            CreateTextBlock(
                text: (index + 1).ToString(),
                parent: idBorder,
                centered: true);

            // Music Name
            CreateTextBlock(
                text: element.MusicName,
                parent: nameBorder,
                centered: false);

            // Duration
            CreateTextBlock(
                text: element.Duration,
                parent: durationBorder,
                centered: true);

            // Rating
            CreateTextBlock(
                text: element.Rating,
                parent: ratingBorder,
                centered: true);

            // Creation Date And Time
            CreateTextBlock(
                text: element.CreationDate,
                parent: creationTimeBorder,
                centered: true);


            mainBorder.Child = grid;
            grid.Children.Add(idBorder);
            grid.Children.Add(nameBorder);
            grid.Children.Add(durationBorder);
            grid.Children.Add(ratingBorder);
            grid.Children.Add(creationTimeBorder);

            DisplayedList.AddElement(mainBorder);
        }

        /// <summary>
        /// Inner border separating parts of informations
        /// </summary>
        private static Border CreateContentBorder(int column)
        {
            Border border = new Border() { Style = Application.Current.Resources["InnerBorder"] as Style };

            Grid.SetColumn(border, column);

            return border;
        }

        /// <summary>
        /// TextBlock within Inner Border
        /// </summary>
        /// <param name="text">Displayed Text</param>
        /// <param name="parent">Inner Border</param>
        /// <param name="centered">Should text be centered</param>
        private static void CreateTextBlock(string text, Border parent, bool centered = false)
        {
            TextBlock textBlock = new TextBlock()
            {
                Style = Application.Current.Resources["NormalTextBlock"] as Style,
                Text = text,
                Margin = new Thickness(5, 0, 5, 0),
                HorizontalAlignment = centered ? HorizontalAlignment.Center : HorizontalAlignment.Left
            };

            parent.Child = textBlock;
        }

        /// <summary>
        /// Main Border
        /// </summary>
        private static Border CreateMainBorder(int index, Playlist playlist)
        {
            Border border = new Border()
            {
                Style = Application.Current.Resources["MainBorder"] as Style,
                Height = 22,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Focusable = true
            };

            SelectedMusicElement selectedMusicElement = new SelectedMusicElement(border, index, playlist);
            SelectionHandler.entireList.Add(selectedMusicElement);

            border.MouseEnter += (sender, e) => MainBorder_MouseEnterLeaveEvent(sender as Border, selectedMusicElement.isSelected, "Enter");
            border.MouseLeave += (sender, e) => MainBorder_MouseEnterLeaveEvent(sender as Border, selectedMusicElement.isSelected, "Leave");
            border.MouseDown += (sender, e) => MainBorder_MouseDown(sender, e, index, playlist, selectedMusicElement);

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Items.Add(new MenuItem() { Header = "Play" });
            contextMenu.Items.Add(new MenuItem() { Header = "Edit Name" });
            contextMenu.Items.Add(new MenuItem() { Header = "Edit Rating" });
            contextMenu.Items.Add(new MenuItem() { Header = "Delete" });

            (contextMenu.Items[0] as MenuItem).Click += (sender, e) => PlayMusicBasedOnSelection(index, playlist);
            (contextMenu.Items[1] as MenuItem).Click += (sender, e) => EditProperty((border.Child as Grid).Children[1] as Border, index, playlist, "MusicName");
            (contextMenu.Items[2] as MenuItem).Click += (sender, e) => EditProperty((border.Child as Grid).Children[3] as Border, index, playlist, "Rating");

            border.ContextMenu = contextMenu;

            return border;
        }

        private static void MainBorder_MouseEnterLeaveEvent(Border sender, bool isMusicElementSelected, string state)
        {
            if (isMusicElementSelected)
                return;

            const string STATE_NOT_FOUND_ERROR_MESSAGE = "State {0} doesn't exist";

            sender.Background =
                state == "Enter"
                ? Application.Current.Resources["MusicElement_OnHoverGradient"] as LinearGradientBrush
                : (state == "Leave"
                    ? Application.Current.Resources["MusicElement_Background_Border"] as SolidColorBrush
                    : throw new System.Exception(string.Format(STATE_NOT_FOUND_ERROR_MESSAGE, state)));
        }

        /// <summary>
        /// Play music if double clicked, select music element if single clicked for further actions with keys
        /// </summary>
        private static void MainBorder_MouseDown(object sender, MouseButtonEventArgs e, int index, Playlist playlist, SelectedMusicElement selectedMusicElement)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (e.ClickCount == 2)
            {
                PlayMusicAndQueueToRandomIfSelected(index, playlist);
                return;
            }

            SelectionHandler.SelectElements(selectedMusicElement);
        }

        /// <summary>
        /// We want to queue music element if random is on in case we play previous music while random is on <br/>
        /// in case where there are multiple selection made we don't wanna queue it
        /// </summary>
        private static void PlayMusicAndQueueToRandomIfSelected(int index, Playlist playlist)
        {
            if (MainWindow.Instance.IsRandomOn 
                && SelectionHandler.selectedMusicElements?.Count <= 0)
            {
                PlaylistControler.PlayMusicManuallyFromRandom(playlist.listOfMusicElements[index], index, playlist);
                return;
            }

            PlayMusic(index, playlist);
        }

        private static void PlayMusicBasedOnSelection(int index, Playlist playlist)
        {
            if (SelectionHandler.selectedMusicElements.Count <= 0)
            {
                PlayMusicAndQueueToRandomIfSelected(index, playlist);
                return;
            }

            Playlist newPlaylist = new Playlist() { isModified = true };

            int i = 0;
            foreach (SelectedMusicElement selected in SelectionHandler.entireList)
            {
                if (selected.isSelected == false)
                    continue;

                MusicElement musicElement = DisplayedList.Playlist.listOfMusicElements[selected.referencedIndex].DeepClone();
                musicElement.Index = i++;
                newPlaylist.listOfMusicElements.Add(musicElement);
            }

            PlayMusic(0, newPlaylist);
        }

        /// <summary>
        /// Creates input for user for music element modification purposes
        /// </summary>
        public static void EditProperty(Border childsBorder, int index, Playlist playlist, string propertyName)
        {
            if (childsBorder.Child is not TextBlock textBlock)
                return;

            textBlock.Visibility = Visibility.Hidden;

            TextBox textBox = new TextBox()
            {
                Style = Application.Current.Resources["Modifier_TextBox"] as Style,
                Text = textBlock.Text
            };

            textBox.Focus();
            textBox.SelectAll();

            childsBorder.Child = textBox;
            textBox.Focus();

            textBox.LostFocus += (textBox_sender, textBox_e) =>
            {
                if (textBox is null)
                    return;

                ChangeMusicProperty(textBlock, textBox.Text, index, playlist, propertyName);

                textBox = null;
                childsBorder.Child = textBlock;
                textBlock.Visibility = Visibility.Visible;
            };

            textBox.PreviewKeyDown += (textBox_sender, textBox_e) =>
            {
                bool handled = false;

                if (textBox_e.Key == Key.Enter)
                {
                    ChangeMusicProperty(textBlock, textBox.Text, index, playlist, propertyName);
                    handled = true;
                }
                else if (textBox_e.Key == Key.Escape)
                {
                    handled = true;
                }


                if (handled)
                {
                    textBox = null;

                    childsBorder.Child = textBlock;
                    textBlock.Visibility = Visibility.Visible;
                    textBox_e.Handled = true;
                }
            };
        }

        /// <summary>
        /// Changes saved properties to current playlist and applies it to TextBlock as a Text
        /// </summary>
        private static void ChangeMusicProperty(TextBlock textBlockToChange, string destinationText, int index, Playlist playlist, string propertyName)
        {
            typeof(MusicElement).GetProperty(propertyName).SetValue(playlist.listOfMusicElements[index], destinationText);
            textBlockToChange.Text = destinationText;
            DisplayedList.SavePlaylist();
        }

        public static void PlayMusic(int index, Playlist playlist)
        {
            // Zrobić potem zczytywanie indeksu z custom bordera, ktory bedzie w sobie zawieral index
            PlaylistControler.LoadPlaylist(playlist);
            PlaylistControler.Instance.Play(index);
        }

        private static Grid CreateGrid()
        {
            Grid grid = new Grid();

            foreach (GridLength gridLength in COLUMN_GRID_LENGTHS)
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = gridLength });

            return grid;
        }
    }
}