using Music_Player.MusicOrganization;
using Music_Player.MusicPanel;
using Music_Player.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace Music_Player.MusicList
{
    /// <summary>
    /// Mass music selection operations
    /// </summary>
    public static class SelectionHandler
    {
        public static List<SelectedMusicElement> entireList = new List<SelectedMusicElement>();
        /// <summary>
        /// If there is multi selection happening it will fill up based on selected musics <br/>
        /// if there is only one selection it will use currentSingleSelection instead
        /// </summary>
        public static Dictionary<int, SelectedMusicElement> selectedMusicElements = new Dictionary<int, SelectedMusicElement>();
        public static SelectedMusicElement currentSingleSelection;
        public static bool IsHoldingCtrl { get; set; } = false;
        public static bool IsHoldingShift { get; set; } = false;
        public static int FirstSelectedShift { get; set; } = -1;


        public static void SelectElements(SelectedMusicElement selectedMusicElement)
        {
            if (IsHoldingCtrl && IsHoldingShift)
            {
                AddMultipleElementToExistingSelection(selectedMusicElement);
                return;
            }

            if (IsHoldingCtrl)
            {
                AddMultiSelected(selectedMusicElement);
                return;
            }

            if (IsHoldingShift)
            {
                ExtendCurrentSelectionToChoosenElement(selectedMusicElement);
                return;
            }

            ChangeSelectedMusicElement(selectedMusicElement);
        }

        /// <summary>
        /// First click marks beggining for new selection, second click adds choosen range of elements to existing selection
        /// </summary>
        private static void AddMultipleElementToExistingSelection(SelectedMusicElement selectedMusicElement)
        {
            if (!selectedMusicElement.isSelected)
            {
                if (FirstSelectedShift == -1)
                {
                    FirstSelectedShift = selectedMusicElement.referencedIndex;
                    AddMultiSelected(selectedMusicElement, false);
                }
                else
                {
                    SelectMultiple(selectedMusicElement);
                    FirstSelectedShift = -1;
                }
            }
        }

        /// <summary>
        /// Based on current selected element, extends selection to choosen element
        /// </summary>
        private static void ExtendCurrentSelectionToChoosenElement(SelectedMusicElement selectedMusicElement)
        {
            if (currentSingleSelection == null)
            {
                ChangeSelectedMusicElement(selectedMusicElement);
                FirstSelectedShift = selectedMusicElement.referencedIndex;
            }
            else
            {
                FirstSelectedShift = currentSingleSelection.referencedIndex;
                UnselectAll();
                SelectMultiple(selectedMusicElement);
            }
        }

        /// <summary>
        /// Handles shift and ctrl selection for multiple choices
        /// </summary>
        private static void SelectMultiple(SelectedMusicElement selectedMusicElement)
        {
            int second = selectedMusicElement.referencedIndex;

            if (second == FirstSelectedShift)
                return;

            int first = ReverseIndexes(FirstSelectedShift, ref second);

            for (int i = first; i <= second; i++)
            {
                if ((DisplayedList.Musics_StackPanel.Children[i] as Border).Visibility != Visibility.Visible)
                    continue;

                if (!entireList[i].isSelected)
                    AddMultiSelected(entireList[i], false);
            }
        }

        /// <summary>
        /// Multiselection when chosing music elements that are above first selected music index
        /// </summary>
        private static int ReverseIndexes(int first, ref int second)
        {
            if (first > second)
            {
                int reversedFirst = second;
                second = first;
                return reversedFirst;
            }
            else return first;
        }

        public static void ChangeSelectedMusicElement(SelectedMusicElement selectedMusicElement)
        {
            if (selectedMusicElements.Count > 0)
            {
                foreach (SelectedMusicElement selected in selectedMusicElements.Values)
                    Unselect(selected);

                selectedMusicElements.Clear();
            }

            if (currentSingleSelection != null)
                Unselect(currentSingleSelection);

            currentSingleSelection = selectedMusicElement;
            Select(currentSingleSelection);
        }

        public static void AddMultiSelected(SelectedMusicElement selectedMusicElement, bool removeCurrent = true)
        {
            if (removeCurrent && currentSingleSelection != null)
            {
                selectedMusicElements.Add(currentSingleSelection.referencedIndex, currentSingleSelection);
                currentSingleSelection = null;
            }

            if (!selectedMusicElement.isSelected)
            {
                selectedMusicElements.Add(selectedMusicElement.referencedIndex, selectedMusicElement);
                Select(selectedMusicElement);
            }
            else
            {
                selectedMusicElements.Remove(selectedMusicElement.referencedIndex);
                Unselect(selectedMusicElement);
            }
        }

        /// <summary>
        /// Clears current selections
        /// </summary>
        public static void Clear()
        {
            entireList.Clear();
            selectedMusicElements.Clear();
            currentSingleSelection = null;
            IsHoldingCtrl = IsHoldingShift = false;
            FirstSelectedShift = -1;
        }

        public static void UnselectAll()
        {
            foreach (SelectedMusicElement selected in selectedMusicElements.Values)
                Unselect(selected);

            selectedMusicElements.Clear();
        }

        private static void Unselect(SelectedMusicElement selected)
        {
            selected.isSelected = false;
            selected.referencedBorder.Background = Application.Current.Resources["MusicElement_Background_Border"] as SolidColorBrush;
        }

        private static void Select(SelectedMusicElement selected)
        {
            selected.isSelected = true;
            selected.referencedBorder.Background = Application.Current.Resources["MusicElement_SelectionGradient"] as LinearGradientBrush;
        }

        /// <summary>
        /// Plays selected music or creates temporary playlist based on selection and plays musics only from that selection
        /// </summary>
        public static void PlaySelected()
        {
            if (selectedMusicElements.Count <= 0)
            {
                if (currentSingleSelection == null)
                    return;

                PlayMusic(currentSingleSelection.referencedIndex, currentSingleSelection.referencedPlaylist);
                return;
            }


            Playlist newPlaylist = new() { isModified = true };
            int i = 0;
            foreach (SelectedMusicElement selected in entireList)
            {
                if (!selected.isSelected)
                    continue;

                MusicElement musicElement = DisplayedList.Playlist.listOfMusicElements[selected.referencedIndex].DeepClone();
                musicElement.Index = i++;
                newPlaylist.listOfMusicElements.Add(musicElement);
            }

            PlayMusic(0, newPlaylist);
        }

        /// <summary>
        /// Loads playlist if its modified (if multiselected play) or plays the music based on current playlist
        /// </summary>
        private static void PlayMusic(int index, Playlist playlist)
        {
            PlaylistControler.LoadPlaylist(playlist);
            PlaylistControler.Instance.Play(index);
        }

        public static void SelectAll()
        {
            selectedMusicElements = new Dictionary<int, SelectedMusicElement>();

            foreach (SelectedMusicElement selectedMusicElement in entireList)
            {
                if (selectedMusicElement.referencedBorder.Visibility != Visibility.Visible)
                    continue;

                selectedMusicElements.Add(selectedMusicElement.referencedIndex, selectedMusicElement);
                Select(selectedMusicElement);
            }
        }
    }
}