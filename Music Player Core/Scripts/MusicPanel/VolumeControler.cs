using Music_Player.MusicPanel.Sliders;
using System;
using System.Windows;
using System.Windows.Controls;


namespace Music_Player.MusicPanel
{
    public class VolumeControler : CommonSlider
    {
        private bool valueChangedManually = true;
        /// <summary>
        /// Property used for external classes
        /// </summary>
        public int CurrentVolume
        {
            get => Convert.ToInt32(Slider.Value);
            set
            {
                valueChangedManually = false;
                Slider.Value = value;
            }
        }

        /// <summary>
        /// Constructor for controling volume
        /// </summary>
        /// <param name="_slider">Slider for controling volume</param>
        /// <param name="_display">Percentage of the volume</param>
        /// <param name="_mediaElement">Main MediaElement for playing music</param>
        public VolumeControler(Slider _slider, TextBlock _display, MediaElement _mediaElement) 
            : base(_slider, _display, _mediaElement)
        {
            Display.Text += "%";
            // dodać zczytywanie wartosci z pliku
        }

        protected override void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == e.OldValue)
                return;

            Slider.Value = Convert.ToInt32(Utility.Mathf.Clamp(Slider.Value, 0d, 100d));
            Display.Text = Slider.Value + "%";
            MediaElementControler.Instance.Volume = Slider.Value / 100d;

            if (valueChangedManually)
                MuteUnmute.Instance.Unmute();
            else
                valueChangedManually = true;
        }
    }
}