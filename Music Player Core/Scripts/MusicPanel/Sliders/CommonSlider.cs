using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;


namespace Music_Player.MusicPanel.Sliders
{
    public abstract class CommonSlider
    {
        public bool isHoldingOnControler = false;

        protected TextBlock Display { get; }
        protected Slider Slider { get; }
        protected MediaElement Music_MediaElement { get; }


        protected CommonSlider(Slider _slider, TextBlock _display, MediaElement _mediaElement)
        {
            Display = _display;
            Slider = _slider;
            Music_MediaElement = _mediaElement;

            Display.Text = Slider.Value.ToString();
            Slider.ValueChanged += Slider_ValueChanged;
            Slider.MouseMove += Slider_MouseMove;

            Slider.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler((sender, e) =>
            {
                isHoldingOnControler = true;
            }), handledEventsToo: true);

            Slider.AddHandler(UIElement.PreviewMouseLeftButtonUpEvent, new RoutedEventHandler((sender, e) =>
            {
                isHoldingOnControler = false;
            }), handledEventsToo: true);
        }

        protected abstract void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e);

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

                slider.Focus();
            }
        }
    }
}