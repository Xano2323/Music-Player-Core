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
    }
}