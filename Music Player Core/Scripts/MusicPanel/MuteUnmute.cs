using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace Music_Player.MusicPanel
{
    public class MuteUnmute
    {
        // Constants
        private const string CONTENT_WRONG_TYPE_ERROR = "Kontent przycisku do Mute/Unmute nie jest typu Image\nTyp: {0}";

        // Data
        private readonly IDictionary<State, BitmapImage> iconResources = new Dictionary<State, BitmapImage>()
        {
            [State.Unmute] = new BitmapImage(new Uri("pack://application:,,,/Resources/Unmuted.png")),
            [State.Mute] = new BitmapImage(new Uri("pack://application:,,,/Resources/Muted.png"))
        };

        // Components
        private enum State { Mute, Unmute };

        // Variables
        private State state = State.Unmute;
        private int previousVolume;

        // References
        public static MuteUnmute Instance { get; set; }
        public Button button;
        private Image icon;
        private VolumeControler volumeControler;


        // Constructor
        public MuteUnmute(Button _button, VolumeControler _volumeControler)
        {
            if (Instance is not null)
                throw new Exception($"Instance of {nameof(MuteUnmute)} already exists!\nDuplicated Instance has to be removed!");

            Instance = this;

            volumeControler = _volumeControler;
            button = _button;
            button.Click += MuteUnmute_Click;

            previousVolume = volumeControler.CurrentVolume;

            if (button.Content is Image)
                icon = button.Content as Image;
            else throw new Exception(message: string.Format(CONTENT_WRONG_TYPE_ERROR, icon.GetType()));
        }

        // Methods
        private void MuteUnmute_Click(object sender, RoutedEventArgs e)
        {
            if (state == State.Unmute)
                Mute();
            else
                Unmute();
        }

        private void Mute()
        {
            state = State.Mute;
            icon.Source = iconResources[state];
            previousVolume = volumeControler.CurrentVolume;
            volumeControler.CurrentVolume = 0;
        }

        public void Unmute()
        {
            if (state == State.Unmute)
                return;

            state = State.Unmute;
            icon.Source = iconResources[state];
            volumeControler.CurrentVolume = previousVolume;
        }
    }
}