using System;
using System.Windows;
using System.Windows.Controls;


namespace Music_Player.MusicPanel.Sliders
{
    public class MusicLengthSlider
    {
        private bool changedManually = false;

        public static MusicLengthSlider Instance { get; set; }
        private Slider Slider { get; }
        private Border Border { get; }
        private TextBlock CurrentTimer { get; }


        public MusicLengthSlider(Slider _slider, TextBlock _currentTimer)
        {
            if (Instance is not null)
                throw new Exception($"Instance of class {nameof(MusicLengthSlider)} has been duplicated there can only be one instance at a time!");

            Instance = this;
            Slider = _slider;
            CurrentTimer = _currentTimer;
            Border = Slider.Template.FindName("PART_Border", Slider) as Border;

            Slider.ValueChanged += MusicLenght_Slider_ValueChanged;
        }

        private void MusicLenght_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MediaElementController.Instance.PlaylistControler.currentlyPlaying != null)
            {
                Border.Width = Slider.Value / Slider.Maximum * Slider.ActualWidth;

                if (!changedManually)
                    MediaElementController.Instance.SetTime(Slider.Value / Slider.Maximum);
            }
        }

        public void SetValue(double percentageValue, bool manually = true)
        {
            changedManually = manually;
            Slider.Value = percentageValue * Slider.Maximum; // blad - null muza klikniecie start
            changedManually = false;
        }
    }
}