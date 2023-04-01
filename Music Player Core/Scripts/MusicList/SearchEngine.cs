using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.DirectoryServices;
using System.Reflection;
using System.Windows.Media;


namespace Music_Player.MusicList
{
    public static class SearchEngine
    {
        #region Fields
        private static TextBox SearchInput_TextBox { get; set; }
        private static Border SearchBorder_Border { get; set; }
        private static RowDefinition SearchPanel_RowDefinition { get; set; }
        private static TextBox SearchRating_TextBox { get; set; }
        #endregion Fields


        #region Methods
        public static void Initialize()
        {
            SearchInput_TextBox = MainWindow.Instance.SearchInputField_TextBox;
            SearchBorder_Border = MainWindow.Instance.SearchPanel_Border;
            SearchPanel_RowDefinition = MainWindow.Instance.SearchPanel_RowDefinition;
            SearchRating_TextBox = MainWindow.Instance.SearchRating_TextBox;

            MainWindow.Instance.SearchButton.Click += (_, _) => Search();
            MainWindow.Instance.CancelSearchingButton.Click += (_, _) => CancelSearching();
            MainWindow.Instance.CloseSearchPanel_Button.Click += (_, _) => CloseSearchPanel();
            SearchInput_TextBox.KeyDown += (_, e) => SearchInputField_TextBox_KeyDown(e);
            SearchRating_TextBox.KeyDown += (_, e) => SearchInputField_TextBox_KeyDown(e);
        }

        private static void SearchInputField_TextBox_KeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Filters current list of musics based on searched name and rating of music
        /// </summary>
        private static void Search()
        {
            int index = 0;
            string searchResult = SearchInput_TextBox.Text.ToLower();

            foreach (Border border in DisplayedList.Musics_StackPanel.Children)
            {
                string musicName = DisplayedList.Playlist.listOfMusicElements[index].MusicName.ToLower();
                string rating = DisplayedList.Playlist.listOfMusicElements[index].Rating;

                border.Visibility = (HasName(searchResult, musicName) && HasRating(rating))
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                index++;
            }
        }

        private static bool HasName(string searchResult, string musicName) =>
            string.IsNullOrWhiteSpace(searchResult) || musicName.Contains(searchResult);

        #region Rating Filters
        /// <summary>
        /// Filters based on rating
        /// </summary>
        private static bool HasRating(string rating)
        {
            string text = SearchRating_TextBox.Text.Replace(" ", "");

            if (string.IsNullOrEmpty(text))
                return true;

            if (text.Contains("-"))
                return HasRatingRange(text, rating);

            if (text.Contains(">"))
                return IsRatingGreater(text, rating);

            if (text.Contains("<"))
                return IsRatingLesser(text, rating);

            return IsRatingEqual(text, rating);
        }

        private static bool HasRatingRange(string text, string rating)
        {
            decimal min = CheckSide(text, "-", "Left");
            decimal max = CheckSide(text, "-", "Right");

            if (min == -1 || max == 1)
                return false;

            return decimal.TryParse(rating, out decimal result) && result >= min && result <= max;
        }

        private static bool IsRatingGreater(string text, string rating)
        {
            decimal number = CheckSide(text, ">", "Right");

            if (number == -1)
            {
                number = CheckSide(text, ">", "Left");

                if (number == -1)
                    return false;
            }

            return decimal.TryParse(rating, out decimal result) && result >= number;
        }

        private static bool IsRatingLesser(string text, string rating)
        {
            decimal number = CheckSide(text, "<", "Right");

            if (number == -1)
            {
                number = CheckSide(text, "<", "Left");

                if (number == -1)
                    return false;
            }

            return decimal.TryParse(rating, out decimal result) && result <= number;
        }

        private static bool IsRatingEqual(string text, string rating) =>
            decimal.TryParse(text.Replace("=", ""), out decimal result)
            && decimal.TryParse(rating, out decimal ratingResult)
            && ratingResult == result;


        private static decimal CheckSide(string text, string character, string side)
        {
            string number = (side == "Left")
                ? text.Substring(0, text.IndexOf(character))
                : text.Substring(text.IndexOf(character) + 1, text.Length - text.IndexOf(character) - 1);

            if (!string.IsNullOrEmpty(number))
                if (decimal.TryParse(number, out decimal result))
                    return result;

            return -1;
        }
        #endregion Rating Filters

        public static void ShowSearchPanel()
        {
            SearchPanel_RowDefinition.Height = new GridLength(35, GridUnitType.Pixel);
            SearchBorder_Border.Visibility = Visibility.Visible;
            SearchInput_TextBox.Focus();
            SearchInput_TextBox.SelectAll();
        }

        /// <summary>
        /// Closes Search Panel and removes filtered musics
        /// </summary>
        private static void CloseSearchPanel()
        {
            CancelSearching();

            SearchBorder_Border.Visibility = Visibility.Hidden;
            SearchPanel_RowDefinition.Height = new GridLength(0, GridUnitType.Pixel);
        }

        /// <summary>
        /// Removes Filters and brings back all musics from list that where hidden
        /// </summary>
        private static void CancelSearching()
        {
            foreach (Border border in DisplayedList.Musics_StackPanel.Children)
                border.Visibility = Visibility.Visible;
        }
    }
    #endregion Methods
}