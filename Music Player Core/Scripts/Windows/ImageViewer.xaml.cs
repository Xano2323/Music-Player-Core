using System.Windows;


namespace Music_Player.Windows
{
    public partial class ImageViewer : Window
    {
        public bool CanClose { get; set; } = false;


        public ImageViewer() => InitializeComponent();

        public void ShowWindow() => Visibility = Visibility.Visible;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CanClose)
                e.Cancel = true;

            Visibility = Visibility.Hidden;
        }
    }
}