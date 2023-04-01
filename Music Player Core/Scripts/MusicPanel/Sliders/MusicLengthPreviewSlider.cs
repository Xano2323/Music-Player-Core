using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;


namespace Music_Player.MusicPanel.Sliders
{
    public class MusicLengthPreviewSlider
    {
        public bool isHoldingOnControler = false;
        private Rectangle thumbGraphic;

        private TextBlock Display { get; }
        private Slider Slider { get; }
        private Slider RealSlider { get; }


        /// <summary>
        /// Constructs the Preview Slider
        /// </summary>
        /// <param name="_slider">Preview Slider</param>
        /// <param name="_realSlider">Actual Length Slider</param>
        /// <param name="_display">Current Time Display</param>
        /// <param name="_mediaElement">Media Element</param>
        public MusicLengthPreviewSlider(Slider _slider, Slider _realSlider, TextBlock _display, MediaElement _mediaElement)
        {
            Display = _display;
            Slider = _slider;
            RealSlider = _realSlider;

            Thumb thumb = Slider.Template.FindName("Thumb", Slider) as Thumb;
            thumbGraphic = thumb.Template.FindName("ThumbGraphic", thumb) as Rectangle;

            Display.Text = Slider.Value.ToString();
            Slider.MouseMove += Slider_MouseMove;

            Slider.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler((sender, e) =>
            {
                isHoldingOnControler = true;
                thumbGraphic.Visibility = Visibility.Visible;
            }), true);

            Slider.AddHandler(UIElement.PreviewMouseLeftButtonUpEvent, new RoutedEventHandler((sender, e) =>
            {
                if (isHoldingOnControler)
                {
                    thumbGraphic.Visibility = Visibility.Hidden;
                    RealSlider.Value = Slider.Value;
                }

                isHoldingOnControler = false;
            }), true);
        }

        protected void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && isHoldingOnControler)
            {
                Slider slider = sender as Slider;
                Thumb thumb = slider.Template.FindName("Thumb", slider) as Thumb;

                thumb.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
                {
                    RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                    Source = e.Source
                });

                _ = slider.Focus();
            }
        }
    }
}