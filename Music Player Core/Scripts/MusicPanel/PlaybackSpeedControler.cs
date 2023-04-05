using Music_Player.MusicPanel.Sliders;
using System;
using System.Windows;
using System.Windows.Controls;


namespace Music_Player.MusicPanel
{
    public class PlaybackSpeedControler : CommonSlider
    {
        public PlaybackSpeedControler(Slider _slider, TextBlock _display, MediaElement _mediaElement) 
            : base(_slider, _display, _mediaElement)
        {
            Display.Text += "x";
        }

        protected override void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == e.OldValue)
                return;

            if (Slider.Value.ToString() != Slider.Value.ToString("0.00"))
            {
                float number = Convert.ToSingle("0,0" + Slider.Value.ToString("0.00")[3]);
                double value;

                if (number >= 0.05f)
                    value = Convert.ToDouble(Slider.Value.ToString("0.0") + "5");
                else
                    value = Convert.ToDouble(Slider.Value.ToString("0.0"));

                Slider.Value = value;
                MediaElementController.Instance.PlaybackSpeed = Slider.Value;
            }
            
            Display.Text = Slider.Value + "x";
        }
    }
}