using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Music_Player.MusicInformation;
using Music_Player.MusicList;
using Music_Player.MusicOrganization;
using Music_Player.MusicPanel.Sliders;
using Music_Player.Utility;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace Music_Player.MusicPanel
{
    public class PlaylistControler
    {
        #region Constants
        private const string TOOLTIP_NOT_IMPLEMENTED_MESSAGE_ERROR = "Przycisk z Tooltipem \"{0}\" nie został zaimplementowany";
        private const string CAST_TYPE_MESSAGE_ERROR =
            "Przycisk dla elementu sterowania muzyką musi być typu Button a jego objekt child typu Image";
        private const string FILE_FORMAT_NOT_SUPPORTED_MESSAGE_ERROR = "File format: {0} is not supported!";
        #endregion Constants

        #region Data
        private static readonly string[] audioFormats = new string[]
        {
            ".alac",
            ".amr",
            ".ape",
            ".au",
            ".awb",
            ".dss",
            ".dvf",
            ".flac",
            ".gsm",
            ".iklax",
            ".ivs",
            ".m4a",
            ".m4b",
            ".m4p",
            ".mmf",
            ".mp3",
            ".mpc",
            ".msv",
            ".nmf",
            ".ogg",
            ".oga",
            ".mogg",
            ".opus",
            ".org",
            ".ra",
            ".rm",
            ".raw",
            ".rf64",
            ".sln",
            ".tta",
            ".voc",
            ".vox",
            ".wav",
            ".wma",
            ".wv",
            ".webm",
            ".8svx",
            ".cda",
            ".webm",
            ".mkv",
            ".flv",
            ".vob",
            ".ogv",
            ".ogg",
            ".drc",
            ".gif",
            ".gifv",
            ".mng",
            ".avi",
            ".MTS",
            ".M2TS",
            ".TS",
            ".mov",
            ".qt",
            ".wmv",
            ".yuv",
            ".rm",
            ".rmvb",
            ".viv",
            ".asf",
            ".amv",
            ".mp4",
            ".m4p",
            ".m4v",
            ".mpg",
            ".mp2",
            ".mpeg",
            ".mpe",
            ".mpv",
            ".mpg",
            ".mpeg",
            ".m2v",
            ".m4v",
            ".svi",
            ".3gp",
            ".3g2",
            ".mxf",
            ".roq",
            ".nsv",
            ".flv",
            ".f4v",
            ".f4p",
            ".f4a",
            ".f4b"
        };

        private static readonly IDictionary<State, BitmapImage> pauseResumeIcons = new Dictionary<State, BitmapImage>()
        {
            [State.Playing] = new BitmapImage(new Uri("pack://application:,,,/Resources/Pause.png")),
            [State.Paused] = new BitmapImage(new Uri("pack://application:,,,/Resources/Play.png"))
        };
        #endregion Data

        #region Fields
        // Components
        public enum State { Paused, Playing };

        // Variables
        public List<string> validMusicNames = new List<string>();
        public static Playlist currentPlaylist;
        public MusicElement currentlyPlaying;
        public State state = State.Paused;
        public static List<MusicElement> randomMusicElements = new List<MusicElement>();
        private static int _indexOfRandom = -1;

        // References
        public static PlaylistControler Instance { get; set; }
        private MediaElement Music_MediaElement { get; }
        private Image PlayPause_Image { get; set; }
        #endregion Fields


        #region Methods
        public PlaylistControler(UIElementCollection _childrenOfGrid, MediaElement _musicMediaElement)
        {
            if (Instance is not null)
                throw new Exception($"Instance of class {nameof(PlaylistControler)} is duplicated there has to be only one Instance!");

            Instance = this;

            Music_MediaElement = _musicMediaElement;

            foreach (object objekt in _childrenOfGrid)
                AssignButtonFunctionality(objekt);
        }

        #region Initializators
        private void AssignButtonFunctionality(object objekt)
        {
            if (objekt is not Button button || button.Content is not Image image)
                throw new Exception(message: string.Format(CAST_TYPE_MESSAGE_ERROR));

            switch (image.ToolTip)
            {
                case "Previous Music":
                    button.Click += (sender, e) => PlayPrevious();
                    break;

                case "Restart Music":
                    button.Click += (sender, e) => Restart();
                    break;

                case "Stop Music":
                    button.Click += (sender, e) => Stop();
                    break;

                case "Play/Pause Music":
                    PlayPause_Image = button.Content as Image;
                    button.Click += (sender, e) => PauseResume();
                    break;

                case "Next Music":
                    button.Click += (sender, e) => PlayNext();
                    break;

                default:
                    throw new Exception(message: string.Format(TOOLTIP_NOT_IMPLEMENTED_MESSAGE_ERROR, image.ToolTip));
            }
        }

        public void CreatePlaylist()
        {
            Playlist newPlaylist = new Playlist();

            int i = 0;
            foreach (string validName in validMusicNames)
            {
                using ShellObject shell = ShellObject.FromParsingName(validName);
                IShellProperty shellProperty = shell.Properties.System.Media.Duration;

                try
                {
                    ulong time = (ulong)shellProperty.ValueAsObject;
                    TimeSpan normalizedTime = TimeSpan.FromTicks(Convert.ToInt64(time));

                    newPlaylist.listOfMusicElements.Add(new MusicElement()
                    {
                        Initiated = true,
                        Index = i++,
                        MusicPath = validName,
                        MusicName = System.IO.Path.GetFileNameWithoutExtension(validName),
                        Duration = DateAndTime.GetMusicLength(normalizedTime),
                        DurationNumber = time,
                        DurationTimeSpan = normalizedTime
                    });
                }
                catch (NullReferenceException)
                {
                    Logger.ShowStatus(string.Format(FILE_FORMAT_NOT_SUPPORTED_MESSAGE_ERROR, validName), Logger.MessageType.Warning);
                }
                finally
                {
                    shell.Dispose();
                }
            }

            if (newPlaylist.listOfMusicElements.Count > 0)
            {
                currentPlaylist = newPlaylist;
                Load(0);
            }
        }

        /// <summary>
        /// Adds music to list if it is in correct format
        /// </summary>
        public void AddMusicFile(string fileName)
        {
            if (IsMusicType(fileName))
                validMusicNames.Add(fileName);
        }

        /// <summary>
        /// Checks if file format is a music
        /// </summary>
        public bool IsMusicType(string fileName)
        {
            string fileExtension = System.IO.Path.GetExtension(fileName);

            foreach (string format in audioFormats)
                if (fileExtension.Equals(format, StringComparison.InvariantCultureIgnoreCase))
                    return true;

            return false;
        }
        #endregion Initializators

        public static void LoadPlaylist(Playlist playlist)
        {
            if (currentPlaylist == null
                || playlist.Index != currentPlaylist.Index
                || playlist.isModified)
            {
                currentPlaylist = playlist;
                randomMusicElements = new List<MusicElement>();
                _indexOfRandom = -1;
            }
        }

        #region Media Controler Buttons Functionality
        /// <summary>
        /// Plays music based on given index, updates play, pause, stop icons and tooltips, updates logger status, resets scrolling animations, <br/>
        /// updates currently playing music informations and resets current timer with time slider
        /// </summary>
        public void Play(int musicIndex, bool skipLogging = false)
        {
            if (currentPlaylist.listOfMusicElements.Count == 0)
                return;

            if (!skipLogging)
                Logger.ShowStatus($"Now playing {currentPlaylist.listOfMusicElements[musicIndex].MusicName}");

            state = State.Playing;
            StoryboardAnimator.Instance.MusicText = currentPlaylist.listOfMusicElements[musicIndex].MusicName;
            currentlyPlaying = currentPlaylist.listOfMusicElements[musicIndex];
            Music_MediaElement.Source = new Uri(currentlyPlaying.MusicPath);
            Music_MediaElement.Play();
            PlayPauseChangeIcon();
            MediaElementController.Instance.SetTime();
            MusicInformationControler.Instance.ShowInformations();
            MusicInformationControler.Instance.UpdateInformations(currentlyPlaying);
        }

        /// <summary>
        /// Plays previous music based on choosen randomization options to have actual previously played music instead of previous in the index
        /// </summary>
        /// <param name="reversed">
        ///     Determines if alternative button was clicked on music change to quickly 
        ///     change from random to non-random and vice-versa without changing randomization option
        /// </param>
        public void PlayPrevious(bool reversed = false)
        {
            if (currentPlaylist?.listOfMusicElements.Count <= 0 || currentlyPlaying == null)
                return;

            if ((!reversed && MainWindow.Instance.IsRandomOn) || (reversed && !MainWindow.Instance.IsRandomOn))
            {
                if (randomMusicElements.Count == 0 || _indexOfRandom <= 0)
                    return;

                PlayOrLoad(randomMusicElements[--_indexOfRandom].Index);
                return;
            }

            if (currentlyPlaying.Index - 1 < 0)
            {
                PlayOrLoad(currentPlaylist.listOfMusicElements.Count - 1);
                return;
            }

            PlayOrLoad(currentlyPlaying.Index - 1);
        }

        /// <summary>
        /// Plays next music
        /// </summary>
        /// <param name="reversed">
        ///     Determines if alternative button was clicked on music change to quickly 
        ///     change from random to non-random and vice-versa without changing randomization option
        /// </param>
        public void PlayNext(bool reversed = false)
        {
            // If playlist is not loaded or loaded playlist has 0 musics or there is no predefined music playing
            if (currentPlaylist?.listOfMusicElements.Count <= 0 || currentlyPlaying == null)
                return;

            // If we are changing music while it's looped we want it to be reseted instead of changing to different music
            // unless we click it with "alt" to remove loop option and change music to next in the queue
            if (MainWindow.Instance.IsMusicLooped && !reversed)
            {
                Play(currentlyPlaying.Index);
                return;
            }

            // If we have random option (or normal option with alternate music button clicked)
            // queue random music
            if ((!reversed && MainWindow.Instance.IsRandomOn) || (reversed && !MainWindow.Instance.IsRandomOn))
            {
                if (_indexOfRandom == -1)
                {
                    _indexOfRandom++;
                    randomMusicElements.Add(currentlyPlaying);
                }

                _indexOfRandom++;

                if (_indexOfRandom <= (randomMusicElements.Count - 1))
                {
                    PlayOrLoad(randomMusicElements[_indexOfRandom].Index);
                    return;
                }

                // Play music based on randomization option
                // RandomStyle.Off - normal randomization 100 musics = 1% chance for any music
                // WeightedRandomization and RatedRandomization - hover over classes to see formula for randomization
                int randomNumber = (MainWindow.Instance.randomStyle == MainWindow.RandomStyle.Off)
                    ? new Random().Next(0, currentPlaylist.listOfMusicElements.Count)
                    : ((MainWindow.Instance.randomStyle == MainWindow.RandomStyle.Weighted)
                        ? WeightedRandomization.RandomizeWeighted(currentPlaylist, false)
                        : RatedRandomization.RandomizeRated(currentPlaylist, false));

                PlayOrLoad(randomNumber);
                randomMusicElements.Add(currentlyPlaying);
                return;
            }

            // If random is off and this is the last music on playlist play music from the beggining of the playlist
            if (currentlyPlaying.Index + 1 >= currentPlaylist.listOfMusicElements.Count)
            {
                PlayOrLoad(0);
                return;
            }

            // If random is off play next music from index
            PlayOrLoad(currentlyPlaying.Index + 1);
        }

        private void PlayOrLoad(int index)
        {
            if (state == State.Playing)
                Play(index);
            else
                Load(index);
        }

        /// <summary>
        /// If randomization option is on, playing manually music should queue this music as previously played music
        /// not used right now
        /// </summary>
        public static void PlayMusicManuallyFromRandom(MusicElement musicElement, int index, Playlist playlist)
        {
            _indexOfRandom++;
            randomMusicElements.Add(musicElement);
            LoadPlaylist(playlist);
            Instance.Play(index);
        }

        /// <summary>
        /// Completely stop music and resets timers
        /// </summary>
        public void Stop()
        {
            if (currentPlaylist?.listOfMusicElements.Count <= 0 || currentlyPlaying == null)
                return;

            state = State.Paused;
            Music_MediaElement.Stop();
            MusicLengthSlider.Instance.SetValue(0, false);
            PlayPauseChangeIcon();
        }

        /// <summary>
        /// Loads music informations and updates text on MediaElement, pauses music if media is in pause mode <br/>
        /// Mainly used for PlayPrevious, PlayNext and DragAndDropped elements on MediaElement
        /// </summary>
        public void Load(int musicIndex)
        {
            MusicInformationControler.Instance.ShowInformations();

            Play(musicIndex);

            if (state != State.Playing)
                Stop();
        }

        // TODO 314:
        // Potrzebny rework - kolejkuje tą samą muzykę przy kazdym resecie i logguje to do pliku
        public void Restart()
        {
            if (currentPlaylist?.listOfMusicElements.Count <= 0 || currentlyPlaying == null)
                return;

            if (currentlyPlaying.MusicName != string.Empty)
                Play(currentlyPlaying.Index, true);
        }

        public void PauseResume()
        {
            if (state == State.Paused)
                Resume();
            else
                Pause();
        }

        private void Resume()
        {
            if (currentPlaylist?.listOfMusicElements.Count <= 0 || currentlyPlaying == null)
                return;

            Music_MediaElement.Play();
            state = State.Playing;
            PlayPauseChangeIcon();
        }

        private void Pause()
        {
            if (currentPlaylist?.listOfMusicElements.Count <= 0 || currentlyPlaying == null)
                return;

            Music_MediaElement.Pause();
            state = State.Paused;
            PlayPauseChangeIcon();
        }

        private void PlayPauseChangeIcon()
        {
            if (PlayPause_Image.Source == pauseResumeIcons[state])
                return;

            PlayPause_Image.Source = pauseResumeIcons[state];
        }
        #endregion Media Controler Buttons Functionality
        #endregion Methods
    }
}