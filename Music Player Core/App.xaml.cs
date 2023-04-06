using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;


namespace Music_Player
{
    public partial class App : Application
    {
        public static LinearGradientBrush currentThumbBrush;
        public static MainWindow mainWindow;
        private static MainWindow wnd;


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            wnd = new MainWindow();

            wnd.Show();
        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            Thumb thumb = sender as Thumb;

            (thumb.Template.FindName("Border", thumb) as Border).Background
                = currentThumbBrush = Current.Resources["GrayWhiteReversedGradient"] as LinearGradientBrush;
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Thumb thumb = sender as Thumb;

            (thumb.Template.FindName("Border", thumb) as Border).Background
                = currentThumbBrush = Current.Resources["GrayWhiteGradient"] as LinearGradientBrush;
        }

        private void Thumb_MouseEnter(object sender, MouseEventArgs e)
        {
            Thumb thumb = sender as Thumb;

            (thumb.Template.FindName("Border", thumb) as Border).Background
                = Current.Resources["GrayWhiteHighlightedGradient"] as LinearGradientBrush;
        }

        private void Thumb_MouseLeave(object sender, MouseEventArgs e)
        {
            Thumb thumb = sender as Thumb;

            (thumb.Template.FindName("Border", thumb) as Border).Background = currentThumbBrush;
        }

        /// <summary>
        /// Custom ScrollViewer MouseDown on Border (Thumb background - main part of Vertical ScrollBar)
        /// </summary>
        private void ScrollBar_Background_Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border border)
                throw new InvalidCastException($"{nameof(sender)} is of type {sender.GetType()} has to be of type {typeof(Border)}");

            Point point = e.GetPosition(border);

            string trackName = "PART_Track";
            if (border.FindName(trackName) is not Track track)
                throw new InvalidCastException($"\"{trackName}\" not found in template or it's not of type {typeof(Track)}");

            double valueFromPointY = track.ValueFromPoint(point);

            if (border.TemplatedParent is not ScrollBar scrollBar)
                throw new InvalidCastException($"{nameof(border.TemplatedParent)} has to be of type {typeof(ScrollBar)} and not null");

            if (scrollBar.TemplatedParent is not ScrollViewer scrollViewer)
                throw new InvalidCastException($"{nameof(scrollBar.TemplatedParent)} has to be of type {typeof(ScrollViewer)} and not null");

            scrollViewer.ScrollToVerticalOffset(valueFromPointY);
            scrollViewer.UpdateLayout();

            track.Thumb.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = e.Source
            });
        }
    }
}