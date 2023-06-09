﻿using Music_Player.MusicPanel.Sliders;
using Music_Player.Utility;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace Music_Player.MusicPanel
{
    public class MediaElementController
    {
        // References
        public static MediaElementController Instance { get; set; }
        public PlaylistControler PlaylistControler { get; }
        private MediaElement Music_MediaElement { get; }
        private Border MediaHolder { get; }
        private TextBlock CurrentDuration_TextBlock { get; }
        private TextBlock MaxDuration_TextBlock { get; }
        public static bool BlockDragAndDrop { get; set; }

        // Properties
        public double Volume
        {
            get => Music_MediaElement.Volume;
            set => Music_MediaElement.Volume = value;
        }
        public double PlaybackSpeed
        {
            get => Music_MediaElement.SpeedRatio;
            set => Music_MediaElement.SpeedRatio = value;
        }


        // Constructor
        public MediaElementController(MediaElement _mediaElement, Border _mediaHolder,
            TextBlock _currentDuration, TextBlock _maxDuration, Grid _mediaButtons)
        {
            if (Instance != null)
            {
                string MSG = $"Instancja klasy {nameof(MediaElementController)} została utworzona 2 razy, " +
                    $"instancja ta powinna być pseudo-statyczna";

                MessageBox.Show(MSG, "Powielenie instancji klasy");
                throw new Exception(message: MSG);
            }

            Instance = this;
            Music_MediaElement = _mediaElement;
            MediaHolder = _mediaHolder;
            CurrentDuration_TextBlock = _currentDuration;
            MaxDuration_TextBlock = _maxDuration;
            PlaylistControler = new PlaylistControler(_mediaButtons.Children, Music_MediaElement);
            // póki co 1d na starcie żeby dźwięk się zczytał z programu a nie z ustawień windowsa
            Volume = 1d;

            Music_MediaElement.MediaOpened += Music_MediaElement_MediaOpened;
            Music_MediaElement.MediaEnded += Music_MediaElement_MediaEnded;

            MediaHolder.DragEnter += Border_DragEnter;
            MediaHolder.DragLeave += Border_DragLeave;
            MediaHolder.Drop += Border_Drop;

            StartTimer();
        }

        #region MediaHolder Events
        /// <summary>
        /// Checks if files are in correct format and allow to drop them if they are <br/>
        /// Adds music filenames and paths for Drop event to translate it into MusicElement
        /// </summary>
        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            if (BlockDragAndDrop)
                return;

            PlaylistControler.validMusicNames.Clear();
            e.Effects = DragDropEffects.Copy;

            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, true);

            if (fileNames == null)
                return;

            AddMusicsToMediaPreview(fileNames);
        }

        /// <summary>
        /// Resets mouse icon after leaving area of DragAndDrop
        /// </summary>
        private void Border_DragLeave(object sender, DragEventArgs e)
        {
            if (BlockDragAndDrop)
                return;

            e.Effects = DragDropEffects.None;
            ClearMusicsArray();
        }

        /// <summary>
        /// Creates temporary playlist from music dropped on an empty list
        /// </summary>
        private void Border_Drop(object sender, DragEventArgs e)
        {
            if (BlockDragAndDrop)
                return;

            PlayMusicsFromValidatedMusics();
        }

        /// <summary>
        /// Updates music length in hh:mm:ss format
        /// </summary>
        private void Music_MediaElement_MediaOpened(object sender, RoutedEventArgs e) => 
            MaxDuration_TextBlock.Text = PlaylistControler.currentlyPlaying.Duration;

        /// <summary>
        /// Play next music if music is over
        /// </summary>
        private void Music_MediaElement_MediaEnded(object sender, RoutedEventArgs e) => 
            PlaylistControler.PlayNext();
        #endregion MediaHolder Events

        /// <summary>
        /// If there is no displayed list it will add files to temporary, independent playlist
        /// </summary>
        public void AddMusicsFromFileDialog(string[] fileNames)
        {
            AddMusicsToMediaPreview(fileNames);
            PlayMusicsFromValidatedMusics();
            ClearMusicsArray();
        }

        /// <summary>
        /// Validates names and adds correct music files to queue
        /// </summary>
        private void AddMusicsToMediaPreview(string[] fileNames)
        {
            foreach (string fileName in fileNames)
                PlaylistControler.AddMusicFile(fileName);
        }

        /// <summary>
        /// If list of validated musics exists it will play it
        /// </summary>
        private void PlayMusicsFromValidatedMusics()
        {
            if (PlaylistControler.validMusicNames.Count > 0)
                PlaylistControler.CreatePlaylist();
        }

        /// <summary>
        /// Clears current musics array
        /// </summary>
        private void ClearMusicsArray() =>
            PlaylistControler.validMusicNames.Clear();

        /// <summary>
        /// Sets pointer at specific music time tick when changed with a slider
        /// </summary>
        public void SetTime(double percentageValue)
        {
            TimeSpan currentDuration = TimeSpan.FromTicks(Convert.ToInt64(percentageValue * PlaylistControler.currentlyPlaying.DurationNumber));
            Music_MediaElement.Position = currentDuration;

            CurrentDuration_TextBlock.Text = DateAndTime.GetMusicLength(currentDuration);
        }

        /// <summary>
        /// Corrects UI elements (slider for current time) and changes current time TextBlock based actual music time
        /// </summary>
        public void SetTime()
        {
            CurrentDuration_TextBlock.Text = DateAndTime.GetMusicLength(Music_MediaElement.Position);

            MusicLengthSlider.Instance.SetValue(Convert.ToDouble(Music_MediaElement.Position.Ticks)
                / Convert.ToDouble(PlaylistControler.currentlyPlaying.DurationTimeSpan.Ticks));
        }

        /// <summary>
        /// Updates current time of a music if its playing
        /// </summary>
        private async void StartTimer()
        {
            const int REFRESH_TIME = 1000;

            while (true)
            {
                await Task.Delay((int)(REFRESH_TIME / PlaybackSpeed));

                if (PlaylistControler.state == PlaylistControler.State.Playing)
                    SetTime();
            }
        }
    }
}